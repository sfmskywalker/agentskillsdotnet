namespace AgentSkills.Tests;

using AgentSkills.Prompts;
using Xunit;

public class ResourcePolicyTests
{
    [Fact]
    public void IncludeAllResourcePolicy_ShouldIncludeResource_ReturnsTrue()
    {
        // Arrange
        var policy = IncludeAllResourcePolicy.Instance;
        var resource = new SkillResource
        {
            Name = "test.txt",
            RelativePath = "references/test.txt",
            ResourceType = "reference"
        };
        var skill = CreateTestSkill();

        // Act
        var result = policy.ShouldIncludeResource(resource, skill);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IncludeAllResourcePolicy_ShouldIncludeAllowedTools_ReturnsTrue()
    {
        // Arrange
        var policy = IncludeAllResourcePolicy.Instance;
        var skill = CreateTestSkill();

        // Act
        var result = policy.ShouldIncludeAllowedTools(skill);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ExcludeAllResourcePolicy_ShouldIncludeResource_ReturnsFalse()
    {
        // Arrange
        var policy = ExcludeAllResourcePolicy.Instance;
        var resource = new SkillResource
        {
            Name = "test.txt",
            RelativePath = "references/test.txt",
            ResourceType = "reference"
        };
        var skill = CreateTestSkill();

        // Act
        var result = policy.ShouldIncludeResource(resource, skill);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ExcludeAllResourcePolicy_ShouldIncludeAllowedTools_ReturnsFalse()
    {
        // Arrange
        var policy = ExcludeAllResourcePolicy.Instance;
        var skill = CreateTestSkill();

        // Act
        var result = policy.ShouldIncludeAllowedTools(skill);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ResourceTypeFilterPolicy_WithAllowedType_ReturnsTrue()
    {
        // Arrange
        var policy = new ResourceTypeFilterPolicy(new[] { "reference", "asset" });
        var resource = new SkillResource
        {
            Name = "test.txt",
            RelativePath = "references/test.txt",
            ResourceType = "reference"
        };
        var skill = CreateTestSkill();

        // Act
        var result = policy.ShouldIncludeResource(resource, skill);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ResourceTypeFilterPolicy_WithDisallowedType_ReturnsFalse()
    {
        // Arrange
        var policy = new ResourceTypeFilterPolicy(new[] { "reference" });
        var resource = new SkillResource
        {
            Name = "script.sh",
            RelativePath = "scripts/script.sh",
            ResourceType = "script"
        };
        var skill = CreateTestSkill();

        // Act
        var result = policy.ShouldIncludeResource(resource, skill);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ResourceTypeFilterPolicy_WithNullResourceType_ReturnsFalse()
    {
        // Arrange
        var policy = new ResourceTypeFilterPolicy(new[] { "reference" });
        var resource = new SkillResource
        {
            Name = "unknown.txt",
            RelativePath = "unknown.txt",
            ResourceType = null
        };
        var skill = CreateTestSkill();

        // Act
        var result = policy.ShouldIncludeResource(resource, skill);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ResourceTypeFilterPolicy_IsCaseInsensitive()
    {
        // Arrange
        var policy = new ResourceTypeFilterPolicy(new[] { "REFERENCE" });
        var resource = new SkillResource
        {
            Name = "test.txt",
            RelativePath = "references/test.txt",
            ResourceType = "reference"
        };
        var skill = CreateTestSkill();

        // Act
        var result = policy.ShouldIncludeResource(resource, skill);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ResourceTypeFilterPolicy_ShouldIncludeAllowedTools_ReturnsTrue()
    {
        // Arrange
        var policy = new ResourceTypeFilterPolicy(new[] { "reference" });
        var skill = CreateTestSkill();

        // Act
        var result = policy.ShouldIncludeAllowedTools(skill);

        // Assert
        Assert.True(result);
    }

    private static Skill CreateTestSkill()
    {
        return new Skill
        {
            Manifest = new SkillManifest
            {
                Name = "test-skill",
                Description = "A test skill"
            },
            Instructions = "Test instructions",
            Path = "/path/to/skill"
        };
    }
}
