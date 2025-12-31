# Usage Scenarios

Real-world scenarios demonstrating how to use AgentSkills.NET in different contexts.

## Table of Contents

- [Scenario 1: Personal Assistant Agent](#scenario-1-personal-assistant-agent)
- [Scenario 2: Code Review Bot](#scenario-2-code-review-bot)
- [Scenario 3: Customer Support Agent](#scenario-3-customer-support-agent)
- [Scenario 4: Data Analysis Agent](#scenario-4-data-analysis-agent)
- [Scenario 5: Multi-Tenant Agent Platform](#scenario-5-multi-tenant-agent-platform)
- [Scenario 6: CI/CD Pipeline Assistant](#scenario-6-cicd-pipeline-assistant)

## Scenario 1: Personal Assistant Agent

**Context:** Build a personal assistant that can help with email, scheduling, and task management.

### Setup

```csharp
using AgentSkills;
using AgentSkills.Loader;
using AgentSkills.Validation;
using AgentSkills.Prompts;

// Load skills from trusted directory
var loader = new FileSystemSkillLoader();
var skillSet = loader.LoadSkillSet("./skills/personal-assistant");

// Validate and filter
var validator = new SkillValidator();
var validSkills = skillSet.Skills
    .Where(s => validator.Validate(s).IsValid)
    .ToList();
```

### Skill Organization

```
skills/personal-assistant/
├── email-sender/         # Send emails
├── calendar-manager/     # Manage calendar
├── task-tracker/         # Track tasks
├── note-taker/          # Take notes
└── reminder-setter/     # Set reminders
```

### Progressive Disclosure

```csharp
// Stage 1: Present available capabilities
var renderer = new DefaultSkillPromptRenderer();
var metadata = validSkills.Select(s => s.Metadata);
var capabilities = renderer.RenderSkillList(metadata);

var systemPrompt = $@"
You are a personal assistant. You can help with various tasks.

{capabilities}

When the user needs help with a task, activate the appropriate skill.
";

await SendToLLM(systemPrompt);
```

### Handling Activation

```csharp
// Stage 2: When LLM activates a skill
string activatedSkillName = "email-sender"; // From LLM response

var skill = validSkills.FirstOrDefault(s => 
    s.Manifest.Name.Equals(activatedSkillName, StringComparison.OrdinalIgnoreCase));

if (skill != null)
{
    var instructions = renderer.RenderSkillDetails(skill);
    await SendToLLM(instructions);
}
```

### Security Configuration

```csharp
// Use restrictive policy for personal data
var options = new PromptRenderOptions
{
    ResourcePolicy = ExcludeAllResourcePolicy.Instance,
    IncludeAllowedTools = false  // Don't expose tool requests
};

var safePrompt = renderer.RenderSkillDetails(skill, options);
```

### Key Considerations

- **Privacy**: Use restrictive resource policies
- **Validation**: Always validate skills before use
- **Tool Control**: Implement tool access control
- **User Consent**: Confirm before executing actions

## Scenario 2: Code Review Bot

**Context:** Automated code review bot for pull requests.

### Setup

```csharp
// Load code review skills
var skillSet = loader.LoadSkillSet("./skills/code-review");

// Filter by tag
var reviewSkills = skillSet.Skills
    .Where(s => s.Manifest.Tags.Contains("code-review"))
    .Where(s => validator.Validate(s).IsValid)
    .ToList();
```

### Skill Organization

```
skills/code-review/
├── code-reviewer/         # Main review workflow
├── security-checker/      # Security analysis
├── style-checker/         # Code style review
├── test-coverage/         # Test analysis
└── performance-analyzer/  # Performance review
```

### Integration with GitHub

```csharp
// When PR is opened
public async Task ReviewPullRequest(int prNumber)
{
    // Get PR details
    var pr = await GetPullRequest(prNumber);
    
    // Load code review skill
    var reviewSkill = reviewSkills.First(s => s.Manifest.Name == "code-reviewer");
    var instructions = renderer.RenderSkillDetails(reviewSkill);
    
    // Prepare context for LLM
    var prompt = $@"
{instructions}

Review the following pull request:

**Title:** {pr.Title}
**Description:** {pr.Description}
**Changed Files:** {pr.ChangedFiles.Count}

**Diff:**
{pr.Diff}

Provide a structured review following the skill instructions.
";
    
    var review = await SendToLLM(prompt);
    await PostReviewComment(prNumber, review);
}
```

### Specialized Reviews

```csharp
// Security-focused review
var securitySkill = reviewSkills.First(s => s.Manifest.Name == "security-checker");
var securityPrompt = renderer.RenderSkillDetails(securitySkill);

// Performance review
var perfSkill = reviewSkills.First(s => s.Manifest.Name == "performance-analyzer");
var perfPrompt = renderer.RenderSkillDetails(perfSkill);

// Combined review
var combinedPrompt = $@"
Perform a comprehensive review using these specialized skills:

1. Security Analysis:
{securityPrompt}

2. Performance Analysis:
{perfPrompt}
";
```

### Key Considerations

- **Caching**: Cache skill prompts (they don't change often)
- **Parallel Reviews**: Run different review types in parallel
- **Rate Limiting**: Respect API rate limits
- **Quality Gates**: Define thresholds for auto-approval

## Scenario 3: Customer Support Agent

**Context:** Customer support chatbot with access to multiple support skills.

### Setup

```csharp
// Load support skills
var supportSkillSet = loader.LoadSkillSet("./skills/customer-support");

// Categorize by function
var troubleshooting = supportSkillSet.Skills
    .Where(s => s.Manifest.Tags.Contains("troubleshooting"))
    .ToList();

var accountManagement = supportSkillSet.Skills
    .Where(s => s.Manifest.Tags.Contains("account"))
    .ToList();

var billing = supportSkillSet.Skills
    .Where(s => s.Manifest.Tags.Contains("billing"))
    .ToList();
```

### Dynamic Skill Loading

```csharp
// Load skills based on customer issue category
public async Task HandleCustomerIssue(string issueCategory, string issueDescription)
{
    // Select relevant skills
    var relevantSkills = issueCategory switch
    {
        "technical" => troubleshooting,
        "account" => accountManagement,
        "billing" => billing,
        _ => supportSkillSet.Skills // All skills
    };
    
    // Present relevant capabilities only
    var metadata = relevantSkills.Select(s => s.Metadata);
    var capabilities = renderer.RenderSkillList(metadata);
    
    var prompt = $@"
Customer Issue: {issueDescription}
Category: {issueCategory}

Available support capabilities:
{capabilities}

Select and activate the most appropriate skill to help resolve this issue.
";
    
    await SendToLLM(prompt);
}
```

### Session Management

```csharp
// Track activated skills per session
public class SupportSession
{
    public string SessionId { get; set; }
    public List<string> ActivatedSkills { get; set; } = new();
    public Dictionary<string, object> Context { get; set; } = new();
}

public async Task ContinueSession(SupportSession session, string userMessage)
{
    // Include previously activated skills in context
    var previousSkills = session.ActivatedSkills
        .Select(name => validSkills.First(s => s.Manifest.Name == name))
        .ToList();
    
    // Build context-aware prompt
    var contextPrompt = "Previously activated skills:\n";
    foreach (var skill in previousSkills)
    {
        contextPrompt += $"- {skill.Manifest.Name}: {skill.Manifest.Description}\n";
    }
    
    // Continue conversation with context
    await SendToLLM($"{contextPrompt}\n\nUser: {userMessage}");
}
```

### Key Considerations

- **Context Awareness**: Track conversation history
- **Skill Filtering**: Show only relevant skills
- **Escalation**: Define when to escalate to human
- **Audit Trail**: Log all skill activations

## Scenario 4: Data Analysis Agent

**Context:** Agent that helps analyze data files and generate insights.

### Setup

```csharp
// Load analysis skills
var analysisSkills = loader.LoadSkillSet("./skills/data-analysis");

// Organize by data type
var csvSkills = analysisSkills.Skills
    .Where(s => s.Manifest.Tags.Contains("csv"))
    .ToList();

var jsonSkills = analysisSkills.Skills
    .Where(s => s.Manifest.Tags.Contains("json"))
    .ToList();

var sqlSkills = analysisSkills.Skills
    .Where(s => s.Manifest.Tags.Contains("sql"))
    .ToList();
```

### File Type Detection

```csharp
public async Task AnalyzeFile(string filePath)
{
    var extension = Path.GetExtension(filePath).ToLowerInvariant();
    
    var relevantSkills = extension switch
    {
        ".csv" => csvSkills,
        ".json" => jsonSkills,
        ".sql" => sqlSkills,
        _ => analysisSkills.Skills
    };
    
    var metadata = relevantSkills.Select(s => s.Metadata);
    var capabilities = renderer.RenderSkillList(metadata);
    
    var prompt = $@"
File to analyze: {filePath}
File type: {extension}

Available analysis capabilities:
{capabilities}

Select the appropriate analysis skill and describe what insights to extract.
";
    
    await SendToLLM(prompt);
}
```

### Chaining Analysis

```csharp
// Perform multi-step analysis
public async Task PerformDeepAnalysis(string dataPath)
{
    // Step 1: Load and validate data
    var loadSkill = analysisSkills.Skills
        .First(s => s.Manifest.Name == "data-loader");
    var loadInstructions = renderer.RenderSkillDetails(loadSkill);
    
    // Step 2: Statistical analysis
    var statsSkill = analysisSkills.Skills
        .First(s => s.Manifest.Name == "statistical-analyzer");
    var statsInstructions = renderer.RenderSkillDetails(statsSkill);
    
    // Step 3: Visualization
    var vizSkill = analysisSkills.Skills
        .First(s => s.Manifest.Name == "data-visualizer");
    var vizInstructions = renderer.RenderSkillDetails(vizSkill);
    
    var workflow = $@"
Perform analysis in three phases:

Phase 1 - Load Data:
{loadInstructions}

Phase 2 - Analyze:
{statsInstructions}

Phase 3 - Visualize:
{vizInstructions}

Data location: {dataPath}
";
    
    await SendToLLM(workflow);
}
```

### Key Considerations

- **Resource Limits**: Set max file size limits
- **Timeout Handling**: Handle long-running analysis
- **Result Caching**: Cache analysis results
- **Error Recovery**: Handle malformed data gracefully

## Scenario 5: Multi-Tenant Agent Platform

**Context:** Platform hosting multiple agents for different organizations.

### Setup

```csharp
// Per-tenant skill isolation
public class TenantSkillManager
{
    private readonly ISkillLoader _loader;
    private readonly ISkillValidator _validator;
    private readonly Dictionary<string, SkillSet> _tenantSkills = new();
    
    public TenantSkillManager(ISkillLoader loader, ISkillValidator validator)
    {
        _loader = loader;
        _validator = validator;
    }
    
    public SkillSet GetTenantSkills(string tenantId)
    {
        if (!_tenantSkills.ContainsKey(tenantId))
        {
            var tenantPath = $"/data/tenants/{tenantId}/skills";
            var skillSet = _loader.LoadSkillSet(tenantPath);
            _tenantSkills[tenantId] = skillSet;
        }
        return _tenantSkills[tenantId];
    }
}
```

### Access Control

```csharp
public async Task<string> ActivateSkill(string tenantId, string userId, string skillName)
{
    // Verify tenant exists
    if (!IsTenantValid(tenantId))
        throw new UnauthorizedAccessException("Invalid tenant");
    
    // Verify user belongs to tenant
    if (!IsUserAuthorized(tenantId, userId))
        throw new UnauthorizedAccessException("User not authorized");
    
    // Get tenant-specific skills
    var skillSet = _tenantManager.GetTenantSkills(tenantId);
    var skill = skillSet.Skills
        .FirstOrDefault(s => s.Manifest.Name == skillName);
    
    if (skill == null)
        throw new NotFoundException($"Skill '{skillName}' not found");
    
    // Validate skill
    var validation = _validator.Validate(skill);
    if (!validation.IsValid)
        throw new InvalidOperationException("Skill validation failed");
    
    // Apply tenant-specific policies
    var policy = GetTenantResourcePolicy(tenantId);
    var options = new PromptRenderOptions { ResourcePolicy = policy };
    
    // Render with restrictions
    return _renderer.RenderSkillDetails(skill, options);
}
```

### Quota Management

```csharp
public class TenantQuotaManager
{
    public async Task<bool> CheckSkillActivationQuota(string tenantId)
    {
        var usage = await GetTenantUsage(tenantId);
        var limits = await GetTenantLimits(tenantId);
        
        return usage.SkillActivationsToday < limits.DailySkillActivations;
    }
    
    public async Task RecordSkillActivation(string tenantId, string skillName)
    {
        await IncrementCounter($"tenant:{tenantId}:activations:{DateTime.UtcNow:yyyy-MM-dd}");
        await LogActivity(tenantId, "skill_activation", skillName);
    }
}
```

### Key Considerations

- **Isolation**: Complete skill isolation between tenants
- **Quotas**: Enforce usage limits per tenant
- **Audit Logging**: Track all skill activations
- **Custom Policies**: Per-tenant resource policies
- **Performance**: Cache tenant skill sets

## Scenario 6: CI/CD Pipeline Assistant

**Context:** Agent that helps with continuous integration and deployment tasks.

### Setup

```csharp
// Load CI/CD skills
var cicdSkills = loader.LoadSkillSet("./skills/cicd");

// Organize by pipeline stage
var buildSkills = cicdSkills.Skills
    .Where(s => s.Manifest.Tags.Contains("build"))
    .ToList();

var testSkills = cicdSkills.Skills
    .Where(s => s.Manifest.Tags.Contains("test"))
    .ToList();

var deploySkills = cicdSkills.Skills
    .Where(s => s.Manifest.Tags.Contains("deploy"))
    .ToList();
```

### Pipeline Stage Handling

```csharp
public async Task HandlePipelineStage(string stage, PipelineContext context)
{
    var stageSkills = stage switch
    {
        "build" => buildSkills,
        "test" => testSkills,
        "deploy" => deploySkills,
        _ => cicdSkills.Skills
    };
    
    // Present stage-appropriate skills
    var metadata = stageSkills.Select(s => s.Metadata);
    var capabilities = renderer.RenderSkillList(metadata);
    
    var prompt = $@"
Pipeline Stage: {stage}
Repository: {context.Repository}
Branch: {context.Branch}
Commit: {context.CommitSha}

Available capabilities for this stage:
{capabilities}

Analyze the stage results and recommend actions.

Stage Output:
{context.StageOutput}
";
    
    var recommendations = await SendToLLM(prompt);
    await RecordRecommendations(context, recommendations);
}
```

### Failure Analysis

```csharp
public async Task AnalyzePipelineFailure(PipelineRun run)
{
    // Get relevant troubleshooting skills
    var troubleshootingSkills = cicdSkills.Skills
        .Where(s => s.Manifest.Tags.Contains("troubleshooting"))
        .ToList();
    
    foreach (var skill in troubleshootingSkills)
    {
        var instructions = renderer.RenderSkillDetails(skill);
        
        var analysis = await SendToLLM($@"
{instructions}

Analyze this pipeline failure:

Stage: {run.FailedStage}
Error: {run.ErrorMessage}
Logs: {run.Logs}

Provide diagnosis and remediation steps.
");
        
        await NotifyTeam(run, analysis);
    }
}
```

### Key Considerations

- **Security**: Never expose secrets in prompts
- **Sandboxing**: Skills should not execute directly
- **Notifications**: Alert on critical issues
- **Integration**: Connect with CI/CD platforms

## Common Patterns Across Scenarios

### 1. Skill Filtering

```csharp
// Filter by tags
var filtered = skills.Where(s => s.Manifest.Tags.Contains("tag"));

// Filter by validation
var valid = skills.Where(s => validator.Validate(s).IsValid);

// Filter by custom criteria
var matching = skills.Where(s => CustomCriteria(s));
```

### 2. Error Handling

```csharp
var (skill, diagnostics) = loader.LoadSkill(path);

if (skill == null)
{
    // Handle loading failure
    LogErrors(diagnostics);
    return;
}

var validation = validator.Validate(skill);
if (!validation.IsValid)
{
    // Handle validation failure
    LogErrors(validation.Errors);
    return;
}

// Use skill
```

### 3. Security-First Approach

```csharp
// Always use restrictive policies by default
var defaultPolicy = ExcludeAllResourcePolicy.Instance;
var options = new PromptRenderOptions
{
    ResourcePolicy = defaultPolicy,
    IncludeAllowedTools = false
};

// Only relax if explicitly required
if (IsInternalTrustedContext())
{
    options.ResourcePolicy = IncludeAllResourcePolicy.Instance;
}
```

### 4. Performance Optimization

```csharp
// Cache rendered prompts
private readonly Dictionary<string, string> _promptCache = new();

public string GetSkillPrompt(Skill skill)
{
    var key = skill.Manifest.Name;
    
    if (!_promptCache.ContainsKey(key))
    {
        _promptCache[key] = _renderer.RenderSkillDetails(skill);
    }
    
    return _promptCache[key];
}
```

## Additional Resources

- [Getting Started Guide](GETTING_STARTED.md)
- [Security & Safety Guide](SECURITY_AND_SAFETY.md)
- [Public API Reference](PUBLIC_API.md)
- [Example Skills](../../fixtures/skills/)

---

Have a different use case? Share it with the community via [GitHub Discussions](https://github.com/sfmskywalker/agentskillsdotnet/discussions)!
