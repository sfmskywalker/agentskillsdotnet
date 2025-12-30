using AgentSkills;
using AgentSkills.Loader;

// AgentSkills.NET Sample Application
// This is a walking skeleton that demonstrates:
// scan → metadata → validate → render list → activate skill → render instructions

Console.WriteLine("AgentSkills.NET - Sample Walking Skeleton");
Console.WriteLine("==========================================");
Console.WriteLine();

// Determine the path to the fixtures/skills directory
var solutionRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
var skillsPath = Path.Combine(solutionRoot, "fixtures", "skills");

Console.WriteLine($"Skills directory: {skillsPath}");
Console.WriteLine();

// Step 1: Create the skill loader
Console.WriteLine("Step 1: Creating skill loader...");
var loader = new FileSystemSkillLoader();
Console.WriteLine("✓ Skill loader created");
Console.WriteLine();

// Step 2: Load skill metadata (fast path - no full content)
Console.WriteLine("Step 2: Loading skill metadata (fast path)...");
var (metadata, metadataDiagnostics) = loader.LoadMetadata(skillsPath);

Console.WriteLine($"✓ Found {metadata.Count} skill(s)");
if (metadataDiagnostics.Any())
{
    Console.WriteLine($"  ⚠️  {metadataDiagnostics.Count} diagnostic(s) during metadata load");
}
Console.WriteLine();

// Step 3: Display available skills
Console.WriteLine("Step 3: Available skills:");
Console.WriteLine("-------------------------");
foreach (var meta in metadata)
{
    Console.WriteLine($"  • {meta.Name}");
    Console.WriteLine($"    Description: {meta.Description}");
    if (meta.Version != null)
        Console.WriteLine($"    Version: {meta.Version}");
    if (meta.Tags.Any())
        Console.WriteLine($"    Tags: {string.Join(", ", meta.Tags)}");
    Console.WriteLine();
}

// Step 4: Load full skill set
Console.WriteLine("Step 4: Loading full skill set...");
var skillSet = loader.LoadSkillSet(skillsPath);
Console.WriteLine($"✓ Loaded {skillSet.Skills.Count} skill(s)");
Console.WriteLine($"  Valid: {skillSet.IsValid}");
if (skillSet.Diagnostics.Any())
{
    Console.WriteLine($"  Diagnostics: {skillSet.Diagnostics.Count}");
    
    var errors = skillSet.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
    var warnings = skillSet.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ToList();
    
    if (errors.Any())
    {
        Console.WriteLine($"    Errors: {errors.Count}");
        foreach (var error in errors.Take(3))
        {
            Console.WriteLine($"      ❌ {error.Message}");
            if (error.Path != null)
                Console.WriteLine($"         Path: {Path.GetFileName(error.Path)}");
        }
    }
    
    if (warnings.Any())
    {
        Console.WriteLine($"    Warnings: {warnings.Count}");
    }
}
Console.WriteLine();

// Step 5: Activate a specific skill (if available)
if (skillSet.Skills.Any())
{
    var firstSkill = skillSet.Skills.First();
    Console.WriteLine("Step 5: Activating skill (loading full content)...");
    Console.WriteLine($"  Selected: {firstSkill.Manifest.Name}");
    Console.WriteLine();
    
    // Step 6: Display full skill information
    Console.WriteLine("Step 6: Full skill details:");
    Console.WriteLine("---------------------------");
    Console.WriteLine($"Name: {firstSkill.Manifest.Name}");
    Console.WriteLine($"Description: {firstSkill.Manifest.Description}");
    
    if (firstSkill.Manifest.Version != null)
        Console.WriteLine($"Version: {firstSkill.Manifest.Version}");
    
    if (firstSkill.Manifest.Author != null)
        Console.WriteLine($"Author: {firstSkill.Manifest.Author}");
    
    if (firstSkill.Manifest.Tags.Any())
        Console.WriteLine($"Tags: {string.Join(", ", firstSkill.Manifest.Tags)}");
    
    if (firstSkill.Manifest.AllowedTools.Any())
        Console.WriteLine($"Allowed Tools: {string.Join(", ", firstSkill.Manifest.AllowedTools)}");
    
    Console.WriteLine();
    Console.WriteLine("Instructions (first 300 chars):");
    var preview = firstSkill.Instructions.Length > 300 
        ? firstSkill.Instructions.Substring(0, 300) + "..." 
        : firstSkill.Instructions;
    Console.WriteLine(preview);
    Console.WriteLine();
}
else
{
    Console.WriteLine("Step 5: No valid skills found to activate");
    Console.WriteLine();
}

// Summary
Console.WriteLine("==========================================");
Console.WriteLine("✓ Walking skeleton completed successfully!");
Console.WriteLine();
Console.WriteLine("Demonstrated workflow:");
Console.WriteLine("  1. ✓ Scanning for skills in a directory");
Console.WriteLine("  2. ✓ Loading skill metadata (fast, no full content)");
Console.WriteLine("  3. ✓ Validating skills and collecting diagnostics");
Console.WriteLine("  4. ✓ Rendering a list of available skills");
Console.WriteLine("  5. ✓ Activating a specific skill");
Console.WriteLine("  6. ✓ Rendering full skill instructions");


