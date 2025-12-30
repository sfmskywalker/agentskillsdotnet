namespace AgentSkills.Tests;

using AgentSkills;
using AgentSkills.Loader;
using AgentSkills.Prompts;
using AgentSkills.Validation;
using Xunit;

/// <summary>
/// Integration tests for the progressive disclosure workflow:
/// scan → metadata → validate → render list → activate skill → render details
/// </summary>
public class ProgressiveDisclosureIntegrationTests
{
    private readonly string _fixturesPath;

    public ProgressiveDisclosureIntegrationTests()
    {
        // Get path to fixtures directory
        var assemblyLocation = AppContext.BaseDirectory;
        var solutionRoot = Path.GetFullPath(Path.Combine(assemblyLocation, "..", "..", "..", "..", ".."));
        _fixturesPath = Path.Combine(solutionRoot, "fixtures", "skills");
    }

    [Fact]
    public void ProgressiveDisclosure_CompleteWorkflow_Succeeds()
    {
        // Arrange
        var loader = new FileSystemSkillLoader();
        var validator = new SkillValidator();
        var renderer = new DefaultSkillPromptRenderer();

        // Act & Assert - Step 1: Load metadata (fast path)
        var (metadata, metadataDiagnostics) = loader.LoadMetadata(_fixturesPath);
        Assert.NotEmpty(metadata);

        // Step 2: Validate metadata
        var validMetadata = new List<SkillMetadata>();
        foreach (var meta in metadata)
        {
            var result = validator.ValidateMetadata(meta);
            if (result.IsValid)
            {
                validMetadata.Add(meta);
            }
        }
        Assert.NotEmpty(validMetadata);

        // Step 3: Render skill list (progressive disclosure - stage 1)
        var listPrompt = renderer.RenderSkillList(validMetadata);
        Assert.NotEmpty(listPrompt);
        Assert.Contains("Available Skills", listPrompt);
        
        // Verify list contains skill names and descriptions but not full instructions
        foreach (var meta in validMetadata)
        {
            Assert.Contains(meta.Name, listPrompt);
            Assert.Contains(meta.Description, listPrompt);
        }

        // Step 4: Activate a specific skill (simulate LLM choosing one)
        var chosenSkill = validMetadata.First();
        var skillPath = chosenSkill.Path;
        var (skill, skillDiagnostics) = loader.LoadSkill(skillPath);
        Assert.NotNull(skill);

        // Step 5: Render full skill details (progressive disclosure - stage 2)
        var detailsPrompt = renderer.RenderSkillDetails(skill);
        Assert.NotEmpty(detailsPrompt);
        Assert.Contains(skill.Manifest.Name, detailsPrompt);
        Assert.Contains(skill.Manifest.Description, detailsPrompt);
        Assert.Contains(skill.Instructions, detailsPrompt);
    }

    [Fact]
    public void ProgressiveDisclosure_WithResourcePolicy_AppliesPolicyCorrectly()
    {
        // Arrange
        var loader = new FileSystemSkillLoader();
        var renderer = new DefaultSkillPromptRenderer();
        var exampleSkillPath = Path.Combine(_fixturesPath, "example-skill");
        var (skill, _) = loader.LoadSkill(exampleSkillPath);
        Assert.NotNull(skill);

        // Act - Render with different policies
        var includeAllOptions = new PromptRenderOptions
        {
            ResourcePolicy = IncludeAllResourcePolicy.Instance
        };
        var includeAllPrompt = renderer.RenderSkillDetails(skill, includeAllOptions);

        var excludeAllOptions = new PromptRenderOptions
        {
            ResourcePolicy = ExcludeAllResourcePolicy.Instance
        };
        var excludeAllPrompt = renderer.RenderSkillDetails(skill, excludeAllOptions);

        // Assert
        if (skill.Manifest.AllowedTools.Any())
        {
            Assert.Contains("Allowed Tools", includeAllPrompt);
            Assert.DoesNotContain("Allowed Tools", excludeAllPrompt);
        }
    }

    [Fact]
    public void ProgressiveDisclosure_WithCustomOptions_RespectsOptions()
    {
        // Arrange
        var loader = new FileSystemSkillLoader();
        var renderer = new DefaultSkillPromptRenderer();
        var exampleSkillPath = Path.Combine(_fixturesPath, "example-skill");
        var (skill, _) = loader.LoadSkill(exampleSkillPath);
        Assert.NotNull(skill);

        // Act - Render with custom options
        var minimalOptions = new PromptRenderOptions
        {
            IncludeVersion = false,
            IncludeAuthor = false,
            IncludeTags = false,
            IncludeAllowedTools = false
        };
        var minimalPrompt = renderer.RenderSkillDetails(skill, minimalOptions);

        var fullOptions = new PromptRenderOptions
        {
            IncludeVersion = true,
            IncludeAuthor = true,
            IncludeTags = true,
            IncludeAllowedTools = true
        };
        var fullPrompt = renderer.RenderSkillDetails(skill, fullOptions);

        // Assert
        Assert.Contains(skill.Manifest.Name, minimalPrompt);
        Assert.Contains(skill.Instructions, minimalPrompt);

        if (!string.IsNullOrWhiteSpace(skill.Manifest.Version))
        {
            Assert.DoesNotContain(skill.Manifest.Version, minimalPrompt);
            Assert.Contains(skill.Manifest.Version, fullPrompt);
        }

        if (!string.IsNullOrWhiteSpace(skill.Manifest.Author))
        {
            Assert.DoesNotContain(skill.Manifest.Author, minimalPrompt);
            Assert.Contains(skill.Manifest.Author, fullPrompt);
        }
    }

    [Fact]
    public void ProgressiveDisclosure_RenderList_IsMoreCompactThanDetails()
    {
        // Arrange
        var loader = new FileSystemSkillLoader();
        var renderer = new DefaultSkillPromptRenderer();
        var (metadata, _) = loader.LoadMetadata(_fixturesPath);
        var firstMeta = metadata.First();
        var (skill, _) = loader.LoadSkill(firstMeta.Path);
        Assert.NotNull(skill);

        // Act
        var listPrompt = renderer.RenderSkillList(new[] { firstMeta });
        var detailsPrompt = renderer.RenderSkillDetails(skill);

        // Assert - List should be more compact (doesn't include instructions)
        Assert.True(listPrompt.Length < detailsPrompt.Length,
            "Skill list should be more compact than full details");
        
        // List should not contain full instructions
        Assert.DoesNotContain(skill.Instructions, listPrompt);
        
        // Details should contain full instructions
        Assert.Contains(skill.Instructions, detailsPrompt);
    }

    [Fact]
    public void ProgressiveDisclosure_ValidateBeforeRender_FiltersInvalidSkills()
    {
        // Arrange
        var loader = new FileSystemSkillLoader();
        var validator = new SkillValidator();
        var renderer = new DefaultSkillPromptRenderer();
        var (metadata, _) = loader.LoadMetadata(_fixturesPath);

        // Act - Filter to only valid skills
        var validMetadata = metadata
            .Where(m => validator.ValidateMetadata(m).IsValid)
            .ToList();

        var allSkillsPrompt = renderer.RenderSkillList(metadata);
        var validSkillsPrompt = renderer.RenderSkillList(validMetadata);

        // Assert
        // Valid skills prompt should be shorter or equal (no invalid skills)
        Assert.True(validSkillsPrompt.Length <= allSkillsPrompt.Length);
        
        // All valid skills should appear in the prompt
        foreach (var meta in validMetadata)
        {
            Assert.Contains(meta.Name, validSkillsPrompt);
        }
    }
}
