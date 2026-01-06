using System.ComponentModel;
using AgentSkills;
using AgentSkills.Adapters.Microsoft.AgentFramework;
using AgentSkills.Loader;
using AgentSkills.Prompts;
using AgentSkills.Validation;
using Microsoft.Extensions.AI;

// AgentSkills.NET - Microsoft Agent Framework Integration Sample
// This sample demonstrates the full integration pattern:
// - Skills (SKILL.md files) describe when/how to use tools
// - Tools are implemented as C# functions in the host
// - The host maps tool names to function implementations
// - The agent uses skills to know when to call tools

Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║  AgentSkills.NET + Microsoft Agent Framework Integration      ║");
Console.WriteLine("║  Demonstrating: Skills → Tools → C# Function Execution        ║");
Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
Console.WriteLine();

// ====================================================================
// STEP 1: Load Skills from Local Directory
// ====================================================================
Console.WriteLine("📂 Step 1: Loading skills from local directory...");
Console.WriteLine("─────────────────────────────────────────────────────────────────");

var skillsPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "skills");
var fullSkillsPath = Path.GetFullPath(skillsPath);
Console.WriteLine($"Skills directory: {fullSkillsPath}");

var loader = new FileSystemSkillLoader();
var skillSet = loader.LoadSkillSet(fullSkillsPath);
var validator = new SkillValidator();

var validSkills = skillSet.Skills
    .Where(s => validator.Validate(s).IsValid)
    .ToList();

Console.WriteLine($"✓ Loaded {validSkills.Count} valid skill(s): {string.Join(", ", validSkills.Select(s => s.Manifest.Name))}");
Console.WriteLine();

// ====================================================================
// STEP 2: Define C# Function Tools
// ====================================================================
Console.WriteLine("🔧 Step 2: Defining C# function tools...");
Console.WriteLine("─────────────────────────────────────────────────────────────────");

// Calculator tools
var calculatorTools = new[]
{
    AIFunctionFactory.Create(
        ([Description("First number")] double a, [Description("Second number")] double b) => a + b,
        "add",
        "Add two numbers together"),

    AIFunctionFactory.Create(
        ([Description("Number to subtract from")] double a, [Description("Number to subtract")] double b) => a - b,
        "subtract",
        "Subtract the second number from the first"),

    AIFunctionFactory.Create(
        ([Description("First number")] double a, [Description("Second number")] double b) => a * b,
        "multiply",
        "Multiply two numbers together"),

    AIFunctionFactory.Create(
        ([Description("Dividend")] double a, [Description("Divisor")] double b) =>
        {
            if (Math.Abs(b) < 0.0001)
                return double.NaN;
            return a / b;
        },
        "divide",
        "Divide the first number by the second")
};

// Weather tool (simulated - in real app would call weather API)
var weatherTool = AIFunctionFactory.Create(
    ([Description("City name or location")] string location) =>
    {
        // Simulated weather data
        var weatherData = new Dictionary<string, string>
        {
            ["san francisco"] = "65°F (18°C), partly cloudy with light fog in the morning",
            ["new york"] = "72°F (22°C), sunny with clear skies",
            ["london"] = "16°C (61°F), overcast with occasional drizzle",
            ["paris"] = "12°C (54°F), rainy with strong winds",
            ["tokyo"] = "24°C (75°F), clear and pleasant"
        };

        var key = location.ToLowerInvariant();
        if (weatherData.TryGetValue(key, out var weather))
        {
            return $"Current weather in {location}: {weather}";
        }

        return $"Weather data not available for {location} (this is a demo with limited cities)";
    },
    "get_weather",
    "Get current weather for a location");

var allTools = calculatorTools.Append(weatherTool).ToList();
Console.WriteLine($"✓ Registered {allTools.Count} function tools:");
foreach (var tool in allTools)
{
    Console.WriteLine($"  - {tool.Metadata.Name}: {tool.Metadata.Description}");
}
Console.WriteLine();

// ====================================================================
// STEP 3: Build Agent Instructions with Progressive Disclosure
// ====================================================================
Console.WriteLine("📋 Step 3: Building agent instructions (Progressive Disclosure - Part 1)...");
Console.WriteLine("─────────────────────────────────────────────────────────────────");

var promptBuilder = new SkillPromptBuilder();
var baseInstructions = promptBuilder
    .WithBaseInstructions(
        "You are a helpful AI assistant with access to various skills and tools. " +
        "When a user asks you to perform a task, check if there is a skill that can help. " +
        "Skills describe WHEN and HOW to use tools. The tools listed in a skill's 'allowed-tools' " +
        "are available as functions you can call. " +
        "\n\n" +
        "IMPORTANT: When you need to use a skill, you should activate it first to see full instructions, " +
        "then call the appropriate tools as guided by those instructions.")
    .WithSkills(validSkills.Select(s => s.Metadata))
    .Build();

Console.WriteLine("✓ Base instructions built (includes skill list)");
Console.WriteLine();
Console.WriteLine("Preview of agent instructions:");
Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
var preview = baseInstructions.Length > 400 ? baseInstructions[..400] + "..." : baseInstructions;
Console.WriteLine(preview);
Console.WriteLine("└─────────────────────────────────────────────────────────────┘");
Console.WriteLine();

// ====================================================================
// STEP 4: Note on Agent Integration (No API Key Required for Demo!)
// ====================================================================
Console.WriteLine("🤖 Step 4: About agent integration...");
Console.WriteLine("─────────────────────────────────────────────────────────────────");

