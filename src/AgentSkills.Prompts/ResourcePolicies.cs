namespace AgentSkills.Prompts;

using AgentSkills;

/// <summary>
/// A resource policy that includes all resources and metadata.
/// This is the permissive default policy.
/// </summary>
public sealed class IncludeAllResourcePolicy : IResourcePolicy
{
    /// <summary>
    /// Gets the singleton instance of this policy.
    /// </summary>
    public static IncludeAllResourcePolicy Instance { get; } = new();

    private IncludeAllResourcePolicy() { }

    /// <inheritdoc/>
    public bool ShouldIncludeResource(SkillResource resource, Skill skill) => true;

    /// <inheritdoc/>
    public bool ShouldIncludeAllowedTools(Skill skill) => true;
}

/// <summary>
/// A resource policy that excludes all resources and sensitive metadata.
/// This is a restrictive policy for security-conscious scenarios.
/// </summary>
public sealed class ExcludeAllResourcePolicy : IResourcePolicy
{
    /// <summary>
    /// Gets the singleton instance of this policy.
    /// </summary>
    public static ExcludeAllResourcePolicy Instance { get; } = new();

    private ExcludeAllResourcePolicy() { }

    /// <inheritdoc/>
    public bool ShouldIncludeResource(SkillResource resource, Skill skill) => false;

    /// <inheritdoc/>
    public bool ShouldIncludeAllowedTools(Skill skill) => false;
}

/// <summary>
/// A resource policy that filters resources by type.
/// Allows hosts to control which resource types are exposed.
/// </summary>
public sealed class ResourceTypeFilterPolicy : IResourcePolicy
{
    private readonly HashSet<string> _allowedTypes;

    /// <summary>
    /// Initializes a new instance of ResourceTypeFilterPolicy.
    /// </summary>
    /// <param name="allowedTypes">Resource types to include (e.g., "reference", "asset"). Case-insensitive.</param>
    public ResourceTypeFilterPolicy(IEnumerable<string> allowedTypes)
    {
        _allowedTypes = new HashSet<string>(
            allowedTypes.Select(t => t.ToLowerInvariant()),
            StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public bool ShouldIncludeResource(SkillResource resource, Skill skill)
    {
        if (string.IsNullOrEmpty(resource.ResourceType))
            return false;

        return _allowedTypes.Contains(resource.ResourceType);
    }

    /// <inheritdoc/>
    public bool ShouldIncludeAllowedTools(Skill skill) => true;
}
