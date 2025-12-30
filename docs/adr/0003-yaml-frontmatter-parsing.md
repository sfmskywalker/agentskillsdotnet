# ADR 0003: YAML Frontmatter Parsing Strategy

## Status
Accepted

## Context
Epic 4 requires parsing SKILL.md files that contain YAML frontmatter and Markdown body content. The parser must:
- Extract and parse YAML frontmatter into `SkillManifest` models
- Preserve Markdown body content verbatim as instructions
- Handle malformed or absent frontmatter gracefully with diagnostics
- Support progressive disclosure (metadata-only vs full content loading)
- Be robust against edge cases and provide friendly error messages

The parsing strategy must balance correctness, performance, and developer experience.

## Decision

### Parsing Library Choice
We use **YamlDotNet 16.2.0** for YAML parsing because:
- Well-maintained, popular library (38M+ downloads)
- Good error messages for malformed YAML
- Supports dynamic object parsing for extensibility
- MIT license (compatible with our licensing)
- Mature and stable API

### File Format
SKILL.md files follow this structure:
```markdown
---
name: skill-name
description: A description
# optional fields...
---

# Markdown body content
Everything after the closing --- is preserved verbatim.
```

### Parsing Strategy

#### Full Skill Loading
1. Read entire file content as string
2. Split by `---` delimiter
3. If parts.Length < 3, emit `LOADER004` error (frontmatter not found)
4. Extract frontmatter (part 1) and body (parts 2+, joined by `---`)
5. Parse YAML into dictionary, then map to `SkillManifest`
6. Preserve body content (after trimming leading/trailing whitespace, including any `---` that appear in markdown)

#### Metadata-Only Loading (Fast Path)
1. Open file as stream, read line-by-line
2. Check first line is `---`
3. Read lines until closing `---` or EOF
4. If closing `---` found, parse accumulated frontmatter
5. If EOF reached, return null (unclosed frontmatter)
6. Never load markdown body into memory

This approach ensures metadata-only scans are fast and low-memory.

### Required vs Optional Fields
- **Required**: `name` and `description` (non-empty strings)
- **Optional**: `version`, `author`, `tags`, `allowed-tools`
- **Extensibility**: All unknown fields preserved in `AdditionalFields` dictionary

### YAML Parsing Configuration
- **Naming Convention**: HyphenatedNamingConvention (matches YAML style: `allowed-tools`)
- **Unknown Properties**: Ignored by YamlDotNet, manually extracted into `AdditionalFields`
- **Type Flexibility**: Lists can be single values or arrays (YAML convention)

### Error Handling Philosophy
**Diagnostics, not exceptions:**
- YAML parse errors → `LOADER005` diagnostic
- Missing frontmatter → `LOADER004` diagnostic
- Missing required fields → `LOADER006`/`LOADER007` diagnostics
- I/O errors → `LOADER003` diagnostic (exception case)

**Non-destructive loading:**
- Invalid skills do not prevent loading valid skills
- All diagnostics are collected and returned
- Consumer decides if errors are fatal

### Edge Cases Handled

| Case | Behavior | Diagnostic |
|------|----------|------------|
| No SKILL.md file | Return null, emit diagnostic | LOADER002 |
| No frontmatter at all | Return null, emit diagnostic | LOADER004 |
| Unclosed frontmatter (missing closing `---`) | Treated as malformed YAML, emit diagnostic | LOADER005 |
| Malformed YAML syntax | YamlDotNet error captured, emit diagnostic | LOADER005 |
| Missing `name` field | Return null, emit diagnostic | LOADER006 |
| Empty `name` value | Return null, emit diagnostic | LOADER006 |
| Missing `description` field | Return null, emit diagnostic | LOADER007 |
| Empty `description` value | Return null, emit diagnostic | LOADER007 |
| `---` in markdown body | Preserved verbatim (only first 2 delimiters are special) | None |
| Unknown YAML fields | Preserved in `AdditionalFields` | None |

### Body Preservation
The markdown body is preserved exactly as written:
- No trimming beyond initial `---` split
- Horizontal rules (`---`) in markdown are preserved
- No markdown parsing or processing
- Instructions are stored as raw string

## Consequences

### Positive
- Simple, predictable parsing behavior
- Friendly error messages with diagnostic codes
- Fast metadata-only path for skill discovery
- Extensible via `AdditionalFields`
- Robust against common edge cases
- Well-tested (62 test cases covering edge cases)
- No reinventing YAML parsing

### Negative
- Dependency on external YAML library
- Less control over YAML error messages
- `AdditionalFields` is dynamically typed
- Triple-dash split could be confused by malformed files

### Mitigations
- YamlDotNet is stable, popular, and well-maintained
- Error messages are wrapped with diagnostic codes for clarity
- `AdditionalFields` is optional and for extensibility only
- Comprehensive tests validate edge case handling
- Documentation explains expected format

## Alternatives Considered

### Alternative 1: Custom YAML Parser
Write a custom parser for YAML frontmatter.

**Rejected because:**
- Reinventing the wheel
- More bugs, less maintainable
- Poor error messages
- No time savings vs library

### Alternative 2: Regex-Based Extraction
Use regex to extract frontmatter, parse YAML separately.

**Rejected because:**
- Regex for frontmatter is fragile
- Harder to handle edge cases
- No benefit over current approach

### Alternative 3: Streaming YAML Parser for Full Load
Use streaming parser for both metadata and full loads.

**Rejected because:**
- More complex implementation
- No performance benefit for full load
- Harder to preserve body verbatim
- String split is simpler and predictable

### Alternative 4: Throw Exceptions for Validation
Throw exceptions instead of returning diagnostics.

**Rejected because:**
- Violates project invariants
- Can't collect multiple errors
- Poor user experience
- Exceptions should be for exceptional cases

## References
- docs/project_brief.md - Architecture and invariants
- AGENTS.md - Working agreements
- ADR 0001 - Project structure
- ADR 0002 - Core domain models design
- Issue: Epic 4 - Parsing SKILL.md (frontmatter + body)
- [YamlDotNet Documentation](https://github.com/aaubry/YamlDotNet)
