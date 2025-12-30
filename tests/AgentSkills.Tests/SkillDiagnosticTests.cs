namespace AgentSkills.Tests;

public class SkillDiagnosticTests
{
    [Fact]
    public void SkillDiagnostic_CanBeCreated_WithRequiredProperties()
    {
        // Arrange & Act
        var diagnostic = new SkillDiagnostic
        {
            Severity = DiagnosticSeverity.Error,
            Message = "Test error message"
        };

        // Assert
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Equal("Test error message", diagnostic.Message);
        Assert.Null(diagnostic.Path);
        Assert.Null(diagnostic.Line);
        Assert.Null(diagnostic.Column);
        Assert.Null(diagnostic.Code);
    }

    [Fact]
    public void SkillDiagnostic_CanBeCreated_WithAllProperties()
    {
        // Arrange & Act
        var diagnostic = new SkillDiagnostic
        {
            Severity = DiagnosticSeverity.Warning,
            Message = "Test warning",
            Path = "/path/to/skill/SKILL.md",
            Line = 5,
            Column = 10,
            Code = "SK001"
        };

        // Assert
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.Equal("Test warning", diagnostic.Message);
        Assert.Equal("/path/to/skill/SKILL.md", diagnostic.Path);
        Assert.Equal(5, diagnostic.Line);
        Assert.Equal(10, diagnostic.Column);
        Assert.Equal("SK001", diagnostic.Code);
    }

    [Theory]
    [InlineData(DiagnosticSeverity.Info)]
    [InlineData(DiagnosticSeverity.Warning)]
    [InlineData(DiagnosticSeverity.Error)]
    public void SkillDiagnostic_SupportsAllSeverityLevels(DiagnosticSeverity severity)
    {
        // Arrange & Act
        var diagnostic = new SkillDiagnostic
        {
            Severity = severity,
            Message = "Test message"
        };

        // Assert
        Assert.Equal(severity, diagnostic.Severity);
    }
}
