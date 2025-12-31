# Proposed Follow-Up Issues

Based on the comparison between the .NET implementation and the reference implementation, these issues should be created to achieve feature parity and spec compliance.

---

## Priority 1: Spec Compliance (Critical)

### Issue 1: Add support for lowercase "skill.md" filename

**Priority:** High  
**Category:** Spec Compliance  
**Effort:** Small

**Description:**
The reference implementation accepts both `SKILL.md` (preferred) and `skill.md` (fallback). The .NET implementation only accepts `SKILL.md`.

**Tasks:**
- Update `FileSystemSkillLoader.FindSkillFiles()` to check for both filenames
- Prefer `SKILL.md` if both exist (case-insensitive on some file systems)
- Add method `FindSkillMdFile(string directory)` that returns the first found
- Add tests for lowercase skill.md
- Update documentation

**Acceptance Criteria:**
- `skill.md` files are discovered and loaded
- `SKILL.md` is preferred when both exist
- Existing tests still pass
- New tests cover both cases

**Reference:**
- Python: `parser.py::find_skill_md()` lines 12-27
- Test: `test_parser.py::test_find_skill_md_accepts_lowercase`

---

### Issue 2: Add Unicode character support in skill names

**Priority:** High  
**Category:** Spec Compliance / I18n  
**Effort:** Medium

**Description:**
The specification supports Unicode characters in skill names (Chinese, Russian, Arabic, etc.), but the .NET implementation only supports ASCII `[a-z0-9-]`.

**Tasks:**
- Replace `NamePattern()` regex with Unicode-aware pattern
- Support Unicode letters (any script) and numbers
- Implement lowercase check using Unicode rules (`char.IsLower()` + `char.IsLetter()`)
- Keep hyphen rules (no leading/trailing, no consecutive)
- Add test fixtures for Chinese, Russian, Arabic skill names
- Add validation tests for mixed scripts

**Acceptance Criteria:**
- Skill names with Chinese characters validate correctly (e.g., `技能`)
- Skill names with Russian characters validate correctly (e.g., `навык`)
- Uppercase Unicode characters are rejected (e.g., `НАВЫК`)
- Hyphen rules still apply for all scripts
- Directory name matching works with Unicode

**Reference:**
- Python: `validator.py::_validate_name()` lines 25-67
- Tests: `test_validator.py::test_i18n_*` functions
- Spec: specification.mdx lines 59-61

---

### Issue 3: Add NFKC Unicode normalization

**Priority:** Medium  
**Category:** Spec Compliance / I18n  
**Effort:** Small

**Description:**
The reference implementation performs NFKC normalization on skill names to handle combining characters (e.g., café with decomposed vs composed accents). The .NET implementation does not normalize.

**Tasks:**
- Add normalization using `string.Normalize(NormalizationForm.NFKC)`
- Normalize skill name before validation
- Normalize directory name before comparison
- Add test case for decomposed/composed character equivalence

**Acceptance Criteria:**
- Decomposed and composed forms of same name validate as equivalent
- Directory matching works with normalized names
- Test case for café (decomposed) vs café (composed)

**Reference:**
- Python: `validator.py::_validate_name()` line 37
- Test: `test_validator.py::test_nfkc_normalization`

---

### Issue 4: Add validation for unexpected frontmatter fields

**Priority:** Medium  
**Category:** Spec Compliance  
**Effort:** Small

**Description:**
The reference implementation rejects SKILL.md files with unexpected frontmatter fields. The .NET implementation silently stores them in `AdditionalFields`.

**Tasks:**
- Define `ALLOWED_FIELDS` constant in `SkillValidator`
- Check for fields not in the allowed set
- Add diagnostic with severity Error for unexpected fields
- Add test cases for unknown fields
- Document which fields are allowed per spec

**Acceptance Criteria:**
- Unknown fields generate error diagnostic
- Known fields pass validation
- Test covers single and multiple unknown fields
- Error message lists the unexpected field names

**Reference:**
- Python: `validator.py` lines 14-22, 103-115
- Test: `test_validator.py::test_unexpected_fields`
- Spec: specification.mdx lines 46-54

---

## Priority 2: Feature Parity (Important)

### Issue 5: Add XML prompt renderer for Claude compatibility

**Priority:** Medium  
**Category:** Feature Parity  
**Effort:** Medium

**Description:**
The reference implementation generates `<available_skills>` XML format recommended for Claude. The .NET implementation uses Markdown. Add an XML renderer option.

**Tasks:**
- Create `XmlSkillPromptRenderer` implementing `ISkillPromptRenderer`
- Generate `<available_skills>` block with `<skill>` elements
- Include `<name>`, `<description>`, `<location>` tags
- Add HTML escaping for special characters (`<`, `>`, `&`, etc.)
- Include absolute path to SKILL.md in `<location>`
- Add tests comparing output to reference format
- Keep `DefaultSkillPromptRenderer` (Markdown) as default
- Update docs to explain both renderers

**Acceptance Criteria:**
- XML output matches reference implementation format
- Special characters are properly escaped
- Location includes absolute path to SKILL.md
- Tests verify XML structure and escaping
- Documentation explains when to use each renderer

**Reference:**
- Python: `prompt.py` entire file
- Tests: `test_prompt.py` entire file

---

### Issue 6: Align field handling with specification

**Priority:** Low  
**Category:** Architecture / Spec Alignment  
**Effort:** Medium (Breaking Change)

**Description:**
The .NET implementation has `Version`, `Author`, and `Tags` as first-class properties, but these are not in the official spec. The spec has `license` and `compatibility` as optional fields, but .NET stores them in `AdditionalFields`.

