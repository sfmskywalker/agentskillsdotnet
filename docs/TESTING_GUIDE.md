# Testing Guide

This guide explains the testing strategy and how to work with tests in AgentSkills.NET.

## Overview

AgentSkills.NET uses a comprehensive, multi-layered testing approach:

1. **Unit Tests** - Test individual components in isolation
2. **Integration Tests** - Test complete workflows end-to-end
3. **Golden File Tests** - Ensure rendering output remains stable
4. **Performance Tests** - Catch major performance regressions
5. **Regression Tests** - Document and prevent previously fixed issues

## Running Tests

### Run All Tests
```bash
dotnet test
```

### Run Specific Test Suite
```bash
# Run only unit tests for a specific component
dotnet test --filter "FullyQualifiedName~SkillValidatorTests"

# Run only integration tests
dotnet test --filter "FullyQualifiedName~IntegrationTests"

# Run only performance tests
dotnet test --filter "FullyQualifiedName~PerformanceTests"

# Run only golden file tests
dotnet test --filter "FullyQualifiedName~GoldenFileTests"

# Run only regression tests
dotnet test --filter "FullyQualifiedName~RegressionTests"
```

### Run Tests with Detailed Output
```bash
dotnet test --logger "console;verbosity=detailed"
```

## Test Fixtures

All tests use fixtures from `fixtures/skills/` directory. This ensures:
- Consistent test data across all tests
- Realistic test scenarios
- Easy reproduction of issues

### Valid Fixtures
- `example-skill` - Complete skill demonstrating all features
- `minimal-skill` - Minimal valid skill (only required fields)
- `complete-skill` - Skill with all metadata fields and resources
- `large-instructions-skill` - Skill with extensive instructions
- `special-chars-skill` - Tests special characters and unicode

### Invalid Fixtures
- `empty-name-field` - Empty name field
- `invalid-no-name` - Missing name field
- `invalid-uppercase-name` - Uppercase in name
- `invalid-consecutive-hyphens` - Consecutive hyphens
- `invalid-long-name` - Name > 64 characters
- `invalid-long-description` - Description > 1024 characters
- `malformed-yaml` - Invalid YAML syntax
- `unclosed-frontmatter` - Missing closing delimiter
- `no-frontmatter` - No YAML frontmatter
- `triple-dash-in-body` - Triple dash in markdown
- `mismatched-directory` - Directory name mismatch

### Adding New Fixtures

When you discover a new edge case:

1. Add a new fixture to `fixtures/skills/`
2. Follow the naming convention: `descriptive-name-skill`
3. Add tests that use the new fixture
4. Document the fixture purpose in its `SKILL.md`

## Golden File Tests

Golden file tests ensure that rendered output remains stable.

### Understanding Golden Files

Golden files are located in `tests/AgentSkills.Tests/GoldenFiles/` and contain the expected output of rendering operations.

When a golden file test runs:
1. It renders a skill or skill list
2. Compares the output to the golden file
3. Fails if they don't match

### Updating Golden Files

When you intentionally change rendering logic:

```bash
# Regenerate all golden files
UPDATE_GOLDEN_FILES=1 dotnet test --filter "FullyQualifiedName~GoldenFileTests"

# Verify the changes look correct
git diff tests/AgentSkills.Tests/GoldenFiles/

# Commit the updated golden files
git add tests/AgentSkills.Tests/GoldenFiles/
git commit -m "Update golden files for new rendering"
```

### Creating New Golden File Tests

```csharp
[Fact]
public void RenderNewFeature_MatchesGoldenFile()
{
    // Arrange
    var skill = LoadTestSkill();

    // Act
    var rendered = _renderer.RenderWithNewFeature(skill);

    // Assert
    AssertMatchesGoldenFile("new-feature.txt", rendered);
}
```

The golden file will be created automatically the first time the test runs.

## Performance Tests

Performance tests are sanity checks, not strict benchmarks.

### Purpose

- Catch major performance regressions
- Ensure O(n) scaling with number of skills
- Verify memory usage stays reasonable

### Guidelines

Performance tests use generous time bounds:
- Metadata loading: < 1 second
- Full skill loading: < 5 seconds
- Validation: < 500ms
- Rendering: < 100ms

