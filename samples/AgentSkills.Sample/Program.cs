using AgentSkills;
using AgentSkills.Loader;
using AgentSkills.Prompts;
using AgentSkills.Validation;

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

// Step 3: Validate skills
Console.WriteLine("Step 3: Validating skills against v1 specification...");
var validator = new SkillValidator();
var validationResults = new List<(SkillMetadata Meta, ValidationResult Result)>();

foreach (var meta in metadata)
{
    var result = validator.ValidateMetadata(meta);
    validationResults.Add((meta, result));
}

var validSkills = validationResults.Where(r => r.Result.IsValid).ToList();
var invalidSkills = validationResults.Where(r => !r.Result.IsValid).ToList();

Console.WriteLine($"✓ Validation complete");
Console.WriteLine($"  Valid skills: {validSkills.Count}");
Console.WriteLine($"  Invalid skills: {invalidSkills.Count}");

if (invalidSkills.Any())
{
    Console.WriteLine($"  Errors found in {invalidSkills.Count} skill(s):");
    foreach (var (meta, result) in invalidSkills.Take(3))
    {
        Console.WriteLine($"    ❌ {meta.Name}:");
        foreach (var error in result.Errors.Take(2))
        {
            Console.WriteLine($"       {error.Code}: {error.Message}");
        }
    }
}

var totalWarnings = validationResults.Sum(r => r.Result.Warnings.Count());
if (totalWarnings > 0)
{
    Console.WriteLine($"  ⚠️  {totalWarnings} warning(s) across all skills");
}
Console.WriteLine();

// Step 4: Display available skills
Console.WriteLine("Step 4: Available skills (valid only):");
Console.WriteLine("---------------------------------------");
foreach (var (meta, result) in validationResults.Where(r => r.Result.IsValid))
{
    Console.WriteLine($"  • {meta.Name}");
    Console.WriteLine($"    Description: {meta.Description}");
    if (meta.Version != null)
        Console.WriteLine($"    Version: {meta.Version}");
    if (meta.Tags.Any())
        Console.WriteLine($"    Tags: {string.Join(", ", meta.Tags)}");
    if (result.HasWarnings)
        Console.WriteLine($"    ⚠️  {result.Warnings.Count()} warning(s)");
    Console.WriteLine();
}

// Step 4a: Demonstrate prompt rendering for LLMs
Console.WriteLine("Step 4a: Render skill list as prompt (progressive disclosure):");
Console.WriteLine("---------------------------------------------------------------");
var promptRenderer = new DefaultSkillPromptRenderer();
var validMetadata = validationResults.Where(r => r.Result.IsValid).Select(r => r.Meta).ToList();

if (validMetadata.Any())
{
    var skillListPrompt = promptRenderer.RenderSkillList(validMetadata);
    Console.WriteLine("=== Prompt for LLM (Skill List) ===");
    Console.WriteLine(skillListPrompt);
    Console.WriteLine("=== End of Prompt ===");
}
else
{
    Console.WriteLine("No valid skills to render.");
}
Console.WriteLine();

// Step 5: Load full skill set
Console.WriteLine("Step 5: Loading full skill set...");
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

// Step 6: Activate a specific skill (if available)
if (skillSet.Skills.Any())
{
    var firstSkill = skillSet.Skills.First();
    Console.WriteLine("Step 6: Activating skill (loading full content)...");
    Console.WriteLine($"  Selected: {firstSkill.Manifest.Name}");
    Console.WriteLine();

    // Step 7: Validate the activated skill
    Console.WriteLine("Step 7: Validating activated skill...");
    var skillValidation = validator.Validate(firstSkill);
    Console.WriteLine($"  Valid: {skillValidation.IsValid}");
    if (!skillValidation.IsValid)
    {
        Console.WriteLine($"  Errors: {skillValidation.Errors.Count()}");
        foreach (var error in skillValidation.Errors)
        {
            Console.WriteLine($"    ❌ {error.Code}: {error.Message}");
        }
    }
    if (skillValidation.HasWarnings)
    {
        Console.WriteLine($"  Warnings: {skillValidation.Warnings.Count()}");
    }
    Console.WriteLine();

    // Step 8: Display full skill information
    Console.WriteLine("Step 8: Full skill details:");
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

    // Step 8a: Demonstrate full skill prompt rendering
    Console.WriteLine("Step 8a: Render full skill details as prompt:");
    Console.WriteLine("----------------------------------------------");
    var skillDetailsPrompt = promptRenderer.RenderSkillDetails(firstSkill);
    Console.WriteLine("=== Prompt for LLM (Full Skill Details) ===");
    Console.WriteLine(skillDetailsPrompt);
    Console.WriteLine("=== End of Prompt ===");
    Console.WriteLine();

    // Step 8b: Demonstrate resource policy
    Console.WriteLine("Step 8b: Demonstrate resource policies:");
    Console.WriteLine("----------------------------------------");

    Console.WriteLine("Without allowed-tools (using ExcludeAllResourcePolicy):");
    var restrictiveOptions = new PromptRenderOptions
    {
        ResourcePolicy = ExcludeAllResourcePolicy.Instance
    };
    var restrictedPrompt = promptRenderer.RenderSkillDetails(firstSkill, restrictiveOptions);
    var hasAllowedTools = restrictedPrompt.Contains("Allowed Tools");
    Console.WriteLine($"  Contains 'Allowed Tools': {hasAllowedTools}");
    Console.WriteLine();

    Console.WriteLine("With custom visibility options:");
    var customOptions = new PromptRenderOptions
    {
        IncludeVersion = false,
        IncludeAuthor = false
    };
    var customPrompt = promptRenderer.RenderSkillDetails(firstSkill, customOptions);
    var hasVersion = customPrompt.Contains("Version:");
    var hasAuthor = customPrompt.Contains("Author:");
    Console.WriteLine($"  Contains 'Version': {hasVersion}");
    Console.WriteLine($"  Contains 'Author': {hasAuthor}");
    Console.WriteLine();
}
else
{
    Console.WriteLine("Step 6: No valid skills found to activate");
    Console.WriteLine();
}

// Summary
Console.WriteLine("==========================================");
Console.WriteLine("✓ Walking skeleton completed successfully!");
Console.WriteLine();
Console.WriteLine("Demonstrated workflow:");
Console.WriteLine("  1. ✓ Scanning for skills in a directory");
Console.WriteLine("  2. ✓ Loading skill metadata (fast, no full content)");
Console.WriteLine("  3. ✓ Validating skill metadata against v1 specification");
Console.WriteLine("  4. ✓ Rendering a list of available skills");
Console.WriteLine("  4a. ✓ Rendering skill list as LLM prompt (progressive disclosure)");
Console.WriteLine("  5. ✓ Loading full skill set with diagnostics");
Console.WriteLine("  6. ✓ Activating a specific skill");
Console.WriteLine("  7. ✓ Validating the full skill");
Console.WriteLine("  8. ✓ Rendering full skill instructions");
Console.WriteLine("  8a. ✓ Rendering full skill details as LLM prompt");
Console.WriteLine("  8b. ✓ Applying resource policies to control visibility");


