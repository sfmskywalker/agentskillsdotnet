using AgentSkills.Validation;

namespace AgentSkills.Tests;

public class SkillValidatorTests
{
    private readonly SkillValidator _validator = new();
    private readonly string _fixturesPath;

    // Test constants matching the validator's limits
    private const int NameMaxLength = 64;
    private const int DescriptionMaxLength = 1024;
    private const int CompatibilityMaxLength = 500;

    public SkillValidatorTests()
    {
        // Find the fixtures directory by searching upward from the test assembly location
        var testDirectory = Directory.GetCurrentDirectory();
        var current = new DirectoryInfo(testDirectory);
        
        while (current != null && !Directory.Exists(Path.Combine(current.FullName, "fixtures", "skills")))
        {
            current = current.Parent;
        }

        if (current == null)
        {
            throw new InvalidOperationException("Could not find fixtures/skills directory");
        }

        _fixturesPath = Path.Combine(current.FullName, "fixtures", "skills");
    }

    [Fact]
    public void Validate_ValidSkill_ReturnsNoErrors()
    {
        // Arrange
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "example-skill",
                Description = "An example skill for testing and demonstration"
            },
            Instructions = "# Instructions",
            Path = Path.Combine(_fixturesPath, "example-skill")
        };

        // Act
        var result = _validator.Validate(skill);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_MinimalValidSkill_ReturnsNoErrors()
    {
        // Arrange
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "minimal-skill",
                Description = "A minimal skill with only required fields"
            },
            Instructions = "# Instructions",
            Path = Path.Combine(_fixturesPath, "minimal-skill")
        };

        // Act
        var result = _validator.Validate(skill);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_MissingName_ReturnsError()
    {
        // Arrange
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "",
                Description = "A skill with missing name"
            },
            Instructions = "# Instructions",
            Path = "/path/to/skill"
        };

        // Act
        var result = _validator.Validate(skill);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, d => d.Code == "VAL001");
    }

    [Fact]
    public void Validate_MissingDescription_ReturnsError()
    {
        // Arrange
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "test-skill",
                Description = ""
            },
            Instructions = "# Instructions",
            Path = "/path/to/test-skill"
        };

        // Act
        var result = _validator.Validate(skill);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, d => d.Code == "VAL004");
    }

    [Fact]
    public void Validate_NameWithUppercase_ReturnsError()
    {
        // Arrange
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "Invalid-Name",
                Description = "A skill with uppercase in name"
            },
            Instructions = "# Instructions",
            Path = "/path/to/skill"
        };

        // Act
        var result = _validator.Validate(skill);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, d => d.Code == "VAL003");
        Assert.Contains(result.Errors, d => d.Message.Contains("uppercase"));
    }

    [Fact]
    public void Validate_NameTooLong_ReturnsError()
    {
        // Arrange
        var longName = new string('a', NameMaxLength + 1);
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = longName,
                Description = "A skill with name too long"
            },
            Instructions = "# Instructions",
            Path = "/path/to/skill"
        };

        // Act
        var result = _validator.Validate(skill);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, d => d.Code == "VAL002");
    }

    [Fact]
    public void Validate_NameWithConsecutiveHyphens_ReturnsError()
    {
        // Arrange
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "invalid--name",
                Description = "A skill with consecutive hyphens"
            },
            Instructions = "# Instructions",
            Path = "/path/to/skill"
        };

        // Act
        var result = _validator.Validate(skill);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, d => d.Code == "VAL003");
        Assert.Contains(result.Errors, d => d.Message.Contains("consecutive hyphens"));
    }

    [Fact]
    public void Validate_NameStartingWithHyphen_ReturnsError()
    {
        // Arrange
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "-invalid-name",
                Description = "A skill with leading hyphen"
            },
            Instructions = "# Instructions",
            Path = "/path/to/skill"
        };

        // Act
        var result = _validator.Validate(skill);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, d => d.Code == "VAL003");
    }

    [Fact]
    public void Validate_NameEndingWithHyphen_ReturnsError()
    {
        // Arrange
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "invalid-name-",
                Description = "A skill with trailing hyphen"
            },
            Instructions = "# Instructions",
            Path = "/path/to/skill"
        };

        // Act
        var result = _validator.Validate(skill);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, d => d.Code == "VAL003");
    }

    [Fact]
    public void Validate_DescriptionTooLong_ReturnsError()
    {
        // Arrange
        var longDescription = new string('a', DescriptionMaxLength + 1);
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "test-skill",
                Description = longDescription
            },
            Instructions = "# Instructions",
            Path = "/path/to/test-skill"
        };

        // Act
        var result = _validator.Validate(skill);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, d => d.Code == "VAL005");
    }

    [Fact]
    public void Validate_ShortDescription_ReturnsWarning()
    {
        // Arrange
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "test-skill",
                Description = "Short"
            },
            Instructions = "# Instructions",
            Path = "/path/to/test-skill"
        };

        // Act
        var result = _validator.Validate(skill);

        // Assert
        Assert.True(result.IsValid); // Still valid, just a warning
        Assert.True(result.HasWarnings);
        Assert.Contains(result.Warnings, d => d.Code == "VAL006");
    }

    [Fact]
    public void Validate_DirectoryNameMismatch_ReturnsError()
    {
        // Arrange
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "skill-name",
                Description = "A skill with mismatched directory name"
            },
            Instructions = "# Instructions",
            Path = "/path/to/different-name"
        };

        // Act
        var result = _validator.Validate(skill);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, d => d.Code == "VAL010");
        Assert.Contains(result.Errors, d => d.Message.Contains("does not match"));
    }

    [Fact]
    public void Validate_ValidNamePatterns_ReturnsNoErrors()
    {
        // Arrange & Act & Assert
        var validNames = new[]
        {
            "a",
            "abc",
            "skill-name",
            "skill-name-123",
            "123-skill",
            "a-b-c-d-e-f",
            "skill1",
            "skill-1-2-3"
        };

        foreach (var name in validNames)
        {
            var skill = new Skill
            {
                Manifest = new SkillManifest
                {
                    Name = name,
                    Description = "Test skill with valid name pattern"
                },
                Instructions = "# Instructions",
                Path = $"/path/to/{name}"
            };

            var result = _validator.Validate(skill);
            
            Assert.True(result.IsValid, $"Name '{name}' should be valid but validation failed");
        }
    }

    [Fact]
    public void Validate_InvalidNamePatterns_ReturnsErrors()
    {
        // Arrange & Act & Assert
        var invalidNames = new[]
        {
            "Skill-Name",      // uppercase
            "skill_name",      // underscore
            "skill name",      // space
            "skill--name",     // consecutive hyphens
            "-skill",          // starts with hyphen
            "skill-",          // ends with hyphen
            "skill.name",      // dot
            "skill@name"       // special char
        };

        foreach (var name in invalidNames)
        {
            var skill = new Skill
            {
                Manifest = new SkillManifest
                {
                    Name = name,
                    Description = "Test skill with invalid name pattern"
                },
                Instructions = "# Instructions",
                Path = $"/path/to/{name}"
            };

            var result = _validator.Validate(skill);
            
            Assert.False(result.IsValid, $"Name '{name}' should be invalid but validation passed");
            Assert.True(result.Errors.Any(d => d.Code == "VAL003"), 
                $"Name '{name}' should produce VAL003 error");
        }
    }

    [Fact]
    public void Validate_CompatibilityTooLong_ReturnsError()
    {
        // Arrange
        var longCompatibility = new string('a', CompatibilityMaxLength + 1);
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "test-skill",
                Description = "Test skill with long compatibility",
                AdditionalFields = new Dictionary<string, object?>
                {
                    { "compatibility", longCompatibility }
                }
            },
            Instructions = "# Instructions",
            Path = "/path/to/test-skill"
        };

        // Act
        var result = _validator.Validate(skill);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, d => d.Code == "VAL008");
    }

    [Fact]
    public void Validate_EmptyVersion_ReturnsWarning()
    {
        // Arrange
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "test-skill",
                Description = "Test skill with empty version",
                Version = "   "
            },
            Instructions = "# Instructions",
            Path = "/path/to/test-skill"
        };

        // Act
        var result = _validator.Validate(skill);

        // Assert
        Assert.True(result.IsValid); // Still valid, just a warning
        Assert.True(result.HasWarnings);
        Assert.Contains(result.Warnings, d => d.Code == "VAL007");
    }

    [Fact]
    public void ValidateMetadata_ValidMetadata_ReturnsNoErrors()
    {
        // Arrange
        var metadata = new SkillMetadata
        {
            Name = "test-skill",
            Description = "A test skill with valid metadata",
            Path = "/path/to/test-skill"
        };

        // Act
        var result = _validator.ValidateMetadata(metadata);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidateMetadata_InvalidMetadata_ReturnsErrors()
    {
        // Arrange
        var metadata = new SkillMetadata
        {
            Name = "Invalid_Name",
            Description = "A",
            Path = "/path/to/different-name"
        };

        // Act
        var result = _validator.ValidateMetadata(metadata);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void Validate_AllDiagnosticsHavePaths()
    {
        // Arrange
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "Invalid-Name",
                Description = "A"
            },
            Instructions = "# Instructions",
            Path = "/path/to/skill"
        };

        // Act
        var result = _validator.Validate(skill);

        // Assert
        Assert.All(result.Diagnostics, d => Assert.NotNull(d.Path));
    }

    [Fact]
    public void Validate_AllDiagnosticsHaveCodes()
    {
        // Arrange
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "Invalid-Name",
                Description = "A"
            },
            Instructions = "# Instructions",
            Path = "/path/to/skill"
        };

        // Act
        var result = _validator.Validate(skill);

        // Assert
        Assert.All(result.Diagnostics, d => Assert.NotNull(d.Code));
    }
}