**Tasks:**
- Add `License` and `Compatibility` as first-class properties to `SkillManifest`
- Remove or document `Version`, `Author`, `Tags` as extensions
- Update loader to parse `license` and `compatibility` directly
- Update validator to validate these fields
- Consider storing Version/Author/Tags in metadata dict for spec compliance
- Document design decision in ADR
- Update all tests

**Acceptance Criteria:**
- `License` and `Compatibility` are first-class properties
- Extension fields are clearly documented
- Tests updated for new structure
- ADR explains design choices
- No breaking change to public API if possible

**Reference:**
- Python: `models.py::SkillProperties`
- Spec: specification.mdx lines 46-54

---

### Issue 7: Add metadata dict support

**Priority:** Low  
**Category:** Feature Parity  
**Effort:** Medium

**Description:**
The reference implementation has a `metadata` dict in `SkillProperties` for arbitrary key-value pairs. The .NET implementation uses `AdditionalFields` which includes all non-standard fields, not just the metadata section.

**Tasks:**
- Add `Metadata` property to `SkillManifest` as `IReadOnlyDictionary<string, string>`
- Parse frontmatter `metadata:` section into this property
- Keep `AdditionalFields` for truly unknown fields (not in spec, not in metadata)
- Update tests to verify metadata parsing
- Update renderer to optionally include metadata

**Acceptance Criteria:**
- `metadata:` section in frontmatter is parsed correctly
- Values are stored as `Dictionary<string, string>`
- AdditionalFields only contains non-spec fields outside metadata
- Tests verify nested metadata structure

**Reference:**
- Python: `models.py::SkillProperties.metadata`
- Test: `test_parser.py::test_read_with_metadata`

---

## Priority 3: Testing & Documentation

### Issue 8: Add comprehensive i18n test suite

**Priority:** Medium  
**Category:** Testing  
**Effort:** Small

**Description:**
Add test fixtures and test cases for international skill names to ensure Unicode support works correctly across different scripts.

**Tasks:**
- Create test fixtures in `fixtures/skills/`:
  - `技能/` (Chinese)
  - `навык/` (Russian)
  - `مهارة/` (Arabic, if RTL supported)
  - `café/` (French with accent)
- Add validation tests for each script
- Test uppercase/lowercase for each script
- Test directory name matching for Unicode names
- Test NFKC normalization cases

**Acceptance Criteria:**
- All scripts validate correctly
- Uppercase detection works for all scripts
- Tests are deterministic and cross-platform

**Reference:**
- Tests: `test_validator.py` lines 165-218

---

### Issue 9: Document .NET-specific design choices

**Priority:** Low  
**Category:** Documentation  
**Effort:** Small

**Description:**
Create documentation explaining how the .NET implementation differs from the reference and why.

**Tasks:**
- Create ADR for prompt format choice (Markdown vs XML)
- Create ADR for diagnostics model
- Create ADR for metadata vs full load separation
- Update README with "Differences from Reference" section
- Add migration guide for Python reference users
- Link to comparison document

**Acceptance Criteria:**
- ADRs created in `docs/adr/`
- README updated
- Clear explanation of design rationale
- Examples showing how to achieve reference-equivalent behavior

---

## Priority 4: Nice to Have

### Issue 10: Add CLI tool matching reference implementation

**Priority:** Low  
**Category:** Tooling  
**Effort:** Medium

**Description:**
The reference implementation has a CLI with `validate`, `read-properties`, and `to-prompt` commands. Add equivalent .NET CLI.

**Tasks:**
- Create `AgentSkills.Cli` project
- Add `validate <path>` command
- Add `read-properties <path>` command (output JSON)
- Add `to-prompt <path1> <path2>...` command
- Support both Markdown and XML output formats
- Add tests for CLI commands
- Package as dotnet tool

**Acceptance Criteria:**
- Commands match reference implementation behavior
- JSON output format matches reference
- Can be installed as `dotnet tool install`
- Help text is clear

**Reference:**
- Python: `cli.py`
- README: skills-ref/README.md lines 29-39

---

## Summary Table

| Issue | Priority | Effort | Category | Breaking Change |
|-------|----------|--------|----------|----------------|
| #1 Lowercase skill.md | High | Small | Spec Compliance | No |
| #2 Unicode names | High | Medium | Spec Compliance | No |
| #3 NFKC normalization | Medium | Small | Spec Compliance | No |
| #4 Unexpected fields | Medium | Small | Spec Compliance | No |
| #5 XML renderer | Medium | Medium | Feature Parity | No |
| #6 Field alignment | Low | Medium | Architecture | Maybe |
| #7 Metadata dict | Low | Medium | Feature Parity | No |
| #8 I18n tests | Medium | Small | Testing | No |
| #9 Documentation | Low | Small | Documentation | No |
| #10 CLI tool | Low | Medium | Tooling | No |

---

## Recommended Implementation Order

**Phase 1: Core Spec Compliance**
1. Issue #1: Lowercase skill.md support
2. Issue #2: Unicode name support
3. Issue #3: NFKC normalization
4. Issue #4: Unexpected field validation

**Phase 2: Feature Parity**
5. Issue #5: XML prompt renderer
6. Issue #8: I18n test suite

**Phase 3: Documentation & Refinement**
7. Issue #9: Documentation
8. Issue #7: Metadata dict support
9. Issue #6: Field alignment (if needed)

**Phase 4: Optional Enhancements**
10. Issue #10: CLI tool

---

## Notes

- **Breaking Changes:** Issue #6 (field alignment) may require breaking changes. Consider carefully or implement as v2.0.
- **Testing:** Each issue should include comprehensive tests before merging.
- **Documentation:** Update relevant docs (API, guides, ADRs) with each issue.
- **Backwards Compatibility:** Prioritize non-breaking changes where possible.
