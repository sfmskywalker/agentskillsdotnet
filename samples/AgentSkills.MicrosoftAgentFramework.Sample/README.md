# AgentSkills.NET + Microsoft Agent Framework Integration Sample

This sample demonstrates the **complete integration pattern** for using AgentSkills.NET with Microsoft Agent Framework (via Microsoft.Extensions.AI), showing how:

- **Skills** (SKILL.md files) describe **when** and **how** to use tools
- **Tools** are implemented as **C# functions** in the host application
- Skills reference tools by name in their `allowed-tools` metadata
- The host maps tool names to function implementations
- The agent uses skills to guide tool calling
- Tool execution happens in safe, controlled C# code

## Architecture Overview

```
┌─────────────────┐
│  SKILL.md files │  ← Text bundles describing when/how to use tools
│  (in skills/)   │
└────────┬────────┘
         │
         │ Loaded by
         ▼
┌─────────────────┐
│ AgentSkills.NET │  ← Loads, validates, renders skills
│   Libraries     │
└────────┬────────┘
         │
         │ Skills list provided to
         ▼
┌─────────────────┐
│  AI Agent       │  ← Microsoft Agent Framework (Extensions.AI)
│  (with tools)   │
└────────┬────────┘
         │
         │ Calls
         ▼
┌─────────────────┐
│  C# Functions   │  ← Tool implementations in host app
│  (Tools)        │     add(), subtract(), get_weather(), etc.
└─────────────────┘
```

## What This Sample Demonstrates

1. **Loading Skills**: Discovers and loads skills from the `skills/` directory
2. **Validating Skills**: Filters out invalid skills using validation
3. **Progressive Disclosure**: 
   - Agent sees skill list initially (minimal context)
   - When activated, full instructions are loaded
4. **Tool Registration**: C# functions registered as tools via `AIFunctionFactory`
5. **Skill-to-Tool Mapping**: Skills reference tools by name; host provides implementations
6. **Tool Execution**: Demonstrates actual tool calls returning results
7. **No External Dependencies**: Runs as a demonstration without requiring API keys

> **Note on Testing**: This is an educational sample project designed to demonstrate integration patterns. Unit tests are not included as the sample focuses on showcasing the integration flow rather than providing production-ready code. The core AgentSkills.NET libraries have comprehensive test coverage (184+ tests). For production applications, you should add tests that verify:
> - Skills are discovered from your custom skill directories
> - Tool registry contains expected tool names matching skill requirements
> - Tool implementations return expected results
> - Skill-to-tool mappings are correct
6. **Tool Execution**: Demonstrates actual tool calls returning results
7. **No External Dependencies**: Runs with mock client (no API keys required for demo)

## Project Structure

```
AgentSkills.MicrosoftAgentFramework.Sample/
├── Program.cs                          # Main sample code
├── AgentSkills.MicrosoftAgentFramework.Sample.csproj
├── README.md                           # This file
└── skills/                             # Sample skills
    ├── calculator/
    │   └── SKILL.md                    # Math operations skill
    └── weather-lookup/
        └── SKILL.md                    # Weather info skill
```

## Skills Included

### 1. Calculator Skill
- **Purpose**: Perform mathematical calculations
- **Tools Required**: `add`, `subtract`, `multiply`, `divide`
- **Example**: "What is 25 plus 17?" → calls `add(25, 17)` → returns 42

### 2. Weather Lookup Skill
- **Purpose**: Get weather information
- **Tools Required**: `get_weather`
- **Example**: "What's the weather in San Francisco?" → calls `get_weather("San Francisco")`

## How to Run

### Option 1: Run with Mock Client (No Setup Required)

The sample includes a mock chat client, so you can run it immediately without any API keys:

```bash
cd samples/AgentSkills.MicrosoftAgentFramework.Sample
dotnet run
```

This will:
- Load skills from the local `skills/` directory
- Register C# function tools
- Show progressive disclosure in action
- Demonstrate tool calling with actual execution
- Display skill-to-tool mapping

### Option 2: Use with Real AI Model

To integrate with a real AI model (OpenAI, Azure OpenAI, etc.):

1. **Install additional packages** (if using OpenAI):
   ```bash
   dotnet add package OpenAI
   ```

2. **Set up configuration**:
   Create `appsettings.json`:
   ```json
   {
     "OpenAI": {
       "ApiKey": "your-api-key-here",
       "Model": "gpt-4"
     }
   }
   ```

3. **Modify Program.cs**:
   Replace the integration note section with:
   ```csharp
   using OpenAI;
   using Microsoft.Extensions.AI;
   using Microsoft.Extensions.Configuration;

   // Build configuration from appsettings.json
   var configuration = new ConfigurationBuilder()
       .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
       .Build();

   // Load API key from configuration
   var apiKey = configuration["OpenAI:ApiKey"];
   var model = configuration["OpenAI:Model"] ?? "gpt-4";

   // Create real chat client
   var openAIClient = new OpenAIClient(apiKey);
   var chatClient = openAIClient.AsChatClient(model);

   // Register tools with the client
   var options = new ChatOptions
   {
       Tools = allTools
   };

   // Now you can actually converse with the agent
   var messages = new List<ChatMessage>
   {
       new(ChatRole.System, baseInstructions),
       new(ChatRole.User, "What is 25 plus 17?")
   };

   var response = await chatClient.CompleteAsync(messages, options);
   Console.WriteLine(response.Message.Text);
   ```

