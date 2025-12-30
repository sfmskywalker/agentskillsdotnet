namespace AgentSkills.Tests;

public class SkillSetTests
{
    [Fact]
    public void SkillSet_CanBeCreated_Empty()
    {
        // Arrange & Act
        var skillSet = new SkillSet();

        // Assert
        Assert.Empty(skillSet.Skills);
        Assert.Empty(skillSet.Diagnostics);
        Assert.True(skillSet.IsValid);
    }

    [Fact]
    public void SkillSet_CanBeCreated_WithSkills()
    {
        // Arrange
        var skill1 = new Skill
        {
            Manifest = new SkillManifest { Name = "skill-1", Description = "First skill" },
            Instructions = "Instructions 1",
            Path = "/skills/skill-1"
        };

        var skill2 = new Skill
        {
            Manifest = new SkillManifest { Name = "skill-2", Description = "Second skill" },
            Instructions = "Instructions 2",
            Path = "/skills/skill-2"
        };

        // Act
        var skillSet = new SkillSet
        {
            Skills = new[] { skill1, skill2 }
        };

        // Assert
        Assert.Equal(2, skillSet.Skills.Count);
        Assert.Contains(skill1, skillSet.Skills);
        Assert.Contains(skill2, skillSet.Skills);
    }

    [Fact]
    public void SkillSet_IsValid_WhenNoDiagnostics()
    {
        // Arrange & Act
        var skillSet = new SkillSet
        {
            Skills = Array.Empty<Skill>(),
            Diagnostics = Array.Empty<SkillDiagnostic>()
        };

        // Assert
        Assert.True(skillSet.IsValid);
    }

    [Fact]
    public void SkillSet_IsValid_WhenOnlyWarnings()
    {
        // Arrange & Act
        var skillSet = new SkillSet
        {
            Skills = Array.Empty<Skill>(),
            Diagnostics = new[]
            {
                new SkillDiagnostic { Severity = DiagnosticSeverity.Warning, Message = "Warning" }
            }
        };

        // Assert
        Assert.True(skillSet.IsValid);
    }

    [Fact]
    public void SkillSet_IsNotValid_WhenContainsErrors()
    {
        // Arrange & Act
        var skillSet = new SkillSet
        {
            Skills = Array.Empty<Skill>(),
            Diagnostics = new[]
            {
                new SkillDiagnostic { Severity = DiagnosticSeverity.Error, Message = "Error" }
            }
        };

        // Assert
        Assert.False(skillSet.IsValid);
    }

    [Fact]
    public void SkillSet_GetSkill_ReturnsSkillByName()
    {
        // Arrange
        var skill1 = new Skill
        {
            Manifest = new SkillManifest { Name = "skill-1", Description = "First skill" },
            Instructions = "Instructions 1",
            Path = "/skills/skill-1"
        };

        var skill2 = new Skill
        {
            Manifest = new SkillManifest { Name = "skill-2", Description = "Second skill" },
            Instructions = "Instructions 2",
            Path = "/skills/skill-2"
        };

        var skillSet = new SkillSet
        {
            Skills = new[] { skill1, skill2 }
        };

        // Act
        var found = skillSet.GetSkill("skill-2");

        // Assert
        Assert.NotNull(found);
        Assert.Equal("skill-2", found.Manifest.Name);
    }

    [Fact]
    public void SkillSet_GetSkill_IsCaseInsensitive()
    {
        // Arrange
        var skill = new Skill
        {
            Manifest = new SkillManifest { Name = "Test-Skill", Description = "Test" },
            Instructions = "Instructions",
            Path = "/skills/test-skill"
        };

        var skillSet = new SkillSet
        {
            Skills = new[] { skill }
        };

        // Act
        var found = skillSet.GetSkill("test-skill");

        // Assert
        Assert.NotNull(found);
        Assert.Equal("Test-Skill", found.Manifest.Name);
    }

    [Fact]
    public void SkillSet_GetSkill_ReturnsNull_WhenNotFound()
    {
        // Arrange
        var skill = new Skill
        {
            Manifest = new SkillManifest { Name = "skill-1", Description = "First skill" },
            Instructions = "Instructions",
            Path = "/skills/skill-1"
        };

        var skillSet = new SkillSet
        {
            Skills = new[] { skill }
        };

        // Act
        var found = skillSet.GetSkill("non-existent");

        // Assert
        Assert.Null(found);
    }

    [Fact]
    public void SkillSet_GetSkillsByTag_ReturnsMatchingSkills()
    {
        // Arrange
        var skill1 = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "skill-1",
                Description = "First skill",
                Tags = new[] { "test", "demo" }
            },
            Instructions = "Instructions 1",
            Path = "/skills/skill-1"
        };

        var skill2 = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "skill-2",
                Description = "Second skill",
                Tags = new[] { "test", "production" }
            },
            Instructions = "Instructions 2",
            Path = "/skills/skill-2"
        };

        var skill3 = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "skill-3",
                Description = "Third skill",
                Tags = new[] { "production" }
            },
            Instructions = "Instructions 3",
            Path = "/skills/skill-3"
        };

        var skillSet = new SkillSet
        {
            Skills = new[] { skill1, skill2, skill3 }
        };

        // Act
        var testSkills = skillSet.GetSkillsByTag("test").ToList();

        // Assert
        Assert.Equal(2, testSkills.Count);
        Assert.Contains(skill1, testSkills);
        Assert.Contains(skill2, testSkills);
    }

    [Fact]
    public void SkillSet_GetSkillsByTag_IsCaseInsensitive()
    {
        // Arrange
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "skill-1",
                Description = "First skill",
                Tags = new[] { "Test" }
            },
            Instructions = "Instructions",
            Path = "/skills/skill-1"
        };

        var skillSet = new SkillSet
        {
            Skills = new[] { skill }
        };

        // Act
        var found = skillSet.GetSkillsByTag("test").ToList();

        // Assert
        Assert.Single(found);
        Assert.Equal(skill, found[0]);
    }

    [Fact]
    public void SkillSet_GetSkillsByTag_ReturnsEmpty_WhenNoMatches()
    {
        // Arrange
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "skill-1",
                Description = "First skill",
                Tags = new[] { "test" }
            },
            Instructions = "Instructions",
            Path = "/skills/skill-1"
        };

        var skillSet = new SkillSet
        {
            Skills = new[] { skill }
        };

        // Act
        var found = skillSet.GetSkillsByTag("non-existent").ToList();

        // Assert
        Assert.Empty(found);
    }
}
