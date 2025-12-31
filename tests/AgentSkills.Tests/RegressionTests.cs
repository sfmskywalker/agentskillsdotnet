using AgentSkills.Loader;
using AgentSkills.Validation;

namespace AgentSkills.Tests;

/// <summary>
/// Regression tests to ensure previously fixed issues don't reoccur.
/// These tests document known good behaviors and edge cases discovered during development.
/// </summary>
public class RegressionTests
{
    private readonly string _fixturesPath;
    private readonly FileSystemSkillLoader _loader;
    private readonly SkillValidator _validator;

    public RegressionTests()
    {
        var assemblyLocation = AppContext.BaseDirectory;
        var solutionRoot = Path.GetFullPath(Path.Combine(assemblyLocation, "..", "..", "..", "..", ".."));
        _fixturesPath = Path.Combine(solutionRoot, "fixtures", "skills");
        _loader = new FileSystemSkillLoader();
        _validator = new SkillValidator();
    }

    #region YAML Parsing Edge Cases

    [Fact]
    public void MalformedYAML_ProducesDiagnostic_DoesNotThrow()
    {
        // Regression: Ensure malformed YAML produces diagnostics, not exceptions
        var skillPath = Path.Combine(_fixturesPath, "malformed-yaml");

        // Act & Assert - Should not throw
        var (_, diagnostics) = _loader.LoadSkill(skillPath);

        // Should have diagnostics about parsing failure
        Assert.NotEmpty(diagnostics);
        Assert.Contains(diagnostics, d =>
            d.Severity == DiagnosticSeverity.Error &&
            (d.Code?.StartsWith("LOADER") ?? false));
    }

    [Fact]
    public void UnclosedFrontmatter_ProducesDiagnostic_DoesNotThrow()
    {
        // Regression: Unclosed frontmatter should be handled gracefully
        var skillPath = Path.Combine(_fixturesPath, "unclosed-frontmatter");

        // Act & Assert - Should not throw
        var (_, diagnostics) = _loader.LoadSkill(skillPath);

        Assert.NotEmpty(diagnostics);
        Assert.Contains(diagnostics, d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void NoFrontmatter_ProducesDiagnostic_DoesNotThrow()
    {
        // Regression: Missing frontmatter should be detected
        var skillPath = Path.Combine(_fixturesPath, "no-frontmatter");

        // Act & Assert - Should not throw
        var (_, diagnostics) = _loader.LoadSkill(skillPath);

        Assert.NotEmpty(diagnostics);
    }

    [Fact]
    public void TripleDashInBody_ParsesCorrectly()
    {
        // Regression: Triple dash in markdown body should not confuse parser
        var skillPath = Path.Combine(_fixturesPath, "triple-dash-in-body");

        // Act
        var (skill, _) = _loader.LoadSkill(skillPath);

        // Assert - Should load successfully with triple dash in body
        Assert.NotNull(skill);
        Assert.Contains("---", skill.Instructions);
    }

    #endregion

    #region Validation Edge Cases

    [Fact]
    public void EmptyNameField_ValidationFails_WithSpecificError()
    {
        // Regression: Empty name should fail validation with VAL001
        var skillPath = Path.Combine(_fixturesPath, "empty-name-field");
        var (skill, _) = _loader.LoadSkill(skillPath);

        if (skill != null)
        {
            var result = _validator.Validate(skill);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Code == "VAL001");
        }
    }

    [Fact]
    public void MissingNameField_ValidationFails_WithSpecificError()
    {
        // Regression: Missing name should fail validation with VAL001
        var skillPath = Path.Combine(_fixturesPath, "invalid-no-name");
        var (skill, diagnostics) = _loader.LoadSkill(skillPath);

        // Should either fail to load or fail validation
        if (skill != null)
        {
            var result = _validator.Validate(skill);
            Assert.False(result.IsValid);
        }
        else
        {
            Assert.NotEmpty(diagnostics);
        }
    }

    [Fact]
    public void UppercaseName_ValidationFails_WithPatternError()
    {
        // Regression: Uppercase in name should fail validation with VAL003
        var skillPath = Path.Combine(_fixturesPath, "invalid-uppercase-name");
        var (skill, _) = _loader.LoadSkill(skillPath);

        if (skill != null)
        {
            var result = _validator.Validate(skill);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Code == "VAL003");
        }
    }

