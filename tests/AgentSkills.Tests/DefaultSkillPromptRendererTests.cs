namespace AgentSkills.Tests;

using AgentSkills.Prompts;
using Xunit;

public class DefaultSkillPromptRendererTests
{
    [Fact]
    public void RenderSkillList_WithEmptyList_ReturnsNoSkillsMessage()
    {
        // Arrange
        var renderer = new DefaultSkillPromptRenderer();
        var metadata = Array.Empty<SkillMetadata>();

        // Act
        var result = renderer.RenderSkillList(metadata);

        // Assert
        Assert.Contains("Available Skills", result);
        Assert.Contains("No skills available", result);
    }

    [Fact]
    public void RenderSkillList_WithSingleSkill_RendersNameAndDescription()
    {
        // Arrange
        var renderer = new DefaultSkillPromptRenderer();
        var metadata = new[]
        {
            new SkillMetadata
            {
                Name = "test-skill",
                Description = "A test skill for unit testing",
                Path = "/path/to/skill"
            }
        };

        // Act
        var result = renderer.RenderSkillList(metadata);

        // Assert
        Assert.Contains("test-skill", result);
        Assert.Contains("A test skill for unit testing", result);
    }

    [Fact]
    public void RenderSkillList_WithMultipleSkills_RendersAllSkills()
    {
        // Arrange
        var renderer = new DefaultSkillPromptRenderer();
        var metadata = new[]
        {
            new SkillMetadata
            {
                Name = "skill-one",
                Description = "First skill",
                Path = "/path/one"
            },
            new SkillMetadata
            {
                Name = "skill-two",
                Description = "Second skill",
                Path = "/path/two"
            }
        };

        // Act
        var result = renderer.RenderSkillList(metadata);

        // Assert
        Assert.Contains("skill-one", result);
        Assert.Contains("First skill", result);
        Assert.Contains("skill-two", result);
        Assert.Contains("Second skill", result);
    }

    [Fact]
    public void RenderSkillList_WithVersionAndAuthor_IncludesThem()
    {
        // Arrange
        var renderer = new DefaultSkillPromptRenderer();
        var metadata = new[]
        {
            new SkillMetadata
            {
                Name = "test-skill",
                Description = "A test skill",
                Version = "1.0.0",
                Author = "Test Author",
                Path = "/path/to/skill"
            }
        };

        // Act
        var result = renderer.RenderSkillList(metadata);

        // Assert
        Assert.Contains("1.0.0", result);
        Assert.Contains("Test Author", result);
    }

    [Fact]
    public void RenderSkillList_WithTags_IncludesThem()
    {
        // Arrange
        var renderer = new DefaultSkillPromptRenderer();
        var metadata = new[]
        {
            new SkillMetadata
            {
                Name = "test-skill",
                Description = "A test skill",
                Tags = new[] { "testing", "example" },
                Path = "/path/to/skill"
            }
        };

        // Act
        var result = renderer.RenderSkillList(metadata);

        // Assert
        Assert.Contains("testing", result);
        Assert.Contains("example", result);
    }

    [Fact]
    public void RenderSkillList_WithOptionsDisablingVersion_ExcludesVersion()
    {
        // Arrange
        var renderer = new DefaultSkillPromptRenderer();
        var metadata = new[]
        {
            new SkillMetadata
            {
                Name = "test-skill",
                Description = "A test skill",
                Version = "1.0.0",
                Path = "/path/to/skill"
            }
        };
        var options = new PromptRenderOptions { IncludeVersion = false };

        // Act
        var result = renderer.RenderSkillList(metadata, options);

        // Assert
        Assert.DoesNotContain("1.0.0", result);
    }

    [Fact]
    public void RenderSkillList_WithOptionsDisablingAuthor_ExcludesAuthor()
    {
        // Arrange
        var renderer = new DefaultSkillPromptRenderer();
        var metadata = new[]
        {
            new SkillMetadata
            {
                Name = "test-skill",
                Description = "A test skill",
                Author = "Test Author",
                Path = "/path/to/skill"
            }
        };
        var options = new PromptRenderOptions { IncludeAuthor = false };

        // Act
        var result = renderer.RenderSkillList(metadata, options);

        // Assert
        Assert.DoesNotContain("Test Author", result);
    }

