namespace AgentSkills.Validation;

/// <summary>
/// Validates skills against the Agent Skills v1 specification and produces diagnostics.
/// </summary>
public interface ISkillValidator
{
    /// <summary>
    /// Validates a skill against the v1 specification.
    /// </summary>
    /// <param name="skill">The skill to validate.</param>
    /// <returns>A validation result containing diagnostics.</returns>
    ValidationResult Validate(Skill skill);

    /// <summary>
    /// Validates skill metadata against the v1 specification.
    /// This is a lighter validation for metadata-only scenarios.
    /// </summary>
    /// <param name="metadata">The skill metadata to validate.</param>
    /// <returns>A validation result containing diagnostics.</returns>
    ValidationResult ValidateMetadata(SkillMetadata metadata);
}
