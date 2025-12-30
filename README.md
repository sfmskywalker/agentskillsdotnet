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

### Installation

```bash
# Coming soon - NuGet packages will be available after initial release
dotnet add package AgentSkills
```

### Basic Usage

```csharp
// Coming soon - walking skeleton sample
// scan → metadata → validate → render list → activate skill → render instructions
```

## Project Status

🚧 **Early Development** - This project is in active development. APIs may change.

## Documentation

- [Project Brief](docs/project_brief.md) - Architecture and design principles
- [Contributing Guide](CONTRIBUTING.md) - How to contribute
- [Agent Guidelines](AGENTS.md) - Guidelines for AI coding agents
- [Architecture Decisions](docs/adr/) - ADRs documenting key decisions

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
├── fixtures/             # Test fixtures and samples
└── .github/workflows/    # CI/CD automation
```

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
