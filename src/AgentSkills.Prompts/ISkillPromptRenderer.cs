namespace AgentSkills.Prompts;

using AgentSkills;

/// <summary>
/// Renders skill information as prompts for LLMs following progressive disclosure pattern.
/// </summary>
/// <remarks>
/// Progressive disclosure ensures that LLMs first see skill summaries, then can request
/// full details when activating a skill. This reduces token usage and improves relevance.
/// </remarks>
public interface ISkillPromptRenderer
{
    /// <summary>
    /// Renders a summary list of multiple skills showing only key metadata.
    /// Use this for presenting available skills to the LLM.
    /// </summary>
    /// <param name="metadata">Collection of skill metadata to render.</param>
    /// <param name="options">Optional rendering options.</param>
    /// <returns>A formatted string presenting the skill list.</returns>
    string RenderSkillList(IEnumerable<SkillMetadata> metadata, PromptRenderOptions? options = null);

    /// <summary>
    /// Renders full details for a single skill including instructions and resources.
    /// Use this when the LLM activates a specific skill.
    /// </summary>
    /// <param name="skill">The skill to render in full detail.</param>
    /// <param name="options">Optional rendering options.</param>
    /// <returns>A formatted string with complete skill information.</returns>
    string RenderSkillDetails(Skill skill, PromptRenderOptions? options = null);
}
