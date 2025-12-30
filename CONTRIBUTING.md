# Contributing to AgentSkills.NET

Thank you for your interest in contributing to AgentSkills.NET! This document provides guidelines and information for contributors.

## Getting Started

### Prerequisites
- .NET 8.0 SDK or later
- Git
- A code editor (Visual Studio, VS Code, or Rider recommended)

### Building the Project
```bash
dotnet restore
dotnet build
```

### Running Tests
```bash
dotnet test
```

## Repository Structure

```
agentskillsdotnet/
├── src/                  # Core library source code
├── tests/                # Test projects
├── samples/              # Sample applications
├── docs/                 # Documentation
│   └── adr/             # Architecture Decision Records
├── fixtures/             # Test fixtures and sample data
│   └── skills/          # Sample skill definitions
├── AGENTS.md            # Guidelines for AI coding agents
├── README.md            # Project overview
└── CONTRIBUTING.md      # This file
```

## Development Workflow

### 1. Pick an Issue
- Browse [open issues](https://github.com/sfmskywalker/agentskillsdotnet/issues)
- Look for issues tagged `good first issue` if you're new
- Comment on the issue to let others know you're working on it

### 2. Create a Branch
```bash
git checkout -b feature/your-feature-name
# or
git checkout -b fix/issue-number-description
```

### 3. Make Changes
- Follow the coding conventions below
- Write tests for your changes
- Keep commits focused and atomic
- Write clear commit messages

### 4. Test Your Changes
```bash
# Run all tests
dotnet test

# Run tests for a specific project
dotnet test tests/AgentSkills.Tests
```

### 5. Submit a Pull Request
- Push your branch to GitHub
- Create a pull request with a clear description
- Link the related issue(s)
- Wait for review and address feedback

## Coding Conventions

### General Guidelines
- **Read AGENTS.md first** - It contains important invariants and working agreements
- Follow the [.NET coding conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful names for variables, methods, and classes
- Keep methods small and focused on a single responsibility
- Write XML documentation comments for public APIs

### Naming Conventions
- Use PascalCase for class names, method names, and public members
- Use camelCase for local variables and private fields
- Prefix interface names with `I` (e.g., `ISkillLoader`)
- Avoid redundant prefixes (the namespace provides context)

### Project-Specific Guidelines
- **Namespaces**: Use domain-based naming (e.g., `AgentSkills.*`), not platform-based
- **Error Handling**: Return diagnostics instead of throwing exceptions for validation errors
- **Dependencies**: Keep core libraries host-agnostic; avoid framework-specific dependencies
- **Security**: Never execute scripts by default; treat them as data

### Preferred Naming Examples
```csharp
// Good
namespace AgentSkills;
public class Skill { }
public interface ISkillLoader { }

// Avoid
namespace AgentSkills.Core.Framework;
public class AgentSkillItem { }
```

## Testing Guidelines

### Test Organization
- Place tests in corresponding test projects under `tests/`
- Mirror the source code structure in test projects
- Use descriptive test method names that explain what is being tested

### Test Naming Convention
```csharp
[Fact]
public void MethodName_Scenario_ExpectedResult()
{
    // Arrange
    // Act
    // Assert
}
```

### Fixture Usage
- Use fixtures from `fixtures/skills/` for integration tests
- Don't create ad-hoc test data strings; prefer reusable fixtures
- If adding new fixtures, document their purpose

### Test Types
- **Unit Tests**: Test individual components in isolation
- **Integration Tests**: Test the pipeline (scan → parse → validate → render)
- **Golden Tests**: For prompt renderer output (snapshot testing)

## Documentation

### When to Update Docs
- **Always**: When adding or changing public APIs
- **Always**: When making architectural decisions (create an ADR)
- **Often**: When adding features that users will interact with
- **Sometimes**: For significant internal changes that affect maintainability

### Architecture Decision Records (ADRs)
- Create ADRs for significant architectural decisions
- Place them in `docs/adr/`
- Use the format: `NNNN-title-in-kebab-case.md`
- Follow the ADR template (Status, Context, Decision, Consequences)

## Code Review Process

### What Reviewers Look For
- Correctness and completeness
- Test coverage
- Code clarity and maintainability
- Adherence to project conventions
- Security considerations
- Documentation updates

### Responding to Feedback
- Be open to suggestions and constructive criticism
- Ask questions if feedback is unclear
- Make requested changes or explain why an alternative is better
- Mark conversations as resolved when addressed

## Project Invariants (Do Not Break)

These are hard rules from the project architecture:

1. **Metadata-only scan must not load large files**
   - Listing skills should be fast and lightweight
   
2. **Core libraries must remain host-agnostic**
   - No dependencies on agent frameworks in core packages
   
3. **Validation returns diagnostics, not exceptions**
   - Normal validation failures should produce diagnostics
   
4. **Prompt rendering is pluggable**
   - Never hardcode prompt formats
   
5. **Progressive disclosure is mandatory**
   - List skills → activate skill → load full content

## Security Guidelines

- **Scripts are data, not executable code**
  - Never execute scripts by default
  - Hosts are responsible for sandboxing
  
- **No secrets in code**
  - Use environment variables or secure configuration
  
- **Validate all external input**
  - Treat all file contents as untrusted
  
- **Favor safety over convenience**
  - When in doubt, choose the more secure option

## Getting Help

- **Questions about the codebase?** Open a discussion or comment on an issue
- **Found a bug?** Open an issue with reproduction steps
- **Want to propose a feature?** Open an issue to discuss it first
- **Need clarification on an issue?** Comment on the issue

## License

By contributing to AgentSkills.NET, you agree that your contributions will be licensed under the project's license (see LICENSE file).

## Recognition

Contributors will be recognized in release notes and the project README. Thank you for helping make AgentSkills.NET better!