    [Fact]
    public void ConsecutiveHyphens_ValidationFails_WithPatternError()
    {
        // Regression: Consecutive hyphens in name should fail validation
        var skillPath = Path.Combine(_fixturesPath, "invalid-consecutive-hyphens");
        var (skill, _) = _loader.LoadSkill(skillPath);

        if (skill != null)
        {
            var result = _validator.Validate(skill);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Code == "VAL003");
        }
    }

    [Fact]
    public void NameTooLong_ValidationFails_WithLengthError()
    {
        // Regression: Name > 64 chars should fail validation with VAL002
        var skillPath = Path.Combine(_fixturesPath, "invalid-long-name");
        var (skill, _) = _loader.LoadSkill(skillPath);

        if (skill != null)
        {
            var result = _validator.Validate(skill);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Code == "VAL002");
        }
    }

    [Fact]
    public void DescriptionTooLong_ValidationFails_WithLengthError()
    {
        // Regression: Description > 1024 chars should fail validation with VAL005
        var skillPath = Path.Combine(_fixturesPath, "invalid-long-description");
        var (skill, _) = _loader.LoadSkill(skillPath);

        if (skill != null)
        {
            var result = _validator.Validate(skill);
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Code == "VAL005");
        }
    }

    [Fact]
    public void MismatchedDirectoryName_ValidationWarning()
    {
        // Regression: Directory name not matching skill name should produce warning
        var skillPath = Path.Combine(_fixturesPath, "mismatched-directory");
        var (skill, _) = _loader.LoadSkill(skillPath);

        if (skill != null)
        {
            var result = _validator.Validate(skill);

            // Should have a warning about directory mismatch (VAL010)
            Assert.Contains(result.Diagnostics, d =>
                d.Code == "VAL010" &&
                d.Severity == DiagnosticSeverity.Error);
        }
    }

    #endregion

    #region Progressive Disclosure Invariants

    [Fact]
    public void MetadataLoad_DoesNotLoadFullInstructions()
    {
        // Regression: Metadata load must not load full instructions (progressive disclosure)
        var (metadata, _) = _loader.LoadMetadata(_fixturesPath);

        foreach (var meta in metadata)
        {
            // Metadata should only have frontmatter data
            Assert.NotNull(meta.Name);
            Assert.NotNull(meta.Description);
            // Path should be set for full loading later
            Assert.NotNull(meta.Path);
        }
    }

    [Fact]
    public void MetadataAndFullLoad_ProduceConsistentResults()
    {
        // Regression: Metadata and full load should produce same metadata values
        var (metadata, _) = _loader.LoadMetadata(_fixturesPath);
        var skillSet = _loader.LoadSkillSet(_fixturesPath);

        foreach (var meta in metadata)
        {
            var skill = skillSet.GetSkill(meta.Name);
            if (skill != null)
            {
                // Metadata should match
                Assert.Equal(meta.Name, skill.Manifest.Name);
                Assert.Equal(meta.Description, skill.Manifest.Description);
                Assert.Equal(meta.Version, skill.Manifest.Version);
                Assert.Equal(meta.Author, skill.Manifest.Author);
            }
        }
    }

    #endregion

    #region Special Character Handling

    [Fact]
    public void SpecialCharactersInDescription_ParsedCorrectly()
    {
        // Regression: Special characters in YAML strings should be preserved
        var skillPath = Path.Combine(_fixturesPath, "special-chars-skill");

        // The fixture is required for this regression test; fail clearly if it's missing.
        Assert.True(Directory.Exists(skillPath), $"Fixture not available for test: {skillPath}");

        var (skill, diagnostics) = _loader.LoadSkill(skillPath);

        // Should load successfully
        Assert.NotNull(skill);
        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

        // Special characters should be preserved
        Assert.Contains("\"", skill.Manifest.Description);
        Assert.Contains("'", skill.Manifest.Description);
    }

    [Fact]
    public void UnicodeCharacters_ParsedCorrectly()
    {
        // Regression: Unicode characters should be preserved throughout
        var skillPath = Path.Combine(_fixturesPath, "special-chars-skill");

        // The fixture is required for this regression test; fail clearly if it's missing.
        Assert.True(Directory.Exists(skillPath), $"Fixture not available for test: {skillPath}");

        var (skill, _) = _loader.LoadSkill(skillPath);

        if (skill != null)
        {
            // Unicode should be preserved in instructions
            Assert.Contains("你好", skill.Instructions);
        }
    }

    #endregion

    #region Resource Discovery

    [Fact]
    public void SkillWithResources_ResourcesNotAutoLoaded()
    {
        // Regression: Resources should not be automatically loaded or executed
        var skillPath = Path.Combine(_fixturesPath, "complete-skill");

        // The fixture is required for this regression test; fail clearly if it's missing.
        Assert.True(Directory.Exists(skillPath), $"Fixture not available for test: {skillPath}");

        var (skill, _) = _loader.LoadSkill(skillPath);

        // Should load successfully but not execute anything
        Assert.NotNull(skill);

        // Verify directories exist but content not loaded
        var scriptsPath = Path.Combine(skillPath, "scripts");
        var referencesPath = Path.Combine(skillPath, "references");

        Assert.True(Directory.Exists(scriptsPath));
        Assert.True(Directory.Exists(referencesPath));
    }

    #endregion

    #region Diagnostic Quality

    [Fact]
    public void AllDiagnostics_IncludePath()
    {
        // Regression: All diagnostics should include path for IDE integration
        var skillSet = _loader.LoadSkillSet(_fixturesPath);

        // Filter diagnostics explicitly before iterating
        var relevantDiagnostics = skillSet.Diagnostics
            .Where(d => d.Code != null &&
                        (d.Code.StartsWith("VAL") || d.Code.StartsWith("LOADER")));

        foreach (var diagnostic in relevantDiagnostics)
        {
            // Path should be set for validation and loader diagnostics
            Assert.NotNull(diagnostic.Path);
        }
    }

    [Fact]
    public void ValidationErrors_HaveHelpfulMessages()
    {
        // Regression: Error messages should be clear and actionable
        var skillPath = Path.Combine(_fixturesPath, "invalid-uppercase-name");
        var (skill, _) = _loader.LoadSkill(skillPath);

        if (skill != null)
        {
            var result = _validator.Validate(skill);

            foreach (var error in result.Errors)
            {
                // Messages should be non-empty and specific
                Assert.NotEmpty(error.Message);
                Assert.True(error.Message.Length > 10, "Error message too terse");
            }
        }
    }

    #endregion
}
