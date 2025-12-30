namespace AgentSkills;

/// <summary>
/// Represents a resource file within a skill directory (e.g., scripts, references, assets).
/// This is an abstraction to avoid tying the core domain to file system specifics.
/// </summary>
public sealed class SkillResource
{
    /// <summary>
    /// Gets the name of the resource file.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the path to the resource file, relative to the skill directory.
    /// </summary>
    public required string RelativePath { get; init; }

    /// <summary>
    /// Gets the type of resource (e.g., "script", "reference", "asset").
    /// This is typically derived from the subdirectory name.
    /// </summary>
    public string? ResourceType { get; init; }

    /// <summary>
    /// Gets the absolute path to the resource file, if available.
    /// </summary>
    public string? AbsolutePath { get; init; }
}
