# AgentSkills.NET - Frequently Asked Questions (FAQ)

Common questions and answers about AgentSkills.NET.

## General Questions

### What is AgentSkills.NET?

AgentSkills.NET is a .NET library that implements the [Agent Skills open standard](https://agentskills.io/). It enables AI agents to discover, load, and use skills defined in a standardized format (YAML frontmatter + Markdown).

### Why should I use AgentSkills.NET?

- **Standardization**: Follows an open standard for agent skills
- **Safety**: Scripts treated as data, not executed by default
- **Progressive Disclosure**: Efficient token usage with two-stage loading
- **Host-Agnostic**: Works with any .NET agent framework
- **Production-Ready**: Comprehensive validation and error handling

### Is it ready for production?

AgentSkills.NET is in early development. APIs may change. Use in production at your own risk, and pin to specific versions.

### What .NET versions are supported?

.NET 8.0 or higher is required.

## Installation & Setup

### How do I install AgentSkills.NET?

Currently, clone the repository and build locally:
```bash
git clone https://github.com/sfmskywalker/agentskillsdotnet.git
cd agentskillsdotnet
dotnet build
```

NuGet packages will be available after the initial release.

### How do I get started quickly?

See the [Getting Started Guide](GETTING_STARTED.md) for a 15-minute tutorial.

### Do I need any other dependencies?

The core library has minimal dependencies:
- YamlDotNet (for YAML parsing)
- Standard .NET libraries

No agent framework dependencies are required in core packages.

## Skill Authoring

### What makes a valid skill?

A valid skill requires:
1. A directory with a name matching the skill name
2. A `SKILL.md` file with:
   - YAML frontmatter enclosed in `---` delimiters
   - Required fields: `name` and `description`
   - Markdown body with instructions

See [Skill Authoring Guide](SKILL_AUTHORING.md) for details.

### Why can't I use uppercase in skill names?

The Agent Skills specification requires lowercase names with hyphens for consistency and URL-safety. This prevents issues like `MySkill` vs `myskill` being treated differently.

### Can skill names contain underscores?

No. Only lowercase letters (a-z), numbers (0-9), and hyphens are allowed. Use `my-skill`, not `my_skill`.

### Why does my directory name need to match the skill name?

This is required by the specification for consistency and discoverability. If your skill is named `hello-world`, the directory must be `hello-world/`.

### How long can skill names and descriptions be?

- **Name**: 1-64 characters
- **Description**: 1-1024 characters (recommend 20-200)

### Can I use custom fields in the YAML frontmatter?

Yes! Unknown fields are preserved in `SkillManifest.AdditionalFields` for extensibility.

```yaml
---
name: my-skill
description: Example
custom-field: my value
x-organization-id: 12345
---
```

### Can skills include resources like scripts or documentation?

Yes! Skills can include optional subdirectories:
- `scripts/` - Script files (not executed by default)
- `references/` - Reference documentation  
- `assets/` - Supporting files

## Loading & Validation

### What's the difference between LoadMetadata and LoadSkillSet?

- **`LoadMetadata()`**: Fast path - reads only YAML frontmatter, not full content. Use for listing skills.
- **`LoadSkillSet()`**: Reads full skill content including instructions. Use when skills are activated.

This is progressive disclosure in action.

### Why am I getting validation errors?

Common validation errors:
- **VAL001/VAL004**: Missing required field (name or description)
- **VAL002/VAL005**: Field length violation
- **VAL003**: Invalid characters in name or consecutive hyphens
- **VAL010**: Directory name doesn't match skill name

Run the validator to see specific errors:
```csharp
var validator = new SkillValidator();
var result = validator.Validate(skill);
foreach (var error in result.Errors)
{
    Console.WriteLine($"{error.Code}: {error.Message}");
}
```

### Do validation errors throw exceptions?

No! AgentSkills.NET returns diagnostics instead of throwing exceptions for normal validation failures. This prevents denial-of-service attacks via malformed inputs.

### How do I handle diagnostics?

```csharp
var (skill, diagnostics) = loader.LoadSkill(path);

if (skill == null)
{
    // Loading failed - check diagnostics
    foreach (var d in diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))
    {
        Console.WriteLine($"ERROR: {d.Message}");
    }
    return;
}

// Validate loaded skill
var result = validator.Validate(skill);
if (!result.IsValid)
{
    // Handle validation errors
}
```

### Can I load skills from HTTP/Git?

No, core libraries only support filesystem loading. This is by design for security. You can implement your own loader for remote sources, but you're responsible for security.

## Security

### Are skills safe to execute?

Skills are **data**, not executable code. AgentSkills.NET never executes anything. If you choose to execute scripts from skills, **you** are responsible for sandboxing.

### What about the `allowed-tools` field?

It's **advisory only**. It tells you what tools the skill expects, but doesn't grant any permissions. Your host controls what's actually allowed.

### Can malicious skills harm my system?

Not through AgentSkills.NET directly. The library only reads text files. However:
- Malicious instructions could mislead the LLM
- Scripts (if you execute them) could be harmful
- Always load skills from trusted sources

See [Security and Safety Guide](SECURITY_AND_SAFETY.md) for details.

### How do I load skills safely?

```csharp
// ✅ SAFE: Load from trusted path
var trustedPath = "/app/skills";
var skillSet = loader.LoadSkillSet(trustedPath);

// ❌ DANGEROUS: Load from user input
var userPath = Request.Query["path"];  // DON'T DO THIS
var skillSet = loader.LoadSkillSet(userPath);
```

### Should I validate skill content before using it?

Yes! Always:
1. Load from trusted sources only
2. Validate using `SkillValidator`
3. Scan for malicious patterns if loading from untrusted sources
4. Use restrictive resource policies when rendering

## Rendering & Prompts

### How do I present skills to an LLM?

Use progressive disclosure:

```csharp
// Stage 1: Show list
var (metadata, _) = loader.LoadMetadata("./skills");
var listPrompt = renderer.RenderSkillList(metadata);
await SendToLLM(listPrompt);

// Stage 2: Show full details when activated
var (skill, _) = loader.LoadSkill("./skills/chosen-skill");
var detailsPrompt = renderer.RenderSkillDetails(skill);
await SendToLLM(detailsPrompt);
```

### Can I customize the rendered output?

Yes! Use `PromptRenderOptions`:

```csharp
var options = new PromptRenderOptions
{
    IncludeVersion = false,
    IncludeAuthor = false,
    IncludeAllowedTools = false,
    ResourcePolicy = ExcludeAllResourcePolicy.Instance
};

var prompt = renderer.RenderSkillDetails(skill, options);
```

### What are resource policies?

Resource policies control what information is included in rendered prompts:

- **IncludeAllResourcePolicy**: Include everything (default)
- **ExcludeAllResourcePolicy**: Exclude sensitive data
- **ResourceTypeFilterPolicy**: Include only specific resource types
- **Custom**: Implement `IResourcePolicy` for your needs

### Can I create my own renderer?

Yes! Implement `ISkillPromptRenderer`:

```csharp
public class JsonPromptRenderer : ISkillPromptRenderer
{
    public string RenderSkillList(IEnumerable<SkillMetadata> metadata, PromptRenderOptions? options = null)
    {
        // Your implementation
    }
    
    public string RenderSkillDetails(Skill skill, PromptRenderOptions? options = null)
    {
        // Your implementation
    }
}
```

## Integration

### How do I integrate with Microsoft Agent Framework?

See the [Microsoft Agent Framework adapter](../src/AgentSkills.Adapters.Microsoft.AgentFramework/) and [sample](../samples/AgentSkills.Sample.AgentFramework/).

Key pattern:
```csharp
// Load and validate skills
var skillSet = loader.LoadSkillSet("./skills");
var validSkills = skillSet.Skills.Where(s => validator.Validate(s).IsValid);

// Build instructions with skill list
var instructions = new SkillPromptBuilder()
    .WithBaseInstructions("You are a helpful assistant")
    .WithSkillSet(skillSet)
    .Build();

// Register skills as functions (they return full instructions when called)
```

### Can I use this with other agent frameworks?

Yes! Core libraries are host-agnostic. You can integrate with any framework:
- Semantic Kernel
- LangChain
- Custom frameworks

Just load skills, validate, and render prompts as needed.

### How do I filter skills by tag?

```csharp
var skillSet = loader.LoadSkillSet("./skills");

// Filter by single tag
var utilitySkills = skillSet.Skills
    .Where(s => s.Manifest.Tags.Contains("utility"))
    .ToList();

// Filter by multiple tags (any match)
var tags = new[] { "data", "analysis" };
var dataSkills = skillSet.Skills
    .Where(s => s.Manifest.Tags.Intersect(tags).Any())
    .ToList();
```

## Performance

### Is loading skills fast?

Yes! For listing:
- Use `LoadMetadata()` which reads only frontmatter via streaming
- Typical: < 1 second for hundreds of skills

For full loading:
- `LoadSkillSet()` reads all content
- Typical: < 5 seconds for dozens of skills

### How can I optimize performance?

1. **Use metadata loading for listing**: Only load full skills when activated
2. **Cache rendered prompts**: If skills don't change, cache the output
3. **Limit skill count**: Don't load thousands of skills
4. **Use filtering**: Load only skills with relevant tags

### What about memory usage?

- Metadata loading uses streaming (low memory)
- Full loading keeps skill content in memory
- Typical: < 1 MB per skill with moderate instructions

## Troubleshooting

### Skills aren't being found

Check:
1. Path is correct and directory exists
2. Each skill directory contains `SKILL.md`
3. YAML frontmatter is properly formatted
4. Check diagnostics for loading errors

### "LOADER004: YAML frontmatter not found"

Your `SKILL.md` must start with `---` and have closing `---`:

```markdown
---
name: my-skill
description: My description
---

# Instructions

...
```

### "VAL010: Directory name does not match skill name"

Directory name must exactly match the skill name:
- Skill name: `hello-world`
- Directory: `hello-world/` ✅
- Directory: `Hello-World/` ❌

### Tests are failing

Common causes:
1. **Golden file tests**: Rendering changed - update with `UPDATE_GOLDEN_FILES=1 dotnet test`
2. **Validation tests**: Rules changed - update test expectations
3. **Path issues**: Ensure fixture paths are correct

### Build fails

Try:
```bash
dotnet clean
dotnet restore
dotnet build
```

## Contributing

### How can I contribute?

See [CONTRIBUTING.md](../CONTRIBUTING.md) and [AGENTS.md](../AGENTS.md) for guidelines.

Areas we need help:
- Documentation improvements
- Example skills
- Bug reports and fixes
- Performance optimizations
- Integration adapters

### What's the development workflow?

1. Pick an issue (look for `good first issue` label)
2. Create a branch
3. Make focused changes
4. Add tests
5. Update docs
6. Submit PR

### How do I run tests?

```bash
# All tests
dotnet test

# Specific test suite
dotnet test --filter "FullyQualifiedName~SkillValidatorTests"

# With verbose output
dotnet test --logger "console;verbosity=detailed"
```

### Where should I add my example skill?

Add example skills to `fixtures/skills/` with a descriptive name demonstrating a specific pattern or use case.

## Support

### Where can I get help?

- **Documentation**: Start with [Getting Started](GETTING_STARTED.md)
- **Issues**: [GitHub Issues](https://github.com/sfmskywalker/agentskillsdotnet/issues)
- **Discussions**: [GitHub Discussions](https://github.com/sfmskywalker/agentskillsdotnet/discussions)
- **API Docs**: See [PUBLIC_API.md](PUBLIC_API.md)

### How do I report a bug?

Open a GitHub issue with:
1. Description of the problem
2. Steps to reproduce
3. Expected vs actual behavior
4. Version/commit hash
5. Sample skill if applicable

### How do I request a feature?

Open a GitHub issue describing:
1. The feature you want
2. Why it's useful
3. How you envision it working
4. Whether you'd be willing to contribute

### Is there a community?

The project is in early stages. Community resources:
- GitHub Discussions
- Issue tracker
- Contributing via PRs

## Roadmap

### When will NuGet packages be available?

After the initial stable release (v1.0.0). Subscribe to releases on GitHub for notifications.

### What features are planned?

Check the GitHub issues for planned features. Key areas:
- Additional validation rules
- More integration adapters
- Performance optimizations
- Enhanced security features

### Can I influence the roadmap?

Yes! Open issues, participate in discussions, and contribute PRs. Community input helps prioritize features.

## Additional Resources

- [Getting Started Guide](GETTING_STARTED.md)
- [Skill Authoring Guide](SKILL_AUTHORING.md)
- [Security and Safety Guide](SECURITY_AND_SAFETY.md)
- [Public API Reference](PUBLIC_API.md)
- [Testing Guide](TESTING_GUIDE.md)
- [Prompt Rendering Guide](PROMPT_RENDERING_GUIDE.md)
- [Contributing Guidelines](../CONTRIBUTING.md)
- [Agent Skills Specification](https://agentskills.io/)

---

**Didn't find your question?** Open a [GitHub Discussion](https://github.com/sfmskywalker/agentskillsdotnet/discussions) or check existing issues.