## Key Code Sections

### 1. Loading Skills

```csharp
var loader = new FileSystemSkillLoader();
// Path construction ensures skills are found relative to executable
var skillsPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "skills");
var skillSet = loader.LoadSkillSet(skillsPath);
var validator = new SkillValidator();
var validSkills = skillSet.Skills
    .Where(s => validator.Validate(s).IsValid)
    .ToList();
```

### 2. Defining Tools

```csharp
var addTool = AIFunctionFactory.Create(
    (double a, double b) => a + b,
    "add",
    "Add two numbers together");

var weatherTool = AIFunctionFactory.Create(
    (string location) =>
    {
        // Simulated weather data - in real app would call weather API
        var weatherData = new Dictionary<string, string>
        {
            ["san francisco"] = "65°F (18°C), partly cloudy with light fog in the morning",
            ["new york"] = "72°F (22°C), sunny with clear skies"
        };
        
        var key = location.ToLowerInvariant();
        if (weatherData.TryGetValue(key, out var weather))
        {
            return $"Current weather in {location}: {weather}";
        }
        return $"Weather data not available for {location}";
    },
    "get_weather",
    "Get current weather for a location");
```

### 3. Building Agent Instructions

```csharp
var promptBuilder = new SkillPromptBuilder();
var baseInstructions = promptBuilder
    .WithBaseInstructions("You are a helpful AI assistant...")
    .WithSkills(validSkills.Select(s => s.Metadata))
    .Build();
```

### 4. Progressive Disclosure

```csharp
// Part 1: Agent sees skill list (minimal context)
var skillList = promptBuilder.Build();

// Part 2: When skill is activated, load full instructions
var fullInstructions = skill.GetInstructions();
```

## Understanding the Integration Pattern

### Skills are NOT Code

**Important**: Skills are text files (SKILL.md) that describe:
- WHEN to use a tool (e.g., "when user asks for a calculation")
- HOW to use a tool (e.g., "call add() with two numbers")
- WHAT tools are available (listed in `allowed-tools`)

Skills do NOT contain executable code. They are guidance for the AI agent.

### Tools ARE Code

Tools are C# functions implemented in your host application:

```csharp
// This is a tool - actual executable C# code
var addTool = AIFunctionFactory.Create(
    (double a, double b) => a + b,
    "add",
    "Add two numbers together");
```

### Mapping Skills to Tools

1. **Skill declares tools it needs**:
   ```yaml
   # In SKILL.md
   allowed-tools:
     - add
     - subtract
   ```

2. **Host provides implementations**:
   ```csharp
   var tools = new[]
   {
       AIFunctionFactory.Create((double a, double b) => a + b, "add", "..."),
       AIFunctionFactory.Create((double a, double b) => a - b, "subtract", "...")
   };
   ```

3. **Agent matches them up**:
   - Agent reads skill: "For addition, call the 'add' tool"
   - Agent finds tool named 'add' in registered tools
   - Agent calls tool: `add(5, 3)`
   - Tool executes: returns `8`

## Adding Your Own Skills and Tools

### Step 1: Create a Skill

Create a new directory under `skills/` with a `SKILL.md` file:

```markdown
---
name: my-skill
description: Does something useful
version: 1.0.0
allowed-tools:
  - my_tool
---

# My Skill

Instructions for when and how to use my_tool...
```

### Step 2: Implement the Tool

In `Program.cs`, add your tool:

```csharp
var myTool = AIFunctionFactory.Create(
    (string input) => {
        // Your C# implementation
        return $"Processed: {input}";
    },
    "my_tool",
    "Description of what my_tool does");

allTools.Add(myTool);
```

### Step 3: Run

The sample will automatically:
- Discover your new skill
- Map it to your tool
- Make it available to the agent

## Troubleshooting

### Skills Not Found
- Verify `skills/` directory path is correct
- Check that each skill has a `SKILL.md` file (case-sensitive)
- Run with verbose logging to see discovery process

### Tool Not Mapped
- Ensure tool name in code matches name in skill's `allowed-tools`
- Tool names are case-sensitive
- Check that tool is added to `allTools` collection

### Validation Errors
- Run `validator.Validate(skill)` to see specific errors
- Common issues: missing required fields, invalid YAML format
- See validation diagnostics for details

## Further Reading

- [Microsoft Agent Framework Documentation](https://learn.microsoft.com/en-us/agent-framework/)
- [Microsoft.Extensions.AI Documentation](https://learn.microsoft.com/en-us/dotnet/ai/)
- [AgentSkills.NET Documentation](../../docs/)
- [Agent Skills Standard](https://agentskills.io/)

## Next Steps

- **Add more skills**: Create skills for your specific use cases
- **Implement real tools**: Replace mock weather data with actual API calls
- **Connect to LLM**: Use OpenAI or Azure OpenAI for real agent conversations
- **Build workflows**: Chain multiple skills together for complex tasks
- **Add persistence**: Store conversation history and agent state

## License

This sample is part of AgentSkills.NET and is licensed under the MIT License.
