namespace AgentSkills.Prompts;

/// <summary>
/// Options for controlling how skills are rendered as prompts.
/// </summary>
public sealed class PromptRenderOptions
{
    /// <summary>
    /// Gets or sets the resource policy for controlling resource visibility.
    /// If null, all resources are included by default.
    /// </summary>
    public IResourcePolicy? ResourcePolicy { get; init; }

    /// <summary>
    /// Gets or sets whether to include version information in rendered output.
    /// Default is true.
    /// </summary>
    public bool IncludeVersion { get; init; } = true;

    /// <summary>
    /// Gets or sets whether to include author information in rendered output.
    /// Default is true.
    /// </summary>
    public bool IncludeAuthor { get; init; } = true;

    /// <summary>
    /// Gets or sets whether to include tags in rendered output.
    /// Default is true.
    /// </summary>
    public bool IncludeTags { get; init; } = true;

    /// <summary>
    /// Gets or sets whether to include allowed-tools in rendered output.
    /// Default is true.
    /// </summary>
    public bool IncludeAllowedTools { get; init; } = true;
}
