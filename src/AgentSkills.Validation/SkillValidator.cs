using System.Text;
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

    // Name validation pattern: Unicode lowercase letters, numbers, hyphens only
    // Cannot start/end with hyphen, no consecutive hyphens
    // \p{Ll} matches lowercase letters (e.g., a-z, а-я, etc.)
    // \p{Lo} matches other letters without case (e.g., Chinese, Arabic, Hebrew, etc.)
    // \p{Nd} matches any decimal digit in any Unicode script
    [GeneratedRegex(@"^[\p{Ll}\p{Lo}\p{Nd}]+(-[\p{Ll}\p{Lo}\p{Nd}]+)*$")]
    private static partial Regex NamePattern();

    /// <inheritdoc/>
    public ValidationResult Validate(Skill skill)
    {
        List<SkillDiagnostic> diagnostics = [];

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
        List<SkillDiagnostic> diagnostics = [];

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
            diagnostics.Add(CreateDiagnostic(
                DiagnosticSeverity.Error,
                "Required field 'name' is missing or empty",
                skillPath,
                "VAL001"));
            return;
        }

        // Apply NFKC Unicode normalization to ensure composed and decomposed characters are treated equivalently
        var normalizedName = name.Normalize(NormalizationForm.FormKC);

        // Check length constraints
        if (normalizedName.Length < NameMinLength || normalizedName.Length > NameMaxLength)
        {
            diagnostics.Add(CreateDiagnostic(
                DiagnosticSeverity.Error,
                $"Field 'name' must be {NameMinLength}-{NameMaxLength} characters (found: {normalizedName.Length})",
                skillPath,
                "VAL002"));
        }

        // Check pattern: lowercase Unicode letters, numbers, hyphens only, no leading/trailing/consecutive hyphens
        if (!NamePattern().IsMatch(normalizedName))
        {
            List<string> reasons = [];

            // Check for uppercase letters (including Unicode uppercase)
            if (normalizedName.Any(c => char.IsLetter(c) && char.IsUpper(c)))
                reasons.Add("contains uppercase letters");
            if (normalizedName.StartsWith('-') || normalizedName.EndsWith('-'))
                reasons.Add("starts or ends with hyphen");
            if (normalizedName.Contains("--"))
                reasons.Add("contains consecutive hyphens");
            // Check for invalid characters (anything that's not a letter, digit, or hyphen)
            if (normalizedName.Any(c => !char.IsLetterOrDigit(c) && c != '-'))
                reasons.Add("contains invalid characters");

            var reasonText = reasons.Count > 0 ? $" ({string.Join(", ", reasons)})" : "";

            diagnostics.Add(CreateDiagnostic(
                DiagnosticSeverity.Error,
                $"Field 'name' must contain only lowercase letters (any Unicode script), numbers, and hyphens; cannot start/end with hyphen or have consecutive hyphens{reasonText}",
                skillPath,
                "VAL003"));
        }
    }

    private void ValidateDescription(string description, string skillPath, List<SkillDiagnostic> diagnostics)
    {
        // Check if description is null or whitespace
        if (string.IsNullOrWhiteSpace(description))
        {
            diagnostics.Add(CreateDiagnostic(
                DiagnosticSeverity.Error,
                "Required field 'description' is missing or empty",
                skillPath,
                "VAL004"));
            return;
        }

        // Check length constraints
        if (description.Length < DescriptionMinLength || description.Length > DescriptionMaxLength)
        {
            diagnostics.Add(CreateDiagnostic(
                DiagnosticSeverity.Error,
                $"Field 'description' must be {DescriptionMinLength}-{DescriptionMaxLength} characters (found: {description.Length})",
                skillPath,
                "VAL005"));
        }

        // Warning if description is too short (recommended to be descriptive)
        if (description.Length < 20)
        {
            diagnostics.Add(CreateDiagnostic(
                DiagnosticSeverity.Warning,
                "Field 'description' is very short; consider adding more detail about when to use this skill",
                skillPath,
                "VAL006"));
        }
    }

    private void ValidateVersion(string version, string skillPath, List<SkillDiagnostic> diagnostics)
    {
        // Version is optional but should not be empty if provided
        if (string.IsNullOrWhiteSpace(version))
        {
            diagnostics.Add(CreateDiagnostic(
                DiagnosticSeverity.Warning,
                "Field 'version' is present but empty; consider removing it or providing a value",
                skillPath,
                "VAL007"));
        }
    }

    private void ValidateCompatibility(string compatibility, string skillPath, List<SkillDiagnostic> diagnostics)
    {
        if (compatibility.Length > CompatibilityMaxLength)
        {
            diagnostics.Add(CreateDiagnostic(
                DiagnosticSeverity.Error,
                $"Field 'compatibility' must not exceed {CompatibilityMaxLength} characters (found: {compatibility.Length})",
                skillPath,
                "VAL008"));
        }
    }

    private void ValidateDirectoryName(string skillPath, string skillName, List<SkillDiagnostic> diagnostics)
    {
        var directoryName = Path.GetFileName(skillPath);

        if (string.IsNullOrEmpty(directoryName))
        {
            // If we can't determine directory name, issue a warning
            diagnostics.Add(CreateDiagnostic(
                DiagnosticSeverity.Warning,
                "Cannot determine directory name to validate against skill name",
                skillPath,
                "VAL009"));
            return;
        }

        // Apply NFKC Unicode normalization to both directory and skill names for consistent comparison
        var normalizedDirectoryName = directoryName.Normalize(NormalizationForm.FormKC);
        var normalizedSkillName = skillName.Normalize(NormalizationForm.FormKC);

        if (!string.Equals(normalizedDirectoryName, normalizedSkillName, StringComparison.Ordinal))
        {
            diagnostics.Add(CreateDiagnostic(
                DiagnosticSeverity.Error,
                $"Directory name '{directoryName}' does not match skill name '{skillName}'",
                skillPath,
                "VAL010"));
        }
    }

    /// <summary>
    /// Helper method to create a SkillDiagnostic with consistent formatting.
    /// </summary>
    private static SkillDiagnostic CreateDiagnostic(
        DiagnosticSeverity severity,
        string message,
        string? path,
        string code) =>
        new()
        {
            Severity = severity,
            Message = message,
            Path = path,
            Code = code
        };
}
