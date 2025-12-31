# Prompt Packaging & Progressive Disclosure Guide

This guide explains how to use AgentSkills.NET's prompt rendering capabilities to present skills to LLMs efficiently.

## Overview

Progressive disclosure is a design pattern that presents information in stages:
1. **Discovery**: Show skill summaries (name, description, metadata)
2. **Activation**: Show full details when a skill is selected

This reduces token usage and improves relevance by showing only what's needed at each stage.

## Basic Usage

### Step 1: List Available Skills

First, present a list of available skills to the LLM:

```csharp
using AgentSkills;
using AgentSkills.Loader;
using AgentSkills.Prompts;

// Load skill metadata (fast, doesn't read full content)
var loader = new FileSystemSkillLoader();
var (metadata, diagnostics) = loader.LoadMetadata("/path/to/skills");

// Render as a prompt for the LLM
var renderer = new DefaultSkillPromptRenderer();
var listPrompt = renderer.RenderSkillList(metadata);

// Send to LLM
Console.WriteLine(listPrompt);
```

**Output:**
```markdown
# Available Skills

The following skills are available. To use a skill, activate it by name.

## example-skill

**Description:** An example skill for testing and demonstration

**Version:** 1.0.0

**Author:** AgentSkills.NET

**Tags:** example, demo

---

## another-skill

**Description:** Another skill for different purposes

**Tags:** utility, helper

---
```

### Step 2: Show Full Skill Details

When the LLM activates a skill, provide full details:

```csharp
// Load the full skill
var (skill, diagnostics) = loader.LoadSkill("/path/to/skills/example-skill");

if (skill != null)
{
    // Render full details
    var detailsPrompt = renderer.RenderSkillDetails(skill);
    
    // Send to LLM
    Console.WriteLine(detailsPrompt);
}
```

**Output:**
```markdown
# Skill: example-skill

**Description:** An example skill for testing and demonstration

**Version:** 1.0.0

**Author:** AgentSkills.NET

**Tags:** example, demo

**Allowed Tools:** filesystem, calculator

## Instructions

[Full markdown instructions from the skill...]
```

## Controlling Visibility

### Using PromptRenderOptions

Control what metadata appears in prompts:

```csharp
var options = new PromptRenderOptions
{
    IncludeVersion = false,    // Don't show version
    IncludeAuthor = false,     // Don't show author
    IncludeTags = true,        // Show tags
    IncludeAllowedTools = true // Show allowed tools
};

var prompt = renderer.RenderSkillDetails(skill, options);
```

### Using Resource Policies

Control whether sensitive information is exposed:

#### Include Everything (Default)

```csharp
var options = new PromptRenderOptions
{
    ResourcePolicy = IncludeAllResourcePolicy.Instance
};

var prompt = renderer.RenderSkillDetails(skill, options);
```

#### Exclude Everything (Secure)

```csharp
var options = new PromptRenderOptions
{
    ResourcePolicy = ExcludeAllResourcePolicy.Instance
};

var prompt = renderer.RenderSkillDetails(skill, options);
// allowed-tools will be hidden
```

#### Filter by Resource Type

```csharp
// Only show references and assets, hide scripts
var policy = new ResourceTypeFilterPolicy(new[] { "reference", "asset" });
var options = new PromptRenderOptions { ResourcePolicy = policy };

var prompt = renderer.RenderSkillDetails(skill, options);
```

## Custom Resource Policies

Implement `IResourcePolicy` for custom control:

```csharp
public class CustomResourcePolicy : IResourcePolicy
{
    public bool ShouldIncludeResource(SkillResource resource, Skill skill)
    {
        // Example: Only include resources from trusted skills
        if (!string.IsNullOrEmpty(skill.Manifest.Author) && skill.Manifest.Author == "TrustedAuthor")
            return true;
        
        // Or only certain file types
        return resource.Name.EndsWith(".md") || resource.Name.EndsWith(".txt");
    }
    
    public bool ShouldIncludeAllowedTools(Skill skill)
    {
        // Example: Only expose allowed-tools for verified skills
        return skill.Manifest.Version?.StartsWith("1.") == true;
    }
}

// Use the custom policy
var options = new PromptRenderOptions
{
    ResourcePolicy = new CustomResourcePolicy()
};
```

## Integration Patterns

