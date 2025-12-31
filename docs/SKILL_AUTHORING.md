# Skill Authoring Guide

This guide teaches you how to write excellent skills for AgentSkills.NET.

## Table of Contents

- [Skill Structure](#skill-structure)
- [Writing Good Skills](#writing-good-skills)
- [Metadata Reference](#metadata-reference)
- [Validation Rules](#validation-rules)
- [Best Practices](#best-practices)
- [Common Patterns](#common-patterns)
- [Examples](#examples)

## Skill Structure

A skill is a directory containing a `SKILL.md` file and optional resource folders.

### Basic Structure

```
my-skill/
├── SKILL.md           # Required: skill definition
├── scripts/           # Optional: script files
├── references/        # Optional: reference docs
└── assets/           # Optional: supporting files
```

### SKILL.md Format

```markdown
---
name: skill-name
description: Brief description of what this skill does
version: 1.0.0
author: Your Name
tags:
  - tag1
  - tag2
allowed-tools:
  - tool1
  - tool2
---

# Skill Title

Your markdown instructions go here. This is what the LLM sees when the skill is activated.

## Instructions

Detailed step-by-step instructions...

## Examples

Show examples of how to use this skill...

## Tips

Any important notes or tips...
```

## Writing Good Skills

### The Golden Rules

1. **Clear Name**: Use a descriptive, lowercase name with hyphens
2. **Concise Description**: Explain what the skill does in 1-2 sentences
3. **Detailed Instructions**: Provide step-by-step guidance in the body
4. **Examples**: Show don't tell - include concrete examples
5. **Safety First**: Never assume tools are available - they must be explicitly allowed

### Name Guidelines

✅ **Good Names**
- `send-email`
- `analyze-code`
- `generate-report`
- `search-documents`
- `技能` (Chinese)
- `навык` (Russian)
- `مهارة` (Arabic)
- `技能-测试` (Chinese with hyphens)
- `skill-技能` (mixed scripts)

❌ **Bad Names**
- `SendEmail` (uppercase not allowed)
- `НАВЫК` (uppercase Russian not allowed)
- `send--email` (consecutive hyphens not allowed)
- `-send-email` (can't start with hyphen)
- `send_email` (use hyphens, not underscores)

### Description Guidelines

✅ **Good Descriptions**
- "Send emails to specified recipients with attachments"
- "Analyze code for potential bugs and suggest improvements"
- "Generate PDF reports from structured data"

❌ **Bad Descriptions**
- "Email" (too short, not descriptive)
- "This is a skill that can be used to send emails when you need to send an email to someone..." (too verbose)
- "" (empty description not allowed)

### Instruction Guidelines

✅ **Good Instructions**
```markdown
## Instructions

When using this skill:

1. **Identify the recipient**: Verify the email address is valid
2. **Compose the message**: Write a clear, professional email
3. **Attach files**: If needed, reference files by their full path
4. **Review before sending**: Double-check recipient and content
5. **Send**: Execute the send command

## Example

To send a welcome email:
1. Set recipient to "user@example.com"
2. Set subject to "Welcome!"
3. Write a friendly greeting in the body
4. Send the email
```

❌ **Bad Instructions**
```markdown
## Instructions

Send email.
```

## Metadata Reference

### Required Fields

#### `name`
- **Type**: string
- **Required**: Yes
- **Length**: 1-64 characters
- **Pattern**: Unicode lowercase letters, numbers (0-9), hyphens only
  - Supports Unicode letters from any script (Chinese, Russian, Arabic, etc.)
  - Letters without case distinction (e.g., Chinese, Arabic) are allowed
  - Only lowercase letters are allowed for scripts that have case (e.g., Latin, Cyrillic)
- **Rules**: 
  - Cannot start or end with hyphen
  - Cannot contain consecutive hyphens (`--`)
  - Must match directory name exactly

#### `description`
- **Type**: string
- **Required**: Yes
- **Length**: 1-1024 characters
- **Best Practice**: 20-200 characters for clarity

### Optional Fields

#### `version`
- **Type**: string
- **Required**: No
- **Format**: Semantic versioning recommended (e.g., `1.0.0`)
- **Example**: `1.2.3`, `2.0.0-beta`, `1.0.0`

#### `author`
- **Type**: string
- **Required**: No
- **Example**: `John Doe`, `Acme Corp`, `john@example.com`

#### `compatibility`
- **Type**: string
- **Required**: No
- **Max Length**: 500 characters
- **Purpose**: Describe compatibility requirements
- **Example**: `Requires .NET 8.0 or higher`, `Works with Python 3.8+`

#### `tags`
- **Type**: array of strings
- **Required**: No
- **Purpose**: Categorize and filter skills
- **Example**: `[utility, data-processing, experimental]`

#### `allowed-tools`
- **Type**: array of strings
- **Required**: No
- **Purpose**: Declare what tools the skill needs (advisory only)
- **Example**: `[filesystem, network, calculator]`
- **Security Note**: This is informational - hosts decide what's actually allowed

### Custom Fields

You can add custom fields for your own use:

```yaml
---
name: my-skill
description: Example skill
custom-field: custom value
x-my-metadata: anything here
---
```

Custom fields are preserved in `SkillManifest.AdditionalFields` dictionary.

## Validation Rules

AgentSkills.NET validates skills against the [Agent Skills v1 specification](https://agentskills.io/specification).

### Name Validation

| Rule | Error Code | Severity |
|------|-----------|----------|
| Name is missing or empty | VAL001 | Error |
| Name length not 1-64 chars | VAL002 | Error |
| Name contains invalid characters | VAL003 | Error |
| Name starts/ends with hyphen | VAL003 | Error |
| Name has consecutive hyphens | VAL003 | Error |
| Directory name doesn't match skill name | VAL010 | Error |

### Description Validation

| Rule | Error Code | Severity |
|------|-----------|----------|
| Description is missing or empty | VAL004 | Error |
| Description exceeds 1024 chars | VAL005 | Error |
| Description is very short (<20 chars) | VAL006 | Warning |

### Other Field Validation

| Rule | Error Code | Severity |
|------|-----------|----------|
| Version field is empty | VAL007 | Warning |
| Compatibility exceeds 500 chars | VAL008 | Error |

### Testing Your Skills

```csharp
using AgentSkills.Validation;

var validator = new SkillValidator();
var result = validator.Validate(skill);

if (!result.IsValid)
{
    Console.WriteLine("Validation Errors:");
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"  [{error.Code}] {error.Message}");
    }
}

if (result.HasWarnings)
{
    Console.WriteLine("Warnings:");
    foreach (var warning in result.Warnings)
    {
        Console.WriteLine($"  [{warning.Code}] {warning.Message}");
    }
}
```

## Best Practices

### 1. Be Specific

❌ Bad:
```yaml
name: helper
description: Helps with things
```

✅ Good:
```yaml
name: email-composer
description: Compose and send professional emails with attachments
```

### 2. Use Tags Wisely

```yaml
tags:
  - communication    # Category
  - email           # Specific function
  - production      # Maturity level
```

### 3. Document Tool Requirements

```yaml
allowed-tools:
  - filesystem      # Needed to read attachments
  - network         # Needed to send email
```

### 4. Provide Examples

```markdown
## Examples

### Example 1: Simple Email
To: john@example.com
Subject: Hello
Body: Hi John, how are you?

### Example 2: Email with Attachment
To: team@example.com
Subject: Monthly Report
Body: Please find the report attached
Attachments: /reports/monthly-2024.pdf
```

### 5. Version Your Skills

Start at `1.0.0` and use semantic versioning:
- **Major** (1.x.x): Breaking changes
- **Minor** (x.1.x): New features, backward compatible
- **Patch** (x.x.1): Bug fixes

### 6. Keep Instructions Focused

Each skill should do one thing well. If your instructions are getting complex, consider splitting into multiple skills.

## Common Patterns

### Pattern 1: Data Processing Skill

```markdown
---
name: csv-analyzer
description: Analyze CSV files and generate summary statistics
version: 1.0.0
tags:
  - data
  - analysis
  - csv
allowed-tools:
  - filesystem
---

# CSV Analyzer

Analyze CSV files and provide statistical summaries.

## Instructions

1. **Load the CSV file**: Read the specified file path
2. **Parse the data**: Extract columns and rows
3. **Calculate statistics**: For each numeric column:
   - Count (total rows)
   - Mean (average)
   - Median (middle value)
   - Min and Max values
4. **Format results**: Present in a clear table format

## Input Format

Provide the file path as: `/path/to/file.csv`

## Output Format

For each column, report:
- Column name
- Data type
- Statistics (if numeric)
- Sample values (first 3)

## Example

Input: `/data/sales.csv`

Output:
- Product: text, samples: ["Widget A", "Widget B", "Widget C"]
- Quantity: numeric, mean: 45.2, median: 42, min: 10, max: 100
- Price: numeric, mean: 29.99, median: 24.99, min: 9.99, max: 99.99
```

### Pattern 2: Code Generation Skill

```markdown
---
name: api-endpoint-generator
description: Generate REST API endpoint code with error handling
version: 1.0.0
tags:
  - code-generation
  - api
  - backend
---

# API Endpoint Generator

Generate REST API endpoint code following best practices.

## Instructions

1. **Gather requirements**:
   - HTTP method (GET, POST, PUT, DELETE)
   - Route path (e.g., `/api/users/:id`)
   - Input parameters
   - Response format

2. **Generate code structure**:
   - Route handler function
   - Input validation
   - Error handling
   - Response formatting

3. **Include documentation**:
   - Add JSDoc/XML comments
   - Document parameters
   - Document return values

## Template Structure

```
[HTTP METHOD] [ROUTE]
- Parameters: [list parameters]
- Returns: [response type]
- Errors: [possible errors]
```

## Example

Request: Create a POST endpoint for creating users

Generated code includes:
- Route: POST /api/users
- Validation for required fields (name, email)
- Password hashing
- Database insertion
- Error responses (400, 500)
- Success response (201)
```

### Pattern 3: Workflow Skill

```markdown
---
name: code-review-workflow
description: Conduct thorough code reviews following team standards
version: 1.0.0
tags:
  - code-review
  - workflow
  - quality
---

# Code Review Workflow

Perform systematic code reviews following best practices.

## Instructions

Follow this workflow for every code review:

### Phase 1: Initial Scan (2 minutes)
- [ ] Read the PR description
- [ ] Check build/test status
- [ ] Verify changed files are appropriate for the PR

### Phase 2: Code Quality (5 minutes)
- [ ] Check for code style consistency
- [ ] Look for code duplication
- [ ] Verify naming conventions
- [ ] Check for magic numbers/strings

### Phase 3: Logic Review (10 minutes)
- [ ] Understand the intent of each change
- [ ] Verify logic correctness
- [ ] Check edge case handling
- [ ] Look for potential bugs

### Phase 4: Testing (5 minutes)
- [ ] Verify test coverage for new code
- [ ] Check test quality and assertions
- [ ] Ensure tests cover edge cases

### Phase 5: Documentation (3 minutes)
- [ ] Check for updated documentation
- [ ] Verify code comments for complex logic
- [ ] Ensure API changes are documented

### Phase 6: Security (5 minutes)
- [ ] Check for SQL injection risks
- [ ] Verify input validation
- [ ] Check authentication/authorization
- [ ] Look for sensitive data exposure

## Providing Feedback

Structure feedback as:
1. **Critical**: Must be fixed before merge
2. **Suggestions**: Could improve the code
3. **Questions**: Need clarification
4. **Praise**: Highlight good practices

## Example Review Comment

```
🔴 Critical: SQL Injection Risk
Line 45: User input is directly concatenated into SQL query
Suggestion: Use parameterized queries instead

💡 Suggestion: Extract Method
Lines 100-150: This function is doing too much
Consider extracting the validation logic into a separate method

❓ Question: Null Handling
Line 67: What happens if `user` is null here?

✅ Nice: Error Handling
Great error messages that will help debugging!
```
```

## Examples

### Minimal Valid Skill

```markdown
---
name: hello-world
description: A simple greeting skill for demonstration
---

# Hello World

Say hello to the user in a friendly way.
```

### Complete Skill with All Fields

```markdown
---
name: advanced-calculator
description: Perform complex mathematical calculations with unit conversions
version: 2.1.0
author: Math Team
compatibility: Requires .NET 8.0 or higher
tags:
  - mathematics
  - calculator
  - utility
allowed-tools:
  - calculator
---

# Advanced Calculator

Perform advanced mathematical operations including:
- Basic arithmetic (+, -, ×, ÷)
- Exponents and roots
- Trigonometric functions
- Unit conversions

## Instructions

1. **Parse the expression**: Identify operators and operands
2. **Apply order of operations**: Follow PEMDAS/BODMAS
3. **Calculate result**: Perform the computation
4. **Format output**: Round to appropriate precision

## Supported Operations

- Addition: `2 + 3 = 5`
- Subtraction: `10 - 4 = 6`
- Multiplication: `3 × 4 = 12`
- Division: `15 ÷ 3 = 5`
- Exponents: `2^8 = 256`
- Square root: `√16 = 4`
- Sine: `sin(90°) = 1`

## Unit Conversions

Convert between:
- Length: meters, feet, miles, kilometers
- Weight: grams, pounds, ounces, kilograms
- Temperature: Celsius, Fahrenheit, Kelvin

## Examples

### Example 1: Basic Math
Input: `(2 + 3) × 4`
Output: `20`

### Example 2: Unit Conversion
Input: `Convert 100 miles to kilometers`
Output: `160.93 kilometers`

### Example 3: Complex Expression
Input: `√(16 + 9) + 2^3`
Output: `13` (5 + 8)
```

## Common Mistakes to Avoid

### 1. Using Uppercase in Names
❌ `MySkill` → ✅ `my-skill`

### 2. Empty or Missing Required Fields
❌ Missing description → ✅ Always include description

### 3. Directory Name Mismatch
❌ Directory: `my_skill`, Name: `my-skill` → ✅ Both should be `my-skill`

### 4. Vague Instructions
❌ "Do the thing" → ✅ "1. Read input 2. Process data 3. Return result"

### 5. Assuming Tool Availability
❌ "Just use the filesystem" → ✅ Declare in `allowed-tools: [filesystem]`

## Validation Checklist

Before publishing your skill:

- [ ] Name follows rules (lowercase, hyphens, no consecutive hyphens)
- [ ] Description is clear and 20+ characters
- [ ] Directory name matches skill name exactly
- [ ] Instructions are detailed and actionable
- [ ] Examples are provided
- [ ] Tags are relevant
- [ ] Allowed tools are declared
- [ ] Version is specified (if applicable)
- [ ] Runs through validator without errors

```bash
# Test your skill
dotnet run --project path/to/validator -- path/to/your-skill
```

## Resources

- [Agent Skills Specification](https://agentskills.io/specification) - Official spec
- [Example Skills](../../fixtures/skills/) - Real examples in this repo
- [Public API Reference](PUBLIC_API.md) - Complete API docs
- [Testing Guide](TESTING_GUIDE.md) - How to test skills

## Need Help?

- Check the [FAQ](FAQ.md)
- Open a [GitHub Discussion](https://github.com/sfmskywalker/agentskillsdotnet/discussions)
- Review existing skills for inspiration

Happy skill authoring! 🎯
