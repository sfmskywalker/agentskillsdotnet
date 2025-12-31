# AGENTS.md — Working Agreements for Coding Agents

This repository uses small, scoped issues to build AgentSkills.NET safely and consistently.
If you are a coding agent (including GitHub Copilot), follow these rules.

---

## 0) Read this first (required)
Before changing code, read:
1. `docs/PROJECT_BRIEF.md` (canonical architecture + invariants)
2. Any relevant ADRs in `docs/adr/`
3. The issue you are working on (and its acceptance criteria)

If any of these are missing or unclear, prefer making the smallest safe change and add/update docs.

---

## 1) Project goals (what we are building)
AgentSkills.NET provides:
- Skill discovery and loading from folders
- Parsing and validation of `SKILL.md` (YAML frontmatter + Markdown body)
- Diagnostics (errors/warnings) instead of exceptions for normal validation failures
- Prompt rendering with progressive disclosure (metadata list first, full instructions on activation)
- Optional host adapters (e.g. Microsoft Agent Framework) that do NOT pollute core packages

---

## 2) Non-goals / safety stance (do not violate)
- Do NOT execute scripts by default.
- Do NOT introduce host framework dependencies into core libraries.
- Do NOT add network fetching in core libraries.
- Do NOT implement autonomous agent decision-making logic.
- Do NOT expand scope beyond the issue.

If you think you need any of the above, stop and propose an ADR or a separate issue.

---

## 3) Repo invariants (hard rules)
- Metadata-only scan must not load large files.
- Core libraries must remain host-agnostic.
- Validation returns diagnostics; it does not throw for normal problems.
- Prompt rendering is pluggable via interfaces.
- Progressive disclosure is mandatory: list skills → activate skill → load full content.

---

## 4) Working style for issues
Implement issues in small, mergeable slices.

For each issue:
- Respect the issue’s **Non-Goals**.
- Implement only what is needed to satisfy **Acceptance Criteria**.
- If a design decision affects multiple issues: add/update an ADR in `docs/adr/`.

---

## 5) Tests are required
Every functional change must include tests.

Preferred test approach:
- Unit tests for parsing/validation rules.
- Golden tests (snapshots) for prompt renderer output if applicable.
- Integration test for pipeline: scan → parse → validate → render.

Use fixtures under `fixtures/skills/` (or create them if missing) instead of inventing ad-hoc strings.

---

## 6) Linting and formatting

This project enforces code formatting using `dotnet format`.

**Before committing:**
- Always run `dotnet format --verify-no-changes --no-restore` to check for formatting issues
- If issues are found, run `dotnet format --no-restore` to auto-fix them
- The CI pipeline will fail if formatting issues are detected

**Common formatting rules:**
- No trailing whitespace
- Consistent indentation (spaces, not tabs)
- Proper line endings

---

## 7) Keep the walking skeleton sample working
There should be a sample app (or soon will be) demonstrating:
scan → metadata → validate → render list → activate skill → render instructions

Do not break this sample. If it breaks, fix it in the same PR.

---

## 8) API and naming conventions
- Namespaces should be clean and domain-based (e.g., `AgentSkills.*`), not platform-based.
- Avoid redundant prefixes in types (the namespace already provides context).
- Prefer explicit, boring names:
  - `Skill`, `SkillSet`, `SkillMetadata`, `SkillManifest`
  - `SkillDiagnostic`, `ValidationResult`
  - `ISkillLoader`, `ISkillValidator`, `ISkillPromptRenderer`

Public API changes must be reflected in `docs/PUBLIC_API.md` (when present).

---

## 9) Dependency rules
Do not add new external dependencies lightly.
If reminded by an issue to choose a library (e.g., YAML parsing), prefer:
- Well-maintained, popular libraries
- Minimal surface area
- Good license compatibility

If you add a dependency:
- Update documentation (brief or ADR)
- Add a note in the PR description
- Ensure it’s used in only the appropriate package (core vs adapter)

---

## 10) Error handling rules
- Parsing errors should produce diagnostics, not throw, unless truly exceptional (I/O failure etc.).
- Validation should aggregate diagnostics and return them.
- Provide file/path context in diagnostics where possible.

---

## 11) What to do when something is unclear
If the issue lacks context:
1. Check `docs/PROJECT_BRIEF.md` and ADRs.
2. Look for similar patterns in existing code/tests.
3. Implement the smallest safe version that meets acceptance criteria.
4. Add a brief doc note (or ADR) if a decision was required.

Avoid asking for clarification unless absolutely necessary—prefer minimal, reversible decisions.

---

## 12) Checklist before opening a PR
- [ ] Issue acceptance criteria satisfied
- [ ] Tests added/updated and passing
- [ ] Code formatting verified with `dotnet format --verify-no-changes --no-restore`
- [ ] No scope creep
- [ ] Sample app still works
- [ ] Docs updated if needed (brief/API/ADR)
- [ ] No new dependencies without documentation

---

## 13) Quick map
- Core code: `src/`
- Tests: `tests/`
- Samples: `samples/`
- Docs: `docs/`
- ADRs: `docs/adr/`
- Fixtures: `fixtures/`

End of file.