// Note: This sample demonstrates the pattern without requiring an actual AI model.
// In a real application, you would:
// 1. Create an IChatClient (e.g., OpenAIChatClient, AzureOpenAIChatClient)
// 2. Pass baseInstructions as system message
// 3. Register allTools with ChatOptions
// 4. The agent would then call tools based on skill guidance

Console.WriteLine("✓ This demo shows the integration pattern (no API key needed)");
Console.WriteLine("  For actual AI integration, see README.md 'Option 2: Use with Real AI Model'");
Console.WriteLine();

// ====================================================================
// STEP 5: Demonstrate Progressive Disclosure
// ====================================================================
Console.WriteLine("📖 Step 5: Demonstrating Progressive Disclosure...");
Console.WriteLine("─────────────────────────────────────────────────────────────────");

if (validSkills.Any())
{
    var calcSkill = validSkills.FirstOrDefault(s => s.Manifest.Name == "calculator");
    if (calcSkill != null)
    {
        Console.WriteLine($"When agent activates skill '{calcSkill.Manifest.Name}', it receives full instructions:");
        Console.WriteLine();
        var fullInstructions = calcSkill.GetInstructions();
        var instructionsPreview = fullInstructions.Length > 600
            ? fullInstructions[..600] + "\n... (truncated for display)"
            : fullInstructions;
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine(instructionsPreview);
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘");
        Console.WriteLine();
    }
}

// ====================================================================
// STEP 6: Demonstrate Tool Calling with Skills
// ====================================================================
Console.WriteLine("⚡ Step 6: Demonstrating tool execution...");
Console.WriteLine("─────────────────────────────────────────────────────────────────");

// Example 1: Calculator skill → tool calls
Console.WriteLine("Example 1: Math calculation using calculator skill");
Console.WriteLine("User asks: 'What is 15 plus 27?'");
Console.WriteLine();

var addTool = allTools.First(t => t.Metadata.Name == "add");
var result = await addTool.InvokeAsync(new Dictionary<string, object?> { ["a"] = 15.0, ["b"] = 27.0 });
Console.WriteLine($"  → Tool 'add' called with (15, 27)");
Console.WriteLine($"  → Result: {result}");
Console.WriteLine($"  ✓ Agent responds: '15 plus 27 equals {result}'");
Console.WriteLine();

// Example 2: Weather skill → tool call
Console.WriteLine("Example 2: Weather lookup using weather skill");
Console.WriteLine("User asks: 'What's the weather in San Francisco?'");
Console.WriteLine();

var weatherToolInstance = allTools.First(t => t.Metadata.Name == "get_weather");
var weatherResult = await weatherToolInstance.InvokeAsync(new Dictionary<string, object?> { ["location"] = "san francisco" });
Console.WriteLine($"  → Tool 'get_weather' called with location='san francisco'");
Console.WriteLine($"  → Result: {weatherResult}");
Console.WriteLine($"  ✓ Agent responds with weather information");
Console.WriteLine();

// ====================================================================
// STEP 7: Show Skill-to-Tool Mapping
// ====================================================================
Console.WriteLine("🗺️  Step 7: Skill-to-Tool Mapping Summary");
Console.WriteLine("─────────────────────────────────────────────────────────────────");
Console.WriteLine();

foreach (var skill in validSkills)
{
    Console.WriteLine($"Skill: {skill.Manifest.Name}");
    var allowedTools = skill.Manifest.AllowedTools ?? new List<string>();
    Console.WriteLine($"  Allowed Tools: {string.Join(", ", allowedTools)}");

    var matchedTools = allTools.Where(t => allowedTools.Contains(t.Metadata.Name)).ToList();
    if (matchedTools.Any())
    {
        Console.WriteLine($"  ✓ Mapped to {matchedTools.Count} C# function(s):");
        foreach (var tool in matchedTools)
        {
            Console.WriteLine($"    - {tool.Metadata.Name}(): {tool.Metadata.Description}");
        }
    }
    else
    {
        Console.WriteLine("  ⚠ No matching tools registered (tools may be available in real environment)");
    }
    Console.WriteLine();
}

// ====================================================================
// Summary
// ====================================================================
Console.WriteLine();
Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║  ✓ Sample Completed Successfully!                             ║");
Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
Console.WriteLine();
Console.WriteLine("Key Takeaways:");
Console.WriteLine("─────────────────────────────────────────────────────────────────");
Console.WriteLine("1. ✓ Skills are SKILL.md files describing when/how to use tools");
Console.WriteLine("2. ✓ Tools are C# functions implemented in the host application");
Console.WriteLine("3. ✓ Skills reference tools by name in 'allowed-tools' metadata");
Console.WriteLine("4. ✓ Host maps tool names to AIFunction implementations");
Console.WriteLine("5. ✓ Progressive disclosure: list skills → activate → load full instructions");
Console.WriteLine("6. ✓ Agent calls tools based on skill guidance");
Console.WriteLine("7. ✓ Tool execution happens in C# code (safe, controlled)");
Console.WriteLine();
Console.WriteLine("Next Steps:");
Console.WriteLine("─────────────────────────────────────────────────────────────────");
Console.WriteLine("• To use with a real AI model:");
Console.WriteLine("  - Create an IChatClient (OpenAI, Azure OpenAI, etc.)");
Console.WriteLine("  - Pass baseInstructions as system message");
Console.WriteLine("  - Register allTools with ChatOptions");
Console.WriteLine("  - See README.md for detailed integration instructions");
Console.WriteLine("• Add more skills and tools as needed for your use case");
Console.WriteLine("• Customize the prompt builder for your agent's personality");
Console.WriteLine();
