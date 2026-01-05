---
name: calculator
description: Perform mathematical calculations using various operations
version: 1.0.0
author: AgentSkills.NET Sample
tags:
  - math
  - calculation
  - arithmetic
allowed-tools:
  - add
  - subtract
  - multiply
  - divide
---

# Calculator Skill

Perform mathematical calculations using the calculator tools.

## Purpose

This skill enables you to perform basic arithmetic operations when users request calculations. Use the appropriate tool based on the operation needed.

## Instructions

When a user asks for a calculation:

1. **Identify the operation** needed (addition, subtraction, multiplication, or division)
2. **Extract the numbers** from the user's request
3. **Call the appropriate tool**:
   - Use `add` for addition: "What is 5 plus 3?"
   - Use `subtract` for subtraction: "What is 10 minus 4?"
   - Use `multiply` for multiplication: "What is 6 times 7?"
   - Use `divide` for division: "What is 15 divided by 3?"
4. **Return the result** in a clear, natural language format

## Examples

### Addition Example
**User:** "What's 25 plus 17?"
**Action:** Call `add(25, 17)`
**Response:** "25 plus 17 equals 42."

### Subtraction Example
**User:** "Subtract 8 from 20"
**Action:** Call `subtract(20, 8)`
**Response:** "20 minus 8 equals 12."

### Multiplication Example
**User:** "Calculate 9 times 6"
**Action:** Call `multiply(9, 6)`
**Response:** "9 times 6 equals 54."

### Division Example
**User:** "Divide 100 by 5"
**Action:** Call `divide(100, 5)`
**Response:** "100 divided by 5 equals 20."

## Error Handling

- If dividing by zero, explain that division by zero is undefined
- For invalid numbers, ask the user to clarify
- For complex expressions, break them down into steps

## Notes

- This skill requires access to the calculator tools listed in `allowed-tools`
- Tools are implemented in the host environment and executed as C# code
- The skill provides guidance on when and how to use each tool
