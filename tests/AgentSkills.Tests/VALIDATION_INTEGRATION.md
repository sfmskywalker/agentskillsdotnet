# Validation Integration Tests

This directory contains integration tests for validation scenarios.

## CI Integration Example

The `SkillValidator` produces diagnostics that are suitable for CI/CD pipelines. Each diagnostic includes:

- **Code**: A unique identifier (e.g., `VAL001`, `VAL002`)
- **Message**: A clear, actionable error message
- **Path**: The file or directory path where the issue was found
- **Severity**: Error, Warning, or Info

### Example CI Output Format

```csharp
var validator = new SkillValidator();
var result = validator.ValidateMetadata(metadata);

if (!result.IsValid)
{
    foreach (var error in result.Errors)
    {
        // Safe plain text format (works with all CI systems)
        Console.Error.WriteLine($"ERROR [{error.Code}] {error.Path}: {error.Message}");
    }
    Environment.Exit(1);
}

foreach (var warning in result.Warnings)
{
    Console.WriteLine($"WARNING [{warning.Code}] {warning.Path}: {warning.Message}");
}
```

**Security Note:**
Paths and messages from skill files are untrusted input. If you need to use GitHub Actions workflow commands (`::error`, `::warning`), you must properly escape all user-controlled values to prevent workflow command injection. The plain text format shown above is recommended for all CI systems as it avoids this security risk.

### Running Validation in CI

```bash
# Example: Validate all skills in a directory
dotnet run --project tools/SkillValidator -- /path/to/skills

# Exit code 0 = valid
# Exit code 1 = invalid (errors found)
```

### Diagnostic Codes

| Code | Severity | Description |
|------|----------|-------------|
| VAL001 | Error | Required field 'name' is missing or empty |
| VAL002 | Error | Field 'name' length constraint violation (1-64 characters) |
| VAL003 | Error | Field 'name' pattern violation (lowercase, numbers, hyphens only) |
| VAL004 | Error | Required field 'description' is missing or empty |
| VAL005 | Error | Field 'description' length constraint violation (1-1024 characters) |
| VAL006 | Warning | Field 'description' is very short (less than 20 characters) |
| VAL007 | Warning | Field 'version' is present but empty |
| VAL008 | Error | Field 'compatibility' exceeds maximum length (500 characters) |
| VAL009 | Warning | Cannot determine directory name to validate |
| VAL010 | Error | Directory name does not match skill name |

All diagnostics include the file path for easy navigation and integration with CI tools.
