namespace AgentSkills.Adapters.Microsoft.AgentFramework;

using AgentSkills;
using AgentSkills.Prompts;

/// <summary>
/// Extension methods for integrating AgentSkills with Microsoft Agent Framework.
/// </summary>
public static class SkillExtensions
{
    /// <summary>
    /// Gets the skill instructions as a formatted prompt suitable for agent consumption.
    /// This provides the full skill details when the skill is activated.
    /// </summary>
    /// <param name="skill">The skill to get instructions from.</param>
    /// <param name="renderer">Optional prompt renderer. If null, uses DefaultSkillPromptRenderer.</param>
    /// <param name="options">Optional rendering options.</param>
    /// <returns>The formatted skill instructions as a string.</returns>
    /// <remarks>
    /// Use this method to get skill details when an agent activates a specific skill.
    /// This follows the progressive disclosure pattern: skills are listed first (in base instructions),
    /// then activated individually when needed.
    /// </remarks>
    public static string GetInstructions(
        this Skill skill,
        ISkillPromptRenderer? renderer = null,
        PromptRenderOptions? options = null)
    {
        var promptRenderer = renderer ?? new DefaultSkillPromptRenderer();
        return promptRenderer.RenderSkillDetails(skill, options);
    }

    /// <summary>
    /// Gets a sanitized function name for the skill.
    /// Converts skill name (with hyphens) to a function-safe name (with underscores).
    /// </summary>
    /// <param name="skill">The skill to get the function name for.</param>
    /// <returns>A sanitized function name suitable for use in agent frameworks.</returns>
    /// <remarks>
    /// Skill names use hyphens (e.g., "example-skill") per the Agent Skills specification.
    /// Function names in most frameworks typically use underscores (e.g., "example_skill").
    /// </remarks>
    public static string GetFunctionName(this Skill skill)
    {
        return skill.Manifest.Name.Replace('-', '_');
    }

    /// <summary>
    /// Gets a description suitable for function tool metadata.
    /// </summary>
    /// <param name="skill">The skill to get the description for.</param>
    /// <returns>A description prefixed with "Activate skill:" for clarity.</returns>
    public static string GetFunctionDescription(this Skill skill)
    {
        return $"Activate skill: {skill.Manifest.Description}";
    }
}
