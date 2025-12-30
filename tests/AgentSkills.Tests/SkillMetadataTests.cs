namespace AgentSkills.Tests;

public class SkillMetadataTests
{
    [Fact]
    public void SkillMetadata_CanBeCreated_WithRequiredProperties()
    {
        // Arrange & Act
        var metadata = new SkillMetadata
        {
            Name = "example-skill",
            Description = "An example skill",
            Path = "/path/to/skill"
        };

        // Assert
        Assert.Equal("example-skill", metadata.Name);
        Assert.Equal("An example skill", metadata.Description);
        Assert.Equal("/path/to/skill", metadata.Path);
        Assert.Null(metadata.Version);
        Assert.Null(metadata.Author);
        Assert.Empty(metadata.Tags);
    }

    [Fact]
    public void SkillMetadata_CanBeCreated_WithAllProperties()
    {
        // Arrange & Act
        var metadata = new SkillMetadata
        {
            Name = "example-skill",
            Description = "An example skill",
            Path = "/path/to/skill",
            Version = "1.0.0",
            Author = "Test Author",
            Tags = new[] { "test", "example" }
        };

        // Assert
        Assert.Equal("example-skill", metadata.Name);
        Assert.Equal("An example skill", metadata.Description);
        Assert.Equal("/path/to/skill", metadata.Path);
        Assert.Equal("1.0.0", metadata.Version);
        Assert.Equal("Test Author", metadata.Author);
        Assert.Equal(2, metadata.Tags.Count);
        Assert.Contains("test", metadata.Tags);
        Assert.Contains("example", metadata.Tags);
    }
}
