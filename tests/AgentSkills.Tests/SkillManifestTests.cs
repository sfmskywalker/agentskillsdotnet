namespace AgentSkills.Tests;

public class SkillManifestTests
{
    [Fact]
    public void SkillManifest_CanBeCreated_WithRequiredProperties()
    {
        // Arrange & Act
        var manifest = new SkillManifest
        {
            Name = "test-skill",
            Description = "A test skill"
        };

        // Assert
        Assert.Equal("test-skill", manifest.Name);
        Assert.Equal("A test skill", manifest.Description);
        Assert.Null(manifest.Version);
        Assert.Null(manifest.Author);
        Assert.Empty(manifest.Tags);
        Assert.Empty(manifest.AllowedTools);
        Assert.Empty(manifest.AdditionalFields);
    }

    [Fact]
    public void SkillManifest_CanBeCreated_WithAllProperties()
    {
        // Arrange & Act
        var manifest = new SkillManifest
        {
            Name = "test-skill",
            Description = "A test skill",
            Version = "1.0.0",
            Author = "Test Author",
            Tags = new[] { "test", "example" },
            AllowedTools = new[] { "filesystem", "calculator" }
        };

        // Assert
        Assert.Equal("test-skill", manifest.Name);
        Assert.Equal("A test skill", manifest.Description);
        Assert.Equal("1.0.0", manifest.Version);
        Assert.Equal("Test Author", manifest.Author);
        Assert.Equal(2, manifest.Tags.Count);
        Assert.Contains("test", manifest.Tags);
        Assert.Contains("example", manifest.Tags);
        Assert.Equal(2, manifest.AllowedTools.Count);
        Assert.Contains("filesystem", manifest.AllowedTools);
        Assert.Contains("calculator", manifest.AllowedTools);
    }

    [Fact]
    public void SkillManifest_SupportsAdditionalFields()
    {
        // Arrange & Act
        var manifest = new SkillManifest
        {
            Name = "test-skill",
            Description = "A test skill",
            AdditionalFields = new Dictionary<string, object?>
            {
                ["custom-field"] = "custom-value",
                ["numeric-field"] = 42,
                ["boolean-field"] = true
            }
        };

        // Assert
        Assert.Equal(3, manifest.AdditionalFields.Count);
        Assert.Equal("custom-value", manifest.AdditionalFields["custom-field"]);
        Assert.Equal(42, manifest.AdditionalFields["numeric-field"]);
        Assert.Equal(true, manifest.AdditionalFields["boolean-field"]);
    }

    [Fact]
    public void SkillManifest_AdditionalFields_AllowsNullValues()
    {
        // Arrange & Act
        var manifest = new SkillManifest
        {
            Name = "test-skill",
            Description = "A test skill",
            AdditionalFields = new Dictionary<string, object?>
            {
                ["null-field"] = null
            }
        };

        // Assert
        Assert.Single(manifest.AdditionalFields);
        Assert.True(manifest.AdditionalFields.ContainsKey("null-field"));
        Assert.Null(manifest.AdditionalFields["null-field"]);
    }
}
