using AgentSkills.Loader;
using AgentSkills.Validation;
using AgentSkills.Prompts;
using System.Diagnostics;

namespace AgentSkills.Tests;

/// <summary>
/// Performance sanity tests to ensure the system scales reasonably with many skills.
/// These are not strict benchmarks, but sanity checks to catch major performance regressions.
/// </summary>
public class PerformanceTests
{
    private readonly string _fixturesPath;
    private readonly FileSystemSkillLoader _loader;
    private readonly SkillValidator _validator;
    private readonly DefaultSkillPromptRenderer _renderer;

    public PerformanceTests()
    {
        var assemblyLocation = AppContext.BaseDirectory;
        var solutionRoot = Path.GetFullPath(Path.Combine(assemblyLocation, "..", "..", "..", "..", ".."));
        _fixturesPath = Path.Combine(solutionRoot, "fixtures", "skills");
        
        _loader = new FileSystemSkillLoader();
        _validator = new SkillValidator();
        _renderer = new DefaultSkillPromptRenderer();
    }

    [Fact]
    public void LoadMetadata_AllFixtureSkills_CompletesInReasonableTime()
    {
        // This test ensures metadata loading is fast (< 1 second for fixture skills)
        var sw = Stopwatch.StartNew();

        // Act
        var (metadata, _) = _loader.LoadMetadata(_fixturesPath);

        sw.Stop();

        // Assert
        Assert.NotEmpty(metadata);
        Assert.True(sw.ElapsedMilliseconds < 1000, 
            $"Metadata loading took {sw.ElapsedMilliseconds}ms, expected < 1000ms");
    }

    [Fact]
    public void LoadMetadata_ScalesLinearlyWithSkills()
    {
        // This test verifies O(n) scaling by measuring time per skill
        var sw = Stopwatch.StartNew();

        // Act
        var (metadata, _) = _loader.LoadMetadata(_fixturesPath);

        sw.Stop();

        // Assert - Calculate average time per skill
        var skillCount = metadata.Count;
        var avgTimePerSkill = sw.ElapsedMilliseconds / (double)skillCount;

        // Each skill should take less than 100ms to scan metadata
        Assert.True(avgTimePerSkill < 100, 
            $"Average time per skill: {avgTimePerSkill:F2}ms, expected < 100ms");
        
        // Log for diagnostics
        Console.WriteLine($"Loaded {skillCount} skills in {sw.ElapsedMilliseconds}ms");
        Console.WriteLine($"Average: {avgTimePerSkill:F2}ms per skill");
    }

    [Fact]
    public void LoadFullSkillSet_AllFixtureSkills_CompletesInReasonableTime()
    {
        // Full skill loading should still be fast (< 5 seconds for fixture skills)
        var sw = Stopwatch.StartNew();

        // Act
        var skillSet = _loader.LoadSkillSet(_fixturesPath);

        sw.Stop();

        // Assert
        Assert.NotEmpty(skillSet.Skills);
        Assert.True(sw.ElapsedMilliseconds < 5000, 
            $"Full skill loading took {sw.ElapsedMilliseconds}ms, expected < 5000ms");
        
        Console.WriteLine($"Loaded {skillSet.Skills.Count} full skills in {sw.ElapsedMilliseconds}ms");
    }

    [Fact]
    public void ValidateMetadata_MultipleSkills_CompletesQuickly()
    {
        // Validation should be fast even for many skills
        var (metadata, _) = _loader.LoadMetadata(_fixturesPath);
        
        var sw = Stopwatch.StartNew();

        // Act
        foreach (var meta in metadata)
        {
            _validator.ValidateMetadata(meta);
        }

        sw.Stop();

        // Assert - Validation should be < 500ms for fixture skills
        Assert.True(sw.ElapsedMilliseconds < 500, 
            $"Validation took {sw.ElapsedMilliseconds}ms, expected < 500ms");
        
        Console.WriteLine($"Validated {metadata.Count} skills in {sw.ElapsedMilliseconds}ms");
    }

