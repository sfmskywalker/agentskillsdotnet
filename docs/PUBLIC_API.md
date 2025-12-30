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

*Coming soon*

## AgentSkills.Validation

*Coming soon*

## AgentSkills.Prompts

*Coming soon*
