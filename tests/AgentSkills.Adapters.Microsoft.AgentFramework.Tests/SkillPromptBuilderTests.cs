namespace AgentSkills.Adapters.Microsoft.AgentFramework.Tests;

using AgentSkills;
using AgentSkills.Prompts;

public class SkillPromptBuilderTests
{
    [Fact]
    public void Build_WithBaseInstructions_ReturnsBaseInstructions()
    {
        // Arrange
        var builder = new SkillPromptBuilder();
        var instructions = "You are a helpful assistant.";

        // Act
        var result = builder
            .WithBaseInstructions(instructions)
            .Build();

        // Assert
        Assert.Contains(instructions, result);
    }

    [Fact]
    public void Build_WithSkills_IncludesSkillList()
    {
        // Arrange
        var builder = new SkillPromptBuilder();
        var metadata = new List<SkillMetadata>
        {
            new()
            {
                Name = "test-skill",
                Description = "A test skill",
                Path = "/path/to/skill"
            }
        };

        // Act
        var result = builder
            .WithSkills(metadata)
            .Build();

        // Assert
        Assert.Contains("test-skill", result);
        Assert.Contains("A test skill", result);
    }

    [Fact]
    public void Build_WithBaseInstructionsAndSkills_CombinesBoth()
    {
        // Arrange
        var builder = new SkillPromptBuilder();
        var baseInstructions = "You are a helpful assistant.";
        var metadata = new List<SkillMetadata>
        {
            new()
            {
                Name = "test-skill",
                Description = "A test skill",
                Path = "/path/to/skill"
            }
        };

        // Act
        var result = builder
            .WithBaseInstructions(baseInstructions)
            .WithSkills(metadata)
            .Build();

        // Assert
        Assert.Contains(baseInstructions, result);
        Assert.Contains("test-skill", result);
        Assert.Contains("A test skill", result);
    }

    [Fact]
    public void Build_WithSkillSet_IncludesAllSkills()
    {
        // Arrange
        var builder = new SkillPromptBuilder();
        var skillSet = new SkillSet
        {
            Skills = new List<Skill>
            {
                new()
                {
                    Manifest = new SkillManifest
                    {
                        Name = "skill-one",
                        Description = "First skill"
                    },
                    Instructions = "Instructions for skill one",
                    Path = "/path/to/skill-one"
                },
                new()
                {
                    Manifest = new SkillManifest
                    {
                        Name = "skill-two",
                        Description = "Second skill"
                    },
                    Instructions = "Instructions for skill two",
                    Path = "/path/to/skill-two"
                }
            }
        };

        // Act
        var result = builder
            .WithSkillSet(skillSet)
            .Build();

        // Assert
        Assert.Contains("skill-one", result);
        Assert.Contains("First skill", result);
        Assert.Contains("skill-two", result);
        Assert.Contains("Second skill", result);
    }

    [Fact]
    public void BuildSkillDetails_ReturnsFullSkillInstructions()
    {
        // Arrange
        var builder = new SkillPromptBuilder();
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "test-skill",
                Description = "A test skill"
            },
            Instructions = "Detailed instructions for the skill",
            Path = "/path/to/skill"
        };

        // Act
        var result = builder.BuildSkillDetails(skill);

        // Assert
        Assert.Contains("test-skill", result);
        Assert.Contains("A test skill", result);
        Assert.Contains("Detailed instructions for the skill", result);
    }

    [Fact]
    public void Build_WithEmptySkills_ReturnsOnlyBaseInstructions()
    {
        // Arrange
        var builder = new SkillPromptBuilder();
        var instructions = "You are a helpful assistant.";

        // Act
        var result = builder
            .WithBaseInstructions(instructions)
            .WithSkills(Enumerable.Empty<SkillMetadata>())
            .Build();

        // Assert
        Assert.Equal(instructions, result);
    }

    [Fact]
    public void Build_WithOptions_PassesOptionsToRenderer()
    {
        // Arrange
        var builder = new SkillPromptBuilder();
        var metadata = new List<SkillMetadata>
        {
            new()
            {
                Name = "test-skill",
                Description = "A test skill",
                Version = "1.0.0",
                Path = "/path/to/skill"
            }
        };
        var options = new PromptRenderOptions
        {
            IncludeVersion = false
        };

        // Act
        var result = builder
            .WithSkills(metadata)
            .Build(options);

        // Assert
        Assert.Contains("test-skill", result);
        // Version should not be included when IncludeVersion is false
        Assert.DoesNotContain("1.0.0", result);
    }

    [Fact]
    public void Build_WithNoConfiguration_ReturnsEmptyString()
    {
        // Arrange
        var builder = new SkillPromptBuilder();

        // Act
        var result = builder.Build();

        // Assert
        Assert.Equal(string.Empty, result);
    }
}
