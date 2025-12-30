namespace AgentSkills;

/// <summary>
/// Represents the severity level of a diagnostic message.
/// </summary>
public enum DiagnosticSeverity
{
    /// <summary>
    /// Informational message that does not indicate a problem.
    /// </summary>
    Info = 0,

    /// <summary>
    /// Warning message that indicates a potential problem but does not prevent usage.
    /// </summary>
    Warning = 1,

    /// <summary>
    /// Error message that indicates a problem that should prevent usage.
    /// </summary>
    Error = 2
}
