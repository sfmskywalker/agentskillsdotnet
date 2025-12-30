namespace AgentSkills.Loader;

/// <summary>
/// Loads skills from the file system.
/// </summary>
public interface ISkillLoader
{
    /// <summary>
    /// Loads a complete skill set from a directory, including all skill content.
    /// </summary>
    /// <param name="directoryPath">The path to the directory containing skills.</param>
    /// <returns>A SkillSet containing loaded skills and any diagnostics from the loading process.</returns>
    SkillSet LoadSkillSet(string directoryPath);

    /// <summary>
    /// Loads only metadata for skills in a directory (fast path, does not load full content).
    /// </summary>
    /// <param name="directoryPath">The path to the directory containing skills.</param>
    /// <returns>A collection of SkillMetadata objects and any diagnostics from the loading process.</returns>
    (IReadOnlyList<SkillMetadata> Metadata, IReadOnlyList<SkillDiagnostic> Diagnostics) LoadMetadata(string directoryPath);

    /// <summary>
    /// Loads a single skill from a directory.
    /// </summary>
    /// <param name="skillDirectoryPath">The path to the skill directory containing SKILL.md.</param>
    /// <returns>The loaded skill and any diagnostics, or null if the skill could not be loaded.</returns>
    (Skill? Skill, IReadOnlyList<SkillDiagnostic> Diagnostics) LoadSkill(string skillDirectoryPath);
}
