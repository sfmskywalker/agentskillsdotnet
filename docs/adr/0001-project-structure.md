# ADR 0001: Project Structure and Package Organization

## Status
Accepted

## Context
AgentSkills.NET needs a clear, maintainable project structure that:
- Separates core functionality from host-specific adapters
- Supports multiple .NET projects with clear boundaries
- Follows .NET conventions and best practices
- Enables independent versioning and packaging
- Makes dependencies explicit and minimizes coupling

The project must remain host-agnostic in its core packages while allowing for host-specific adapters.

## Decision

### Directory Structure
```
agentskillsdotnet/
├── src/                    # Source code
│   ├── AgentSkills/       # Core domain models and abstractions
│   ├── AgentSkills.Loader/        # Directory scanning and skill loading
│   ├── AgentSkills.Validation/    # Validation rules and diagnostics
│   ├── AgentSkills.Prompts/       # Prompt rendering abstractions
│   └── AgentSkills.Adapters.*/    # Host-specific integrations (future)
├── tests/                  # Test projects
│   └── AgentSkills.Tests/ # Main test project
├── samples/               # Sample applications
│   └── AgentSkills.Sample/ # Walking skeleton sample
├── docs/                  # Documentation
│   ├── adr/              # Architecture Decision Records
│   └── project_brief.md  # Project overview and architecture
├── fixtures/             # Test fixtures and sample data
│   └── skills/          # Example skill definitions
├── .github/
│   └── workflows/       # CI/CD workflows
├── AGENTS.md            # Guidelines for AI coding agents
├── CONTRIBUTING.md      # Contributor guidelines
├── README.md            # Project overview
└── AgentSkills.sln     # Solution file
```

### Package Boundaries

**AgentSkills (Core)**
- Domain models: `Skill`, `SkillSet`, `SkillMetadata`, `SkillManifest`
- Core abstractions and interfaces
- No external dependencies (beyond BCL)
- Host-agnostic

**AgentSkills.Loader**
- File system operations
- Directory scanning
- YAML parsing (dependency: YamlDotNet or similar)
- Markdown parsing
- References: `AgentSkills`

**AgentSkills.Validation**
- Validation rules
- Diagnostic generation
- Schema validation
- References: `AgentSkills`

**AgentSkills.Prompts**
- Prompt rendering interfaces
- Progressive disclosure implementation
- Template abstractions
- References: `AgentSkills`

**AgentSkills.Adapters.*** (Future)
- Host-specific integrations
- Microsoft Agent Framework adapter
- Semantic Kernel adapter (if needed)
- References: `AgentSkills`, specific adapter packages

### Framework Target
- .NET 8.0 (LTS)
- Allows use of latest C# features
- Broad compatibility with modern .NET applications

### Testing Strategy
- xUnit as test framework (standard in .NET ecosystem)
- Test project mirrors source structure
- Fixtures in `fixtures/` directory, reused across tests
- Integration tests cover full pipeline
- Unit tests for individual components

## Consequences

### Positive
- Clear separation of concerns
- Core libraries remain host-agnostic
- Easy to add new adapters without affecting core
- Standard .NET project structure is familiar to contributors
- Can version and release packages independently
- Test fixtures are reusable and centralized

### Negative
- More projects means more build complexity
- Need to manage inter-project dependencies carefully
- May need to split test projects as codebase grows

### Mitigations
- Use Directory.Build.props for shared MSBuild configuration
- Document package dependencies in project brief
- Keep dependencies minimal and explicit
- Review dependency graph regularly

## References
- docs/project_brief.md - Project architecture overview
- AGENTS.md - Working agreements and invariants