These are intentionally loose to avoid flaky tests on slow CI systems.

### When Performance Tests Fail

1. Run locally to verify it's not a CI flake
2. Use profiling tools to identify bottlenecks
3. Fix performance issue or adjust bounds if needed
4. Document why bounds were changed in commit message

## Regression Tests

Regression tests document previously fixed bugs.

### Purpose

- Prevent regressions
- Document known issues and their fixes
- Serve as living documentation

### Adding Regression Tests

When you fix a bug:

1. Add a regression test that would have caught it
2. Name the test descriptively: `ComponentName_SpecificIssue_ExpectedBehavior`
3. Add a comment linking to the issue or PR
4. Group related regression tests together

Example:
```csharp
[Fact]
public void SkillLoader_MalformedYAML_ProducesDiagnosticNotException()
{
    // Regression: Issue #123 - Malformed YAML caused exceptions
    // Now should produce diagnostic instead
    var (skill, diagnostics) = _loader.LoadSkill(malformedPath);
    
    Assert.NotEmpty(diagnostics);
    Assert.Contains(diagnostics, d => d.Severity == DiagnosticSeverity.Error);
}
```

## Integration Tests

Integration tests verify complete workflows.

### Progressive Disclosure Integration

`ProgressiveDisclosureIntegrationTests.cs` tests the complete workflow:
1. Load metadata (fast path)
2. Validate metadata
3. Render skill list
4. Load full skill
5. Validate full skill
6. Render skill details

### When to Add Integration Tests

Add integration tests when:
- Adding a new workflow or pipeline
- Multiple components interact in a specific way
- End-to-end behavior needs verification

## Writing Good Tests

### Do's

✅ Use fixtures from `fixtures/skills/`
✅ Test diagnostics, not exceptions
✅ Use descriptive test names
✅ Test one thing per test
✅ Add assertions with helpful messages
✅ Group related tests in the same test class

### Don'ts

❌ Don't use ad-hoc test data
❌ Don't test private implementation details
❌ Don't write flaky tests
❌ Don't skip tests (fix or remove them)
❌ Don't test multiple unrelated things

### Test Naming Convention

```
ComponentName_Scenario_ExpectedBehavior
```

Examples:
- `SkillValidator_EmptyName_FailsWithVAL001`
- `SkillLoader_ValidSkill_ReturnsNoDiagnostics`
- `PromptRenderer_WithOptions_AppliesOptions`

## Test Coverage

While we don't enforce a specific coverage percentage, aim to cover:

- ✅ All public APIs
- ✅ Error cases and edge cases
- ✅ Validation rules
- ✅ Integration workflows
- ✅ Known regressions

Coverage is a tool, not a goal. Focus on meaningful tests.

## Continuous Integration

All tests run on every PR:
- Tests must pass before merging
- Golden file tests catch unintended rendering changes
- Performance tests catch major regressions
- Regression tests prevent reintroducing bugs

## Troubleshooting

### Test Fails Locally But Passes in CI

- Check for file path differences (Windows vs Linux)
- Verify line ending normalization
- Ensure fixtures are committed to git

### Golden File Test Fails

1. Check if rendering intentionally changed
2. Review the diff: `git diff tests/AgentSkills.Tests/GoldenFiles/`
3. If correct: `UPDATE_GOLDEN_FILES=1 dotnet test --filter GoldenFileTests`
4. Commit updated golden files

### Performance Test Fails

1. Run locally with profiler
2. Check if system was under load
3. Verify bounds are reasonable
4. Adjust or fix as appropriate

### Test Is Flaky

Flaky tests are worse than no tests. Either:
- Fix the root cause (timing, dependencies, etc.)
- Remove the test if it can't be fixed

## Resources

- [ADR 0005: Testing Strategy](../docs/adr/0005-testing-strategy.md)
- [AGENTS.md](../AGENTS.md) - Testing requirements for agents
- [Xunit Documentation](https://xunit.net/)

## Questions?

If you're unsure about testing:
1. Look at existing tests for patterns
2. Check the ADRs for design decisions
3. Ask in PR comments or discussions
