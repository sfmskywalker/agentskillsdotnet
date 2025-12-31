---
name: special-chars-skill
description: "A skill testing special characters: quotes \"test\", apostrophes 'test', and symbols !@#$%"
version: 1.0.0
author: Test & Validation Team
tags:
  - special-characters
  - unicode
  - edge-cases
---

# Special Characters Skill

This skill tests handling of special characters in various contexts.

## Unicode Support

Testing unicode characters: 你好, привет, مرحبا, こんにちは

## Special Symbols

Testing various symbols and punctuation:
- Quotes: "double" and 'single'
- Mathematical: ∑, ∫, ∂, ∞, ≈, ≠
- Currency: $, €, £, ¥, ₹
- Arrows: →, ←, ↑, ↓, ⇒, ⇐
- Symbols: ©, ®, ™, ±, ×, ÷

## Markdown Edge Cases

Testing markdown special characters:
- Asterisks: * and **
- Underscores: _ and __
- Backticks: ` and ```
- Brackets: [link] and (url)
- Hash: # heading

## Code Blocks

```javascript
function test() {
    const str = "Testing special chars: <>&\"'";
    return str.replace(/[<>&"']/g, (c) => entities[c]);
}
```

## HTML Entities

Testing HTML entity handling: &lt;, &gt;, &amp;, &quot;, &#39;

This skill ensures proper handling of special characters throughout the parsing and rendering pipeline.
