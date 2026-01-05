using System.Text;
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

    [Fact]
    public void Validate_ChineseCharacters_ReturnsNoErrors()
    {
        // Arrange
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "技能",
                Description = "A skill with Chinese characters in the name"
            },
            Instructions = "# Instructions",
            Path = Path.Combine(_fixturesPath, "技能")
        };

        // Act
        var result = _validator.Validate(skill);

        // Assert
        Assert.True(result.IsValid, $"Chinese skill name should be valid. Errors: {string.Join(", ", result.Errors.Select(e => e.Message))}");
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_RussianCharacters_ReturnsNoErrors()
    {
        // Arrange
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "навык",
                Description = "A skill with Russian characters in the name"
            },
            Instructions = "# Instructions",
            Path = Path.Combine(_fixturesPath, "навык")
        };

        // Act
        var result = _validator.Validate(skill);

        // Assert
        Assert.True(result.IsValid, $"Russian skill name should be valid. Errors: {string.Join(", ", result.Errors.Select(e => e.Message))}");
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_ArabicCharacters_ReturnsNoErrors()
    {
        // Arrange
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "مهارة",
                Description = "A skill with Arabic characters in the name"
            },
            Instructions = "# Instructions",
            Path = Path.Combine(_fixturesPath, "مهارة")
        };

        // Act
        var result = _validator.Validate(skill);

        // Assert
        Assert.True(result.IsValid, $"Arabic skill name should be valid. Errors: {string.Join(", ", result.Errors.Select(e => e.Message))}");
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_ChineseWithHyphens_ReturnsNoErrors()
    {
        // Arrange
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "技能-测试",
                Description = "A skill with Chinese characters and hyphens"
            },
            Instructions = "# Instructions",
            Path = Path.Combine(_fixturesPath, "技能-测试")
        };

        // Act
        var result = _validator.Validate(skill);

        // Assert
        Assert.True(result.IsValid, $"Chinese skill name with hyphens should be valid. Errors: {string.Join(", ", result.Errors.Select(e => e.Message))}");
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_UppercaseRussianCharacters_ReturnsError()
    {
        // Arrange
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "НАВЫК",
                Description = "A skill with uppercase Russian characters"
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
    public void Validate_MixedScriptName_ReturnsNoErrors()
    {
        // Arrange
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "skill-技能-навык",
                Description = "A skill with mixed scripts (English, Chinese, Russian)"
            },
            Instructions = "# Instructions",
            Path = "/path/to/skill-技能-навык"
        };

        // Act
        var result = _validator.Validate(skill);

        // Assert
        Assert.True(result.IsValid, $"Mixed script skill name should be valid. Errors: {string.Join(", ", result.Errors.Select(e => e.Message))}");
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_UnicodeWithConsecutiveHyphens_ReturnsError()
    {
        // Arrange
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "技能--测试",
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
    public void Validate_UnicodeStartingWithHyphen_ReturnsError()
    {
        // Arrange
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "-技能",
                Description = "A skill starting with hyphen"
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
    public void Validate_UnicodeEndingWithHyphen_ReturnsError()
    {
        // Arrange
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "技能-",
                Description = "A skill ending with hyphen"
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
    public void ValidateMetadata_UnicodeSkill_ReturnsNoErrors()
    {
        // Arrange
        var metadata = new SkillMetadata
        {
            Name = "技能",
            Description = "A skill with Unicode characters",
            Path = "/path/to/技能"
        };

        // Act
        var result = _validator.ValidateMetadata(metadata);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_ComposedUnicodeCharacters_ReturnsNoErrors()
    {
        // Arrange - using café with composed é (U+00E9)
        var composedName = "caf\u00e9"; // café with single codepoint for é
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = composedName,
                Description = "A skill with composed Unicode characters in the name"
            },
            Instructions = "# Instructions",
            Path = Path.Combine(_fixturesPath, composedName)
        };

        // Act
        var result = _validator.Validate(skill);

        // Assert
        Assert.True(result.IsValid, $"Composed Unicode skill name should be valid. Errors: {string.Join(", ", result.Errors.Select(e => e.Message))}");
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_DecomposedUnicodeCharacters_ReturnsNoErrors()
    {
        // Arrange - using café with decomposed é (e + combining acute accent)
        var decomposedName = "cafe\u0301"; // café with e + combining accent U+0301
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = decomposedName,
                Description = "A skill with decomposed Unicode characters in the name"
            },
            Instructions = "# Instructions",
            Path = Path.Combine(_fixturesPath, decomposedName)
        };

        // Act
        var result = _validator.Validate(skill);

        // Assert
        Assert.True(result.IsValid, $"Decomposed Unicode skill name should be valid. Errors: {string.Join(", ", result.Errors.Select(e => e.Message))}");
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_ComposedAndDecomposedUnicodeAreEquivalent()
    {
        // Arrange
        var composedName = "caf\u00e9"; // café with single codepoint for é (U+00E9)
        var decomposedName = "cafe\u0301"; // café with e + combining accent (U+0301)

        // Both forms should normalize to the same string
        Assert.NotEqual(composedName, decomposedName); // Different before normalization
        Assert.Equal(
            composedName.Normalize(NormalizationForm.FormKC),
            decomposedName.Normalize(NormalizationForm.FormKC)); // Same after normalization

        var composedSkill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = composedName,
                Description = "A skill with composed Unicode characters"
            },
            Instructions = "# Instructions",
            Path = Path.Combine(_fixturesPath, composedName)
        };

        var decomposedSkill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = decomposedName,
                Description = "A skill with decomposed Unicode characters"
            },
            Instructions = "# Instructions",
            Path = Path.Combine(_fixturesPath, decomposedName)
        };

        // Act
        var composedResult = _validator.Validate(composedSkill);
        var decomposedResult = _validator.Validate(decomposedSkill);

        // Assert - both should be valid
        Assert.True(composedResult.IsValid, "Composed form should be valid");
        Assert.True(decomposedResult.IsValid, "Decomposed form should be valid");
    }

    [Fact]
    public void Validate_DirectoryNameMatchesAfterNormalization()
    {
        // Arrange - skill name is composed, directory name is decomposed
        var composedName = "caf\u00e9"; // café with single codepoint for é
        var decomposedDirName = "cafe\u0301"; // café with e + combining accent

        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = composedName,
                Description = "A skill testing normalization in directory matching"
            },
            Instructions = "# Instructions",
            Path = Path.Combine("/path/to", decomposedDirName)
        };

        // Act
        var result = _validator.Validate(skill);

        // Assert - should be valid because both normalize to the same form
        Assert.True(result.IsValid, $"Skill should be valid when directory name matches after normalization. Errors: {string.Join(", ", result.Errors.Select(e => e.Message))}");
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_DirectoryNameMismatchAfterNormalization()
    {
        // Arrange - even after normalization, these are different
        var skillName = "caf\u00e9"; // café
        var directoryName = "cafe"; // cafe (no accent at all)

        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = skillName,
                Description = "A skill with mismatched directory name even after normalization"
            },
            Instructions = "# Instructions",
            Path = Path.Combine("/path/to", directoryName)
        };

        // Act
        var result = _validator.Validate(skill);

        // Assert - should have directory mismatch error
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, d => d.Code == "VAL010");
    }

    [Fact]
    public void Validate_NFKCNormalizesCompatibilityCharacters()
    {
        // Arrange - using compatibility character that NFKC will normalize
        // U+FB01 (ﬁ ligature) should normalize to "fi" in NFKC
        var nameWithLigature = "\ufb01le"; // ﬁle (with fi ligature)
        var normalizedExpected = "file"; // Expected after NFKC normalization

        Assert.Equal(normalizedExpected, nameWithLigature.Normalize(NormalizationForm.FormKC));

        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = nameWithLigature,
                Description = "A skill testing NFKC compatibility character normalization"
            },
            Instructions = "# Instructions",
            Path = Path.Combine(_fixturesPath, nameWithLigature)
        };

        // Act
        var result = _validator.Validate(skill);

        // Assert - should be valid since "file" is a valid name
        Assert.True(result.IsValid, $"Skill with NFKC-normalizable characters should be valid. Errors: {string.Join(", ", result.Errors.Select(e => e.Message))}");
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidateMetadata_ComposedAndDecomposedUnicode_BothValid()
    {
        // Arrange
        var composedName = "caf\u00e9";
        var decomposedName = "cafe\u0301";

        var composedMetadata = new SkillMetadata
        {
            Name = composedName,
            Description = "A skill with composed Unicode characters",
            Path = Path.Combine("/path/to", composedName)
        };

        var decomposedMetadata = new SkillMetadata
        {
            Name = decomposedName,
            Description = "A skill with decomposed Unicode characters",
            Path = Path.Combine("/path/to", decomposedName)
        };

        // Act
        var composedResult = _validator.ValidateMetadata(composedMetadata);
        var decomposedResult = _validator.ValidateMetadata(decomposedMetadata);

        // Assert
        Assert.True(composedResult.IsValid, "Composed form metadata should be valid");
        Assert.True(decomposedResult.IsValid, "Decomposed form metadata should be valid");
        Assert.Empty(composedResult.Errors);
        Assert.Empty(decomposedResult.Errors);
    }

    [Fact]
    public void Validate_UnexpectedFields_ReturnsError()
    {
        // Arrange
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "test-skill",
                Description = "A skill with unexpected fields",
                AdditionalFields = new Dictionary<string, object?>
                {
                    { "unexpected-field", "value" },
                    { "another-bad-field", "another value" }
                }
            },
            Instructions = "# Instructions",
            Path = "/path/to/test-skill"
        };

        // Act
        var result = _validator.Validate(skill);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, d => d.Code == "VAL011");
        var error = result.Errors.First(d => d.Code == "VAL011");
        Assert.Contains("unexpected-field", error.Message);
        Assert.Contains("another-bad-field", error.Message);
    }

    [Fact]
    public void Validate_OnlyAllowedFields_ReturnsNoErrors()
    {
        // Arrange
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "test-skill",
                Description = "A skill with only allowed fields",
                Version = "1.0.0",
                Author = "Test Author",
                Tags = ["tag1", "tag2"],
                AllowedTools = ["tool1", "tool2"],
                AdditionalFields = new Dictionary<string, object?>
                {
                    { "compatibility", "Some compatibility info" }
                }
            },
            Instructions = "# Instructions",
            Path = "/path/to/test-skill"
        };

        // Act
        var result = _validator.Validate(skill);

        // Assert
        Assert.True(result.IsValid, $"Skill with only allowed fields should be valid. Errors: {string.Join(", ", result.Errors.Select(e => e.Message))}");
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_NoAdditionalFields_ReturnsNoErrors()
    {
        // Arrange
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "test-skill",
                Description = "A skill with no additional fields",
                AdditionalFields = new Dictionary<string, object?>()
            },
            Instructions = "# Instructions",
            Path = "/path/to/test-skill"
        };

        // Act
        var result = _validator.Validate(skill);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_SingleUnexpectedField_ErrorMessageListsField()
    {
        // Arrange
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "test-skill",
                Description = "A skill with one unexpected field",
                AdditionalFields = new Dictionary<string, object?>
                {
                    { "bad-field", "value" }
                }
            },
            Instructions = "# Instructions",
            Path = "/path/to/test-skill"
        };

        // Act
        var result = _validator.Validate(skill);

        // Assert
        Assert.False(result.IsValid);
        var error = result.Errors.First(d => d.Code == "VAL011");
        Assert.Contains("'bad-field'", error.Message);
        Assert.Contains("Allowed fields are:", error.Message);
    }
}