    [Fact]
    public void RenderSkillList_WithOptionsDisablingTags_ExcludesTags()
    {
        // Arrange
        var renderer = new DefaultSkillPromptRenderer();
        var metadata = new[]
        {
            new SkillMetadata
            {
                Name = "test-skill",
                Description = "A test skill",
                Tags = new[] { "testing" },
                Path = "/path/to/skill"
            }
        };
        var options = new PromptRenderOptions { IncludeTags = false };

        // Act
        var result = renderer.RenderSkillList(metadata, options);

        // Assert
        Assert.DoesNotContain("testing", result);
    }

    [Fact]
    public void RenderSkillDetails_RendersNameDescriptionAndInstructions()
    {
        // Arrange
        var renderer = new DefaultSkillPromptRenderer();
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "test-skill",
                Description = "A test skill"
            },
            Instructions = "Follow these instructions carefully.",
            Path = "/path/to/skill"
        };

        // Act
        var result = renderer.RenderSkillDetails(skill);

        // Assert
        Assert.Contains("test-skill", result);
        Assert.Contains("A test skill", result);
        Assert.Contains("Follow these instructions carefully", result);
    }

    [Fact]
    public void RenderSkillDetails_WithAllMetadata_RendersEverything()
    {
        // Arrange
        var renderer = new DefaultSkillPromptRenderer();
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "test-skill",
                Description = "A test skill",
                Version = "1.0.0",
                Author = "Test Author",
                Tags = new[] { "testing", "example" },
                AllowedTools = new[] { "filesystem", "calculator" }
            },
            Instructions = "Instructions here.",
            Path = "/path/to/skill"
        };

        // Act
        var result = renderer.RenderSkillDetails(skill);

        // Assert
        Assert.Contains("test-skill", result);
        Assert.Contains("A test skill", result);
        Assert.Contains("1.0.0", result);
        Assert.Contains("Test Author", result);
        Assert.Contains("testing", result);
        Assert.Contains("example", result);
        Assert.Contains("filesystem", result);
        Assert.Contains("calculator", result);
        Assert.Contains("Instructions here", result);
    }

    [Fact]
    public void RenderSkillDetails_WithOptionsDisablingAllowedTools_ExcludesAllowedTools()
    {
        // Arrange
        var renderer = new DefaultSkillPromptRenderer();
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "test-skill",
                Description = "A test skill",
                AllowedTools = new[] { "filesystem" }
            },
            Instructions = "Instructions",
            Path = "/path/to/skill"
        };
        var options = new PromptRenderOptions { IncludeAllowedTools = false };

        // Act
        var result = renderer.RenderSkillDetails(skill, options);

        // Assert
        Assert.DoesNotContain("filesystem", result);
        Assert.DoesNotContain("Allowed Tools", result);
    }

    [Fact]
    public void RenderSkillDetails_WithResourcePolicyExcludingAllowedTools_ExcludesAllowedTools()
    {
        // Arrange
        var renderer = new DefaultSkillPromptRenderer();
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "test-skill",
                Description = "A test skill",
                AllowedTools = new[] { "filesystem" }
            },
            Instructions = "Instructions",
            Path = "/path/to/skill"
        };
        var options = new PromptRenderOptions
        {
            ResourcePolicy = ExcludeAllResourcePolicy.Instance
        };

        // Act
        var result = renderer.RenderSkillDetails(skill, options);

        // Assert
        Assert.DoesNotContain("filesystem", result);
        Assert.DoesNotContain("Allowed Tools", result);
    }

    [Fact]
    public void RenderSkillDetails_WithResourcePolicyIncludingAllowedTools_IncludesAllowedTools()
    {
        // Arrange
        var renderer = new DefaultSkillPromptRenderer();
        var skill = new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "test-skill",
                Description = "A test skill",
                AllowedTools = new[] { "filesystem" }
            },
            Instructions = "Instructions",
            Path = "/path/to/skill"
        };
        var options = new PromptRenderOptions
        {
            ResourcePolicy = IncludeAllResourcePolicy.Instance
        };

        // Act
        var result = renderer.RenderSkillDetails(skill, options);

        // Assert
        Assert.Contains("filesystem", result);
        Assert.Contains("Allowed Tools", result);
    }
}
