namespace AgentSkills.Tests;

public class SkillResourceTests
{
    [Fact]
    public void SkillResource_CanBeCreated_WithRequiredProperties()
    {
        // Arrange & Act
        var resource = new SkillResource
        {
            Name = "script.py",
            RelativePath = "scripts/script.py"
        };

        // Assert
        Assert.Equal("script.py", resource.Name);
        Assert.Equal("scripts/script.py", resource.RelativePath);
        Assert.Null(resource.ResourceType);
        Assert.Null(resource.AbsolutePath);
    }

    [Fact]
    public void SkillResource_CanBeCreated_WithAllProperties()
    {
        // Arrange & Act
        var resource = new SkillResource
        {
            Name = "script.py",
            RelativePath = "scripts/script.py",
            ResourceType = "script",
            AbsolutePath = "/skills/example/scripts/script.py"
        };

        // Assert
        Assert.Equal("script.py", resource.Name);
        Assert.Equal("scripts/script.py", resource.RelativePath);
        Assert.Equal("script", resource.ResourceType);
        Assert.Equal("/skills/example/scripts/script.py", resource.AbsolutePath);
    }

    [Fact]
    public void SkillResource_SupportsVariousResourceTypes()
    {
        // Arrange & Act
        var script = new SkillResource
        {
            Name = "script.py",
            RelativePath = "scripts/script.py",
            ResourceType = "script"
        };

        var reference = new SkillResource
        {
            Name = "docs.md",
            RelativePath = "references/docs.md",
            ResourceType = "reference"
        };

        var asset = new SkillResource
        {
            Name = "image.png",
            RelativePath = "assets/image.png",
            ResourceType = "asset"
        };

        // Assert
        Assert.Equal("script", script.ResourceType);
        Assert.Equal("reference", reference.ResourceType);
        Assert.Equal("asset", asset.ResourceType);
    }
}
