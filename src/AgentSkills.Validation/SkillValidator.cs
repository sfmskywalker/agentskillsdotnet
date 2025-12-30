using System.Text.RegularExpressions;

namespace AgentSkills.Validation;

/// <summary>
/// Validates skills against the Agent Skills v1 specification.
/// </summary>
public sealed partial class SkillValidator : ISkillValidator
{
    // Validation constants from v1 spec
    private const int NameMinLength = 1;
    private const int NameMaxLength = 64;
    private const int DescriptionMinLength = 1;
    private const int DescriptionMaxLength = 1024;
    private const int CompatibilityMaxLength = 500;

    // Name validation pattern: lowercase letters, numbers, hyphens only
    // Cannot start/end with hyphen, no consecutive hyphens
    [GeneratedRegex(@"^[a-z0-9]+(-[a-z0-9]+)*$")]
    private static partial Regex NamePattern();

    /// <inheritdoc/>
    public ValidationResult Validate(Skill skill)
    {
        var diagnostics = new List<SkillDiagnostic>();

        // Validate manifest fields
        ValidateManifest(skill.Manifest, skill.Path, diagnostics);

        // Validate that directory name matches skill name
        ValidateDirectoryName(skill.Path, skill.Manifest.Name, diagnostics);

        return new ValidationResult
        {
            Diagnostics = diagnostics
        };
    }

    /// <inheritdoc/>
    public ValidationResult ValidateMetadata(SkillMetadata metadata)
    {
        var diagnostics = new List<SkillDiagnostic>();

        // Validate name field
        ValidateName(metadata.Name, metadata.Path, diagnostics);

        // Validate description field
        ValidateDescription(metadata.Description, metadata.Path, diagnostics);

        // Validate that directory name matches skill name
        ValidateDirectoryName(metadata.Path, metadata.Name, diagnostics);

        return new ValidationResult
        {
            Diagnostics = diagnostics
        };
    }

    private void ValidateManifest(SkillManifest manifest, string skillPath, List<SkillDiagnostic> diagnostics)
    {
        // Validate required fields
        ValidateName(manifest.Name, skillPath, diagnostics);
        ValidateDescription(manifest.Description, skillPath, diagnostics);

        // Validate optional fields if present
        if (manifest.Version != null)
        {
            ValidateVersion(manifest.Version, skillPath, diagnostics);
        }

        // Validate compatibility field length if present
        if (manifest.AdditionalFields != null 
            && manifest.AdditionalFields.TryGetValue("compatibility", out var compatibilityValue) 
            && compatibilityValue is string compatibility)
        {
            ValidateCompatibility(compatibility, skillPath, diagnostics);
        }
    }

    private void ValidateName(string name, string skillPath, List<SkillDiagnostic> diagnostics)
    {
        // Check if name is null or whitespace (should be caught by loader, but double-check)
        if (string.IsNullOrWhiteSpace(name))
        {
            diagnostics.Add(new SkillDiagnostic
            {
                Severity = DiagnosticSeverity.Error,
                Message = "Required field 'name' is missing or empty",
                Path = skillPath,
                Code = "VAL001"
            });
            return;
        }

        // Check length constraints
        if (name.Length < NameMinLength || name.Length > NameMaxLength)
        {
            diagnostics.Add(new SkillDiagnostic
            {
                Severity = DiagnosticSeverity.Error,
                Message = $"Field 'name' must be {NameMinLength}-{NameMaxLength} characters (found: {name.Length})",
                Path = skillPath,
                Code = "VAL002"
            });
        }

        // Check pattern: lowercase, numbers, hyphens only, no leading/trailing/consecutive hyphens
        if (!NamePattern().IsMatch(name))
        {
            var reasons = new List<string>();
            
            if (name.Any(char.IsUpper))
                reasons.Add("contains uppercase letters");
            if (name.StartsWith('-') || name.EndsWith('-'))
                reasons.Add("starts or ends with hyphen");
            if (name.Contains("--"))
                reasons.Add("contains consecutive hyphens");
            if (name.Any(c => !char.IsLetterOrDigit(c) && c != '-'))
                reasons.Add("contains invalid characters");

            var reasonText = reasons.Any() ? $" ({string.Join(", ", reasons)})" : "";
            
            diagnostics.Add(new SkillDiagnostic
            {
                Severity = DiagnosticSeverity.Error,
                Message = $"Field 'name' must contain only lowercase letters, numbers, and hyphens; cannot start/end with hyphen or have consecutive hyphens{reasonText}",
                Path = skillPath,
                Code = "VAL003"
            });
        }
    }

    private void ValidateDescription(string description, string skillPath, List<SkillDiagnostic> diagnostics)
    {
        // Check if description is null or whitespace
        if (string.IsNullOrWhiteSpace(description))
        {
            diagnostics.Add(new SkillDiagnostic
            {
                Severity = DiagnosticSeverity.Error,
                Message = "Required field 'description' is missing or empty",
                Path = skillPath,
                Code = "VAL004"
            });
            return;
        }

        // Check length constraints
        if (description.Length < DescriptionMinLength || description.Length > DescriptionMaxLength)
        {
            diagnostics.Add(new SkillDiagnostic
            {
                Severity = DiagnosticSeverity.Error,
                Message = $"Field 'description' must be {DescriptionMinLength}-{DescriptionMaxLength} characters (found: {description.Length})",
                Path = skillPath,
                Code = "VAL005"
            });
        }

        // Warning if description is too short (recommended to be descriptive)
        if (description.Length < 20)
        {
            diagnostics.Add(new SkillDiagnostic
            {
                Severity = DiagnosticSeverity.Warning,
                Message = "Field 'description' is very short; consider adding more detail about when to use this skill",
                Path = skillPath,
                Code = "VAL006"
            });
        }
    }

    private void ValidateVersion(string version, string skillPath, List<SkillDiagnostic> diagnostics)
    {
        // Version is optional but should not be empty if provided
        if (string.IsNullOrWhiteSpace(version))
        {
            diagnostics.Add(new SkillDiagnostic
            {
                Severity = DiagnosticSeverity.Warning,
                Message = "Field 'version' is present but empty; consider removing it or providing a value",
                Path = skillPath,
                Code = "VAL007"
            });
        }
    }

    private void ValidateCompatibility(string compatibility, string skillPath, List<SkillDiagnostic> diagnostics)
    {
        if (compatibility.Length > CompatibilityMaxLength)
        {
            diagnostics.Add(new SkillDiagnostic
            {
                Severity = DiagnosticSeverity.Error,
                Message = $"Field 'compatibility' must not exceed {CompatibilityMaxLength} characters (found: {compatibility.Length})",
                Path = skillPath,
                Code = "VAL008"
            });
        }
    }

    private void ValidateDirectoryName(string skillPath, string skillName, List<SkillDiagnostic> diagnostics)
    {
        var directoryName = Path.GetFileName(skillPath);
        
        if (string.IsNullOrEmpty(directoryName))
        {
            // If we can't determine directory name, issue a warning
            diagnostics.Add(new SkillDiagnostic
            {
                Severity = DiagnosticSeverity.Warning,
                Message = "Cannot determine directory name to validate against skill name",
                Path = skillPath,
                Code = "VAL009"
            });
            return;
        }

        if (!string.Equals(directoryName, skillName, StringComparison.Ordinal))
        {
            diagnostics.Add(new SkillDiagnostic
            {
                Severity = DiagnosticSeverity.Error,
                Message = $"Directory name '{directoryName}' does not match skill name '{skillName}'",
                Path = skillPath,
                Code = "VAL010"
            });
        }
    }
}
