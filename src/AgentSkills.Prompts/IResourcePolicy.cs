namespace AgentSkills.Prompts;

using AgentSkills;

/// <summary>
/// Policy interface for controlling visibility of skill resources in prompts.
/// </summary>
/// <remarks>
/// Implementations can control which resources are exposed to the LLM,
/// enabling security policies, token optimization, or conditional access.
/// </remarks>
public interface IResourcePolicy
{
    /// <summary>
    /// Determines whether a specific resource should be included in the prompt.
    /// </summary>
    /// <param name="resource">The resource to evaluate.</param>
    /// <param name="skill">The skill that contains the resource.</param>
    /// <returns>True if the resource should be included; false to redact it.</returns>
    bool ShouldIncludeResource(SkillResource resource, Skill skill);

    /// <summary>
    /// Determines whether allowed-tools metadata should be included in the prompt.
    /// </summary>
    /// <param name="skill">The skill to evaluate.</param>
    /// <returns>True if allowed-tools should be included; false to redact.</returns>
    bool ShouldIncludeAllowedTools(Skill skill);
}
