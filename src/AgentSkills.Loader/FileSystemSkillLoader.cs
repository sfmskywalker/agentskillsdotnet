using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace AgentSkills.Loader;

/// <summary>
/// Loads skills from the file system by scanning directories for SKILL.md files.
/// </summary>
public sealed class FileSystemSkillLoader : ISkillLoader
{
    private const string SkillFileName = "SKILL.md";
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
        var skills = new List<Skill>();
        var diagnostics = new List<SkillDiagnostic>();

        if (!Directory.Exists(directoryPath))
        {
            diagnostics.Add(new SkillDiagnostic
            {
                Severity = DiagnosticSeverity.Error,
                Message = $"Directory not found: {directoryPath}",
                Path = directoryPath,
                Code = "LOADER001"
            });

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
        var metadataList = new List<SkillMetadata>();
        var diagnostics = new List<SkillDiagnostic>();

        if (!Directory.Exists(directoryPath))
        {
            diagnostics.Add(new SkillDiagnostic
            {
                Severity = DiagnosticSeverity.Error,
                Message = $"Directory not found: {directoryPath}",
                Path = directoryPath,
                Code = "LOADER001"
            });

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
        var diagnostics = new List<SkillDiagnostic>();
        var skillFilePath = Path.Combine(skillDirectoryPath, SkillFileName);

        if (!File.Exists(skillFilePath))
        {
            diagnostics.Add(new SkillDiagnostic
            {
                Severity = DiagnosticSeverity.Error,
                Message = $"SKILL.md not found in directory: {skillDirectoryPath}",
                Path = skillDirectoryPath,
                Code = "LOADER002"
            });

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
            diagnostics.Add(new SkillDiagnostic
            {
                Severity = DiagnosticSeverity.Error,
                Message = $"Failed to read skill file: {ex.Message}",
                Path = skillFilePath,
                Code = "LOADER003"
            });

            return (null, diagnostics);
        }
    }

    private (SkillMetadata? Metadata, IReadOnlyList<SkillDiagnostic> Diagnostics) LoadSkillMetadata(string skillDirectoryPath)
    {
        var diagnostics = new List<SkillDiagnostic>();
        var skillFilePath = Path.Combine(skillDirectoryPath, SkillFileName);

        if (!File.Exists(skillFilePath))
        {
            diagnostics.Add(new SkillDiagnostic
            {
                Severity = DiagnosticSeverity.Error,
                Message = $"SKILL.md not found in directory: {skillDirectoryPath}",
                Path = skillDirectoryPath,
                Code = "LOADER002"
            });

            return (null, diagnostics);
        }

        try
        {
            // For metadata-only load, we only parse the YAML frontmatter
            var frontmatter = ExtractFrontmatter(skillFilePath);
            if (frontmatter == null)
            {
                diagnostics.Add(new SkillDiagnostic
                {
                    Severity = DiagnosticSeverity.Error,
                    Message = "YAML frontmatter not found or invalid",
                    Path = skillFilePath,
                    Code = "LOADER004"
                });

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
            diagnostics.Add(new SkillDiagnostic
            {
                Severity = DiagnosticSeverity.Error,
                Message = $"Failed to read skill file: {ex.Message}",
                Path = skillFilePath,
                Code = "LOADER003"
            });

            return (null, diagnostics);
        }
    }

    private IEnumerable<string> FindSkillFiles(string directoryPath)
    {
        try
        {
            return Directory.GetFiles(directoryPath, SkillFileName, SearchOption.AllDirectories);
        }
        catch (UnauthorizedAccessException)
        {
            // Return empty if we don't have access
            return Array.Empty<string>();
        }
        catch (DirectoryNotFoundException)
        {
            return Array.Empty<string>();
        }
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
        var diagnostics = new List<SkillDiagnostic>();

        // Split frontmatter and body
        var parts = content.Split(new[] { FrontmatterDelimiter }, StringSplitOptions.None);

        if (parts.Length < 3)
        {
            diagnostics.Add(new SkillDiagnostic
            {
                Severity = DiagnosticSeverity.Error,
                Message = "Invalid SKILL.md format: YAML frontmatter not found or malformed",
                Path = filePath,
                Code = "LOADER004"
            });

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
        var diagnostics = new List<SkillDiagnostic>();

        try
        {
            var yamlObject = _yamlDeserializer.Deserialize<Dictionary<string, object>>(yaml);

            if (yamlObject == null)
            {
                diagnostics.Add(new SkillDiagnostic
                {
                    Severity = DiagnosticSeverity.Error,
                    Message = "Failed to parse YAML frontmatter",
                    Path = filePath,
                    Code = "LOADER005"
                });

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
                diagnostics.Add(new SkillDiagnostic
                {
                    Severity = DiagnosticSeverity.Error,
                    Message = "Required field 'name' is missing or invalid",
                    Path = filePath,
                    Code = "LOADER006"
                });
            }

            string? description = null;
            if (yamlObject.TryGetValue("description", out var descObj) && descObj is string descStr && !string.IsNullOrWhiteSpace(descStr))
            {
                description = descStr;
            }
            else
            {
                diagnostics.Add(new SkillDiagnostic
                {
                    Severity = DiagnosticSeverity.Error,
                    Message = "Required field 'description' is missing or invalid",
                    Path = filePath,
                    Code = "LOADER007"
                });
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
            var knownFields = new HashSet<string> { "name", "description", "version", "author", "tags", "allowed-tools" };
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
            diagnostics.Add(new SkillDiagnostic
            {
                Severity = DiagnosticSeverity.Error,
                Message = $"Failed to parse YAML: {ex.Message}",
                Path = filePath,
                Code = "LOADER005"
            });

            return (null, diagnostics);
        }
    }

    private static IReadOnlyList<string> ExtractStringList(Dictionary<string, object> yamlObject, string key)
    {
        if (!yamlObject.TryGetValue(key, out var value))
        {
            return Array.Empty<string>();
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
            return new[] { str };
        }

        return Array.Empty<string>();
    }
}
