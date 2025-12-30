namespace AgentSkills;

/// <summary>
/// Represents a complete skill with its manifest and content.
/// This is the full representation loaded when a skill is activated.
/// </summary>
public sealed class Skill
{
    /// <summary>
    /// Gets the skill manifest (parsed YAML frontmatter).
    /// </summary>
    public required SkillManifest Manifest { get; init; }

    /// <summary>
    /// Gets the skill instructions (Markdown body content).
    /// </summary>
    public required string Instructions { get; init; }

    /// <summary>
    /// Gets the path to the skill directory.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Gets the metadata for this skill (derived from manifest and path).
    /// This provides fast access to listing information.
    /// </summary>
    public SkillMetadata Metadata => new()
    {
        Name = Manifest.Name,
        Description = Manifest.Description,
        Version = Manifest.Version,
        Author = Manifest.Author,
        Tags = Manifest.Tags,
        Path = Path
    };
}
