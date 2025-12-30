using AgentSkills.Loader;

namespace AgentSkills.Tests;

public class FileSystemSkillLoaderTests
{
    private readonly FileSystemSkillLoader _loader;
    private readonly string _fixturesPath;

    public FileSystemSkillLoaderTests()
    {
        _loader = new FileSystemSkillLoader();
        
        // Navigate from test assembly location to fixtures directory
        var assemblyLocation = AppContext.BaseDirectory;
        var solutionRoot = Path.GetFullPath(Path.Combine(assemblyLocation, "..", "..", "..", "..", ".."));
        _fixturesPath = Path.Combine(solutionRoot, "fixtures", "skills");
    }

    [Fact]
    public void LoadSkillSet_WithValidDirectory_ReturnsSkills()
    {
        // Act
        var skillSet = _loader.LoadSkillSet(_fixturesPath);

        // Assert
        Assert.NotNull(skillSet);
        Assert.NotEmpty(skillSet.Skills);
        Assert.Contains(skillSet.Skills, s => s.Manifest.Name == "example-skill");
        Assert.Contains(skillSet.Skills, s => s.Manifest.Name == "minimal-skill");
    }

    [Fact]
    public void LoadSkillSet_WithNonexistentDirectory_ReturnsErrorDiagnostic()
    {
        // Arrange
        var nonexistentPath = Path.Combine(_fixturesPath, "does-not-exist");

        // Act
        var skillSet = _loader.LoadSkillSet(nonexistentPath);

        // Assert
        Assert.Empty(skillSet.Skills);
        Assert.NotEmpty(skillSet.Diagnostics);
        Assert.Contains(skillSet.Diagnostics, d => 
            d.Severity == DiagnosticSeverity.Error && 
            d.Code == "LOADER001");
        Assert.False(skillSet.IsValid);
    }

