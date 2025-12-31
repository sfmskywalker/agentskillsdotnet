using AgentSkills.Loader;
using AgentSkills.Prompts;
using System.Text;

namespace AgentSkills.Tests;

/// <summary>
/// Golden file tests for prompt rendering.
/// These tests ensure rendering output remains stable and catches unintended changes.
/// </summary>
public class GoldenFileTests
{
    private readonly string _fixturesPath;
    private readonly string _goldenFilesPath;
    private readonly FileSystemSkillLoader _loader;
    private readonly DefaultSkillPromptRenderer _renderer;

    public GoldenFileTests()
    {
        var assemblyLocation = AppContext.BaseDirectory;
        var solutionRoot = Path.GetFullPath(Path.Combine(assemblyLocation, "..", "..", "..", "..", ".."));
        _fixturesPath = Path.Combine(solutionRoot, "fixtures", "skills");
        _goldenFilesPath = Path.Combine(assemblyLocation, "..", "..", "..", "GoldenFiles");
        
        _loader = new FileSystemSkillLoader();
        _renderer = new DefaultSkillPromptRenderer();
    }

    [Fact]
    public void RenderSkillList_ExampleSkill_MatchesGoldenFile()
    {
        // Arrange
        var (metadata, _) = _loader.LoadMetadata(_fixturesPath);
        var exampleSkill = metadata.FirstOrDefault(m => m.Name == "example-skill");
        Assert.NotNull(exampleSkill);

        // Act
        var rendered = _renderer.RenderSkillList(new[] { exampleSkill });

        // Assert
        AssertMatchesGoldenFile("skill-list-example.txt", rendered);
    }

    [Fact]
    public void RenderSkillList_MultipleSkills_MatchesGoldenFile()
    {
        // Arrange
        var (metadata, _) = _loader.LoadMetadata(_fixturesPath);
        var validSkills = metadata
            .Where(m => m.Name is "example-skill" or "minimal-skill" or "complete-skill")
            .OrderBy(m => m.Name)
            .ToList();
        
        Assert.NotEmpty(validSkills);

        // Act
        var rendered = _renderer.RenderSkillList(validSkills);

        // Assert
        AssertMatchesGoldenFile("skill-list-multiple.txt", rendered);
    }

    [Fact]
    public void RenderSkillDetails_ExampleSkill_MatchesGoldenFile()
    {
        // Arrange
        var skillPath = Path.Combine(_fixturesPath, "example-skill");
        var (skill, _) = _loader.LoadSkill(skillPath);
        Assert.NotNull(skill);

        // Act
        var rendered = _renderer.RenderSkillDetails(skill);

        // Assert
        AssertMatchesGoldenFile("skill-details-example.txt", rendered);
    }

    [Fact]
    public void RenderSkillDetails_CompleteSkill_MatchesGoldenFile()
    {
        // Arrange
        var skillPath = Path.Combine(_fixturesPath, "complete-skill");
        var (skill, _) = _loader.LoadSkill(skillPath);
        Assert.NotNull(skill);

        // Act
        var rendered = _renderer.RenderSkillDetails(skill);

        // Assert
        AssertMatchesGoldenFile("skill-details-complete.txt", rendered);
    }

    [Fact]
    public void RenderSkillDetails_MinimalSkill_MatchesGoldenFile()
    {
        // Arrange
        var skillPath = Path.Combine(_fixturesPath, "minimal-skill");
        var (skill, _) = _loader.LoadSkill(skillPath);
        Assert.NotNull(skill);

        // Act
        var rendered = _renderer.RenderSkillDetails(skill);

        // Assert
        AssertMatchesGoldenFile("skill-details-minimal.txt", rendered);
    }

    [Fact]
    public void RenderSkillDetails_WithOptionsNoVersion_MatchesGoldenFile()
    {
        // Arrange
        var skillPath = Path.Combine(_fixturesPath, "example-skill");
        var (skill, _) = _loader.LoadSkill(skillPath);
        Assert.NotNull(skill);

        var options = new PromptRenderOptions
        {
            IncludeVersion = false,
            IncludeAuthor = true,
            IncludeTags = true
        };

        // Act
        var rendered = _renderer.RenderSkillDetails(skill, options);

        // Assert
        AssertMatchesGoldenFile("skill-details-no-version.txt", rendered);
    }

    [Fact]
    public void RenderSkillDetails_WithResourcePolicyExcludeAll_MatchesGoldenFile()
    {
        // Arrange
        var skillPath = Path.Combine(_fixturesPath, "example-skill");
        var (skill, _) = _loader.LoadSkill(skillPath);
        Assert.NotNull(skill);

        var options = new PromptRenderOptions
        {
            ResourcePolicy = ExcludeAllResourcePolicy.Instance
        };

        // Act
        var rendered = _renderer.RenderSkillDetails(skill, options);

        // Assert
        AssertMatchesGoldenFile("skill-details-no-resources.txt", rendered);
    }

    /// <summary>
    /// Helper method to compare rendered output with golden file.
    /// Set UPDATE_GOLDEN_FILES environment variable to update golden files.
    /// </summary>
    private void AssertMatchesGoldenFile(string fileName, string actual)
    {
        var goldenFilePath = Path.Combine(_goldenFilesPath, fileName);
        var updateGoldenFiles = Environment.GetEnvironmentVariable("UPDATE_GOLDEN_FILES") == "1";

        // Normalize line endings for cross-platform compatibility
        actual = NormalizeLineEndings(actual);

        if (updateGoldenFiles || !File.Exists(goldenFilePath))
        {
            // Create directory if it doesn't exist
            Directory.CreateDirectory(Path.GetDirectoryName(goldenFilePath)!);
            
            // Write the golden file
            File.WriteAllText(goldenFilePath, actual, Encoding.UTF8);
            
            if (updateGoldenFiles)
            {
                // In update mode, test passes after writing
                Assert.True(true, $"Golden file updated: {fileName}");
                return;
            }
        }

        // Read expected content
        var expected = File.ReadAllText(goldenFilePath, Encoding.UTF8);
        expected = NormalizeLineEndings(expected);

        // Compare
        if (actual != expected)
        {
            // Provide helpful diff information
            var message = new StringBuilder();
            message.AppendLine($"Golden file mismatch: {fileName}");
            message.AppendLine("Set UPDATE_GOLDEN_FILES=1 to update golden files.");
            message.AppendLine();
            message.AppendLine("Expected:");
            message.AppendLine(expected);
            message.AppendLine();
            message.AppendLine("Actual:");
            message.AppendLine(actual);
            
            Assert.Fail(message.ToString());
        }
    }

    /// <summary>
    /// Normalize line endings to LF for cross-platform compatibility.
    /// </summary>
    private static string NormalizeLineEndings(string text)
    {
        return text.Replace("\r\n", "\n").Replace("\r", "\n");
    }
}