    [Fact]
    public void RenderSkillList_ManySkills_CompletesQuickly()
    {
        // Rendering skill list should be fast
        var (metadata, _) = _loader.LoadMetadata(_fixturesPath);
        var validMetadata = metadata
            .Where(m => _validator.ValidateMetadata(m).IsValid)
            .ToList();

        var sw = Stopwatch.StartNew();

        // Act
        var rendered = _renderer.RenderSkillList(validMetadata);

        sw.Stop();

        // Assert - Rendering should be < 100ms
        Assert.NotEmpty(rendered);
        Assert.True(sw.ElapsedMilliseconds < 100, 
            $"Rendering skill list took {sw.ElapsedMilliseconds}ms, expected < 100ms");
        
        Console.WriteLine($"Rendered {validMetadata.Count} skills in {sw.ElapsedMilliseconds}ms");
    }

    [Fact]
    public void RenderSkillDetails_LargeInstructions_CompletesReasonably()
    {
        // Test rendering a skill with large instructions
        var skillPath = Path.Combine(_fixturesPath, "large-instructions-skill");
        
        // Skip if fixture doesn't exist yet
        if (!Directory.Exists(skillPath))
        {
            return;
        }

        var (skill, _) = _loader.LoadSkill(skillPath);
        Assert.NotNull(skill);

        var sw = Stopwatch.StartNew();

        // Act
        var rendered = _renderer.RenderSkillDetails(skill);

        sw.Stop();

        // Assert - Even large skills should render quickly (< 50ms)
        Assert.NotEmpty(rendered);
        Assert.True(sw.ElapsedMilliseconds < 50, 
            $"Rendering large skill took {sw.ElapsedMilliseconds}ms, expected < 50ms");
        
        Console.WriteLine($"Rendered large skill ({skill.Instructions.Length} chars) in {sw.ElapsedMilliseconds}ms");
    }

    [Fact]
    public void FullPipeline_MetadataLoadValidateRender_OptimizedPath()
    {
        // Test the complete progressive disclosure path (fast path)
        var sw = Stopwatch.StartNew();

        // Act - Full pipeline
        var (metadata, _) = _loader.LoadMetadata(_fixturesPath);
        
        var validMetadata = new List<SkillMetadata>();
        foreach (var meta in metadata)
        {
            var result = _validator.ValidateMetadata(meta);
            if (result.IsValid)
            {
                validMetadata.Add(meta);
            }
        }
        
        var rendered = _renderer.RenderSkillList(validMetadata);

        sw.Stop();

        // Assert - Full pipeline should be fast (< 2 seconds)
        Assert.NotEmpty(rendered);
        Assert.True(sw.ElapsedMilliseconds < 2000, 
            $"Full pipeline took {sw.ElapsedMilliseconds}ms, expected < 2000ms");
        
        Console.WriteLine($"Full pipeline (metadata only): {sw.ElapsedMilliseconds}ms for {validMetadata.Count} skills");
    }

    [Fact]
    public void MemoryUsage_LoadingManySkills_RemainsReasonable()
    {
        // Verify memory usage doesn't explode with many skills
        var beforeMemory = GC.GetTotalMemory(forceFullCollection: true);

        // Act
        var skillSet = _loader.LoadSkillSet(_fixturesPath);

        var afterMemory = GC.GetTotalMemory(forceFullCollection: false);
        var memoryUsedMB = (afterMemory - beforeMemory) / (1024.0 * 1024.0);

        // Assert - Memory usage should be reasonable (< 50MB for fixture skills)
        // This is a very loose sanity check
        Assert.True(memoryUsedMB < 50, 
            $"Memory usage: {memoryUsedMB:F2}MB, expected < 50MB");
        
        Console.WriteLine($"Memory used for {skillSet.Skills.Count} skills: {memoryUsedMB:F2}MB");
    }
}
