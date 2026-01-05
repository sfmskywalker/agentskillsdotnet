# AgentSkills.NET vs Reference Implementation Comparison

**Date:** 2025-12-31  
**Reference Implementation Version:** skills-ref (main branch)  
**Purpose:** Compare the .NET port against the official Python reference implementation to ensure feature parity and identify gaps.

---

## Executive Summary

The .NET implementation is **largely feature-complete** with the reference implementation, with some intentional design differences for .NET idioms and some minor gaps. The core functionality—parsing, validation, and prompt rendering—is well-aligned with the specification.

### Key Findings
- ✅ Core parsing and validation logic is equivalent
- ✅ Progressive disclosure is implemented correctly
- ⚠️ Some validation rules differ slightly (internationalization, field validation)
- ⚠️ Prompt rendering uses Markdown instead of XML format
- ⚠️ Missing some edge case handling (lowercase skill.md, NFKC normalization)
- ℹ️ .NET implementation has additional features (SkillSet, diagnostics model, resource policies)

---

## 1. Architecture Comparison

### Reference Implementation (Python)

**Structure:**
```
skills-ref/
├── src/skills_ref/
│   ├── __init__.py
│   ├── errors.py          # Exception hierarchy
│   ├── models.py          # SkillProperties dataclass
│   ├── parser.py          # YAML parsing & property extraction
│   ├── validator.py       # Validation logic
│   ├── prompt.py          # XML prompt generation
│   └── cli.py             # CLI commands
└── tests/
```

**Key Design Patterns:**
- Single `SkillProperties` dataclass for all metadata
- Raises exceptions for errors (ParseError, ValidationError)
- Returns list of error strings from validation
- Uses StrictYAML for parsing
- Generates `<available_skills>` XML for Claude

### .NET Implementation

**Structure:**
```
src/
├── AgentSkills/
│   ├── Skill.cs
│   ├── SkillMetadata.cs
│   ├── SkillManifest.cs
│   ├── SkillDiagnostic.cs
│   └── ValidationResult.cs
├── AgentSkills.Loader/
│   ├── ISkillLoader.cs
│   └── FileSystemSkillLoader.cs
├── AgentSkills.Validation/
│   ├── ISkillValidator.cs
│   └── SkillValidator.cs
└── AgentSkills.Prompts/
    ├── ISkillPromptRenderer.cs
    └── DefaultSkillPromptRenderer.cs
```

**Key Design Patterns:**
- Separation: `SkillMetadata` (fast scan) vs `Skill` (full load)
- Returns diagnostics instead of throwing exceptions
- Uses YamlDotNet for parsing
- Interface-based abstractions (ISkillLoader, ISkillValidator, ISkillPromptRenderer)
- Generates Markdown prompts (not XML)

**Differences:**
- ✅ **Better:** .NET has explicit separation of metadata vs full skill for progressive disclosure
- ✅ **Better:** .NET uses diagnostics pattern (severity levels: Error/Warning/Info)
- ⚠️ **Different:** .NET is more modular with interfaces
- ⚠️ **Different:** Prompt format differs (Markdown vs XML)

---

## 2. Data Models Comparison

### Reference Implementation: SkillProperties

```python
@dataclass
class SkillProperties:
    name: str                              # Required
    description: str                       # Required
    license: Optional[str] = None
    compatibility: Optional[str] = None
    allowed_tools: Optional[str] = None    # Stored as string
    metadata: dict[str, str] = field(default_factory=dict)
```

### .NET Implementation: SkillManifest + SkillMetadata

**SkillManifest** (full frontmatter):
```csharp
public sealed class SkillManifest {
    public required string Name { get; init; }
    public required string Description { get; init; }
    public string? Version { get; init; }          // Extra field
    public string? Author { get; init; }           // Extra field
    public IReadOnlyList<string> Tags { get; init; }  // Extra field
    public IReadOnlyList<string> AllowedTools { get; init; }  // Parsed as list
    public IReadOnlyDictionary<string, object?> AdditionalFields { get; init; }
}
```

**SkillMetadata** (fast scan):
```csharp
public sealed class SkillMetadata {
    public required string Name { get; init; }
    public required string Description { get; init; }
    public string? Version { get; init; }
    public string? Author { get; init; }
    public IReadOnlyList<string> Tags { get; init; }
    public required string Path { get; init; }
}
```

