namespace AgentSkills;

/// <summary>
/// Represents a collection of skills that have been discovered and loaded.
/// </summary>
public sealed class SkillSet
{
    /// <summary>
    /// Gets the collection of skills in this skill set.
    /// </summary>
    public IReadOnlyList<Skill> Skills { get; init; } = Array.Empty<Skill>();

    /// <summary>
    /// Gets the diagnostics produced while loading this skill set.
    /// </summary>
    public IReadOnlyList<SkillDiagnostic> Diagnostics { get; init; } = Array.Empty<SkillDiagnostic>();

    /// <summary>
    /// Gets a value indicating whether the skill set is valid (no errors in diagnostics).
    /// </summary>
    public bool IsValid => !Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);

    /// <summary>
    /// Gets a skill by name.
    /// </summary>
    /// <param name="name">The name of the skill to retrieve.</param>
    /// <returns>The skill if found; otherwise, null.</returns>
    public Skill? GetSkill(string name)
    {
        return Skills.FirstOrDefault(s => s.Manifest.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets all skills that have a specific tag.
    /// </summary>
    /// <param name="tag">The tag to search for.</param>
    /// <returns>A collection of skills with the specified tag.</returns>
    public IEnumerable<Skill> GetSkillsByTag(string tag)
    {
        return Skills.Where(s => s.Manifest.Tags.Any(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase)));
    }
}
