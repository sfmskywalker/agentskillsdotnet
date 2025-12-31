# Test Fixtures & Example Skills

This directory contains skill fixtures used for testing AgentSkills.NET and example skills demonstrating common patterns.

## Purpose

These fixtures provide consistent, realistic test data for:
- Unit tests
- Integration tests
- Golden file tests
- Performance tests
- Regression tests
- Documentation and examples

## Valid Example Skills

### example-skill
A complete skill demonstrating all features of the skill specification.

**Contains:**
- All optional metadata fields (version, author, tags, allowed-tools)
- Complete instructions in markdown
- Demonstrates proper structure

**Use for:** Integration tests, rendering tests, example documentation

### minimal-skill
A minimal valid skill with only the required fields.

**Contains:**
- Required fields only (name, description)
- Minimal instructions

**Use for:** Testing minimal valid cases, baseline functionality

### complete-skill
A comprehensive skill with all metadata fields and resource directories.

**Contains:**
- All optional metadata fields
- Resource directories: `scripts/`, `references/`, `assets/`
- Example files in each resource directory

**Use for:** Resource discovery tests, complete workflow tests

### email-sender
A real-world example of an email composition skill.

**Contains:**
- Professional skill structure
- Detailed instructions with examples
- Error handling guidance
- Best practices and templates
- Security considerations

**Use for:** Example of production-quality skill, documentation reference

**Demonstrates:**
- Multi-step workflow instructions
- Input validation guidance
- Error handling patterns
- Professional formatting
- Template examples

### code-reviewer
A systematic code review workflow skill.

**Contains:**
- Phased review process
- Quality, logic, testing, security, and documentation checks
- Structured feedback templates
- Best practices and checklists

**Use for:** Example of workflow-based skill, documentation reference

**Demonstrates:**
- Complex multi-phase workflows
- Structured checklists
- Feedback templates with severity levels
- Comprehensive instructions

### large-instructions-skill
A skill with extensive instructions for performance testing.

**Contains:**
- Standard metadata
- Large markdown instructions (~3KB)
- Multiple sections and subsections

**Use for:** Performance tests, rendering performance tests

### special-chars-skill
A skill testing special character and unicode handling.

**Contains:**
- Special characters in description (quotes, symbols)
- Unicode characters in instructions
- Markdown special characters
- Code blocks with special characters

**Use for:** Parser robustness tests, character encoding tests

## Invalid Fixtures

These fixtures test various validation and parsing error cases.

### empty-name-field
Tests empty string in the required `name` field.

**Expected:** Validation error VAL001

### invalid-no-name
Tests missing `name` field entirely.

**Expected:** Validation error VAL001 or parsing error

### invalid-uppercase-name
Tests uppercase characters in the `name` field.

**Contains:** Name with uppercase letters

**Expected:** Validation error VAL003 (pattern violation)

### invalid-consecutive-hyphens
Tests consecutive hyphens in the `name` field.

**Contains:** Name like `invalid--consecutive`

**Expected:** Validation error VAL003 (pattern violation)

### invalid-long-name
Tests `name` field exceeding maximum length.

**Contains:** Name with 87 characters (limit is 64)

**Expected:** Validation error VAL002 (length violation)

### invalid-long-description
Tests `description` field exceeding maximum length.

**Contains:** Description with 1100+ characters (limit is 1024)

**Expected:** Validation error VAL005 (length violation)

### malformed-yaml
Tests invalid YAML syntax in frontmatter.

**Contains:** Malformed YAML (syntax errors)

**Expected:** Parsing error diagnostic (LOADER series)

### unclosed-frontmatter
Tests missing closing `---` delimiter for frontmatter.

**Contains:** Opening `---` but no closing delimiter

**Expected:** Parsing error diagnostic

### no-frontmatter
Tests skill without any YAML frontmatter.

**Contains:** Plain markdown with no frontmatter

**Expected:** Parsing error diagnostic

### triple-dash-in-body
Tests that triple dash `---` in markdown body doesn't confuse parser.

**Contains:** Valid frontmatter with `---` in markdown body

**Expected:** Should parse successfully (regression test)

### mismatched-directory
Tests directory name not matching skill name.

**Contains:** Directory named `mismatched-directory` but skill name is different

**Expected:** Validation warning/error VAL010

## Adding New Fixtures

When adding a new fixture:

1. Create a directory under `fixtures/skills/`
2. Add a `SKILL.md` file
3. Document the fixture purpose in this README
4. Update tests to use the new fixture
5. Commit both fixture and tests together

## Fixture Guidelines

- **Keep fixtures focused**: Each fixture tests one specific scenario
- **Use realistic data**: Fixtures should resemble real-world skills
- **Document purpose**: Clearly state what each fixture tests
- **Keep it simple**: Don't over-complicate fixtures with unnecessary details
- **Version control**: Commit all fixture files (including resources)

## Testing with Fixtures

```csharp
// Example: Using fixtures in tests
public class MyTests
{
    private readonly string _fixturesPath;

    public MyTests()
    {
        var solutionRoot = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..")
        );
        _fixturesPath = Path.Combine(solutionRoot, "fixtures", "skills");
    }

    [Fact]
    public void LoadSkill_ExampleSkill_Succeeds()
    {
        var skillPath = Path.Combine(_fixturesPath, "example-skill");
        var (skill, diagnostics) = _loader.LoadSkill(skillPath);
        
        Assert.NotNull(skill);
        Assert.Empty(diagnostics);
    }
}
```

## Resources

- [Testing Guide](../docs/TESTING_GUIDE.md) - Complete testing documentation
- [ADR 0005: Testing Strategy](../docs/adr/0005-testing-strategy.md) - Testing strategy ADR
- [Agent Skills Specification](https://agentskills.io/) - Skill format specification
