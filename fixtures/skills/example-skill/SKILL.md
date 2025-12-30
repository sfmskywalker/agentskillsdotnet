---
name: example-skill
description: An example skill for testing and demonstration
version: 1.0.0
author: AgentSkills.NET
tags:
  - example
  - demo
allowed-tools:
  - filesystem
  - calculator
---

# Example Skill

This is an example skill that demonstrates the structure of a skill definition.

## Instructions

When this skill is activated, the agent should:

1. Read the skill metadata to understand its purpose
2. Parse the YAML frontmatter for configuration
3. Load these instructions from the Markdown body
4. Access any resources in the skill directory if needed

## Usage

This skill is intended for testing and demonstration purposes. It shows:

- YAML frontmatter with metadata
- Markdown body with instructions
- Proper structure for skill definitions

## Resources

Skills may include additional resources in subdirectories:
- `scripts/` - For script files (not executed by default)
- `references/` - For reference documentation
- `assets/` - For any other supporting files
