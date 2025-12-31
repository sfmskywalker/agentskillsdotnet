namespace AgentSkills.Adapters.Microsoft.AgentFramework;

using AgentSkills;
using AgentSkills.Prompts;

/// <summary>
/// Builder for creating agent instructions that include skill information.
/// Follows progressive disclosure pattern: skills are listed first, then activated individually.
/// </summary>
public sealed class SkillPromptBuilder
{
    private readonly ISkillPromptRenderer _renderer;
    private readonly List<SkillMetadata> _availableSkills = [];
    private string? _baseInstructions;

    /// <summary>
    /// Initializes a new instance of <see cref="SkillPromptBuilder"/>.
    /// </summary>
    /// <param name="renderer">Optional prompt renderer. If null, uses DefaultSkillPromptRenderer.</param>
    public SkillPromptBuilder(ISkillPromptRenderer? renderer = null)
    {
        _renderer = renderer ?? new DefaultSkillPromptRenderer();
    }

    /// <summary>
    /// Sets the base instructions for the agent.
    /// </summary>
    /// <param name="instructions">The base agent instructions.</param>
    /// <returns>This builder for method chaining.</returns>
    public SkillPromptBuilder WithBaseInstructions(string instructions)
    {
        _baseInstructions = instructions;
        return this;
    }

    /// <summary>
    /// Adds skills to be made available to the agent.
    /// </summary>
    /// <param name="metadata">Skill metadata to add.</param>
    /// <returns>This builder for method chaining.</returns>
    public SkillPromptBuilder WithSkills(IEnumerable<SkillMetadata> metadata)
    {
        _availableSkills.AddRange(metadata);
        return this;
    }

    /// <summary>
    /// Adds a skill set to be made available to the agent.
    /// </summary>
    /// <param name="skillSet">The skill set to add.</param>
    /// <returns>This builder for method chaining.</returns>
    public SkillPromptBuilder WithSkillSet(SkillSet skillSet)
    {
        _availableSkills.AddRange(skillSet.Skills.Select(s => s.Metadata));
        return this;
    }

    /// <summary>
    /// Builds the complete instructions including base instructions and skill listing.
    /// </summary>
    /// <param name="options">Optional rendering options.</param>
    /// <returns>The complete agent instructions as a string.</returns>
    public string Build(PromptRenderOptions? options = null)
    {
        List<string> parts = [];

        if (!string.IsNullOrWhiteSpace(_baseInstructions))
        {
            parts.Add(_baseInstructions.Trim());
        }

        if (_availableSkills.Count > 0)
        {
            var skillsPrompt = _renderer.RenderSkillList(_availableSkills, options);
            parts.Add(skillsPrompt.Trim());
        }

        return string.Join("\n\n", parts);
    }

    /// <summary>
    /// Renders detailed instructions for a specific activated skill.
    /// Call this when an agent needs to see the full instructions for a skill.
    /// </summary>
    /// <param name="skill">The skill to render details for.</param>
    /// <param name="options">Optional rendering options.</param>
    /// <returns>The detailed skill instructions as a string.</returns>
    public string BuildSkillDetails(Skill skill, PromptRenderOptions? options = null)
    {
        return _renderer.RenderSkillDetails(skill, options);
    }
}
