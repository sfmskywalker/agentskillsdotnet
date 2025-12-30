using AgentSkills.Loader;

namespace AgentSkills.Tests;

/// <summary>
/// Integration tests that verify the full pipeline: scan → parse → validate → load
/// </summary>
public class IntegrationTests
{
    private readonly FileSystemSkillLoader _loader;
    private readonly string _fixturesPath;

    public IntegrationTests()
    {
        _loader = new FileSystemSkillLoader();
        
        var assemblyLocation = AppContext.BaseDirectory;
        var solutionRoot = Path.GetFullPath(Path.Combine(assemblyLocation, "..", "..", "..", "..", ".."));
        _fixturesPath = Path.Combine(solutionRoot, "fixtures", "skills");
    }

    [Fact]
    public void FullPipeline_LoadMetadataThenLoadSkillSet_ProducesSameSkills()
    {
        // Act - Load metadata first (fast path)
        var (metadata, _) = _loader.LoadMetadata(_fixturesPath);
        
        // Act - Load full skill set
        var skillSet = _loader.LoadSkillSet(_fixturesPath);

        // Assert - Metadata and skills should match
        Assert.Equal(metadata.Count, skillSet.Skills.Count);
        
        foreach (var meta in metadata)
        {
            var skill = skillSet.GetSkill(meta.Name);
            Assert.NotNull(skill);
            Assert.Equal(meta.Name, skill.Manifest.Name);
            Assert.Equal(meta.Description, skill.Manifest.Description);
            Assert.Equal(meta.Version, skill.Manifest.Version);
            Assert.Equal(meta.Author, skill.Manifest.Author);
            Assert.Equal(meta.Tags, skill.Manifest.Tags);
        }
    }

    [Fact]
    public void FullPipeline_LoadSkillSet_SkillMetadataMatchesManifest()
    {
        // Act
        var skillSet = _loader.LoadSkillSet(_fixturesPath);

        // Assert
        foreach (var skill in skillSet.Skills)
        {
            var metadata = skill.Metadata;
            Assert.Equal(skill.Manifest.Name, metadata.Name);
            Assert.Equal(skill.Manifest.Description, metadata.Description);
            Assert.Equal(skill.Manifest.Version, metadata.Version);
            Assert.Equal(skill.Manifest.Author, metadata.Author);
            Assert.Equal(skill.Manifest.Tags, metadata.Tags);
        }
    }

    [Fact]
    public void FullPipeline_SkillSetQueries_WorkCorrectly()
    {
        // Act
        var skillSet = _loader.LoadSkillSet(_fixturesPath);

        // Assert - GetSkill works
        var exampleSkill = skillSet.GetSkill("example-skill");
        Assert.NotNull(exampleSkill);
        Assert.Equal("example-skill", exampleSkill.Manifest.Name);

        // Assert - GetSkill is case-insensitive
        var exampleSkill2 = skillSet.GetSkill("EXAMPLE-SKILL");
        Assert.NotNull(exampleSkill2);
        Assert.Equal("example-skill", exampleSkill2.Manifest.Name);

        // Assert - GetSkillsByTag works
        var demoSkills = skillSet.GetSkillsByTag("demo").ToList();
        Assert.NotEmpty(demoSkills);
        Assert.Contains(demoSkills, s => s.Manifest.Name == "example-skill");

        // Assert - GetSkillsByTag is case-insensitive
        var demoSkills2 = skillSet.GetSkillsByTag("DEMO").ToList();
        Assert.Equal(demoSkills.Count, demoSkills2.Count);
    }

    [Fact]
    public void FullPipeline_DiagnosticsAreCollected_ForInvalidSkills()
    {
        // Act
        var skillSet = _loader.LoadSkillSet(_fixturesPath);

        // Assert - Should have some diagnostics from invalid skills
        Assert.NotEmpty(skillSet.Diagnostics);
        
        // Should have errors from invalid-no-name
        Assert.Contains(skillSet.Diagnostics, d => 
            d.Code == "LOADER006" && d.Message.Contains("name"));
        
        // Should have errors from no-frontmatter
        Assert.Contains(skillSet.Diagnostics, d => 
            d.Code == "LOADER004");
    }

    [Fact]
    public void FullPipeline_ValidSkillsAreLoaded_DespiteInvalidOnes()
    {
        // Act
        var skillSet = _loader.LoadSkillSet(_fixturesPath);

        // Assert - Valid skills should be present
        Assert.NotEmpty(skillSet.Skills);
        Assert.Contains(skillSet.Skills, s => s.Manifest.Name == "example-skill");
        Assert.Contains(skillSet.Skills, s => s.Manifest.Name == "minimal-skill");
        
        // Invalid skills should not be in the collection
        Assert.DoesNotContain(skillSet.Skills, s => s.Manifest.Name == "invalid-no-name");
    }

    [Fact]
    public void FullPipeline_SkillSetIsValid_OnlyWhenNoErrors()
    {
        // Act
        var skillSet = _loader.LoadSkillSet(_fixturesPath);

        // Assert
        var hasErrors = skillSet.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
        Assert.Equal(!hasErrors, skillSet.IsValid);
    }

    [Fact]
    public void FullPipeline_EmptyDirectory_ReturnsEmptySkillSet()
    {
        // Arrange - Create a temporary empty directory
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Act
            var skillSet = _loader.LoadSkillSet(tempDir);

            // Assert
            Assert.Empty(skillSet.Skills);
            Assert.Empty(skillSet.Diagnostics);
            Assert.True(skillSet.IsValid);
        }
        finally
        {
            // Cleanup
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void FullPipeline_LoadSingleSkill_WorksIndependently()
    {
        // Arrange
        var skillPath = Path.Combine(_fixturesPath, "example-skill");

        // Act
        var (skill, diagnostics) = _loader.LoadSkill(skillPath);

        // Assert
        Assert.NotNull(skill);
        Assert.Empty(diagnostics);
        Assert.Equal("example-skill", skill.Manifest.Name);
        Assert.NotEmpty(skill.Instructions);
        
        // Metadata should be accessible
        var metadata = skill.Metadata;
        Assert.Equal(skill.Manifest.Name, metadata.Name);
    }
}
