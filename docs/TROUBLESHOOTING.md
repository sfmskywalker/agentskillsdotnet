# Troubleshooting Guide

Common issues and their solutions when working with AgentSkills.NET.

## Table of Contents

- [Installation & Setup Issues](#installation--setup-issues)
- [Skill Loading Issues](#skill-loading-issues)
- [Validation Errors](#validation-errors)
- [Parsing Errors](#parsing-errors)
- [Performance Issues](#performance-issues)
- [Integration Issues](#integration-issues)
- [Testing Issues](#testing-issues)

## Installation & Setup Issues

### Build fails with "SDK not found"

**Problem:** `dotnet build` fails with SDK version errors

**Solution:**
1. Ensure .NET 8.0 SDK is installed:
   ```bash
   dotnet --version
   ```
2. If not installed, download from [dotnet.microsoft.com](https://dotnet.microsoft.com/download)
3. Verify installation:
   ```bash
   dotnet --list-sdks
   ```

### "Package restore failed"

**Problem:** `dotnet restore` fails or packages can't be downloaded

**Solutions:**
1. Clear NuGet cache:
   ```bash
   dotnet nuget locals all --clear
   ```
2. Restore again:
   ```bash
   dotnet restore --force
   ```
3. Check network connectivity and proxy settings

### Sample app won't run

**Problem:** Sample crashes or doesn't start

**Solutions:**
1. Build the solution first:
   ```bash
   dotnet build
   ```
2. Check for build errors
3. Ensure fixtures directory exists:
   ```bash
   ls fixtures/skills/
   ```

## Skill Loading Issues

### "Directory not found" error (LOADER001)

**Problem:** `LoadSkillSet` or `LoadMetadata` returns LOADER001 diagnostic

**Diagnosis:**
```csharp
var (metadata, diagnostics) = loader.LoadMetadata(path);
foreach (var d in diagnostics)
{
    Console.WriteLine($"{d.Code}: {d.Message}");
    Console.WriteLine($"Path: {d.Path}");
}
```

**Solutions:**
1. Verify path exists:
   ```bash
   ls -la /path/to/skills
   ```
2. Use absolute paths or verify working directory:
   ```csharp
   var absolutePath = Path.GetFullPath("./skills");
   var skillSet = loader.LoadSkillSet(absolutePath);
   ```
3. Check permissions:
   ```bash
   chmod +r /path/to/skills
   ```

### "SKILL.md not found" error (LOADER002)

**Problem:** Directory exists but SKILL.md is missing

**Solutions:**
1. Verify SKILL.md (or skill.md) exists in each skill directory:
   ```bash
   find ./skills -name "SKILL.md" -o -name "skill.md"
   ```
2. Check filename case (must be either `SKILL.md` or `skill.md`)
   - `SKILL.md` (uppercase) is preferred
   - `skill.md` (lowercase) is also accepted as fallback
   - If both exist, `SKILL.md` takes precedence
3. Ensure file is not hidden or has correct extension

### Skill loads but returns null

**Problem:** `LoadSkill` returns null instead of skill object

**Diagnosis:**
```csharp
var (skill, diagnostics) = loader.LoadSkill(path);
if (skill == null)
{
    Console.WriteLine("Skill failed to load:");
    foreach (var d in diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))
    {
        Console.WriteLine($"  {d.Code}: {d.Message}");
    }
}
```

**Common Causes:**
- Missing required fields (name or description)
- Malformed YAML frontmatter
- Missing frontmatter delimiters
- File I/O errors

### "Failed to read skill file" (LOADER003)

**Problem:** I/O error reading SKILL.md

**Solutions:**
1. Check file permissions:
   ```bash
   ls -l /path/to/skill/SKILL.md
   ```
2. Verify file is not locked by another process
3. Check disk space:
   ```bash
   df -h
   ```
4. Verify file is readable:
   ```bash
   cat /path/to/skill/SKILL.md
   ```

## Validation Errors

### VAL001: Required field 'name' is missing or empty

**Problem:** Skill doesn't have a name field or it's empty

**Fix:**
```yaml
---
name: my-skill-name
description: My description
---
```

**Common Mistakes:**
- `name: ""` (empty string)
- `name:` (no value)
- Missing the name field entirely

### VAL002: Field 'name' length constraint violation

**Problem:** Skill name is longer than 64 characters

**Fix:** Shorten the name to 64 characters or less:
```yaml
---
name: this-is-a-short-skill-name
description: My description
---
```

### VAL003: Field 'name' pattern violation

**Problem:** Skill name contains invalid characters

**Common Issues:**
- Uppercase letters: `MySkill` → should be `my-skill`
- Underscores: `my_skill` → should be `my-skill`
- Consecutive hyphens: `my--skill` → should be `my-skill`
- Leading/trailing hyphens: `-my-skill-` → should be `my-skill`
- Spaces: `my skill` → should be `my-skill`

**Fix:**
```yaml
---
name: my-skill-name
description: My description
---
```

**Valid pattern:** Unicode lowercase letters (any script), numbers, and single hyphens only. Supports Chinese, Russian, Arabic, and other Unicode scripts. For scripts with case distinction (like Latin or Cyrillic), only lowercase letters are allowed.

### VAL004: Required field 'description' is missing or empty

**Problem:** Skill doesn't have a description or it's empty

**Fix:**
```yaml
---
name: my-skill
description: A clear, concise description of what this skill does
---
```

### VAL005: Field 'description' exceeds maximum length

**Problem:** Description is longer than 1024 characters

**Fix:** Shorten the description. Details can go in the markdown body:
```yaml
---
name: my-skill
description: Brief description (under 1024 characters)
---

# Detailed Information

More details can go here in the markdown body...
```

### VAL006: Field 'description' is very short (Warning)

**Problem:** Description is less than 20 characters

**Fix:** Provide a more descriptive explanation:
```yaml
# ❌ Too short
description: Email

# ✅ Better
description: Send emails to recipients with attachments and formatting
```

### VAL010: Directory name does not match skill name

**Problem:** Directory name and skill name don't match exactly

**Example:**
- Directory: `my_skill/`
- Skill name in YAML: `my-skill`

**Fix:** Rename the directory to match exactly:
```bash
mv my_skill/ my-skill/
```

**Note:** Match must be exact, case-sensitive.

## Parsing Errors

### LOADER004: YAML frontmatter not found

**Problem:** SKILL.md doesn't have proper YAML frontmatter delimiters

**Fix:** Ensure file starts with `---` and has closing `---`:
```markdown
---
name: my-skill
description: My description
---

# Instructions

...
```

**Common Issues:**
- Missing opening `---`
- Missing closing `---`
- Space before opening `---`
- Using different delimiter (like `===`)

### LOADER005: Failed to parse YAML

**Problem:** YAML syntax error in frontmatter

**Common Causes:**
1. **Improper indentation:**
   ```yaml
   # ❌ Wrong
   tags:
   - tag1
   - tag2
   
   # ✅ Correct
   tags:
     - tag1
     - tag2
   ```

2. **Missing quotes for special characters:**
   ```yaml
   # ❌ Wrong
   description: It's great!
   
   # ✅ Correct
   description: "It's great!"
   ```

3. **Invalid list syntax:**
   ```yaml
   # ❌ Wrong
   tags: tag1, tag2
   
   # ✅ Correct
   tags:
     - tag1
     - tag2
   ```

**Solution:** Validate YAML online at [yamllint.com](http://www.yamllint.com/)

### Unclosed frontmatter error

**Problem:** Opening `---` but no closing `---`

**Fix:** Add closing delimiter:
```markdown
---
name: my-skill
description: My description
---
← This closing delimiter is required

# Instructions
```

### Triple dash (---) in body causes issues

**Solution:** This should work fine. The parser only treats the first two `---` sequences as delimiters. If experiencing issues, this is a bug - please report it.

## Performance Issues

### Metadata loading is slow

**Problem:** `LoadMetadata()` takes more than 1 second

**Diagnosis:**
```csharp
var sw = Stopwatch.StartNew();
var (metadata, _) = loader.LoadMetadata(path);
sw.Stop();
Console.WriteLine($"Loaded {metadata.Count} skills in {sw.ElapsedMilliseconds}ms");
```

**Solutions:**
1. Reduce number of skills
2. Check disk I/O performance:
   ```bash
   dd if=/dev/zero of=test.dat bs=1M count=100
   ```
3. Ensure path is local (not network drive)
4. Profile to find bottleneck

### Full skill loading is very slow

**Problem:** `LoadSkillSet()` takes more than 5 seconds

**Solutions:**
1. Use `LoadMetadata()` for listing instead
2. Load skills on-demand (only when activated)
3. Reduce skill count or instruction size
4. Consider caching loaded skills

### Validation is slow

**Problem:** Validation takes noticeable time

**Solutions:**
1. Validate metadata only for listings:
   ```csharp
   validator.ValidateMetadata(metadata)
   ```
2. Cache validation results
3. Validate asynchronously in background

### Memory usage is high

**Problem:** Application uses too much memory

**Solutions:**
1. Use `LoadMetadata()` instead of `LoadSkillSet()` where possible
2. Don't keep large skill sets in memory
3. Dispose of skill data after use
4. Load skills on-demand

## Integration Issues

### Skills not appearing in LLM responses

**Problem:** Skills are loaded but LLM doesn't use them

**Diagnosis:**
1. Verify skills are rendered in prompt:
   ```csharp
   var prompt = renderer.RenderSkillList(metadata);
   Console.WriteLine(prompt);
   ```
2. Check if prompt is actually sent to LLM
3. Verify LLM receives complete prompt

**Solutions:**
1. Ensure skill descriptions are clear and specific
2. Add relevant tags for discoverability
3. Provide examples in skill instructions
4. Test with explicit skill mention in user query

### LLM doesn't activate skills properly

**Problem:** LLM lists skills but doesn't activate them

**Solutions:**
1. Ensure your activation pattern is clear
2. Register skills as functions (for framework integration)
3. Provide clear activation instructions to LLM
4. Example activation pattern:
   ```
   To use a skill, call the function with the skill name.
   ```

### Resource policy not working

**Problem:** Resources still appear despite policy

**Diagnosis:**
```csharp
var policy = ExcludeAllResourcePolicy.Instance;
var options = new PromptRenderOptions { ResourcePolicy = policy };
var prompt = renderer.RenderSkillDetails(skill, options);

// Check if "allowed-tools" appears in output
if (prompt.Contains("allowed-tools"))
{
    Console.WriteLine("Policy not applied correctly");
}
```

**Solutions:**
1. Verify policy is passed correctly
2. Check if custom policy implements interface correctly
3. Test with built-in policies first

## Testing Issues

### Tests fail after code changes

**Common Causes:**

1. **Golden file tests failing:**
   ```bash
   # Update golden files if rendering intentionally changed
   UPDATE_GOLDEN_FILES=1 dotnet test --filter "GoldenFileTests"
   
   # Review changes
   git diff tests/AgentSkills.Tests/GoldenFiles/
   ```

2. **Validation tests failing:**
   - Check if validation rules changed
   - Update test expectations accordingly
   - Verify fixtures still match test requirements

3. **Path issues:**
   - Ensure fixtures path is correct
   - Use `Path.Combine()` for cross-platform compatibility
   - Verify working directory in test

### Can't run specific test

**Problem:** `dotnet test --filter` doesn't find test

**Solutions:**
```bash
# Use fully qualified name
dotnet test --filter "FullyQualifiedName~SkillValidatorTests"

# Or by class name
dotnet test --filter "FullyQualifiedName~SkillValidator"

# List all tests
dotnet test -t
```

### Test fixtures not found

**Problem:** Tests fail with "File not found" for fixtures

**Solution:** Verify fixtures path calculation:
```csharp
var solutionRoot = Path.GetFullPath(
    Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..")
);
var fixturesPath = Path.Combine(solutionRoot, "fixtures", "skills");

if (!Directory.Exists(fixturesPath))
{
    Console.WriteLine($"Fixtures not found at: {fixturesPath}");
}
```

## Getting More Help

If you're still stuck:

1. **Check the FAQ:** [docs/FAQ.md](FAQ.md)
2. **Review the docs:**
   - [Getting Started](GETTING_STARTED.md)
   - [Public API Reference](PUBLIC_API.md)
   - [Skill Authoring Guide](SKILL_AUTHORING.md)
3. **Search existing issues:** [GitHub Issues](https://github.com/sfmskywalker/agentskillsdotnet/issues)
4. **Ask the community:** [GitHub Discussions](https://github.com/sfmskywalker/agentskillsdotnet/discussions)
5. **Open a new issue** with:
   - Clear description of the problem
   - Steps to reproduce
   - Expected vs actual behavior
   - Version/commit hash
   - Relevant code/configuration

## Debugging Tips

### Enable detailed logging

```csharp
// Add logging to see what's happening
var (skill, diagnostics) = loader.LoadSkill(path);

Console.WriteLine($"Loaded: {skill != null}");
Console.WriteLine($"Diagnostics: {diagnostics.Count}");
foreach (var d in diagnostics)
{
    Console.WriteLine($"  [{d.Severity}] {d.Code}: {d.Message}");
    if (d.Path != null)
        Console.WriteLine($"    Path: {d.Path}");
}
```

### Validate incrementally

```csharp
// Test loading first
var (skill, loadDiagnostics) = loader.LoadSkill(path);
Console.WriteLine($"Load succeeded: {skill != null}");

// Then validate
if (skill != null)
{
    var result = validator.Validate(skill);
    Console.WriteLine($"Validation succeeded: {result.IsValid}");
}
```

### Test with minimal skill

Create the simplest possible skill to isolate the issue:

```markdown
---
name: test-skill
description: Minimal test skill
---

# Test

This is a test.
```

If this works, gradually add complexity to find the problem.

### Use the samples

The sample applications are working examples. Compare your code to them:
- [AgentSkills.Sample](../../samples/AgentSkills.Sample/)
- [AgentSkills.Sample.AgentFramework](../../samples/AgentSkills.Sample.AgentFramework/)

---

**Still need help?** Don't hesitate to ask! The community is here to help. 🤝
