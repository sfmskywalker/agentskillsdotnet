namespace AgentSkills.Tests;

public class SkillTests
{
    [Fact]
    public void Skill_CanBeCreated_WithRequiredProperties()
    {
        // Arrange
        var manifest = new SkillManifest
        {
            Name = "test-skill",
            Description = "A test skill"
        };

        // Act
        var skill = new Skill
        {
            Manifest = manifest,
            Instructions = "# Test Instructions\n\nThis is a test.",
            Path = "/path/to/skill"
        };

        // Assert
        Assert.Equal(manifest, skill.Manifest);
        Assert.Equal("# Test Instructions\n\nThis is a test.", skill.Instructions);
        Assert.Equal("/path/to/skill", skill.Path);
    }

    [Fact]
    public void Skill_Metadata_IsDerivedFromManifest()
    {
        // Arrange
        var manifest = new SkillManifest
        {
            Name = "test-skill",
            Description = "A test skill",
            Version = "1.0.0",
            Author = "Test Author",
            Tags = new[] { "test", "example" }
        };

        var skill = new Skill
        {
            Manifest = manifest,
            Instructions = "Instructions",
            Path = "/path/to/skill"
        };

        // Act
        var metadata = skill.Metadata;

        // Assert
        Assert.Equal("test-skill", metadata.Name);
        Assert.Equal("A test skill", metadata.Description);
        Assert.Equal("1.0.0", metadata.Version);
        Assert.Equal("Test Author", metadata.Author);
        Assert.Equal(2, metadata.Tags.Count);
        Assert.Equal("/path/to/skill", metadata.Path);
    }

    [Fact]
    public void Skill_WithCompleteManifest()
    {
        // Arrange & Act
        var manifest = new SkillManifest
        {
            Name = "example-skill",
            Description = "An example skill",
            Version = "2.0.0",
            Author = "AgentSkills.NET",
            Tags = new[] { "example", "demo" },
            AllowedTools = new[] { "filesystem", "calculator" },
            AdditionalFields = new Dictionary<string, object?>
            {
                ["custom"] = "value"
            }
        };

        var skill = new Skill
        {
            Manifest = manifest,
            Instructions = "# Example Skill\n\nThese are the instructions.",
            Path = "/skills/example-skill"
        };

        // Assert
        Assert.NotNull(skill.Manifest);
        Assert.Equal("example-skill", skill.Manifest.Name);
        Assert.Equal(2, skill.Manifest.AllowedTools.Count);
        Assert.Single(skill.Manifest.AdditionalFields);
        Assert.Contains("# Example Skill", skill.Instructions);
    }
}
