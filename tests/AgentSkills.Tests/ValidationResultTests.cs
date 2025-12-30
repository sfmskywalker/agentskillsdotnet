namespace AgentSkills.Tests;

public class ValidationResultTests
{
    [Fact]
    public void ValidationResult_IsValid_WhenNoDiagnostics()
    {
        // Arrange
        var result = new ValidationResult
        {
            Diagnostics = Array.Empty<SkillDiagnostic>()
        };

        // Assert
        Assert.True(result.IsValid);
        Assert.False(result.HasWarnings);
        Assert.Empty(result.Errors);
        Assert.Empty(result.Warnings);
        Assert.Empty(result.Infos);
    }

    [Fact]
    public void ValidationResult_IsValid_WhenOnlyWarningsAndInfos()
    {
        // Arrange
        var result = new ValidationResult
        {
            Diagnostics = new[]
            {
                new SkillDiagnostic { Severity = DiagnosticSeverity.Warning, Message = "Warning" },
                new SkillDiagnostic { Severity = DiagnosticSeverity.Info, Message = "Info" }
            }
        };

        // Assert
        Assert.True(result.IsValid);
        Assert.True(result.HasWarnings);
        Assert.Empty(result.Errors);
        Assert.Single(result.Warnings);
        Assert.Single(result.Infos);
    }

    [Fact]
    public void ValidationResult_IsNotValid_WhenContainsErrors()
    {
        // Arrange
        var result = new ValidationResult
        {
            Diagnostics = new[]
            {
                new SkillDiagnostic { Severity = DiagnosticSeverity.Error, Message = "Error" },
                new SkillDiagnostic { Severity = DiagnosticSeverity.Warning, Message = "Warning" }
            }
        };

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.HasWarnings);
        Assert.Single(result.Errors);
        Assert.Single(result.Warnings);
        Assert.Empty(result.Infos);
    }

    [Fact]
    public void ValidationResult_FiltersErrorsCorrectly()
    {
        // Arrange
        var result = new ValidationResult
        {
            Diagnostics = new[]
            {
                new SkillDiagnostic { Severity = DiagnosticSeverity.Error, Message = "Error 1" },
                new SkillDiagnostic { Severity = DiagnosticSeverity.Warning, Message = "Warning" },
                new SkillDiagnostic { Severity = DiagnosticSeverity.Error, Message = "Error 2" }
            }
        };

        // Act
        var errors = result.Errors.ToList();

        // Assert
        Assert.Equal(2, errors.Count);
        Assert.All(errors, e => Assert.Equal(DiagnosticSeverity.Error, e.Severity));
    }

    [Fact]
    public void ValidationResult_FiltersWarningsCorrectly()
    {
        // Arrange
        var result = new ValidationResult
        {
            Diagnostics = new[]
            {
                new SkillDiagnostic { Severity = DiagnosticSeverity.Warning, Message = "Warning 1" },
                new SkillDiagnostic { Severity = DiagnosticSeverity.Error, Message = "Error" },
                new SkillDiagnostic { Severity = DiagnosticSeverity.Warning, Message = "Warning 2" }
            }
        };

        // Act
        var warnings = result.Warnings.ToList();

        // Assert
        Assert.Equal(2, warnings.Count);
        Assert.All(warnings, w => Assert.Equal(DiagnosticSeverity.Warning, w.Severity));
    }

    [Fact]
    public void ValidationResult_FiltersInfosCorrectly()
    {
        // Arrange
        var result = new ValidationResult
        {
            Diagnostics = new[]
            {
                new SkillDiagnostic { Severity = DiagnosticSeverity.Info, Message = "Info 1" },
                new SkillDiagnostic { Severity = DiagnosticSeverity.Error, Message = "Error" },
                new SkillDiagnostic { Severity = DiagnosticSeverity.Info, Message = "Info 2" }
            }
        };

        // Act
        var infos = result.Infos.ToList();

        // Assert
        Assert.Equal(2, infos.Count);
        Assert.All(infos, i => Assert.Equal(DiagnosticSeverity.Info, i.Severity));
    }
}
