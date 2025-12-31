using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace AgentSkills.Loader;

/// <summary>
/// Loads skills from the file system by scanning directories for SKILL.md files.
/// </summary>
public sealed class FileSystemSkillLoader : ISkillLoader
{
    private const string SkillFileName = "SKILL.md";
    private const string SkillFileNameLowercase = "skill.md";
    private const string FrontmatterDelimiter = "---";

    private readonly IDeserializer _yamlDeserializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemSkillLoader"/> class.
    /// </summary>
    public FileSystemSkillLoader()
    {
        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(HyphenatedNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    /// <inheritdoc/>
    public SkillSet LoadSkillSet(string directoryPath)
    {
        List<Skill> skills = [];
        List<SkillDiagnostic> diagnostics = [];

        if (!Directory.Exists(directoryPath))
        {
            diagnostics.Add(CreateDiagnostic(
                DiagnosticSeverity.Error,
                $"Directory not found: {directoryPath}",
                directoryPath,
                "LOADER001"));

            return new SkillSet
            {
                Skills = skills,
                Diagnostics = diagnostics
            };
        }

        // Find all SKILL.md files in subdirectories
        var skillFiles = FindSkillFiles(directoryPath);

        foreach (var skillFile in skillFiles)
        {
            var skillDirectory = Path.GetDirectoryName(skillFile)!;
            var (skill, skillDiagnostics) = LoadSkill(skillDirectory);

            if (skill != null)
            {
                skills.Add(skill);
            }

            diagnostics.AddRange(skillDiagnostics);
        }

        return new SkillSet
        {
            Skills = skills,
            Diagnostics = diagnostics
        };
    }

    /// <inheritdoc/>
    public (IReadOnlyList<SkillMetadata> Metadata, IReadOnlyList<SkillDiagnostic> Diagnostics) LoadMetadata(string directoryPath)
    {
        List<SkillMetadata> metadataList = [];
        List<SkillDiagnostic> diagnostics = [];

        if (!Directory.Exists(directoryPath))
        {
            diagnostics.Add(CreateDiagnostic(
                DiagnosticSeverity.Error,
                $"Directory not found: {directoryPath}",
                directoryPath,
                "LOADER001"));

            return (metadataList, diagnostics);
        }

        var skillFiles = FindSkillFiles(directoryPath);

        foreach (var skillFile in skillFiles)
        {
            var skillDirectory = Path.GetDirectoryName(skillFile)!;
            var (metadata, metadataDiagnostics) = LoadSkillMetadata(skillDirectory);

            if (metadata != null)
            {
                metadataList.Add(metadata);
            }

            diagnostics.AddRange(metadataDiagnostics);
        }

        return (metadataList, diagnostics);
    }

    /// <inheritdoc/>
    public (Skill? Skill, IReadOnlyList<SkillDiagnostic> Diagnostics) LoadSkill(string skillDirectoryPath)
    {
        List<SkillDiagnostic> diagnostics = [];
        var skillFilePath = FindSkillMdFile(skillDirectoryPath);

        if (skillFilePath == null)
        {
            diagnostics.Add(CreateDiagnostic(
                DiagnosticSeverity.Error,
                $"SKILL.md not found in directory: {skillDirectoryPath}",
                skillDirectoryPath,
                "LOADER002"));

            return (null, diagnostics);
        }

        try
        {
            var content = File.ReadAllText(skillFilePath);
            var (manifest, instructions, parseDiagnostics) = ParseSkillFile(content, skillFilePath);

            diagnostics.AddRange(parseDiagnostics);

            if (manifest == null)
            {
                return (null, diagnostics);
            }

            var skill = new Skill
            {
                Manifest = manifest,
                Instructions = instructions ?? string.Empty,
                Path = skillDirectoryPath
            };

            return (skill, diagnostics);
        }
        catch (IOException ex)
        {
            diagnostics.Add(CreateDiagnostic(
                DiagnosticSeverity.Error,
                $"Failed to read skill file: {ex.Message}",
                skillFilePath,
                "LOADER003"));

            return (null, diagnostics);
        }
    }

    private (SkillMetadata? Metadata, IReadOnlyList<SkillDiagnostic> Diagnostics) LoadSkillMetadata(string skillDirectoryPath)
    {
        List<SkillDiagnostic> diagnostics = [];
        var skillFilePath = FindSkillMdFile(skillDirectoryPath);

        if (skillFilePath == null)
        {
            diagnostics.Add(CreateDiagnostic(
                DiagnosticSeverity.Error,
                $"SKILL.md not found in directory: {skillDirectoryPath}",
                skillDirectoryPath,
                "LOADER002"));

            return (null, diagnostics);
        }

        try
        {
            // For metadata-only load, we only parse the YAML frontmatter
            var frontmatter = ExtractFrontmatter(skillFilePath);
            if (frontmatter == null)
            {
                diagnostics.Add(CreateDiagnostic(
                    DiagnosticSeverity.Error,
                    "YAML frontmatter not found or invalid",
                    skillFilePath,
                    "LOADER004"));

                return (null, diagnostics);
            }

            var (manifest, parseDiagnostics) = ParseYamlFrontmatter(frontmatter, skillFilePath);
            diagnostics.AddRange(parseDiagnostics);

            if (manifest == null)
            {
                return (null, diagnostics);
            }

            var metadata = new SkillMetadata
            {
                Name = manifest.Name,
                Description = manifest.Description,
                Version = manifest.Version,
                Author = manifest.Author,
                Tags = manifest.Tags,
                Path = skillDirectoryPath
            };

            return (metadata, diagnostics);
        }
        catch (IOException ex)
        {
            diagnostics.Add(CreateDiagnostic(
                DiagnosticSeverity.Error,
                $"Failed to read skill file: {ex.Message}",
                skillFilePath,
                "LOADER003"));

            return (null, diagnostics);
        }
    }

    private IEnumerable<string> FindSkillFiles(string directoryPath)
    {
        try
        {
            // Find directories containing SKILL.md or skill.md files
            var uppercaseFiles = Directory.GetFiles(directoryPath, SkillFileName, SearchOption.AllDirectories);
            var lowercaseFiles = Directory.GetFiles(directoryPath, SkillFileNameLowercase, SearchOption.AllDirectories);
            
            // Combine and deduplicate: prefer SKILL.md when both exist in same directory
            var directories = new HashSet<string>();
            var result = new List<string>();
            
            // First, add all uppercase files
            foreach (var file in uppercaseFiles)
            {
                var dir = Path.GetDirectoryName(file)!;
                directories.Add(dir);
                result.Add(file);
            }
            
            // Then, add lowercase files only if their directory doesn't already have an uppercase file
            foreach (var file in lowercaseFiles)
            {
                var dir = Path.GetDirectoryName(file)!;
                if (!directories.Contains(dir))
                {
                    result.Add(file);
                }
            }
            
            return result;
        }
        catch (UnauthorizedAccessException)
        {
            // Return empty if we don't have access
            return [];
        }
        catch (DirectoryNotFoundException)
        {
            return [];
        }
    }

    /// <summary>
    /// Finds the skill file (SKILL.md or skill.md) in the specified directory.
    /// Prefers SKILL.md if both exist.
    /// </summary>
    /// <param name="directory">The directory to search in.</param>
    /// <returns>The path to the skill file, or null if not found.</returns>
    private string? FindSkillMdFile(string directory)
    {
        var uppercaseFile = Path.Combine(directory, SkillFileName);
        if (File.Exists(uppercaseFile))
        {
            return uppercaseFile;
        }
        
        var lowercaseFile = Path.Combine(directory, SkillFileNameLowercase);
        if (File.Exists(lowercaseFile))
        {
            return lowercaseFile;
        }
        
        return null;
    }

    private string? ExtractFrontmatter(string skillFilePath)
    {
        using var reader = new StreamReader(skillFilePath);

        var firstLine = reader.ReadLine();
        if (firstLine?.Trim() != FrontmatterDelimiter)
        {
            return null;
        }

        var frontmatter = new System.Text.StringBuilder();
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            if (line.Trim() == FrontmatterDelimiter)
            {
                return frontmatter.ToString();
            }
            frontmatter.AppendLine(line);
        }

        // No closing delimiter found
        return null;
    }

    private (SkillManifest? Manifest, string? Instructions, IReadOnlyList<SkillDiagnostic> Diagnostics) ParseSkillFile(
        string content, string filePath)
    {
        List<SkillDiagnostic> diagnostics = [];

        // Split frontmatter and body
        var parts = content.Split([FrontmatterDelimiter], StringSplitOptions.None);

        if (parts.Length < 3)
        {
            diagnostics.Add(CreateDiagnostic(
                DiagnosticSeverity.Error,
                "Invalid SKILL.md format: YAML frontmatter not found or malformed",
                filePath,
                "LOADER004"));

            return (null, null, diagnostics);
        }

        var frontmatter = parts[1].Trim();
        var instructions = string.Join(FrontmatterDelimiter, parts.Skip(2)).Trim();

        var (manifest, parseDiagnostics) = ParseYamlFrontmatter(frontmatter, filePath);
        diagnostics.AddRange(parseDiagnostics);

        return (manifest, instructions, diagnostics);
    }

    private (SkillManifest? Manifest, IReadOnlyList<SkillDiagnostic> Diagnostics) ParseYamlFrontmatter(
        string yaml, string filePath)
    {
        List<SkillDiagnostic> diagnostics = [];

        try
        {
            var yamlObject = _yamlDeserializer.Deserialize<Dictionary<string, object>>(yaml);

            if (yamlObject == null)
            {
                diagnostics.Add(CreateDiagnostic(
                    DiagnosticSeverity.Error,
                    "Failed to parse YAML frontmatter",
                    filePath,
                    "LOADER005"));

                return (null, diagnostics);
            }

            // Extract required fields
            string? name = null;
            if (yamlObject.TryGetValue("name", out var nameObj) && nameObj is string nameStr && !string.IsNullOrWhiteSpace(nameStr))
            {
                name = nameStr;
            }
            else
            {
                diagnostics.Add(CreateDiagnostic(
                    DiagnosticSeverity.Error,
                    "Required field 'name' is missing or invalid",
                    filePath,
                    "LOADER006"));
            }

            string? description = null;
            if (yamlObject.TryGetValue("description", out var descObj) && descObj is string descStr && !string.IsNullOrWhiteSpace(descStr))
            {
                description = descStr;
            }
            else
            {
                diagnostics.Add(CreateDiagnostic(
                    DiagnosticSeverity.Error,
                    "Required field 'description' is missing or invalid",
                    filePath,
                    "LOADER007"));
            }

            // If required fields are missing, return null
            if (diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
            {
                return (null, diagnostics);
            }

            // Extract optional fields
            var version = yamlObject.TryGetValue("version", out var versionObj) && versionObj is string versionStr
                ? versionStr
                : null;

            var author = yamlObject.TryGetValue("author", out var authorObj) && authorObj is string authorStr
                ? authorStr
                : null;

            var tags = ExtractStringList(yamlObject, "tags");
            var allowedTools = ExtractStringList(yamlObject, "allowed-tools");

            // Extract additional fields (fields not in the standard set)
            HashSet<string> knownFields = ["name", "description", "version", "author", "tags", "allowed-tools"];
            var additionalFields = yamlObject
                .Where(kvp => !knownFields.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => (object?)kvp.Value);

            var manifest = new SkillManifest
            {
                Name = name!,
                Description = description!,
                Version = version,
                Author = author,
                Tags = tags,
                AllowedTools = allowedTools,
                AdditionalFields = additionalFields
            };

            return (manifest, diagnostics);
        }
        catch (YamlDotNet.Core.YamlException ex)
        {
            diagnostics.Add(CreateDiagnostic(
                DiagnosticSeverity.Error,
                $"Failed to parse YAML: {ex.Message}",
                filePath,
                "LOADER005"));

            return (null, diagnostics);
        }
    }

    private static IReadOnlyList<string> ExtractStringList(Dictionary<string, object> yamlObject, string key)
    {
        if (!yamlObject.TryGetValue(key, out var value))
        {
            return [];
        }

        if (value is List<object> list)
        {
            return list
                .Where(item => item is string)
                .Cast<string>()
                .ToList();
        }

        if (value is string str)
        {
            return [str];
        }

        return [];
    }

    /// <summary>
    /// Helper method to create a SkillDiagnostic with consistent formatting.
    /// </summary>
    private static SkillDiagnostic CreateDiagnostic(
        DiagnosticSeverity severity,
        string message,
        string? path,
        string code) =>
        new()
        {
            Severity = severity,
            Message = message,
            Path = path,
            Code = code
        };
}
