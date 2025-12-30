namespace AgentSkills;

/// <summary>
/// Represents a diagnostic message (error, warning, or info) about a skill.
/// </summary>
public sealed class SkillDiagnostic
{
    /// <summary>
    /// Gets the severity level of this diagnostic.
    /// </summary>
    public required DiagnosticSeverity Severity { get; init; }

    /// <summary>
    /// Gets the diagnostic message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Gets the path to the file or skill that this diagnostic is about, if applicable.
    /// </summary>
    public string? Path { get; init; }

    /// <summary>
    /// Gets the line number within the file, if applicable.
    /// </summary>
    public int? Line { get; init; }

    /// <summary>
    /// Gets the column number within the file, if applicable.
    /// </summary>
    public int? Column { get; init; }

    /// <summary>
    /// Gets a diagnostic code or identifier, if applicable.
    /// </summary>
    public string? Code { get; init; }
}
