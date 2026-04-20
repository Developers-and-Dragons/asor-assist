using AsorAssistant.Domain.Models;
using AsorAssistant.Domain.Validation;

namespace AsorAssistant.Domain.Tests.Validation;

public class AgentDefinitionValidatorTests
{
    private static AgentDefinition CreateMinimalValid() => new()
    {
        Name = "Test Agent",
        Description = "A test agent",
        Url = "https://example.com/agent",
        Version = "1.0.0",
        Provider = new Provider { Id = "test-provider" },
        Platform = new Platform { Id = "OTHER" },
        Capabilities = new Capabilities(),
        Skills =
        [
            new AgentSkill
            {
                Id = "skill-1",
                Name = "Test Skill",
                Description = "A test skill",
                Tags = [new SkillTag { Tag = "test" }]
            }
        ]
    };

    [Fact]
    public void Valid_minimal_definition_passes()
    {
        var definition = CreateMinimalValid();
        var result = AgentDefinitionValidator.Validate(definition);
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Missing_name_fails()
    {
        var definition = CreateMinimalValid();
        definition.Name = null;
        var result = AgentDefinitionValidator.Validate(definition);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("name", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Empty_name_fails()
    {
        var definition = CreateMinimalValid();
        definition.Name = "  ";
        var result = AgentDefinitionValidator.Validate(definition);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("name", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Missing_description_fails()
    {
        var definition = CreateMinimalValid();
        definition.Description = null;
        var result = AgentDefinitionValidator.Validate(definition);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("description", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Missing_url_fails()
    {
        var definition = CreateMinimalValid();
        definition.Url = null;
        var result = AgentDefinitionValidator.Validate(definition);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("url", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Missing_version_fails()
    {
        var definition = CreateMinimalValid();
        definition.Version = null;
        var result = AgentDefinitionValidator.Validate(definition);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("version", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Missing_provider_fails()
    {
        var definition = CreateMinimalValid();
        definition.Provider = null;
        var result = AgentDefinitionValidator.Validate(definition);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("provider", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Missing_provider_id_fails()
    {
        var definition = CreateMinimalValid();
        definition.Provider = new Provider { Id = null };
        var result = AgentDefinitionValidator.Validate(definition);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("provider", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Missing_platform_fails()
    {
        var definition = CreateMinimalValid();
        definition.Platform = null;
        var result = AgentDefinitionValidator.Validate(definition);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("platform", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Missing_platform_id_fails()
    {
        var definition = CreateMinimalValid();
        definition.Platform = new Platform { Id = null };
        var result = AgentDefinitionValidator.Validate(definition);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("platform", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Missing_capabilities_fails()
    {
        var definition = CreateMinimalValid();
        definition.Capabilities = null;
        var result = AgentDefinitionValidator.Validate(definition);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("capabilities", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Null_skills_fails()
    {
        var definition = CreateMinimalValid();
        definition.Skills = null;
        var result = AgentDefinitionValidator.Validate(definition);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("skill", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Empty_skills_array_fails()
    {
        var definition = CreateMinimalValid();
        definition.Skills = [];
        var result = AgentDefinitionValidator.Validate(definition);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("skill", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Skill_missing_id_fails()
    {
        var definition = CreateMinimalValid();
        definition.Skills![0].Id = null;
        var result = AgentDefinitionValidator.Validate(definition);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("id", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Skill_missing_name_fails()
    {
        var definition = CreateMinimalValid();
        definition.Skills![0].Name = null;
        var result = AgentDefinitionValidator.Validate(definition);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("name", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Skill_missing_description_fails()
    {
        var definition = CreateMinimalValid();
        definition.Skills![0].Description = null;
        var result = AgentDefinitionValidator.Validate(definition);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("description", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Skill_without_tags_passes()
    {
        var definition = CreateMinimalValid();
        definition.Skills![0].Tags = null;
        var result = AgentDefinitionValidator.Validate(definition);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Duplicate_skill_ids_fail()
    {
        var definition = CreateMinimalValid();
        definition.Skills!.Add(new AgentSkill
        {
            Id = "skill-1", // duplicate
            Name = "Another Skill",
            Description = "Another skill",
            Tags = [new SkillTag { Tag = "test" }]
        });
        var result = AgentDefinitionValidator.Validate(definition);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("duplicate", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Invalid_execution_mode_fails()
    {
        var definition = CreateMinimalValid();
        definition.WorkdayConfig =
        [
            new AgentSkillResource
            {
                SkillId = "skill-1",
                ExecutionMode = new ExecutionMode { Id = "InvalidMode" },
                WorkdayResources = [new WorkdayResource { ToolName = "Test_Tool" }]
            }
        ];
        var result = AgentDefinitionValidator.Validate(definition);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("executionMode", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void WorkdayConfig_references_nonexistent_skill_fails()
    {
        var definition = CreateMinimalValid();
        definition.WorkdayConfig =
        [
            new AgentSkillResource
            {
                SkillId = "nonexistent-skill",
                ExecutionMode = new ExecutionMode { Id = ExecutionMode.Ambient },
                WorkdayResources = [new WorkdayResource { ToolName = "Test_Tool" }]
            }
        ];
        var result = AgentDefinitionValidator.Validate(definition);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("nonexistent-skill", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Valid_definition_with_workday_config_passes()
    {
        var definition = CreateMinimalValid();
        definition.WorkdayConfig =
        [
            new AgentSkillResource
            {
                SkillId = "skill-1",
                ExecutionMode = new ExecutionMode { Id = ExecutionMode.Delegate },
                WorkdayResources = [new WorkdayResource { ToolName = "Test_Tool" }]
            }
        ];
        var result = AgentDefinitionValidator.Validate(definition);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void WorkdayConfig_with_execution_mode_and_empty_resources_passes()
    {
        var definition = CreateMinimalValid();
        definition.WorkdayConfig =
        [
            new AgentSkillResource
            {
                SkillId = "skill-1",
                ExecutionMode = new ExecutionMode { Id = ExecutionMode.Delegate },
                WorkdayResources = []
            }
        ];
        var result = AgentDefinitionValidator.Validate(definition);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void WorkdayConfig_without_execution_mode_fails()
    {
        var definition = CreateMinimalValid();
        definition.WorkdayConfig =
        [
            new AgentSkillResource
            {
                SkillId = "skill-1",
                ExecutionMode = new ExecutionMode(),
                WorkdayResources = [new WorkdayResource { ToolName = "Test_Tool" }]
            }
        ];
        var result = AgentDefinitionValidator.Validate(definition);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("executionMode", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Multiple_errors_reported_together()
    {
        var definition = new AgentDefinition();
        var result = AgentDefinitionValidator.Validate(definition);
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count > 1);
    }
}
