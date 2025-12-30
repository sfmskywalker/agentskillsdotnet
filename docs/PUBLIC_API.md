# AgentSkills.NET Public API

This document describes the public API surface of AgentSkills.NET core packages.

## AgentSkills (Core Domain Models)

### Domain Models

#### `Skill`
Represents a complete skill with its manifest and content. This is the full representation loaded when a skill is activated.

**Properties:**
- `SkillManifest Manifest` (required) - The skill manifest (parsed YAML frontmatter)
- `string Instructions` (required) - The skill instructions (Markdown body content)
- `string Path` (required) - The path to the skill directory
- `SkillMetadata Metadata` (computed) - Metadata derived from manifest and path for fast listing

#### `SkillManifest`
Represents the parsed YAML frontmatter from a SKILL.md file. Contains the skill's metadata and configuration.

**Properties:**
- `string Name` (required) - The unique name of the skill
- `string Description` (required) - The description of the skill
- `string? Version` (optional) - The version of the skill
- `string? Author` (optional) - The author of the skill
- `IReadOnlyList<string> Tags` (optional) - Tags associated with the skill
- `IReadOnlyList<string> AllowedTools` (optional) - List of allowed tools (advisory only)
- `IReadOnlyDictionary<string, object?> AdditionalFields` (optional) - Additional fields for extensibility

**Extensibility:**
The `AdditionalFields` dictionary allows unknown fields from YAML frontmatter to be preserved, enabling forward compatibility and custom extensions.

#### `SkillMetadata`
Represents metadata about a skill that can be loaded without reading the full content. Enables fast skill listing and discovery.

**Properties:**
- `string Name` (required) - The unique name of the skill
- `string Description` (required) - The description of the skill
- `string Path` (required) - The path to the skill directory
- `string? Version` (optional) - The version of the skill
- `string? Author` (optional) - The author of the skill
- `IReadOnlyList<string> Tags` (optional) - Tags associated with the skill

#### `SkillSet`
Represents a collection of skills that have been discovered and loaded.

**Properties:**
- `IReadOnlyList<Skill> Skills` - The collection of skills
- `IReadOnlyList<SkillDiagnostic> Diagnostics` - Diagnostics produced while loading
- `bool IsValid` - True if no errors in diagnostics

**Methods:**
- `Skill? GetSkill(string name)` - Get a skill by name (case-insensitive)
- `IEnumerable<Skill> GetSkillsByTag(string tag)` - Get skills with a specific tag (case-insensitive)

#### `SkillResource`
Represents a resource file within a skill directory (e.g., scripts, references, assets).

**Properties:**
- `string Name` (required) - The name of the resource file
- `string RelativePath` (required) - The path relative to the skill directory
- `string? ResourceType` (optional) - The type of resource (e.g., "script", "reference", "asset")
- `string? AbsolutePath` (optional) - The absolute path to the resource file

### Diagnostics and Validation

#### `DiagnosticSeverity` (enum)
Represents the severity level of a diagnostic message.

**Values:**
- `Info = 0` - Informational message
- `Warning = 1` - Warning message (potential problem, does not prevent usage)
- `Error = 2` - Error message (problem that should prevent usage)

#### `SkillDiagnostic`
Represents a diagnostic message (error, warning, or info) about a skill.

**Properties:**
- `DiagnosticSeverity Severity` (required) - The severity level
- `string Message` (required) - The diagnostic message
- `string? Path` (optional) - Path to the file or skill
- `int? Line` (optional) - Line number within the file
- `int? Column` (optional) - Column number within the file
- `string? Code` (optional) - Diagnostic code or identifier

#### `ValidationResult`
Represents the result of validating a skill, containing diagnostics.

**Properties:**
- `IReadOnlyList<SkillDiagnostic> Diagnostics` - The collection of diagnostics
- `bool IsValid` - True if no errors (computed)
- `bool HasWarnings` - True if any warnings (computed)
- `IEnumerable<SkillDiagnostic> Errors` - All error diagnostics (computed)
- `IEnumerable<SkillDiagnostic> Warnings` - All warning diagnostics (computed)
- `IEnumerable<SkillDiagnostic> Infos` - All info diagnostics (computed)

### Design Principles

1. **Immutability**: All domain models use `init` properties to ensure immutability after construction.
2. **Required vs Optional**: Required properties use the `required` modifier; optional properties are nullable or have default empty collections.
3. **Extensibility**: `SkillManifest.AdditionalFields` allows unknown fields to be preserved for future compatibility.
4. **No Exceptions**: Validation produces diagnostics instead of throwing exceptions for normal validation failures.
5. **Progressive Disclosure**: `SkillMetadata` provides fast access to listing information without loading full content.
6. **Host-Agnostic**: Core models have no dependencies on specific agent frameworks or platforms.