**Differences:**
- ⚠️ **Gap:** Reference stores `metadata` as dict, .NET stores extra fields in `AdditionalFields`
- ⚠️ **Gap:** Reference stores `allowed-tools` as string, .NET parses it as list
- ⚠️ **Extra:** .NET has Version/Author/Tags as first-class fields (not in spec)
- ⚠️ **Different:** Reference has `license` field, .NET stores it in AdditionalFields
- ⚠️ **Different:** Reference has `compatibility` field, .NET stores it in AdditionalFields

**Spec Alignment:**
- Reference implementation follows spec exactly (only name, description, license, compatibility, allowed-tools, metadata)
- .NET implementation adds Version/Author/Tags which are NOT in the official spec

---

## 3. Parsing Comparison

### Reference Implementation (parser.py)

**Key Features:**
- `find_skill_md()`: Prefers SKILL.md (uppercase), accepts skill.md (lowercase)
- `parse_frontmatter()`: Splits on "---", validates YAML
- Uses StrictYAML for type safety
- Raises ParseError for malformed files
- Raises ValidationError if name/description missing
- Converts metadata dict values to strings

### .NET Implementation (FileSystemSkillLoader.cs)

**Key Features:**
- Only looks for "SKILL.md" (uppercase)
- `ExtractFrontmatter()`: Reads line-by-line to avoid loading full file for metadata
- Uses YamlDotNet with hyphenated naming convention
- Returns diagnostics for parse errors (doesn't throw)
- Handles extra fields via `AdditionalFields` dictionary

**Differences:**
- ❌ **Gap:** .NET does NOT accept lowercase "skill.md"
- ❌ **Gap:** .NET does NOT normalize field names to handle `metadata` specially
- ✅ **Better:** .NET has optimized metadata-only loading (streaming)
- ⚠️ **Different:** Error handling (diagnostics vs exceptions)

---

## 4. Validation Comparison

### Reference Implementation (validator.py)

**Constants:**
```python
MAX_SKILL_NAME_LENGTH = 64
MAX_DESCRIPTION_LENGTH = 1024
MAX_COMPATIBILITY_LENGTH = 500
ALLOWED_FIELDS = {
    "name", "description", "license", 
    "allowed-tools", "metadata", "compatibility"
}
```

**Name Validation:**
- Unicode normalization (NFKC)
- Lowercase check (supports Unicode characters)
- Length: 1-64 characters
- Cannot start/end with hyphen
- No consecutive hyphens
- Only alphanumeric + hyphens
- Directory name must match

**Field Validation:**
- Checks for unexpected fields
- Description: 1-1024 characters
- Compatibility: max 500 characters

### .NET Implementation (SkillValidator.cs)

**Constants:**
```csharp
const int NameMinLength = 1;
const int NameMaxLength = 64;
const int DescriptionMinLength = 1;
const int DescriptionMaxLength = 1024;
const int CompatibilityMaxLength = 500;
```

**Name Validation:**
- Regex pattern: `^[a-z0-9]+(-[a-z0-9]+)*$`
- No Unicode normalization
- Lowercase check (ASCII only)
- Length: 1-64 characters
- Cannot start/end with hyphen
- No consecutive hyphens
- Directory name must match

**Field Validation:**
- Does NOT check for unexpected fields
- Description: 1-1024 characters (+ warning if < 20)
- Compatibility: max 500 characters
- Version: warning if empty string

**Differences:**
- ❌ **Gap:** .NET does NOT support Unicode characters in skill names (only ASCII)
- ❌ **Gap:** .NET does NOT perform NFKC normalization
- ❌ **Gap:** .NET does NOT validate against unexpected fields
- ⚠️ **Extra:** .NET adds warning for very short descriptions
- ⚠️ **Extra:** .NET validates version field (not in reference)

---

## 5. Prompt Rendering Comparison

### Reference Implementation (prompt.py)

**Format:** XML (for Claude)
```xml
<available_skills>
<skill>
<name>
my-skill
</name>
<description>
What this skill does
</description>
<location>
/path/to/my-skill/SKILL.md
</location>
</skill>
</available_skills>
```

**Features:**
- HTML-escapes name and description
- Includes absolute path to SKILL.md
- Resolves paths with `Path.resolve()`

### .NET Implementation (DefaultSkillPromptRenderer.cs)

**Format:** Markdown
```markdown
# Available Skills

## my-skill

**Description:** What this skill does

**Version:** 1.0 (if present)
**Author:** Author Name (if present)
**Tags:** tag1, tag2 (if present)

---
```

**Full Skill Details:**
```markdown
# Skill: my-skill

**Description:** What this skill does
...
## Instructions

<full markdown body>
```

**Differences:**
- ❌ **Gap:** .NET uses Markdown instead of XML
- ❌ **Gap:** .NET does NOT include skill location/path in prompt
- ❌ **Gap:** .NET does NOT HTML-escape content
- ⚠️ **Different:** .NET has two-stage rendering (list vs details)
- ℹ️ **Note:** Prompt format is pluggable via ISkillPromptRenderer

---

## 6. Error Handling Comparison

### Reference Implementation

**Exceptions:**
- `SkillError` (base)
- `ParseError` (YAML/file errors)
- `ValidationError` (validation failures)

**Validation:**
- Returns `list[str]` of error messages
- Empty list = valid

### .NET Implementation

**Diagnostics Model:**
```csharp
public class SkillDiagnostic {
    public DiagnosticSeverity Severity { get; init; }  // Error/Warning/Info
    public string Message { get; init; }
    public string? Path { get; init; }
    public string Code { get; init; }  // e.g., "VAL001"
}
```

**ValidationResult:**
- Returns `ValidationResult` with diagnostics list
- `IsValid` = no errors (warnings allowed)

**Differences:**
- ✅ **Better:** .NET has severity levels (Error/Warning/Info)
- ✅ **Better:** .NET has error codes for programmatic handling
- ⚠️ **Different:** .NET doesn't throw for validation errors (by design)

---

## 7. Test Coverage Comparison

### Reference Implementation Tests

**test_parser.py:**
- Valid frontmatter parsing
- Missing frontmatter
- Unclosed frontmatter
- Invalid YAML
- Non-dict frontmatter
- find_skill_md (uppercase preferred, lowercase accepted)
- Missing name/description
- metadata dict handling
- allowed-tools parsing

**test_validator.py:**
- Valid skill
- Nonexistent path
- Not a directory
- Missing SKILL.md
- Name validation (uppercase, too long, leading hyphen, consecutive hyphens, invalid chars)
- Directory name mismatch
- Unexpected fields
- I18n support (Chinese, Russian, lowercase/uppercase)
- NFKC normalization
- Description too long
- Compatibility field validation

**test_prompt.py:**
- Empty skill list
- Single skill
- Multiple skills
- Special character escaping (XML)

### .NET Implementation Tests

**FileSystemSkillLoaderTests.cs:**
- Load skill set
- Load metadata
- Load single skill
- Missing directory
- Missing SKILL.md
- Malformed YAML

**SkillValidatorTests.cs:**
- Valid skill
- Name validation (length, pattern, uppercase, hyphens)
- Description validation (length, short warning)
- Directory name mismatch
- Compatibility validation
- Version validation

**DefaultSkillPromptRendererTests.cs:**
- Render skill list
- Render skill details
- Empty list
- Multiple skills

**GoldenFileTests.cs:**
- Snapshot testing for prompt output

**IntegrationTests.cs:**
- Full pipeline (scan → validate → render)

**ProgressiveDisclosureIntegrationTests.cs:**
- Metadata-only load performance

**Differences:**
- ❌ **Gap:** .NET tests do NOT cover lowercase skill.md
- ❌ **Gap:** .NET tests do NOT cover Unicode/i18n names
- ❌ **Gap:** .NET tests do NOT cover NFKC normalization
- ❌ **Gap:** .NET tests do NOT cover unexpected field validation
- ❌ **Gap:** .NET tests do NOT cover XML escaping (uses Markdown)
- ✅ **Extra:** .NET has performance tests
- ✅ **Extra:** .NET has golden file (snapshot) tests

---

## 8. Feature Gaps Summary

### Critical Gaps (Spec Violations)

1. **No lowercase "skill.md" support**
   - Reference: Accepts both SKILL.md and skill.md
   - .NET: Only accepts SKILL.md
   - **Impact:** High - Some skills may not load

2. **No Unicode name support**
   - Reference: Supports Unicode characters (Chinese, Russian, etc.)
   - .NET: ASCII-only regex `[a-z0-9-]`
   - **Impact:** High - Blocks international skills

3. **No NFKC normalization**
   - Reference: Normalizes names (e.g., café with combining accent)
   - .NET: No normalization
   - **Impact:** Medium - May cause false mismatches

4. **No unexpected field validation**
   - Reference: Errors on unknown frontmatter fields
   - .NET: Silently stores in AdditionalFields
   - **Impact:** Medium - Reduces validation strictness

### Medium Gaps (Behavior Differences)

5. **Prompt format (XML vs Markdown)**
   - Reference: Generates `<available_skills>` XML
   - .NET: Generates Markdown
   - **Impact:** Medium - Different from Claude convention

6. **No location in prompt**
   - Reference: Includes SKILL.md path in prompt
   - .NET: Does not include path
   - **Impact:** Low - Less discoverable

7. **allowed-tools representation**
   - Reference: Stored as single string
   - .NET: Parsed as list of strings
   - **Impact:** Low - Both are valid interpretations

### Minor Gaps (Nice to Have)

8. **No HTML/XML escaping**
   - Reference: Escapes special characters in XML
   - .NET: No escaping (uses Markdown)
   - **Impact:** Low - Not needed for Markdown

9. **Field mapping differences**
   - Reference: license, compatibility are first-class
   - .NET: license, compatibility in AdditionalFields
   - **Impact:** Low - Still accessible

### Extra Features (Not in Reference)

10. **SkillSet abstraction**
    - .NET has SkillSet for grouping skills
    - Not in reference (works with single skills)

11. **Diagnostic codes and severity**
    - .NET has structured diagnostics with codes
    - Reference uses plain error strings

12. **Resource policies**
    - .NET has IResourcePolicy for controlling what's included
    - Not in reference

13. **Version/Author/Tags fields**
    - .NET treats these as first-class
    - Not in official spec (should be in metadata dict)

---

## 9. Recommended Actions

### Priority 1: Fix Spec Violations

1. **Add lowercase skill.md support**
   - Update FileSystemSkillLoader to check for both SKILL.md and skill.md
   - Prefer SKILL.md if both exist
   - Add tests

2. **Add Unicode name validation**
   - Replace ASCII-only regex with Unicode-aware pattern
   - Support Unicode letters, numbers, hyphens
   - Check lowercase using Unicode rules
   - Add i18n tests (Chinese, Russian, etc.)

3. **Add NFKC normalization**
   - Normalize skill names before validation
   - Normalize directory names before comparison
   - Add normalization tests

4. **Add unexpected field validation**
   - Define ALLOWED_FIELDS constant
   - Check for extra fields in frontmatter
   - Add diagnostic for unexpected fields
   - Add tests

### Priority 2: Improve Parity

5. **Consider XML prompt renderer**
   - Create XmlSkillPromptRenderer implementing ISkillPromptRenderer
   - Generate `<available_skills>` format
   - Include location field
   - Add HTML escaping
   - Keep Markdown renderer as default

6. **Align field handling**
   - Move license/compatibility to first-class properties
   - Remove non-spec fields (Version/Author/Tags) or document as extensions
   - Add metadata dict support

### Priority 3: Documentation

7. **Document differences**
   - Update docs to note differences from reference
   - Explain .NET-specific design choices
   - Add migration guide for Python users

8. **Add ADR for prompt format**
   - Document decision to use Markdown vs XML
   - Explain pluggable renderer design

---

## 10. Test Coverage Recommendations

### Add Missing Test Cases

1. **Lowercase skill.md tests** (after implementing support)
2. **Unicode name tests** (Chinese, Russian, Arabic, etc.)
3. **NFKC normalization tests** (combining characters)
4. **Unexpected field tests** (unknown frontmatter fields)
5. **Special character tests** (if XML renderer added)

### Maintain Existing Tests

- Keep progressive disclosure performance tests
- Keep golden file tests
- Keep integration tests

---

## 11. Conclusion

The .NET implementation is **well-architected** and **largely feature-complete**. The main gaps are:

1. **Unicode/i18n support** (critical for international use)
2. **Lowercase skill.md** (important for compatibility)
3. **Unexpected field validation** (reduces strictness)
4. **Prompt format** (different from Claude convention)

The .NET version has **architectural advantages**:
- Better separation of concerns (metadata vs full load)
- Richer diagnostic model
- Interface-based extensibility
- Progressive disclosure optimizations

**Recommendation:** Address Priority 1 items to achieve full spec compliance, then consider Priority 2 items for better interoperability with the reference implementation and Claude ecosystem.

---

## Appendix: Specification Reference

**Official Spec Fields (from specification.mdx):**
- `name` (required): 1-64 chars, lowercase unicode alphanumeric + hyphens
- `description` (required): 1-1024 chars
- `license` (optional): string
- `compatibility` (optional): 1-500 chars
- `metadata` (optional): dict[str, str]
- `allowed-tools` (optional): space-delimited string

**Not in Spec but in .NET:**
- `version` (should be in metadata)
- `author` (should be in metadata)
- `tags` (should be in metadata)

**Validation Rules:**
- Name must match directory name
- No consecutive hyphens
- No leading/trailing hyphens
- NFKC normalization
- Lowercase check
