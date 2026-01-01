# Feature Parity Checklist

This checklist tracks feature parity between the .NET implementation and the Python reference implementation.

**Last Updated:** 2025-12-31  
**Status:** ~85% Complete

---

## Core Functionality

### Parsing
- [x] YAML frontmatter parsing
- [x] Markdown body extraction
- [x] Required fields (name, description)
- [x] Optional fields (version, author, tags)
- [ ] ⚠️ lowercase "skill.md" support (only SKILL.md supported)
- [x] Handle malformed YAML gracefully
- [x] Handle missing frontmatter
- [x] Split on "---" delimiter
- [x] Progressive disclosure (metadata-only load)

### Validation
- [x] Name validation (length 1-64)
- [x] Name pattern (hyphens rules)
- [ ] ⚠️ Unicode character support (ASCII-only)
- [ ] ⚠️ NFKC normalization
- [x] Description validation (length 1-1024)
- [x] Directory name matching
- [ ] ⚠️ Unexpected field detection
- [x] Compatibility field (max 500 chars)
- [x] No leading/trailing hyphens
- [x] No consecutive hyphens

### Prompt Rendering
- [x] Render skill list
- [x] Render skill details
- [x] Include name and description
- [x] Progressive disclosure pattern
- [ ] ⚠️ XML format (`<available_skills>`)
- [ ] ⚠️ Include location path
- [ ] ⚠️ HTML/XML escaping
- [x] Markdown format (alternative)

### Error Handling
- [x] Return diagnostics (not exceptions)
- [x] Severity levels (Error/Warning/Info)
- [x] Error codes (e.g., VAL001)
- [x] File path context
- [x] Multiple diagnostics per skill
- [x] Aggregate validation results

---

## Data Model Fields

### Spec-Required Fields
- [x] name (string, required)
- [x] description (string, required)
- [ ] ⚠️ license (optional, not first-class in .NET)
- [ ] ⚠️ compatibility (optional, not first-class in .NET)
- [x] allowed-tools (optional, but parsed as list not string)
- [ ] ⚠️ metadata (optional, AdditionalFields instead)

### .NET-Specific Extensions (not in spec)
- [x] version (first-class field)
- [x] author (first-class field)
- [x] tags (first-class field)
- [x] AdditionalFields (for unknown fields)

---

## Edge Cases & Special Handling

### File System
- [x] Handle missing directories
- [x] Handle missing SKILL.md
- [x] Handle directory traversal
- [x] Recursive skill discovery
- [x] Handle I/O errors gracefully
- [ ] ⚠️ Case-insensitive filename support

### Unicode & I18n
- [ ] ⚠️ Unicode letters in names (Chinese, Russian, Arabic)
- [ ] ⚠️ Lowercase detection for Unicode
- [ ] ⚠️ NFKC normalization (café vs café)
- [x] UTF-8 file reading

### Special Characters
- [x] Handle quotes in descriptions
- [x] Handle newlines in descriptions
- [x] Handle YAML special chars
- [ ] ⚠️ XML/HTML escaping (not needed for Markdown)

---

## Test Coverage

### Parser Tests
- [x] Valid frontmatter
- [x] Missing frontmatter
- [x] Unclosed frontmatter
- [x] Invalid YAML
- [x] Non-dict frontmatter
- [x] Missing required fields
- [ ] ⚠️ Lowercase skill.md (not tested)
- [x] Metadata dict handling

### Validator Tests
- [x] Valid skill
- [x] Nonexistent path
- [x] Not a directory
- [x] Missing SKILL.md
- [x] Name too long
- [x] Name with uppercase
- [x] Name with leading hyphen
- [x] Name with consecutive hyphens
- [x] Name with invalid chars
- [x] Directory name mismatch
- [ ] ⚠️ Unexpected fields (not tested)
- [ ] ⚠️ Unicode names (not tested)
- [ ] ⚠️ NFKC normalization (not tested)
- [x] Description too long
- [x] Compatibility validation

### Prompt Tests
- [x] Empty skill list
- [x] Single skill
- [x] Multiple skills
- [x] Render skill details
- [ ] ⚠️ XML format (not tested, uses Markdown)
- [ ] ⚠️ Special char escaping (not tested, Markdown doesn't need it)

### Integration Tests
- [x] Full pipeline (scan → validate → render)
- [x] Progressive disclosure performance
- [x] Golden file tests
- [x] Regression tests

---

## Architecture & Design

### Separation of Concerns
- [x] SkillMetadata (fast scan)
- [x] Skill (full load)
- [x] SkillManifest (frontmatter)
- [x] SkillSet (collection)
- [x] Modular packages

### Interfaces & Extensibility
- [x] ISkillLoader
- [x] ISkillValidator
- [x] ISkillPromptRenderer
- [x] IResourcePolicy
- [x] Pluggable renderers

### Safety & Security
- [x] No script execution by default
- [x] Scripts treated as data
- [x] No network access in core
- [x] Host-agnostic design
- [x] Validation returns diagnostics

---

## Documentation

### Core Documentation
- [x] README with quick start
- [x] PROJECT_BRIEF.md
- [x] GETTING_STARTED.md
- [x] SKILL_AUTHORING.md
- [x] PUBLIC_API.md
- [x] TESTING_GUIDE.md
- [x] ADRs for key decisions
- [x] Reference comparison docs

### Examples & Samples
- [x] Walking skeleton sample
- [x] Microsoft Agent Framework sample
- [x] Example skills in fixtures/
- [x] Complete skill example
- [x] Minimal skill example

---

## Priority Legend

- ✅ Implemented and tested
- ⚠️ Partially implemented or differs from reference
- ❌ Not implemented

---

## Summary by Priority

**Priority 1 (Spec Compliance):**
- [ ] Unicode name support
- [ ] Lowercase skill.md
- [ ] NFKC normalization
- [ ] Unexpected field validation

**Priority 2 (Feature Parity):**
- [ ] XML prompt renderer
- [ ] Field alignment (license/compatibility)
- [ ] Metadata dict support

**Priority 3 (Testing):**
- [ ] Unicode test suite
- [ ] Unexpected field tests
- [ ] NFKC tests

**Priority 4 (Nice to Have):**
- [ ] CLI tool
- [ ] Advanced i18n tests
