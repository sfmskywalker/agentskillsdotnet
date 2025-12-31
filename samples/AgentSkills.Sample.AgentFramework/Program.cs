using AgentSkills;
using AgentSkills.Adapters.Microsoft.AgentFramework;
using AgentSkills.Loader;
using AgentSkills.Prompts;
using AgentSkills.Validation;

// AgentSkills.NET - Microsoft Agent Framework Adapter Sample
// This sample demonstrates how to integrate AgentSkills with Microsoft Agent Framework
// showing the progressive disclosure pattern: list skills → activate skill → load instructions

Console.WriteLine("AgentSkills.NET - Microsoft Agent Framework Adapter Sample");
Console.WriteLine("============================================================");
Console.WriteLine();

// Determine the path to the fixtures/skills directory
var solutionRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
var skillsPath = Path.Combine(solutionRoot, "fixtures", "skills");

Console.WriteLine($"Skills directory: {skillsPath}");
Console.WriteLine();

// Step 1: Load and validate skills
Console.WriteLine("Step 1: Loading and validating skills...");
var loader = new FileSystemSkillLoader();
var skillSet = loader.LoadSkillSet(skillsPath);
var validator = new SkillValidator();

var validSkills = skillSet.Skills
    .Where(s => validator.Validate(s).IsValid)
    .ToList();

Console.WriteLine($"✓ Found {validSkills.Count} valid skill(s)");
Console.WriteLine();

// Step 2: Build base agent instructions with skill list (Progressive Disclosure - Part 1)
Console.WriteLine("Step 2: Building agent instructions with skill listing...");
var promptBuilder = new SkillPromptBuilder();

var baseInstructions = promptBuilder
    .WithBaseInstructions(
        "You are a helpful assistant that can use various skills to help users. " +
        "When you need to perform a task, check if there is an available skill that can help. " +
        "To use a skill, call its corresponding function.")
    .WithSkills(validSkills.Select(s => s.Metadata))
    .Build();

Console.WriteLine("✓ Agent instructions built");
Console.WriteLine();

// Step 3: Display the base instructions (what would go to the agent initially)
Console.WriteLine("Step 3: Base agent instructions (initial context):");
Console.WriteLine("---------------------------------------------------");
Console.WriteLine(baseInstructions);
Console.WriteLine();

// Step 4: Simulate skill activation (Progressive Disclosure - Part 2)
if (validSkills.Any())
{
    var skill = validSkills.First();
    Console.WriteLine($"Step 4: Simulating activation of skill '{skill.Manifest.Name}'...");
    Console.WriteLine();

    // When the agent "activates" a skill (e.g., by calling a function),
    // it would receive the full skill instructions
    var skillInstructions = skill.GetInstructions();

    Console.WriteLine("Step 5: Full skill instructions (returned when activated):");
    Console.WriteLine("-----------------------------------------------------------");
    Console.WriteLine(skillInstructions);
    Console.WriteLine();

    // Step 6: Demonstrate function metadata helpers
    Console.WriteLine("Step 6: Function metadata helpers:");
    Console.WriteLine("-----------------------------------");
    Console.WriteLine($"Function Name: {skill.GetFunctionName()}");
    Console.WriteLine($"Function Description: {skill.GetFunctionDescription()}");
    Console.WriteLine();

    // Step 7: Show how this would integrate with actual Agent Framework
    Console.WriteLine("Step 7: Integration pattern with Microsoft Agent Framework:");
    Console.WriteLine("------------------------------------------------------------");
    Console.WriteLine();
    Console.WriteLine("To integrate with Microsoft Agent Framework, you would:");
    Console.WriteLine();
    Console.WriteLine("1. Create an AIAgent with the base instructions:");
    Console.WriteLine("   var agent = chatClient.CreateAIAgent(instructions: baseInstructions, ...);");
    Console.WriteLine();
    Console.WriteLine("2. Register each skill as a tool/function:");
    Console.WriteLine("   foreach (var skill in validSkills)");
    Console.WriteLine("   {");
    Console.WriteLine("       var function = AIFunctionFactory.Create(");
    Console.WriteLine("           method: () => skill.GetInstructions(),");
    Console.WriteLine("           name: skill.GetFunctionName(),");
    Console.WriteLine("           description: skill.GetFunctionDescription());");
    Console.WriteLine("       // Add function to agent tools");
    Console.WriteLine("   }");
    Console.WriteLine();
    Console.WriteLine("3. When the agent calls a skill function, it receives full instructions");
    Console.WriteLine("   This implements progressive disclosure:");
    Console.WriteLine("   - Agent sees skill list in base instructions (minimal tokens)");
    Console.WriteLine("   - Agent activates specific skill when needed (full details)");
    Console.WriteLine();
}
else
{
    Console.WriteLine("Step 4: No valid skills found to demonstrate activation");
    Console.WriteLine();
}

// Step 8: Demonstrate resource policy control
Console.WriteLine("Step 8: Resource policy control:");
Console.WriteLine("---------------------------------");
if (validSkills.Any())
{
    var skill = validSkills.First();

    Console.WriteLine("Default rendering (includes all metadata):");
    var defaultInstructions = skill.GetInstructions();
    Console.WriteLine($"  Length: {defaultInstructions.Length} characters");
    Console.WriteLine();

    Console.WriteLine("Restrictive rendering (excludes allowed-tools):");
    var restrictiveOptions = new PromptRenderOptions
    {
        ResourcePolicy = ExcludeAllResourcePolicy.Instance
    };
    var restrictiveInstructions = skill.GetInstructions(options: restrictiveOptions);
    Console.WriteLine($"  Length: {restrictiveInstructions.Length} characters");
    Console.WriteLine();
}

// Summary
Console.WriteLine("============================================================");
Console.WriteLine("✓ Sample completed successfully!");
Console.WriteLine();
Console.WriteLine("Key takeaways:");
Console.WriteLine("  1. ✓ Adapter enables thin integration with Agent Framework");
Console.WriteLine("  2. ✓ Progressive disclosure: list skills → activate → load details");
Console.WriteLine("  3. ✓ No runtime details bleed into core packages");
Console.WriteLine("  4. ✓ Helper methods simplify function registration");
Console.WriteLine("  5. ✓ Resource policies provide fine-grained control");
Console.WriteLine();
