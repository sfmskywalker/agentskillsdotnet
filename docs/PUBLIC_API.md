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
- Unclosed frontmatter (missing closing `---`) → `LOADER005` error
- Malformed YAML syntax → `LOADER005` error
- Missing required fields → `LOADER006` or `LOADER007` error
- Empty required field values → `LOADER006` or `LOADER007` error
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

The validation package provides validation rules and interfaces for validating skills against the Agent Skills v1 specification.

### Interfaces

#### `ISkillValidator`
Interface for validating skills and producing diagnostics.

**Methods:**
- `ValidationResult Validate(Skill skill)` - Validates a full skill against the v1 specification. Returns a ValidationResult containing diagnostics.
- `ValidationResult ValidateMetadata(SkillMetadata metadata)` - Validates skill metadata against the v1 specification. This is a lighter validation for metadata-only scenarios.

### Implementations

#### `SkillValidator`
Default implementation of `ISkillValidator` that validates skills against the Agent Skills v1 specification.

**Validation Rules:**

Based on the [Agent Skills v1 specification](https://agentskills.io/specification):

1. **Name Field (Required)**
   - Must be present and non-empty
   - Length: 1-64 characters
   - Pattern: lowercase letters (a-z), numbers (0-9), and hyphens only
   - Cannot start or end with a hyphen
   - Cannot contain consecutive hyphens (`--`)
   - Must match the parent directory name exactly

2. **Description Field (Required)**
   - Must be present and non-empty
   - Length: 1-1024 characters
   - Warning if less than 20 characters (recommended to be descriptive)

3. **Version Field (Optional)**
   - If present, should not be empty or whitespace-only

4. **Compatibility Field (Optional)**
   - If present, maximum 500 characters

5. **Directory Name**
   - Must match the skill name exactly

**Usage:**
```csharp
var validator = new SkillValidator();

// Validate a full skill
var skill = loader.LoadSkill("/path/to/skill");
var result = validator.Validate(skill);

if (!result.IsValid)
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"{error.Code}: {error.Message}");
    }
}

// Validate metadata only (lighter validation)
var (metadata, _) = loader.LoadMetadata("/path/to/skills");
foreach (var meta in metadata)
{
    var result = validator.ValidateMetadata(meta);
    // Process diagnostics...
}
```

**Diagnostic Codes:**
- `VAL001` - Required field 'name' is missing or empty
- `VAL002` - Field 'name' length constraint violation (1-64 characters)
- `VAL003` - Field 'name' pattern violation (lowercase, numbers, hyphens only; no leading/trailing/consecutive hyphens)
- `VAL004` - Required field 'description' is missing or empty
- `VAL005` - Field 'description' length constraint violation (1-1024 characters)
- `VAL006` - Field 'description' is very short (warning, less than 20 characters)
- `VAL007` - Field 'version' is present but empty (warning)
- `VAL008` - Field 'compatibility' exceeds maximum length (500 characters)
- `VAL009` - Cannot determine directory name to validate against skill name (warning)
- `VAL010` - Directory name does not match skill name

**CI/CD Integration:**

The validator produces structured diagnostics that can be easily consumed by CI/CD pipelines:

```csharp
// Example: Exit code based on validation (using safe plain text output)
var result = validator.Validate(skill);
if (!result.IsValid)
{
    foreach (var error in result.Errors)
    {
        // Plain text format (safe for all CI systems)
        Console.Error.WriteLine($"ERROR [{error.Code}] {error.Path}: {error.Message}");
    }
    Environment.Exit(1);
}

// Example: Show warnings
foreach (var warning in result.Warnings)
{
    Console.WriteLine($"WARNING [{warning.Code}] {warning.Path}: {warning.Message}");
}
```

**Security Note for GitHub Actions:**
If using GitHub Actions workflow commands (`::error`, `::warning`), you must escape user-controlled content to prevent workflow command injection. Paths and messages from skill files are untrusted input and should not be directly interpolated into workflow commands. Use plain text output (as shown above) or properly escape all fields before using workflow commands.

**Design Principles:**

1. **Specification Compliance**: All validation rules match the Agent Skills v1 specification exactly.
2. **Actionable Diagnostics**: Every diagnostic includes a code, message, and file path for easy debugging.
3. **CI-Friendly**: Diagnostics include all information needed for automated checks (codes, paths, clear messages).
4. **No Exceptions**: Validation produces diagnostics, never throws for validation failures.
5. **Extensible**: Additional validation rules can be added without breaking existing code.

---

## AgentSkills.Prompts

The prompts package provides rendering capabilities for transforming skills into LLM-ready prompts following progressive disclosure pattern.

### Interfaces

#### `ISkillPromptRenderer`
Interface for rendering skill information as prompts for LLMs.

**Methods:**
- `string RenderSkillList(IEnumerable<SkillMetadata> metadata, PromptRenderOptions? options = null)` - Renders a summary list of multiple skills showing only key metadata. Use this for presenting available skills to the LLM.
- `string RenderSkillDetails(Skill skill, PromptRenderOptions? options = null)` - Renders full details for a single skill including instructions and resources. Use this when the LLM activates a specific skill.

**Progressive Disclosure Pattern:**
The renderer implements progressive disclosure to minimize token usage and improve relevance:
1. First, present skills using `RenderSkillList` - shows only name, description, and key metadata
2. When a skill is activated, use `RenderSkillDetails` - shows full instructions and all metadata

#### `IResourcePolicy`
Policy interface for controlling visibility of skill resources and metadata in prompts.

**Methods:**
- `bool ShouldIncludeResource(SkillResource resource, Skill skill)` - Determines whether a specific resource should be included in the prompt. Returns true to include, false to redact.
- `bool ShouldIncludeAllowedTools(Skill skill)` - Determines whether allowed-tools metadata should be included in the prompt. Returns true to include, false to redact.

**Use Cases:**
- Security policies: Prevent exposure of sensitive resources
- Token optimization: Exclude large or irrelevant resources
- Conditional access: Apply runtime policies based on skill content

### Implementations

#### `DefaultSkillPromptRenderer`
Default implementation of `ISkillPromptRenderer` that produces clear, structured Markdown prompts.

**Features:**
- Renders skill lists with name, description, version, author, and tags
- Renders full skill details with complete instructions
- Supports optional metadata inclusion via `PromptRenderOptions`
- Integrates with resource policies for controlled visibility
- Produces clean Markdown output optimized for LLM consumption

**Usage:**
```csharp
var renderer = new DefaultSkillPromptRenderer();

// Render list of skills (progressive disclosure - step 1)
var metadata = loader.LoadMetadata("/path/to/skills");
var listPrompt = renderer.RenderSkillList(metadata);
// Send listPrompt to LLM

// When LLM activates a skill, render full details (step 2)
var (skill, diagnostics) = loader.LoadSkill("/path/to/skills/chosen-skill");
if (skill != null)
{
    var detailsPrompt = renderer.RenderSkillDetails(skill);
    // Send detailsPrompt to LLM
}
```

#### Resource Policy Implementations

##### `IncludeAllResourcePolicy`
A permissive resource policy that includes all resources and metadata.

**Usage:**
```csharp
var options = new PromptRenderOptions
{
    ResourcePolicy = IncludeAllResourcePolicy.Instance
};
var prompt = renderer.RenderSkillDetails(skill, options);
```

##### `ExcludeAllResourcePolicy`
A restrictive resource policy that excludes all resources and sensitive metadata.

**Usage:**
```csharp
var options = new PromptRenderOptions
{
    ResourcePolicy = ExcludeAllResourcePolicy.Instance
};
var prompt = renderer.RenderSkillDetails(skill, options);
// allowed-tools will not be included
```

##### `ResourceTypeFilterPolicy`
A configurable resource policy that filters resources by type.

**Usage:**
```csharp
// Only include references and assets, exclude scripts
var policy = new ResourceTypeFilterPolicy(new[] { "reference", "asset" });
var options = new PromptRenderOptions { ResourcePolicy = policy };
var prompt = renderer.RenderSkillDetails(skill, options);
```

### Options and Configuration

#### `PromptRenderOptions`
Options for controlling how skills are rendered as prompts.

**Properties:**
- `IResourcePolicy? ResourcePolicy` - The resource policy for controlling resource visibility. If null, all resources are included by default.
- `bool IncludeVersion` - Whether to include version information in rendered output. Default is true.
- `bool IncludeAuthor` - Whether to include author information in rendered output. Default is true.
- `bool IncludeTags` - Whether to include tags in rendered output. Default is true.
- `bool IncludeAllowedTools` - Whether to include allowed-tools in rendered output. Default is true.

**Usage:**
```csharp
var options = new PromptRenderOptions
{
    IncludeVersion = false,
    IncludeAuthor = false,
    IncludeTags = true,
    ResourcePolicy = ExcludeAllResourcePolicy.Instance
};
var prompt = renderer.RenderSkillDetails(skill, options);
```

### Design Principles

1. **Progressive Disclosure**: Always show summaries first, full details only when activated.
2. **Policy-Based Security**: Resource visibility is controlled by pluggable policies, not hardcoded rules.
3. **Token Optimization**: Minimize token usage by showing only relevant information at each stage.
4. **Host Control**: Hosts decide what to expose through policies and options, not the library.
5. **Clean Output**: Produces well-formatted Markdown that's easy for both LLMs and humans to read.
6. **No Side Effects**: Rendering is a pure function with no side effects or state changes.

---



## AgentSkills.Adapters.Microsoft.AgentFramework

The Microsoft Agent Framework adapter provides helpers for integrating AgentSkills with Microsoft Agent Framework agents. This is a thin adapter that keeps framework-specific code isolated from core packages.

### Core Classes

#### `SkillPromptBuilder`
Builder for creating agent instructions that include skill information following progressive disclosure pattern.

**Constructor:**
- `SkillPromptBuilder(ISkillPromptRenderer? renderer = null)` - Creates a new builder. If renderer is null, uses DefaultSkillPromptRenderer.

**Methods:**
- `WithBaseInstructions(string instructions)` - Sets the base instructions for the agent. Returns the builder for chaining.
- `WithSkills(IEnumerable<SkillMetadata> metadata)` - Adds skills to be made available to the agent. Returns the builder for chaining.
- `WithSkillSet(SkillSet skillSet)` - Adds a skill set to be made available to the agent. Returns the builder for chaining.
- `Build(PromptRenderOptions? options = null)` - Builds the complete instructions including base instructions and skill listing.
- `BuildSkillDetails(Skill skill, PromptRenderOptions? options = null)` - Renders detailed instructions for a specific activated skill.

**Usage:**
```csharp
var instructions = new SkillPromptBuilder()
    .WithBaseInstructions("You are a helpful assistant.")
    .WithSkillSet(skillSet)
    .Build();
```

**Progressive Disclosure:**
The builder implements progressive disclosure:
1. `Build()` returns base instructions + skill list (summary only)
2. `BuildSkillDetails()` returns full skill instructions (when activated)

### Extension Methods

#### `SkillExtensions`
Extension methods for integrating skills with Microsoft Agent Framework.

**Methods:**

##### `GetInstructions`
```csharp
public static string GetInstructions(
    this Skill skill,
    ISkillPromptRenderer? renderer = null,
    PromptRenderOptions? options = null)
```
Gets the skill instructions as a formatted prompt suitable for agent consumption. Use this when an agent activates a specific skill.

**Example:**
```csharp
var instructions = skill.GetInstructions();
```

##### `GetFunctionName`
```csharp
public static string GetFunctionName(this Skill skill)
```
Gets a sanitized function name for the skill. Converts hyphens to underscores (e.g., "example-skill" → "example_skill").

**Example:**
```csharp
var functionName = skill.GetFunctionName(); // "my_skill"
```

##### `GetFunctionDescription`
```csharp
public static string GetFunctionDescription(this Skill skill)
```
Gets a description suitable for function tool metadata. Returns "Activate skill: {description}".

**Example:**
```csharp
var description = skill.GetFunctionDescription();
// "Activate skill: An example skill for testing"
```

### Integration Pattern

The recommended integration pattern with Microsoft Agent Framework:

```csharp
// 1. Load skills
var loader = new FileSystemSkillLoader();
var skillSet = loader.LoadSkillSet("/path/to/skills");
var validator = new SkillValidator();
var validSkills = skillSet.Skills.Where(s => validator.Validate(s).IsValid);

// 2. Build base instructions with skill list (progressive disclosure - part 1)
var baseInstructions = new SkillPromptBuilder()
    .WithBaseInstructions("You are a helpful assistant...")
    .WithSkills(validSkills.Select(s => s.Metadata))
    .Build();

// 3. Create agent with base instructions
var agent = chatClient.CreateAIAgent(
    instructions: baseInstructions,
    name: "MyAgent");

// 4. Register each skill as a function (progressive disclosure - part 2)
foreach (var skill in validSkills)
{
    var function = AIFunctionFactory.Create(
        method: () => skill.GetInstructions(),
        name: skill.GetFunctionName(),
        description: skill.GetFunctionDescription());
    
    // Add function to agent's tools
    // agent.AddTool(function);
}

// 5. When agent calls a skill function, it receives full instructions
// This completes progressive disclosure: list → activate → load details
```

### Design Principles

1. **Thin Adapter**: Provides helpers only, no framework dependencies
2. **Progressive Disclosure**: Enforced through API design (list first, details on activation)
3. **No Bleeding**: Framework-specific code stays in adapter, never in core
4. **Host Control**: Applications decide what to expose and when
5. **Type Safe**: Leverages strong typing where possible

### No Framework Dependencies

The adapter intentionally does NOT reference Microsoft Agent Framework packages. This:
- Keeps the adapter thin and maintainable
- Avoids version lock-in and dependency conflicts
- Makes the adapter work across framework versions
- Provides flexibility in how skills are integrated

Instead, the adapter provides:
- String-based instructions compatible with any framework
- Helper methods using standard .NET types
- Clear integration patterns via documentation and samples

### Sample Application

See [AgentSkills.Sample.AgentFramework](../../samples/AgentSkills.Sample.AgentFramework/) for a complete working example demonstrating:
- Loading and validating skills
- Building agent instructions with skill listings
- Simulating skill activation
- Demonstrating the integration pattern

### Related Documentation

- [ADR 0004: Microsoft Agent Framework Adapter Design](adr/0004-microsoft-agent-framework-adapter.md)
- [Adapter README](../../src/AgentSkills.Adapters.Microsoft.AgentFramework/README.md)
- [Microsoft Agent Framework Documentation](https://learn.microsoft.com/en-us/agent-framework/)

---

