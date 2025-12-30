namespace AgentSkills;

/// <summary>
/// Represents metadata about a skill that can be loaded without reading the full content.
/// This enables fast skill listing and discovery.
/// </summary>
public sealed class SkillMetadata
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
    /// Gets the path to the skill directory.
    /// </summary>
    public required string Path { get; init; }
}