---

## AgentSkills.Loader

The loader package provides functionality for discovering and loading skills from the file system.

### Interfaces

#### `ISkillLoader`
Interface for loading skills from storage.

**Methods:**
- `SkillSet LoadSkillSet(string directoryPath)` - Loads a complete skill set from a directory, including all skill content. Returns a SkillSet with loaded skills and diagnostics.
- `(IReadOnlyList<SkillMetadata> Metadata, IReadOnlyList<SkillDiagnostic> Diagnostics) LoadMetadata(string directoryPath)` - Loads only metadata for skills in a directory (fast path, does not load full content). Returns metadata collection and diagnostics.
- `(Skill? Skill, IReadOnlyList<SkillDiagnostic> Diagnostics) LoadSkill(string skillDirectoryPath)` - Loads a single skill from a directory. Returns the loaded skill and diagnostics, or null if loading failed.

### Implementations

#### `FileSystemSkillLoader`
Default implementation of `ISkillLoader` that loads skills from the file system.

**Features:**
- Scans directories recursively for `SKILL.md` files
- Parses YAML frontmatter and Markdown body
- Supports metadata-only loading (fast path)
- Validates required fields (name, description)
- Collects diagnostics for all errors and warnings
- Preserves unknown YAML fields in `AdditionalFields`
- Handles file system errors gracefully

**Usage:**
```csharp
var loader = new FileSystemSkillLoader();

// Load metadata only (fast)
var (metadata, diagnostics) = loader.LoadMetadata("/path/to/skills");

// Load full skill set
var skillSet = loader.LoadSkillSet("/path/to/skills");

// Load single skill
var (skill, diagnostics) = loader.LoadSkill("/path/to/skills/my-skill");
```

**Dependencies:**
- YamlDotNet 16.2.0 - YAML parsing library

**SKILL.md Format:**

The `FileSystemSkillLoader` expects SKILL.md files to follow this format:
```markdown
---
name: skill-name
description: A brief description
version: 1.0.0
author: Author Name
tags:
  - tag1
  - tag2
allowed-tools:
  - tool1
  - tool2
---

# Skill Instructions

The markdown body content goes here...
```

**Parsing Behavior:**
1. **Frontmatter Delimiters**: YAML frontmatter must be enclosed between `---` delimiters (start and end)
2. **Required Fields**: `name` and `description` are required and must not be empty
3. **Optional Fields**: All other fields are optional
4. **Additional Fields**: Unknown YAML fields are preserved in `SkillManifest.AdditionalFields`
5. **Markdown Body**: Everything after the closing `---` is preserved as instructions, with leading and trailing whitespace trimmed
6. **Triple Dashes in Body**: `---` appearing in the markdown body are preserved as-is (not treated as delimiters)
7. **Metadata-Only Load**: Uses streaming to read only the frontmatter section, never loading the full file content

**Edge Cases Handled:**
- Missing SKILL.md file → `LOADER002` error
- No frontmatter delimiters → `LOADER004` error
- Unclosed frontmatter (missing closing `---`) → `LOADER004` error
- Malformed YAML syntax → `LOADER005` error
- Missing required fields → `LOADER006` or `LOADER007` error
- Empty required field values → `LOADER006` or `LOODER007` error
- File I/O errors → `LOADER003` error
- Invalid skills do not prevent loading of valid skills

**Diagnostic Codes:**
- `LOADER001` - Directory not found
- `LOADER002` - SKILL.md not found in directory
- `LOADER003` - Failed to read skill file (I/O error)
- `LOADER004` - YAML frontmatter not found or malformed
- `LOADER005` - Failed to parse YAML
- `LOADER006` - Required field 'name' is missing or invalid
- `LOADER007` - Required field 'description' is missing or invalid

### Design Principles

1. **Progressive Disclosure**: `LoadMetadata` reads only YAML frontmatter for fast skill listing. `LoadSkillSet` reads full content.
2. **Diagnostics Over Exceptions**: I/O errors for truly exceptional cases only. Validation failures produce diagnostics.
3. **Non-Destructive**: Invalid skills do not prevent loading of valid skills. All diagnostics are collected and returned.
4. **Host-Agnostic**: No dependencies on specific agent frameworks or platforms.

---

## AgentSkills.Validation

*Coming soon*

## AgentSkills.Prompts

*Coming soon*
