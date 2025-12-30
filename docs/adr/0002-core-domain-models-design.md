# ADR 0002: Core Domain Models Design

## Status
Accepted

## Context
Epic 2 requires defining stable, host-agnostic domain models representing a skill and its contents. These models must support:
- Progressive disclosure (metadata-only vs full load)
- Validation and diagnostics without throwing exceptions
- Extensibility for future fields
- Immutability and safety
- Host framework independence

The models will form the foundation of the entire AgentSkills.NET library and must be carefully designed for long-term stability.

## Decision

### Domain Model Structure

We define the following core domain models in the `AgentSkills` package:

1. **SkillMetadata** (existing)
   - Lightweight model for skill discovery
   - Contains only essential listing information (name, description, version, author, tags, path)
   - Fast to load without reading full content

2. **SkillManifest** (new)
   - Represents parsed YAML frontmatter
   - Contains all standard fields (name, description, version, author, tags, allowed-tools)
   - Includes `AdditionalFields` dictionary for unknown fields
   - Enables extensibility and forward compatibility

3. **Skill** (new)
   - Complete skill representation with manifest and content
   - Contains `SkillManifest`, markdown instructions, and path
   - Computed `Metadata` property for fast access to listing info
   - Loaded when a skill is activated (progressive disclosure)

4. **SkillSet** (new)
   - Collection of skills discovered from a directory
   - Contains skills and diagnostics from loading process
   - Provides query methods (`GetSkill`, `GetSkillsByTag`)
   - Supports validation state (`IsValid`)

5. **SkillResource** (new)
   - Abstraction for files in skill directories
   - Represents scripts, references, assets without file system coupling
   - Contains name, relative path, resource type, optional absolute path

### Diagnostics System

We define a diagnostics-based validation approach:

1. **DiagnosticSeverity** (enum)
   - Three levels: Info (0), Warning (1), Error (2)
   - Numeric values allow ordering/filtering

2. **SkillDiagnostic** (class)
   - Contains severity, message, optional file location (path, line, column)
   - Optional diagnostic code for categorization
   - Immutable after creation

3. **ValidationResult** (class)
   - Aggregates diagnostics from validation
   - Computed properties: `IsValid`, `HasWarnings`
   - Filter methods: `Errors`, `Warnings`, `Infos`
   - No exceptions thrown for normal validation failures

### Immutability and Safety

All domain models follow these patterns:
- Properties use `init` keyword for immutability
- Required fields use `required` modifier (enforced at compile time)
- Optional fields are nullable or have default empty collections
- Collections exposed as `IReadOnlyList<T>` or `IReadOnlyDictionary<K, V>`
- No setters or mutable state after construction

### Extensibility

`SkillManifest.AdditionalFields` provides extensibility:
- Unknown YAML fields are preserved in `AdditionalFields` dictionary
- Allows forward compatibility (old parsers can read new fields)
- Enables custom extensions without breaking schema
- Type is `IReadOnlyDictionary<string, object?>` to support any YAML value

### Progressive Disclosure Support

The design explicitly supports progressive disclosure:
1. **Discovery Phase**: Load only `SkillMetadata` from YAML frontmatter
2. **Activation Phase**: Load full `Skill` with manifest and instructions
3. `Skill.Metadata` computed property allows fast access to listing info

## Consequences

### Positive
- Clear separation between metadata-only and full skill load
- Diagnostics approach avoids exceptions for normal validation failures
- Immutability prevents accidental state modification
- `AdditionalFields` enables extensibility without breaking changes
- Host-agnostic design allows use with any agent framework
- Strongly typed with compile-time safety
- No external dependencies in core package

### Negative
- More types to learn and understand
- `AdditionalFields` dictionary is less type-safe than known properties
- Computed `Skill.Metadata` property may allocate unnecessarily if called repeatedly

### Mitigations
- Comprehensive documentation in PUBLIC_API.md
- Extensive test coverage (34+ tests)
- Clear naming conventions
- XML documentation comments on all public APIs
- `AdditionalFields` usage is optional and for edge cases only
- Future optimization: cache `Metadata` property if needed

## Alternatives Considered

### Alternative 1: Single Skill Model
Use one `Skill` class with optional properties for full vs metadata load.

**Rejected because:**
- Violates progressive disclosure invariant
- Unclear which properties are available in each loading phase
- Risk of accessing unloaded content
- Metadata-only load would still allocate strings for unneeded data

### Alternative 2: Exception-Based Validation
Throw exceptions for validation failures.

**Rejected because:**
- Violates project invariants (diagnostics, not exceptions)
- Poor user experience when multiple errors exist
- Exceptions are for exceptional conditions, not normal validation
- Can't collect all errors in one pass

### Alternative 3: Strongly-Typed Additional Fields
Use inheritance or interfaces for additional fields.

**Rejected because:**
- Requires code changes for new field types
- Breaks forward compatibility goal
- Overly complex for edge case feature
- Dynamic typing is appropriate for unknown fields

## References
- docs/project_brief.md - Architecture and invariants
- AGENTS.md - Working agreements
- ADR 0001 - Project structure
- Issue: Epic 2 - Spec modeling (core domain)
