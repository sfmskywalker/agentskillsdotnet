# Getting Started with AgentSkills.NET

Welcome! This guide will help you start using AgentSkills.NET in **under 15 minutes**.

## What is AgentSkills.NET?

AgentSkills.NET is a .NET library that implements the [Agent Skills open standard](https://agentskills.io/). It helps AI agents discover, load, and use skills defined in a standardized format.

Think of it as a **plugin system for AI agents** where skills are:
- Defined in simple Markdown files with YAML frontmatter
- Discovered automatically from directories
- Validated against a specification
- Presented to LLMs with progressive disclosure (summaries first, full details on activation)

## Quick Start (5 minutes)

### 1. Installation

```bash
# Clone the repository (NuGet packages coming soon)
git clone https://github.com/sfmskywalker/agentskillsdotnet.git
cd agentskillsdotnet

# Build the solution
dotnet build

# Run the sample
dotnet run --project samples/AgentSkills.Sample/AgentSkills.Sample.csproj
```

### 2. Your First Skill

Create a new directory `my-skills/hello-world/` with a `SKILL.md` file:

```markdown
---
name: hello-world
description: A simple greeting skill
version: 1.0.0
tags:
  - greeting
  - demo
---

# Hello World Skill

When activated, greet the user warmly and introduce yourself.

## Instructions

1. Say "Hello!" to the user
2. Introduce yourself as a helpful AI assistant
3. Ask how you can help today
```

### 3. Load and Use Your Skill

```csharp
using AgentSkills;
using AgentSkills.Loader;
using AgentSkills.Validation;
using AgentSkills.Prompts;

// Load skill metadata (fast - doesn't read full content)
var loader = new FileSystemSkillLoader();
var (metadata, diagnostics) = loader.LoadMetadata("./my-skills");

Console.WriteLine($"Found {metadata.Count} skill(s)");
foreach (var meta in metadata)
{
    Console.WriteLine($"  • {meta.Name}: {meta.Description}");
}

// Validate skills
var validator = new SkillValidator();
foreach (var meta in metadata)
{
    var result = validator.ValidateMetadata(meta);
    if (!result.IsValid)
    {
        Console.WriteLine($"Validation errors for {meta.Name}:");
        foreach (var error in result.Errors)
        {
            Console.WriteLine($"  - {error.Message}");
        }
    }
}

// Load full skill when needed
var (skill, loadDiagnostics) = loader.LoadSkill("./my-skills/hello-world");
if (skill != null)
{
    Console.WriteLine($"\nActivated: {skill.Manifest.Name}");
    Console.WriteLine($"Instructions:\n{skill.Instructions}");
}
```

## Core Concepts (5 minutes)

### Skills Are Directories

Each skill is a directory containing:
- **`SKILL.md`** (required) - YAML frontmatter + Markdown instructions
- **`scripts/`** (optional) - Script files (not executed by default)
- **`references/`** (optional) - Reference documentation
- **`assets/`** (optional) - Supporting files

### Progressive Disclosure

AgentSkills.NET follows a two-stage pattern to minimize token usage:

**Stage 1: Discovery**
```csharp
// Fast: Loads only metadata (name, description, tags, etc.)
var (metadata, _) = loader.LoadMetadata("./skills");

// Show list to LLM
var renderer = new DefaultSkillPromptRenderer();
var listPrompt = renderer.RenderSkillList(metadata);
```

**Stage 2: Activation**
```csharp
// When LLM selects a skill, load full content
var (skill, _) = loader.LoadSkill("./skills/chosen-skill");

// Show full details to LLM
var detailsPrompt = renderer.RenderSkillDetails(skill);
```

### Validation & Diagnostics

Instead of throwing exceptions, AgentSkills.NET returns diagnostics:

```csharp
var validator = new SkillValidator();
var result = validator.Validate(skill);

if (!result.IsValid)
{
    // Handle errors
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"ERROR {error.Code}: {error.Message}");
    }
}

if (result.HasWarnings)
{
    // Handle warnings
    foreach (var warning in result.Warnings)
    {
        Console.WriteLine($"WARNING {warning.Code}: {warning.Message}");
    }
}
```

## Common Patterns (5 minutes)

### Pattern 1: Loading and Filtering Skills

```csharp
// Load all skills
var skillSet = loader.LoadSkillSet("./skills");

// Filter valid skills
var validator = new SkillValidator();
var validSkills = skillSet.Skills
    .Where(s => validator.Validate(s).IsValid)
    .ToList();

// Filter by tag
var utilitySkills = validSkills
    .Where(s => s.Manifest.Tags.Contains("utility"))
    .ToList();
```

### Pattern 2: Rendering for LLMs

```csharp
var renderer = new DefaultSkillPromptRenderer();

// Render list (progressive disclosure - step 1)
var metadata = validSkills.Select(s => s.Metadata);
var listPrompt = renderer.RenderSkillList(metadata);
await SendToLLM(listPrompt);

// When LLM activates a skill (step 2)
var chosenSkill = validSkills.First(s => s.Manifest.Name == "chosen-name");
var detailsPrompt = renderer.RenderSkillDetails(chosenSkill);
await SendToLLM(detailsPrompt);
```

### Pattern 3: Controlling Visibility

```csharp
// Use options to control what's shown
var options = new PromptRenderOptions
{
    IncludeVersion = true,
    IncludeAuthor = true,
    IncludeTags = true,
    IncludeAllowedTools = false,  // Hide for security
    ResourcePolicy = ExcludeAllResourcePolicy.Instance  // Restrictive
};

var prompt = renderer.RenderSkillDetails(skill, options);
```

### Pattern 4: Error Handling

```csharp
// Load with error handling
var (skill, diagnostics) = loader.LoadSkill(skillPath);

if (skill == null)
{
    // Loading failed
    foreach (var diagnostic in diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))
    {
        Console.WriteLine($"Failed to load: {diagnostic.Message}");
    }
    return;
}

// Validate loaded skill
var result = validator.Validate(skill);
if (!result.IsValid)
{
    // Validation failed
    Console.WriteLine("Skill is invalid, cannot use it");
    return;
}

// Use skill
Console.WriteLine($"Using skill: {skill.Manifest.Name}");
```

## What's Next?

You now know the basics! Here's where to go next:

### Learn More About...

- **[Skill Authoring Guide](SKILL_AUTHORING.md)** - How to write great skills
- **[Security & Safety](SECURITY_AND_SAFETY.md)** - Understanding the security model
- **[Prompt Rendering Guide](PROMPT_RENDERING_GUIDE.md)** - Advanced rendering patterns
- **[Public API Reference](PUBLIC_API.md)** - Complete API documentation

### Try the Samples

```bash
# Basic sample (walking skeleton)
dotnet run --project samples/AgentSkills.Sample/AgentSkills.Sample.csproj

# Microsoft Agent Framework integration
dotnet run --project samples/AgentSkills.Sample.AgentFramework/AgentSkills.Sample.AgentFramework.csproj
```

### Explore Example Skills

Check out the `fixtures/skills/` directory for example skills demonstrating:
- Minimal skills (only required fields)
- Complete skills (all optional fields)
- Skills with resources (scripts, references, assets)
- Edge cases and special characters

### Join the Community

- **Report Issues**: [GitHub Issues](https://github.com/sfmskywalker/agentskillsdotnet/issues)
- **Contribute**: See [CONTRIBUTING.md](../CONTRIBUTING.md)
- **Learn the Standard**: [Agent Skills Specification](https://agentskills.io/)

## Troubleshooting

### "Directory not found" error
Make sure the path to your skills directory is correct and exists.

### Skill not loading
Check diagnostics returned by the loader:
```csharp
var (skill, diagnostics) = loader.LoadSkill(path);
foreach (var d in diagnostics)
{
    Console.WriteLine($"{d.Severity}: {d.Message}");
}
```

### Validation errors
Common issues:
- **Name must be lowercase**: Use `hello-world` not `Hello-World`
- **Name can't have consecutive hyphens**: Use `hello-world` not `hello--world`
- **Description required**: Every skill needs a description field
- **Directory name must match**: Directory name must exactly match the skill name

### Need Help?

- Check the [FAQ](FAQ.md)
- Review [PUBLIC_API.md](PUBLIC_API.md) for detailed API docs
- Open a [GitHub Discussion](https://github.com/sfmskywalker/agentskillsdotnet/discussions)

## What You Learned

✅ How to install and set up AgentSkills.NET  
✅ How to create your first skill  
✅ How to load and validate skills  
✅ Core concepts: progressive disclosure, diagnostics, security  
✅ Common patterns for working with skills  
✅ Where to find more resources  

**Time to adopt: ~15 minutes** ⏱️

Ready to build something amazing? Start by creating your first skill! 🚀
