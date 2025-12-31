namespace AgentSkills.Adapters.Microsoft.AgentFramework.Tests;

using AgentSkills;
using AgentSkills.Prompts;

public class SkillExtensionsTests
{
    [Fact]
    public void GetInstructions_ReturnsFormattedSkillDetails()
    {
        // Arrange
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "test-skill",
                Description = "A test skill"
            },
            Instructions = "Detailed instructions",
            Path = "/path/to/skill"
        };

        // Act
        var result = skill.GetInstructions();

        // Assert
        Assert.Contains("test-skill", result);
        Assert.Contains("A test skill", result);
        Assert.Contains("Detailed instructions", result);
    }

    [Fact]
    public void GetInstructions_WithOptions_AppliesOptions()
    {
        // Arrange
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "test-skill",
                Description = "A test skill",
                Version = "1.0.0",
                Author = "Test Author"
            },
            Instructions = "Detailed instructions",
            Path = "/path/to/skill"
        };
        var options = new PromptRenderOptions
        {
            IncludeVersion = false,
            IncludeAuthor = false
        };

        // Act
        var result = skill.GetInstructions(options: options);

        // Assert
        Assert.Contains("test-skill", result);
        Assert.DoesNotContain("1.0.0", result);
        Assert.DoesNotContain("Test Author", result);
    }

    [Fact]
    public void GetInstructions_WithCustomRenderer_UsesCustomRenderer()
    {
        // Arrange
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "test-skill",
                Description = "A test skill"
            },
            Instructions = "Detailed instructions",
            Path = "/path/to/skill"
        };
        var customRenderer = new TestSkillPromptRenderer();

        // Act
        var result = skill.GetInstructions(renderer: customRenderer);

        // Assert
        Assert.Equal("TEST RENDERED SKILL DETAILS", result);
    }

    [Fact]
    public void GetFunctionName_ConvertsDashesToUnderscores()
    {
        // Arrange
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "my-test-skill",
                Description = "A test skill"
            },
            Instructions = "Instructions",
            Path = "/path/to/skill"
        };

        // Act
        var result = skill.GetFunctionName();

        // Assert
        Assert.Equal("my_test_skill", result);
    }

    [Fact]
    public void GetFunctionName_HandlesNoHyphens()
    {
        // Arrange
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "testskill",
                Description = "A test skill"
            },
            Instructions = "Instructions",
            Path = "/path/to/skill"
        };

        // Act
        var result = skill.GetFunctionName();

        // Assert
        Assert.Equal("testskill", result);
    }

    [Fact]
    public void GetFunctionDescription_PrefixesWithActivateSkill()
    {
        // Arrange
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "test-skill",
                Description = "A helpful test skill"
            },
            Instructions = "Instructions",
            Path = "/path/to/skill"
        };

        // Act
        var result = skill.GetFunctionDescription();

        // Assert
        Assert.Equal("Activate skill: A helpful test skill", result);
    }

    [Fact]
    public void GetFunctionDescription_UsesSkillDescription()
    {
        // Arrange
        var description = "This is the skill description";
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "test-skill",
                Description = description
            },
            Instructions = "Instructions",
            Path = "/path/to/skill"
        };

        // Act
        var result = skill.GetFunctionDescription();

        // Assert
        Assert.Contains(description, result);
    }

    // Test helper: Custom renderer for testing
    private class TestSkillPromptRenderer : ISkillPromptRenderer
    {
        public string RenderSkillList(IEnumerable<SkillMetadata> metadata, PromptRenderOptions? options = null)
        {
            return "TEST RENDERED SKILL LIST";
        }

        public string RenderSkillDetails(Skill skill, PromptRenderOptions? options = null)
        {
            return "TEST RENDERED SKILL DETAILS";
        }
    }
}
