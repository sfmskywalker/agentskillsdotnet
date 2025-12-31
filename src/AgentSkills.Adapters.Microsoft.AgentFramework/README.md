# AgentSkills.Adapters.Microsoft.AgentFramework

A thin adapter package that enables integration between AgentSkills.NET and the Microsoft Agent Framework.

## Overview

This adapter provides helper classes and extension methods to make it easy to expose AgentSkills to agents built with Microsoft Agent Framework. It follows the progressive disclosure pattern and ensures no runtime details bleed into the core AgentSkills packages.

## Installation

```bash
dotnet add package AgentSkills.Adapters.Microsoft.AgentFramework
```

## Key Features

- **SkillPromptBuilder**: Build agent instructions that include skill listings
- **Extension Methods**: Easily convert skills to function-compatible formats
- **Progressive Disclosure**: List skills first, then load full details when activated
- **Host-Agnostic Core**: Adapter keeps framework-specific code isolated

## Usage

### Basic Integration Pattern

```csharp
using AgentSkills;
using AgentSkills.Loader;
using AgentSkills.Adapters.Microsoft.AgentFramework;

// 1. Load skills
var loader = new FileSystemSkillLoader();
var skillSet = loader.LoadSkillSet("/path/to/skills");

// 2. Build base agent instructions with skill listing
var promptBuilder = new SkillPromptBuilder();
var baseInstructions = promptBuilder
    .WithBaseInstructions("You are a helpful assistant that can use skills.")
    .WithSkillSet(skillSet)
    .Build();

// 3. Create agent with base instructions
// var agent = chatClient.CreateAIAgent(instructions: baseInstructions, ...);

// 4. Register each skill as a tool/function
foreach (var skill in skillSet.Skills)
{
    // Get function metadata
    var functionName = skill.GetFunctionName();
    var functionDescription = skill.GetFunctionDescription();
    
    // Create function that returns skill instructions
    var function = AIFunctionFactory.Create(
        method: () => skill.GetInstructions(),
        name: functionName,
        description: functionDescription);
    
    // Add function to agent tools
}
```

### Progressive Disclosure Pattern

The adapter implements progressive disclosure to minimize token usage:

1. **List Skills**: Agent sees a summary of available skills in base instructions (names, descriptions)
2. **Activate Skill**: When needed, agent calls a skill function
3. **Load Details**: Function returns full skill instructions and metadata

Example:

```csharp
// Step 1: Agent receives base instructions with skill list
var baseInstructions = promptBuilder
    .WithSkills(skills.Select(s => s.Metadata))
    .Build();
// Output: "Available Skills: example-skill (An example skill)..."

// Step 2: Agent decides to use "example-skill" and calls the function

// Step 3: Function returns full details
var fullInstructions = skill.GetInstructions();
// Output: "# Skill: example-skill\n\n## Instructions\n\n[full content]..."
```

### Extension Methods

The adapter provides convenient extension methods:

```csharp
// Get formatted instructions
string instructions = skill.GetInstructions();

// Get instructions with custom options
var options = new PromptRenderOptions 
{ 
    ResourcePolicy = ExcludeAllResourcePolicy.Instance 
};
string safeInstructions = skill.GetInstructions(options: options);

// Get function-safe name (converts hyphens to underscores)
string functionName = skill.GetFunctionName(); // "example-skill" → "example_skill"

// Get function description
string description = skill.GetFunctionDescription(); // "Activate skill: [description]"
```

### Custom Rendering

You can provide a custom prompt renderer:

```csharp
var customRenderer = new MyCustomRenderer();
var builder = new SkillPromptBuilder(customRenderer);

// Or use with extension methods
var instructions = skill.GetInstructions(renderer: customRenderer);
```

## Architecture

This adapter is designed to be thin and focused:

- **No Framework Dependencies**: The adapter doesn't reference Microsoft Agent Framework packages directly
- **Helper Utilities Only**: Provides helpers for common integration patterns
- **Core Independence**: Core AgentSkills packages remain completely host-agnostic
- **Separation of Concerns**: All framework-specific logic is isolated to this adapter

## Examples

See the [AgentSkills.Sample.AgentFramework](../../samples/AgentSkills.Sample.AgentFramework) project for a complete working example.

## Design Principles

1. **Thin Adapter**: Keep the adapter minimal and focused on integration helpers
2. **Progressive Disclosure**: Always follow the pattern: list → activate → load
3. **No Bleeding**: Runtime details stay in adapter, never in core packages
4. **Host Control**: The host (your application) decides what to expose and when

## Related Packages

- **AgentSkills**: Core domain models
- **AgentSkills.Loader**: Load skills from file system
- **AgentSkills.Validation**: Validate skills against specification
- **AgentSkills.Prompts**: Render skills as prompts for LLMs

## License

See the [LICENSE](../../LICENSE) file for license information.
