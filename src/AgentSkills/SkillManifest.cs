namespace AgentSkills;

/// <summary>
/// Represents the parsed YAML frontmatter from a SKILL.md file.
/// This contains the skill's metadata and configuration.
/// </summary>
public sealed class SkillManifest
{
    /// <summary>
    /// Gets the unique name of the skill.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the description of the skill.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets the version of the skill.
    /// </summary>
    public string? Version { get; init; }

    /// <summary>
    /// Gets the author of the skill.
    /// </summary>
    public string? Author { get; init; }

    /// <summary>
    /// Gets the tags associated with the skill.
    /// </summary>
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets the list of allowed tools that this skill may use.
    /// This is advisory only; hosts are responsible for enforcement.
    /// </summary>
    public IReadOnlyList<string> AllowedTools { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets additional fields that were present in the YAML frontmatter but not mapped to known properties.
    /// This allows for extensibility and forward compatibility.
    /// </summary>
    public IReadOnlyDictionary<string, object?> AdditionalFields { get; init; } = 
        new Dictionary<string, object?>();
}
