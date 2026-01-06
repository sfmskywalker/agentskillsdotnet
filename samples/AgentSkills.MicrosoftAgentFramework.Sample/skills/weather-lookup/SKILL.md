---
name: weather-lookup
description: Get current weather information for any location
version: 1.0.0
author: AgentSkills.NET Sample
tags:
  - weather
  - information
  - location
allowed-tools:
  - get_weather
---

# Weather Lookup Skill

Get current weather information for cities and locations.

## Purpose

This skill enables you to retrieve weather information when users ask about weather conditions in specific locations.

## Instructions

When a user asks about the weather:

1. **Extract the location** from the user's request
   - City name
   - City and country (e.g., "London, UK")
   - Zip code (if applicable)

2. **Call the weather tool**: Use `get_weather` with the location parameter

3. **Format the response** with:
   - Location name
   - Current temperature
   - Weather condition (e.g., sunny, cloudy, rainy)
   - Additional details if available

## Examples

### Example 1: Simple Weather Query
**User:** "What's the weather in San Francisco?"
**Action:** Call `get_weather("San Francisco")`
**Response:** "In San Francisco, it's currently 65°F and partly cloudy."

### Example 2: Weather with Country
**User:** "Tell me the weather in Paris, France"
**Action:** Call `get_weather("Paris, France")`
**Response:** "In Paris, France, it's currently 12°C (54°F) and overcast with light rain."

### Example 3: Multiple Location Query
**User:** "What's the weather in New York and London?"
**Action:** Call `get_weather("New York")` then `get_weather("London")`
**Response:** "In New York, it's 72°F and sunny. In London, it's 16°C (61°F) and cloudy."

## Tips

- If the location is ambiguous, you can ask for clarification
- Provide temperature in the user's preferred unit if known
- Include relevant details like humidity or wind if the user asks

## Notes

- This skill requires access to the `get_weather` tool
- The tool is implemented in the host environment
- Actual weather data would come from a weather API in a real implementation
