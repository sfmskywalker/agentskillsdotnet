# AgentSkills.NET — Project Brief

## Purpose
AgentSkills.NET is a .NET implementation of the [Agent Skills open standard](https://agentskills.io/).
It enables agents built on .NET (including Microsoft Agent Framework) to:
- Discover skills from folders
- Parse and validate `SKILL.md`
- Expose skills to LLMs via progressive disclosure
- Load full instructions and resources only when a skill is activated

The project prioritizes correctness, safety, and host-agnostic design.

## Non-Goals (v0.x)
- No autonomous agent decision-making logic
- No opinionated orchestration framework
- No script execution by default
- No network-based skill fetching (git, HTTP) in core libraries
- No UI components

These may be added later via adapters or extensions.

---

## Core Concepts

### Skill
A skill is a directory containing:
- `SKILL.md` (YAML frontmatter + Markdown body)
- Optional subfolders: `scripts/`, `references/`, `assets/`

### Skill Metadata vs Skill Content
- **Metadata-only load**
  - Used for listing “available skills”
  - Reads YAML frontmatter only
  - Must be fast and low-allocation
- **Full skill load**
  - Loads Markdown body
  - Enumerates referenced resources
  - Never auto-executes anything

### Progressive Disclosure
1. Agent sees a list of available skills (name + description).
2. Agent activates a skill.
3. Full instructions and optional resources are provided.

This is a hard invariant.

### Validation & Diagnostics
- Validation produces diagnostics (errors, warnings, info).
- Normal validation failures do NOT throw.
- Consumers decide whether diagnostics are fatal.

---

## Architecture Overview

### Packages
- `AgentSkills`
  - Core domain models
  - SkillSet, Skill, metadata, resource abstractions
- `AgentSkills.Loader`
  - Directory scanning and loading
- `AgentSkills.Validation`
  - Validation rules and diagnostics
- `AgentSkills.Prompts`
  - Prompt rendering abstractions
- `AgentSkills.Adapters.*`
  - Host-specific integrations (e.g. Microsoft Agent Framework)

Core packages must remain host-agnostic.

### Key Interfaces
- `ISkillLoader`
- `ISkillValidator`
- `ISkillPromptRenderer`
- `ISkillResourceProvider`

---

## Security Model

- Scripts are treated as **data**, not executable code.
- No execution occurs unless explicitly enabled by the host.
- Any “allowed-tools” metadata is advisory only.
- Hosts are responsible for sandboxing and enforcement.

Security decisions always favor safety over convenience.

---

## Invariants (Do Not Break)
- Metadata-only scan must not load large files.
- Core libraries must not depend on agent frameworks.
- Validation returns diagnostics, not exceptions.
- Rendering is pluggable, never hardcoded.
- No implicit behavior based on folder names beyond spec rules.

---

## Public API Philosophy
- APIs should be boring, explicit, and discoverable.
- Avoid magic behavior.
- Prefer additive changes over breaking ones.
- Namespaces describe domain, not platform.

---

## Definition of Done (for any feature)
- Public API reviewed for clarity
- Tests added or updated
- Sample app still works
- Relevant docs updated
- No scope creep beyond the issue

---

## Walking Skeleton
A sample application must always demonstrate:
scan → metadata → validate → render list → activate skill → render full instructions

If this breaks, something is wrong.
