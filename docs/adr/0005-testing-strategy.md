# ADR 0005: Testing Strategy

**Status:** Accepted

**Date:** 2024-12-31

**Context:**

We need a comprehensive testing strategy that provides confidence for contributors and catches regressions early. The system must be testable at multiple levels (unit, integration, performance) and ensure that both the core functionality and edge cases are covered.

**Decision:**

We adopt a multi-layered testing approach with the following components:

## 1. Test Fixtures

Location: `fixtures/skills/`

We maintain a comprehensive set of test fixtures covering:

### Valid Skills
- `example-skill` - Complete skill with all optional fields
- `minimal-skill` - Minimal skill with only required fields
- `complete-skill` - Skill with all metadata and resource directories (scripts/, references/, assets/)
- `large-instructions-skill` - Skill with extensive instructions for performance testing
- `special-chars-skill` - Skill testing special characters and unicode

### Invalid Skills
- `empty-name-field` - Empty name field
- `invalid-no-name` - Missing name field
- `invalid-uppercase-name` - Uppercase characters in name
- `invalid-consecutive-hyphens` - Consecutive hyphens in name
- `invalid-long-name` - Name exceeding 64 characters
- `invalid-long-description` - Description exceeding 1024 characters
- `malformed-yaml` - Invalid YAML syntax
- `unclosed-frontmatter` - Missing closing frontmatter delimiter
- `no-frontmatter` - No YAML frontmatter
- `triple-dash-in-body` - Triple dash in markdown body
- `mismatched-directory` - Directory name doesn't match skill name

## 2. Golden File Testing

Location: `tests/AgentSkills.Tests/GoldenFiles/`

We use golden file testing for prompt rendering to ensure output stability:

- Golden files are text files containing expected rendered output
- Tests compare actual output against golden files
- Set `UPDATE_GOLDEN_FILES=1` environment variable to regenerate golden files
- Line endings are normalized for cross-platform compatibility

Golden file tests cover:
- Skill list rendering (single and multiple skills)
- Skill detail rendering (with various options)
- Resource policy application
- Custom rendering options

## 3. Performance Sanity Tests

Location: `tests/AgentSkills.Tests/PerformanceTests.cs`

Performance tests provide sanity checks (not strict benchmarks):

- Metadata loading should complete in < 1 second
- Full skill loading should complete in < 5 seconds
- Validation should complete in < 500ms
- Rendering should complete in < 100ms
- Memory usage should remain reasonable (< 50MB for fixture skills)
- Scaling should be approximately O(n) with number of skills

These are loose bounds designed to catch major performance regressions, not to enforce strict SLAs.

## 4. Regression Tests

Location: `tests/AgentSkills.Tests/RegressionTests.cs`

Regression tests document previously fixed issues and edge cases:

- YAML parsing edge cases (malformed, unclosed, no frontmatter)
- Validation edge cases (empty fields, invalid patterns, length violations)
- Progressive disclosure invariants (metadata doesn't load full content)
- Special character handling (quotes, unicode, symbols)
- Resource discovery (resources exist but aren't auto-loaded)
- Diagnostic quality (all diagnostics include paths and helpful messages)

## 5. Test Organization

Tests are organized by concern:
- Unit tests in files named `*Tests.cs` for specific components
- Integration tests in `IntegrationTests.cs` and `ProgressiveDisclosureIntegrationTests.cs`
- Golden file tests in `GoldenFileTests.cs`
- Performance tests in `PerformanceTests.cs`
- Regression tests in `RegressionTests.cs`

## 6. Testing Principles

1. **Use Fixtures, Not Ad-Hoc Data**: All tests use fixtures from `fixtures/skills/` directory
2. **Test Diagnostics, Not Exceptions**: Normal validation failures produce diagnostics
3. **Golden Files for Rendering**: Prompt rendering uses golden files to catch unintended changes
4. **Performance Sanity Checks**: Loose performance bounds catch regressions without brittleness
5. **Document Regressions**: When a bug is fixed, add a regression test documenting the issue

## 7. Walking Skeleton

The sample app in `samples/AgentSkills.Sample/` must always demonstrate the complete workflow:
scan → metadata → validate → render list → activate skill → render instructions

If this breaks, it indicates a fundamental issue with the system.

**Consequences:**

### Positive
- High confidence in changes through comprehensive test coverage
- Early detection of regressions through dedicated regression tests
- Stable rendering output through golden file testing
- Performance awareness through sanity tests
- Clear test organization makes it easy to add new tests
- Fixtures provide realistic test data

### Negative
- Golden files require manual updates when rendering intentionally changes
- Performance tests may occasionally fail on slow/busy CI systems
- More test fixtures to maintain

### Mitigation
- Golden files can be regenerated with `UPDATE_GOLDEN_FILES=1`
- Performance tests use generous bounds (not strict benchmarks)
- Fixture maintenance is minimal - add new fixtures only when discovering new edge cases

**Notes:**

- This testing strategy supports the project goals of correctness and safety
- Tests follow the principle: "Make it work, make it right, make it fast"
- All tests must pass before merging (enforced by CI)
- When adding new features, add corresponding tests in the appropriate category