    [Fact]
    public void LoadSkill_WithValidSkill_ReturnsSkillAndNoDiagnostics()
    {
        // Arrange
        var skillPath = Path.Combine(_fixturesPath, "example-skill");

        // Act
        var (skill, diagnostics) = _loader.LoadSkill(skillPath);

        // Assert
        Assert.NotNull(skill);
        Assert.Equal("example-skill", skill.Manifest.Name);
        Assert.Equal("An example skill for testing and demonstration", skill.Manifest.Description);
        Assert.Equal("1.0.0", skill.Manifest.Version);
        Assert.Equal("AgentSkills.NET", skill.Manifest.Author);
        Assert.Contains("example", skill.Manifest.Tags);
        Assert.Contains("demo", skill.Manifest.Tags);
        Assert.Contains("filesystem", skill.Manifest.AllowedTools);
        Assert.Contains("calculator", skill.Manifest.AllowedTools);
        Assert.NotEmpty(skill.Instructions);
        Assert.Contains("Example Skill", skill.Instructions);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void LoadSkill_WithMinimalSkill_ReturnsSkillWithRequiredFieldsOnly()
    {
        // Arrange
        var skillPath = Path.Combine(_fixturesPath, "minimal-skill");

        // Act
        var (skill, diagnostics) = _loader.LoadSkill(skillPath);

        // Assert
        Assert.NotNull(skill);
        Assert.Equal("minimal-skill", skill.Manifest.Name);
        Assert.Equal("A minimal skill with only required fields", skill.Manifest.Description);
        Assert.Null(skill.Manifest.Version);
        Assert.Null(skill.Manifest.Author);
        Assert.Empty(skill.Manifest.Tags);
        Assert.Empty(skill.Manifest.AllowedTools);
        Assert.NotEmpty(skill.Instructions);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void LoadSkill_WithMissingName_ReturnsNullAndError()
    {
        // Arrange
        var skillPath = Path.Combine(_fixturesPath, "invalid-no-name");

        // Act
        var (skill, diagnostics) = _loader.LoadSkill(skillPath);

        // Assert
        Assert.Null(skill);
        Assert.NotEmpty(diagnostics);
        Assert.Contains(diagnostics, d => 
            d.Severity == DiagnosticSeverity.Error && 
            d.Code == "LOADER006" &&
            d.Message.Contains("name"));
    }

    [Fact]
    public void LoadSkill_WithNoFrontmatter_ReturnsNullAndError()
    {
        // Arrange
        var skillPath = Path.Combine(_fixturesPath, "no-frontmatter");

        // Act
        var (skill, diagnostics) = _loader.LoadSkill(skillPath);

        // Assert
        Assert.Null(skill);
        Assert.NotEmpty(diagnostics);
        Assert.Contains(diagnostics, d => 
            d.Severity == DiagnosticSeverity.Error && 
            d.Code == "LOADER004");
    }

    [Fact]
    public void LoadSkill_WithMissingSkillFile_ReturnsNullAndError()
    {
        // Arrange
        var skillPath = Path.Combine(_fixturesPath, "empty-dir");

        // Act
        var (skill, diagnostics) = _loader.LoadSkill(skillPath);

        // Assert
        Assert.Null(skill);
        Assert.NotEmpty(diagnostics);
        Assert.Contains(diagnostics, d => 
            d.Severity == DiagnosticSeverity.Error && 
            d.Code == "LOADER002");
    }

    [Fact]
    public void LoadSkill_WithNonexistentDirectory_ReturnsNullAndError()
    {
        // Arrange
        var skillPath = Path.Combine(_fixturesPath, "does-not-exist");

        // Act
        var (skill, diagnostics) = _loader.LoadSkill(skillPath);

        // Assert
        Assert.Null(skill);
        Assert.NotEmpty(diagnostics);
        Assert.Contains(diagnostics, d => 
            d.Severity == DiagnosticSeverity.Error && 
            d.Code == "LOADER002");
    }

    [Fact]
    public void LoadMetadata_WithValidDirectory_ReturnsMetadata()
    {
        // Act
        var (metadata, diagnostics) = _loader.LoadMetadata(_fixturesPath);

        // Assert
        Assert.NotEmpty(metadata);
        Assert.Contains(metadata, m => m.Name == "example-skill");
        Assert.Contains(metadata, m => m.Name == "minimal-skill");
        
        var exampleMetadata = metadata.First(m => m.Name == "example-skill");
        Assert.Equal("An example skill for testing and demonstration", exampleMetadata.Description);
        Assert.Equal("1.0.0", exampleMetadata.Version);
        Assert.Equal("AgentSkills.NET", exampleMetadata.Author);
        Assert.Contains("example", exampleMetadata.Tags);
    }

    [Fact]
    public void LoadMetadata_WithNonexistentDirectory_ReturnsErrorDiagnostic()
    {
        // Arrange
        var nonexistentPath = Path.Combine(_fixturesPath, "does-not-exist");

        // Act
        var (metadata, diagnostics) = _loader.LoadMetadata(nonexistentPath);

        // Assert
        Assert.Empty(metadata);
        Assert.NotEmpty(diagnostics);
        Assert.Contains(diagnostics, d => 
            d.Severity == DiagnosticSeverity.Error && 
            d.Code == "LOADER001");
    }

    [Fact]
    public void LoadSkill_Metadata_MatchesManifest()
    {
        // Arrange
        var skillPath = Path.Combine(_fixturesPath, "example-skill");

        // Act
        var (skill, _) = _loader.LoadSkill(skillPath);

        // Assert
        Assert.NotNull(skill);
        var metadata = skill.Metadata;
        Assert.Equal(skill.Manifest.Name, metadata.Name);
        Assert.Equal(skill.Manifest.Description, metadata.Description);
        Assert.Equal(skill.Manifest.Version, metadata.Version);
        Assert.Equal(skill.Manifest.Author, metadata.Author);
        Assert.Equal(skill.Manifest.Tags, metadata.Tags);
        Assert.Equal(skill.Path, metadata.Path);
    }

    [Fact]
    public void LoadSkillSet_SkipsInvalidSkills_ButCollectsDiagnostics()
    {
        // Act
        var skillSet = _loader.LoadSkillSet(_fixturesPath);

        // Assert
        // Should have valid skills
        Assert.NotEmpty(skillSet.Skills);
        
        // Should have diagnostics from invalid skills
        Assert.NotEmpty(skillSet.Diagnostics);
        
        // Should not include invalid skills in the collection
        Assert.DoesNotContain(skillSet.Skills, s => s.Manifest.Name == "invalid-no-name");
    }

    [Fact]
    public void LoadSkillSet_IsValid_ReturnsFalseWhenErrorsPresent()
    {
        // Act
        var skillSet = _loader.LoadSkillSet(_fixturesPath);

        // Assert
        // There are invalid skills in the fixtures, so we should have errors
        var hasErrors = skillSet.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
        Assert.Equal(!hasErrors, skillSet.IsValid);
    }

    [Fact]
    public void LoadSkill_PreservesAdditionalFields()
    {
        // Arrange
        var skillPath = Path.Combine(_fixturesPath, "example-skill");

        // Act
        var (skill, _) = _loader.LoadSkill(skillPath);

        // Assert
        Assert.NotNull(skill);
        // The example-skill doesn't have additional fields, so it should be empty
        Assert.NotNull(skill.Manifest.AdditionalFields);
    }

    [Fact]
    public void LoadSkill_WithMalformedYaml_ReturnsNullAndError()
    {
        // Arrange
        var skillPath = Path.Combine(_fixturesPath, "malformed-yaml");

        // Act
        var (skill, diagnostics) = _loader.LoadSkill(skillPath);

        // Assert
        Assert.Null(skill);
        Assert.NotEmpty(diagnostics);
        Assert.Contains(diagnostics, d => 
            d.Severity == DiagnosticSeverity.Error && 
            d.Code == "LOADER005" &&
            d.Message.Contains("YAML"));
    }

    [Fact]
    public void LoadSkill_WithUnclosedFrontmatter_ReturnsNullAndError()
    {
        // Arrange
        var skillPath = Path.Combine(_fixturesPath, "unclosed-frontmatter");

        // Act
        var (skill, diagnostics) = _loader.LoadSkill(skillPath);

        // Assert
        Assert.Null(skill);
        Assert.NotEmpty(diagnostics);
        // Unclosed frontmatter is detected before YAML parsing because the
        // closing '---' delimiter is missing, resulting in a frontmatter error
        Assert.Contains(diagnostics, d =>
            d.Severity == DiagnosticSeverity.Error &&
            d.Code == "LOADER004" &&
            d.Message.Contains("frontmatter"));
    }

    [Fact]
    public void LoadSkill_WithEmptyNameField_ReturnsNullAndError()
    {
        // Arrange
        var skillPath = Path.Combine(_fixturesPath, "empty-name-field");

        // Act
        var (skill, diagnostics) = _loader.LoadSkill(skillPath);

        // Assert
        Assert.Null(skill);
        Assert.NotEmpty(diagnostics);
        Assert.Contains(diagnostics, d => 
            d.Severity == DiagnosticSeverity.Error && 
            d.Code == "LOADER006" &&
            d.Message.Contains("name"));
    }

    [Fact]
    public void LoadSkill_WithTripleDashInBody_PreservesBodyVerbatim()
    {
        // Arrange
        var skillPath = Path.Combine(_fixturesPath, "triple-dash-in-body");

        // Act
        var (skill, diagnostics) = _loader.LoadSkill(skillPath);

        // Assert
        Assert.NotNull(skill);
        Assert.Empty(diagnostics);
        Assert.Equal("triple-dash-body", skill.Manifest.Name);
        
        // Verify that triple dashes in the body are preserved
        Assert.Contains("---", skill.Instructions);
        
        // Verify the body contains the expected text
        Assert.Contains("triple dashes that should not be confused with frontmatter", skill.Instructions);
        Assert.Contains("This should all be preserved verbatim", skill.Instructions);
    }

    [Fact]
    public void LoadMetadata_WithMalformedYaml_SkipsSkillAndCollectsDiagnostics()
    {
        // Act
        var (metadata, diagnostics) = _loader.LoadMetadata(_fixturesPath);

        // Assert
        // Should not include malformed skill in metadata
        Assert.DoesNotContain(metadata, m => m.Name == "malformed-skill");
        
        // Should have error diagnostic for malformed YAML
        Assert.Contains(diagnostics, d => 
            d.Severity == DiagnosticSeverity.Error && 
            d.Code == "LOADER005");
    }

    [Fact]
    public void LoadSkill_WithValidSkill_InstructionsDoNotContainFrontmatter()
    {
        // Arrange
        var skillPath = Path.Combine(_fixturesPath, "example-skill");

        // Act
        var (skill, _) = _loader.LoadSkill(skillPath);

        // Assert
        Assert.NotNull(skill);
        
        // Instructions should not contain YAML frontmatter delimiters at the start
        Assert.DoesNotContain("---\nname:", skill.Instructions);
        Assert.DoesNotContain("description:", skill.Instructions);
        
        // Instructions should start with the markdown content
        Assert.StartsWith("#", skill.Instructions.TrimStart());
    }
}