### Pattern 1: Two-Stage Discovery

```csharp
// Stage 1: Show list
var metadata = loader.LoadMetadata(skillsPath);
var listPrompt = renderer.RenderSkillList(metadata);
await SendToLLM(listPrompt);

// Wait for LLM to choose a skill
var chosenSkillName = await GetLLMChoice();

// Stage 2: Show full details
var (skill, _) = loader.LoadSkill($"{skillsPath}/{chosenSkillName}");
if (skill != null)
{
    var detailsPrompt = renderer.RenderSkillDetails(skill);
    await SendToLLM(detailsPrompt);
}
```

### Pattern 2: Filtered Lists

```csharp
// Show only skills with specific tags
var taggedMetadata = metadata.Where(m => m.Tags.Contains("utility"));
var filteredPrompt = renderer.RenderSkillList(taggedMetadata);
```

### Pattern 3: Security-Conscious Rendering

```csharp
// Different policies for different environments
var policy = environment == "production"
    ? ExcludeAllResourcePolicy.Instance
    : IncludeAllResourcePolicy.Instance;

var options = new PromptRenderOptions { ResourcePolicy = policy };
var prompt = renderer.RenderSkillDetails(skill, options);
```

### Pattern 4: Token Optimization

```csharp
// Minimal metadata for token savings
var minimalOptions = new PromptRenderOptions
{
    IncludeVersion = false,
    IncludeAuthor = false,
    IncludeTags = false,
    IncludeAllowedTools = false
};

var compactPrompt = renderer.RenderSkillDetails(skill, minimalOptions);
```

## Best Practices

1. **Always validate before rendering**: Use `SkillValidator` to ensure skills are valid before presenting them to LLMs.

2. **Handle diagnostics**: Check for errors and warnings in loader diagnostics before rendering.

3. **Use metadata loading for lists**: `LoadMetadata()` is much faster than `LoadSkillSet()` for discovery.

4. **Apply appropriate policies**: Use restrictive policies in production environments.

5. **Cache rendered prompts**: If the same skill is activated multiple times, cache the rendered output.

6. **Filter invalid skills**: Only render skills that pass validation.

```csharp
var validator = new SkillValidator();
var validMetadata = new List<SkillMetadata>();

foreach (var meta in metadata)
{
    var result = validator.ValidateMetadata(meta);
    if (result.IsValid)
    {
        validMetadata.Add(meta);
    }
}

// Only render valid skills
var prompt = renderer.RenderSkillList(validMetadata);
```

## Advanced: Custom Renderers

Implement `ISkillPromptRenderer` for custom formatting:

```csharp
public class JsonPromptRenderer : ISkillPromptRenderer
{
    public string RenderSkillList(IEnumerable<SkillMetadata> metadata, PromptRenderOptions? options = null)
    {
        var skills = metadata.Select(m => new
        {
            m.Name,
            m.Description,
            m.Version,
            m.Tags
        });
        
        return JsonSerializer.Serialize(new { skills }, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
    }
    
    public string RenderSkillDetails(Skill skill, PromptRenderOptions? options = null)
    {
        return JsonSerializer.Serialize(new
        {
            skill.Manifest.Name,
            skill.Manifest.Description,
            skill.Instructions,
            Metadata = new
            {
                skill.Manifest.Version,
                skill.Manifest.Author,
                skill.Manifest.Tags,
                skill.Manifest.AllowedTools
            }
        }, new JsonSerializerOptions { WriteIndented = true });
    }
}
```

## Troubleshooting

### Prompts are too large

- Use `PromptRenderOptions` to exclude unnecessary metadata
- Apply a restrictive `ResourcePolicy`
- Filter skills by tag or other criteria before rendering

### Sensitive information exposed

- Use `ExcludeAllResourcePolicy` or `ResourceTypeFilterPolicy`
- Set `IncludeAllowedTools = false` in options
- Validate skill content before rendering

### Skills not appearing

- Check loader diagnostics for errors
- Validate skills before rendering
- Ensure skills pass validation rules

## Next Steps

- See [PUBLIC_API.md](PUBLIC_API.md) for complete API reference
- See [samples/AgentSkills.Sample](../samples/AgentSkills.Sample) for working examples
- See [PROJECT_BRIEF.md](project_brief.md) for architecture overview
