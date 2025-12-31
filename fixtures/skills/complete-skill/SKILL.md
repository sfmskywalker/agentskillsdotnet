---
name: complete-skill
description: A comprehensive skill with all possible fields and resource types for testing complete parsing
version: 2.1.0
author: Test Author
compatibility: Requires .NET 8.0 or higher
tags:
  - testing
  - comprehensive
  - validation
allowed-tools:
  - filesystem
  - network
  - calculator
---

# Complete Skill

This skill demonstrates all possible fields in the YAML frontmatter and includes all resource types.

## Purpose

This fixture is used for testing that the loader correctly handles:
- All optional metadata fields
- Multiple tags
- Multiple allowed-tools
- Resources in subdirectories (scripts, references, assets)

## Instructions

1. Verify all metadata fields are parsed correctly
2. Check that resources can be discovered in subdirectories
3. Validate that the full manifest is loaded without errors

## Resource References

This skill includes:
- Scripts in the `scripts/` directory
- Reference documentation in the `references/` directory
- Supporting assets in the `assets/` directory

All resources should be treated as data and never executed automatically.
