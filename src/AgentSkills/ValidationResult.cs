namespace AgentSkills;

/// <summary>
/// Represents the result of validating a skill, containing diagnostics.
/// </summary>
public sealed class ValidationResult
{
    /// <summary>
    /// Gets the collection of diagnostics produced during validation.
    /// </summary>
    public IReadOnlyList<SkillDiagnostic> Diagnostics { get; init; } = Array.Empty<SkillDiagnostic>();

    /// <summary>
    /// Gets a value indicating whether the validation succeeded (no errors).
    /// </summary>
    public bool IsValid => !Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);

    /// <summary>
    /// Gets a value indicating whether there are any warnings.
    /// </summary>
    public bool HasWarnings => Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Warning);

    /// <summary>
    /// Gets all error diagnostics.
    /// </summary>
    public IEnumerable<SkillDiagnostic> Errors => Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);

    /// <summary>
    /// Gets all warning diagnostics.
    /// </summary>
    public IEnumerable<SkillDiagnostic> Warnings => Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning);

    /// <summary>
    /// Gets all info diagnostics.
    /// </summary>
    public IEnumerable<SkillDiagnostic> Infos => Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Info);
}
