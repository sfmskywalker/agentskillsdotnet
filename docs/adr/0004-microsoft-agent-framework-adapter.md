# ADR 0004: Microsoft Agent Framework Adapter Design

## Status

Accepted

## Context

AgentSkills.NET provides core functionality for loading, validating, and rendering skills according to the Agent Skills specification. To integrate with specific agent frameworks like Microsoft Agent Framework, we need adapter packages that bridge AgentSkills with framework-specific APIs while maintaining clean separation between core and framework-specific concerns.

Key requirements:
- Adapters should not pollute core packages with framework dependencies
- Integration should be simple and straightforward for developers
- Progressive disclosure pattern must be preserved
- Host applications should maintain control over skill exposure

## Decision

We implement a thin adapter pattern with the following design:

### Package Structure

Create `AgentSkills.Adapters.Microsoft.AgentFramework` as a separate package that:
- References core AgentSkills packages (AgentSkills, AgentSkills.Prompts)
- Does NOT reference Microsoft Agent Framework packages directly
- Provides helpers that work with standard .NET types

### Core Components

#### 1. SkillPromptBuilder
A builder class for constructing agent instructions that include skill listings:
```csharp
var instructions = new SkillPromptBuilder()
    .WithBaseInstructions("You are a helpful assistant...")
    .WithSkillSet(skillSet)
    .Build();
```

**Rationale**: Provides a fluent API for combining base instructions with skill listings, following progressive disclosure.

#### 2. SkillExtensions
Extension methods that make skills easy to integrate with function-based frameworks:
- `GetInstructions()`: Returns formatted skill details
- `GetFunctionName()`: Converts skill name to function-safe format (hyphens → underscores)
- `GetFunctionDescription()`: Returns function description for tool metadata

**Rationale**: Extension methods are discoverable, type-safe, and keep integration code clean.

### Integration Pattern

The adapter supports this workflow:
1. Load skills using core libraries
2. Build base instructions with skill list (progressive disclosure - part 1)
3. Register skills as functions/tools in the framework
4. When a skill function is called, return full instructions (progressive disclosure - part 2)

### No Framework Dependencies

The adapter intentionally does NOT reference Microsoft Agent Framework packages. Instead, it provides:
- String-based instructions that can be passed to any framework
- Helper methods that work with standard types
- Clear documentation on how to integrate with the framework

**Rationale**: This keeps the adapter thin, reduces dependency conflicts, and makes it easier to maintain as the framework evolves.

## Consequences

### Positive

1. **Clean Separation**: Core packages remain completely host-agnostic
2. **Thin Adapter**: Minimal code, easy to understand and maintain
3. **Flexible**: Helper methods work with any function-based agent framework
4. **No Version Lock-in**: No direct framework dependencies means no version conflicts
5. **Progressive Disclosure**: Pattern is enforced by design
6. **Discoverable**: Extension methods show up in IntelliSense

### Negative

1. **Manual Function Registration**: Developers must manually register functions with the framework
2. **No Type Safety for Framework APIs**: Since we don't reference the framework, no compile-time checks for framework-specific code
3. **Documentation Heavy**: Requires good docs and examples to show integration pattern

### Neutral

1. **Not a Complete Abstraction**: This is a helper package, not a full framework abstraction
2. **Host Responsibility**: Host applications decide how to expose skills (security, filtering, etc.)

## Alternatives Considered

### Alternative 1: Full Framework Integration
Create an adapter that directly references Microsoft Agent Framework and provides AIFunction implementations.

**Rejected because**:
- Adds framework dependencies to adapter
- Creates version lock-in
- Violates "thin adapter" principle
- Would require different adapters for each framework version

### Alternative 2: No Adapter Package
Just provide documentation on how to integrate skills.

**Rejected because**:
- Duplicated code across projects
- No standard integration pattern
- Harder to maintain consistency
- Poor developer experience

### Alternative 3: Generic Agent Abstraction
Create a generic agent abstraction that works across all frameworks.

**Rejected because**:
- Too ambitious for initial implementation
- Would require abstracting over framework-specific features
- Not aligned with "thin adapter" goal
- Violates YAGNI principle

## Implementation Notes

### Function Name Conversion
Skill names use hyphens (per Agent Skills spec: "example-skill"). Function names typically use underscores. The adapter converts: `example-skill` → `example_skill`.

### Progressive Disclosure
The adapter enforces progressive disclosure through its API design:
1. `SkillPromptBuilder.Build()` returns summary
2. `SkillExtensions.GetInstructions()` returns full details

This makes it hard to accidentally violate the pattern.

### Resource Policies
The adapter respects `PromptRenderOptions` and `IResourcePolicy`, allowing hosts to control what metadata is exposed to agents.

## References

- [Agent Skills Specification](https://agentskills.io/)
- [Microsoft Agent Framework Documentation](https://learn.microsoft.com/en-us/agent-framework/)
- [Progressive Disclosure Pattern](../PROMPT_RENDERING_GUIDE.md)
- [Sample Implementation](../../samples/AgentSkills.Sample.AgentFramework/)

## Future Enhancements

Potential future improvements (not in scope for initial implementation):
- Helper methods for specific framework versions (if needed)
- Batch function registration helpers
- Integration with framework-specific telemetry
- Support for skill resource files as framework tools

These would be additive and backward-compatible.
