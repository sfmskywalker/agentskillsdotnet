# AgentSkills.NET

[![CI](https://github.com/sfmskywalker/agentskillsdotnet/workflows/CI/badge.svg)](https://github.com/sfmskywalker/agentskillsdotnet/actions)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

A .NET implementation of the [Agent Skills open standard](https://agentskills.io/) by Anthropic.

## Overview

AgentSkills.NET enables .NET-based AI agents to discover, load, and execute skills defined in a standardized format. It provides:

- **Skill Discovery**: Scan directories for skill definitions
- **YAML + Markdown Parsing**: Parse `SKILL.md` files with frontmatter and instructions
- **Validation**: Comprehensive validation with diagnostics (no exceptions for normal errors)
- **Progressive Disclosure**: List skills first, load full content only when activated
- **Host-Agnostic Design**: Core libraries work with any agent framework
- **Safety-First**: Scripts are treated as data; no execution by default

## Quick Start

**New to AgentSkills.NET?** Start here: **[📘 Getting Started Guide](docs/GETTING_STARTED.md)** - Get up and running in 15 minutes!

### Installation

```bash
# Clone and build (NuGet packages coming soon)
git clone https://github.com/sfmskywalker/agentskillsdotnet.git
cd agentskillsdotnet
dotnet build
```

### Basic Usage

```csharp
using AgentSkills.Loader;
using AgentSkills.Validation;
using AgentSkills.Prompts;

// Load skill metadata (fast path - no full content)
var loader = new FileSystemSkillLoader();
var (metadata, diagnostics) = loader.LoadMetadata("./skills");

// Validate and filter
var validator = new SkillValidator();
var validMetadata = metadata
    .Where(m => validator.ValidateMetadata(m).IsValid)
    .ToList();

// Render list for LLM (progressive disclosure - stage 1)
var renderer = new DefaultSkillPromptRenderer();
var listPrompt = renderer.RenderSkillList(validMetadata);

// When LLM activates a skill, load full details (stage 2)
var (skill, _) = loader.LoadSkill("./skills/chosen-skill");
var detailsPrompt = renderer.RenderSkillDetails(skill);
```

### Try the Samples

```bash
# Run the walking skeleton sample (basics)
dotnet run --project samples/AgentSkills.Sample/AgentSkills.Sample.csproj

# Run the Microsoft Agent Framework integration demo (explains concepts)
dotnet run --project samples/AgentSkills.Sample.AgentFramework/AgentSkills.Sample.AgentFramework.csproj

# Run the full Microsoft Agent Framework sample (working tools + function calling)
dotnet run --project samples/AgentSkills.MicrosoftAgentFramework.Sample/AgentSkills.MicrosoftAgentFramework.Sample.csproj
```

## Project Status

🚧 **Early Development** - This project is in active development. APIs may change.

## Documentation

### Getting Started
- **[📘 Getting Started Guide](docs/GETTING_STARTED.md)** - 15-minute quick start tutorial
- **[✍️ Skill Authoring Guide](docs/SKILL_AUTHORING.md)** - How to write excellent skills
- **[❓ FAQ](docs/FAQ.md)** - Frequently asked questions

### Core Concepts
- [🔒 Security & Safety Guide](docs/SECURITY_AND_SAFETY.md) - Security model and host guarantees
- [📋 Project Brief](docs/project_brief.md) - Architecture and design principles
- [📝 Prompt Rendering Guide](docs/PROMPT_RENDERING_GUIDE.md) - Progressive disclosure patterns
- [💡 Usage Scenarios](docs/USAGE_SCENARIOS.md) - Real-world integration examples

### API & Reference
- [📚 Public API Reference](docs/PUBLIC_API.md) - Complete API documentation
- [🧪 Testing Guide](docs/TESTING_GUIDE.md) - Testing strategy and best practices
- [🔧 Troubleshooting Guide](docs/TROUBLESHOOTING.md) - Common issues and solutions
- [🔍 Reference Comparison](docs/COMPARISON_SUMMARY.md) - Comparison with Python reference implementation

### Contributing
- [🤝 Contributing Guide](CONTRIBUTING.md) - How to contribute
- [🤖 Agent Guidelines](AGENTS.md) - Guidelines for AI coding agents
- [📐 Architecture Decisions](docs/adr/) - ADRs documenting key decisions

## Repository Structure

```
├── src/                   # Core library source code
│   ├── AgentSkills/      # Core domain models
│   ├── AgentSkills.Loader/      # Skill loading
│   ├── AgentSkills.Validation/  # Validation rules
│   └── AgentSkills.Prompts/     # Prompt rendering
├── tests/                # Test projects
├── samples/              # Sample applications
├── docs/                 # Documentation
├── fixtures/             # Test fixtures and example skills
│   └── skills/          # Example skills demonstrating patterns
└── .github/workflows/    # CI/CD automation
```

## Example Skills

The [`fixtures/skills/`](fixtures/skills/) directory contains example skills demonstrating various patterns:

**Complete Examples:**
- [`example-skill`](fixtures/skills/example-skill/) - Basic skill structure
- [`complete-skill`](fixtures/skills/complete-skill/) - All optional fields and resources
- [`minimal-skill`](fixtures/skills/minimal-skill/) - Minimal valid skill
- [`email-sender`](fixtures/skills/email-sender/) - Real-world email composition skill
- [`code-reviewer`](fixtures/skills/code-reviewer/) - Systematic code review workflow

**Edge Cases:**
- [`special-chars-skill`](fixtures/skills/special-chars-skill/) - Special characters and Unicode
- [`large-instructions-skill`](fixtures/skills/large-instructions-skill/) - Performance testing

See the [fixtures README](fixtures/README.md) for a complete catalog.

## Building

```bash
# Restore dependencies
dotnet restore

# Build all projects
dotnet build

# Run tests
dotnet test
```

## Contributing

Contributions are welcome! Please read our [Contributing Guide](CONTRIBUTING.md) before submitting PRs.

Key points:
- Follow the conventions in [AGENTS.md](AGENTS.md)
- Keep changes small and focused
- Add tests for new functionality
- Update documentation as needed

## Design Principles

1. **Safety First**: No script execution by default
2. **Progressive Disclosure**: Load metadata fast, full content on demand
3. **Host Agnostic**: Core libraries don't depend on specific frameworks
4. **Diagnostics over Exceptions**: Return diagnostics for validation errors
5. **Explicit APIs**: Boring, clear, discoverable interfaces

See [docs/project_brief.md](docs/project_brief.md) for details.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [Agent Skills Standard](https://agentskills.io/) by Anthropic
- The .NET and AI communities
