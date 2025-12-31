namespace AgentSkills.Prompts;

using System.Text;
using AgentSkills;

/// <summary>
/// Default implementation of ISkillPromptRenderer that produces clear, structured prompts.
/// </summary>
/// <remarks>
/// This renderer follows the progressive disclosure pattern:
/// - RenderSkillList: Shows name, description, and key metadata
/// - RenderSkillDetails: Shows full instructions, resources, and all metadata
/// </remarks>
public sealed class DefaultSkillPromptRenderer : ISkillPromptRenderer
{
    /// <inheritdoc/>
    public string RenderSkillList(IEnumerable<SkillMetadata> metadata, PromptRenderOptions? options = null)
    {
        options ??= new PromptRenderOptions();
        var builder = new StringBuilder();

        builder.AppendLine("# Available Skills");
        builder.AppendLine();
        builder.AppendLine("The following skills are available. To use a skill, activate it by name.");
        builder.AppendLine();

        // Avoid unnecessary enumeration by using IReadOnlyList if available
        var metadataList = metadata as IReadOnlyList<SkillMetadata> ?? metadata.ToList();
        if (metadataList.Count == 0)
        {
            builder.AppendLine("No skills available.");
            return builder.ToString();
        }

        foreach (var meta in metadataList)
        {
            builder.AppendLine($"## {meta.Name}");
            builder.AppendLine();
            builder.AppendLine($"**Description:** {meta.Description}");
            builder.AppendLine();

            if (options.IncludeVersion && !string.IsNullOrWhiteSpace(meta.Version))
            {
                builder.AppendLine($"**Version:** {meta.Version}");
                builder.AppendLine();
            }

            if (options.IncludeAuthor && !string.IsNullOrWhiteSpace(meta.Author))
            {
                builder.AppendLine($"**Author:** {meta.Author}");
                builder.AppendLine();
            }

            if (options.IncludeTags && meta.Tags.Count > 0)
            {
                builder.AppendLine($"**Tags:** {string.Join(", ", meta.Tags)}");
                builder.AppendLine();
            }

            builder.AppendLine("---");
            builder.AppendLine();
        }

        return builder.ToString();
    }

    /// <inheritdoc/>
    public string RenderSkillDetails(Skill skill, PromptRenderOptions? options = null)
    {
        options ??= new PromptRenderOptions();
        var builder = new StringBuilder();

        // Render skill header
        builder.AppendLine($"# Skill: {skill.Manifest.Name}");
        builder.AppendLine();
        builder.AppendLine($"**Description:** {skill.Manifest.Description}");
        builder.AppendLine();

        // Render optional metadata
        if (options.IncludeVersion && !string.IsNullOrWhiteSpace(skill.Manifest.Version))
        {
            builder.AppendLine($"**Version:** {skill.Manifest.Version}");
            builder.AppendLine();
        }

        if (options.IncludeAuthor && !string.IsNullOrWhiteSpace(skill.Manifest.Author))
        {
            builder.AppendLine($"**Author:** {skill.Manifest.Author}");
            builder.AppendLine();
        }

        if (options.IncludeTags && skill.Manifest.Tags.Count > 0)
        {
            builder.AppendLine($"**Tags:** {string.Join(", ", skill.Manifest.Tags)}");
            builder.AppendLine();
        }

        // Render allowed-tools based on policy
        var shouldIncludeAllowedTools = options.IncludeAllowedTools &&
            (options.ResourcePolicy?.ShouldIncludeAllowedTools(skill) ?? true);

        if (shouldIncludeAllowedTools && skill.Manifest.AllowedTools.Count > 0)
        {
            builder.AppendLine($"**Allowed Tools:** {string.Join(", ", skill.Manifest.AllowedTools)}");
            builder.AppendLine();
        }

        // Render instructions
        builder.AppendLine("## Instructions");
        builder.AppendLine();
        builder.AppendLine(skill.Instructions);
        builder.AppendLine();

        return builder.ToString();
    }
}
