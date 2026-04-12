using System.Text.Json;
using AsorAssistant.Application.Serialization;
using AsorAssistant.Domain.Models;

namespace AsorAssistant.Application.Tests.Serialization;

public class AgentDefinitionSerializerTests
{
    private static AgentDefinition CreateMinimalDefinition() => new()
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

    private static AgentDefinition CreateFullDefinition() => new()
    {
        Name = "Full Agent",
        Description = "A fully populated agent",
        Url = "https://example.com/full-agent",
        Version = "2.0.0",
        Provider = new Provider { Id = "full-provider" },
        Platform = new Platform { Id = "OTHER" },
        Capabilities = new Capabilities
        {
            PushNotifications = true,
            Streaming = false,
            StateTransitionHistory = true
        },
        Skills =
        [
            new AgentSkill
            {
                Id = "skill-1",
                Name = "Skill One",
                Description = "First skill",
                Tags = [new SkillTag { Tag = "tag1" }, new SkillTag { Tag = "tag2" }],
                InputModes = ["application/json"],
                OutputModes = ["application/json"]
            }
        ],
        Overview = "Full agent overview",
        IconUrl = "https://example.com/icon.png",
        DocumentationUrl = "https://example.com/docs",
        ExternalAgentID = "ext-agent-001",
        ExternalTenantID = "ext-tenant-001",
        DefaultInputModes = ["application/json"],
        DefaultOutputModes = ["application/json"],
        SupportsAuthenticatedExtendedCard = false,
        WorkdayConfig =
        [
            new AgentSkillResource
            {
                SkillId = "skill-1",
                ExecutionMode = ExecutionMode.Delegate,
                WorkdayResources =
                [
                    new WorkdayResource
                    {
                        AgentResource = new AgentResource { Id = "wd-tool-001" },
                        ToolName = "Get_Data",
                        Description = "Gets data",
                        Tools = [new SecurableItem { Id = "ds-001" }]
                    }
                ]
            }
        ]
    };

    [Fact]
    public void Roundtrip_minimal_definition_preserves_all_fields()
    {
        var original = CreateMinimalDefinition();
        var json = AgentDefinitionSerializer.Serialize(original);
        var deserialized = AgentDefinitionSerializer.Deserialize(json);

        Assert.NotNull(deserialized);
        Assert.Equal(original.Name, deserialized.Name);
        Assert.Equal(original.Description, deserialized.Description);
        Assert.Equal(original.Url, deserialized.Url);
        Assert.Equal(original.Version, deserialized.Version);
        Assert.Equal(original.Provider!.Id, deserialized.Provider!.Id);
        Assert.Equal(original.Platform!.Id, deserialized.Platform!.Id);
        Assert.NotNull(deserialized.Capabilities);
        Assert.Single(deserialized.Skills!);
        Assert.Equal("skill-1", deserialized.Skills![0].Id);
    }

    [Fact]
    public void Roundtrip_full_definition_preserves_all_fields()
    {
        var original = CreateFullDefinition();
        var json = AgentDefinitionSerializer.Serialize(original);
        var deserialized = AgentDefinitionSerializer.Deserialize(json);

        Assert.NotNull(deserialized);
        Assert.Equal(original.Overview, deserialized.Overview);
        Assert.Equal(original.IconUrl, deserialized.IconUrl);
        Assert.Equal(original.DocumentationUrl, deserialized.DocumentationUrl);
        Assert.Equal(original.ExternalAgentID, deserialized.ExternalAgentID);
        Assert.Equal(original.ExternalTenantID, deserialized.ExternalTenantID);
        Assert.Equal(original.DefaultInputModes, deserialized.DefaultInputModes);
        Assert.Equal(original.DefaultOutputModes, deserialized.DefaultOutputModes);
        Assert.Equal(original.SupportsAuthenticatedExtendedCard, deserialized.SupportsAuthenticatedExtendedCard);
        Assert.True(deserialized.Capabilities!.PushNotifications);
        Assert.False(deserialized.Capabilities.Streaming);
        Assert.True(deserialized.Capabilities.StateTransitionHistory);

        var config = deserialized.WorkdayConfig;
        Assert.NotNull(config);
        Assert.Single(config);
        Assert.Equal("skill-1", config[0].SkillId);
        Assert.Equal(ExecutionMode.Delegate, config[0].ExecutionMode);

        var wr = config[0].WorkdayResources;
        Assert.NotNull(wr);
        Assert.Single(wr);
        Assert.Equal("Get_Data", wr[0].ToolName);
        Assert.Equal("wd-tool-001", wr[0].AgentResource!.Id);
        Assert.Single(wr[0].Tools!);
        Assert.Equal("ds-001", wr[0].Tools![0].Id);
    }

    [Fact]
    public void Serialized_json_uses_camelCase_property_names()
    {
        var definition = CreateMinimalDefinition();
        var json = AgentDefinitionSerializer.Serialize(definition);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("name", out _));
        Assert.True(root.TryGetProperty("description", out _));
        Assert.True(root.TryGetProperty("url", out _));
        Assert.True(root.TryGetProperty("version", out _));
        Assert.True(root.TryGetProperty("provider", out _));
        Assert.True(root.TryGetProperty("platform", out _));
        Assert.True(root.TryGetProperty("capabilities", out _));
        Assert.True(root.TryGetProperty("skills", out _));

        // Should NOT have PascalCase versions
        Assert.False(root.TryGetProperty("Name", out _));
        Assert.False(root.TryGetProperty("Description", out _));
    }

    [Fact]
    public void WorkdayResource_uses_snake_case_per_spec()
    {
        var definition = CreateFullDefinition();
        var json = AgentDefinitionSerializer.Serialize(definition);
        var doc = JsonDocument.Parse(json);

        var workdayResource = doc.RootElement
            .GetProperty("workdayConfig")[0]
            .GetProperty("workdayResources")[0];

        Assert.True(workdayResource.TryGetProperty("tool_name", out _));
        Assert.True(workdayResource.TryGetProperty("agent_resource", out _));

        // Should NOT have camelCase versions of these spec-defined snake_case fields
        Assert.False(workdayResource.TryGetProperty("toolName", out _));
        Assert.False(workdayResource.TryGetProperty("agentResource", out _));
    }

    [Fact]
    public void Null_optional_fields_omitted_from_output()
    {
        var definition = CreateMinimalDefinition();
        var json = AgentDefinitionSerializer.Serialize(definition);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.False(root.TryGetProperty("id", out _));
        Assert.False(root.TryGetProperty("overview", out _));
        Assert.False(root.TryGetProperty("iconUrl", out _));
        Assert.False(root.TryGetProperty("documentationUrl", out _));
        Assert.False(root.TryGetProperty("externalAgentID", out _));
        Assert.False(root.TryGetProperty("externalTenantID", out _));
        Assert.False(root.TryGetProperty("defaultInputModes", out _));
        Assert.False(root.TryGetProperty("defaultOutputModes", out _));
        Assert.False(root.TryGetProperty("supportsAuthenticatedExtendedCard", out _));
        Assert.False(root.TryGetProperty("workdayConfig", out _));
    }

    [Fact]
    public void Deserialize_minimal_sample_payload()
    {
        var json = File.ReadAllText(
            Path.Combine(GetSamplesDir(), "agent-card-minimal.json"));
        var definition = AgentDefinitionSerializer.Deserialize(json);

        Assert.NotNull(definition);
        Assert.Equal("Expense Report Assistant", definition.Name);
        Assert.Equal("OTHER", definition.Platform!.Id);
        Assert.Single(definition.Skills!);
        Assert.Equal("submit-expense", definition.Skills![0].Id);
    }

    [Fact]
    public void Deserialize_full_sample_payload()
    {
        var json = File.ReadAllText(
            Path.Combine(GetSamplesDir(), "agent-card-full.json"));
        var definition = AgentDefinitionSerializer.Deserialize(json);

        Assert.NotNull(definition);
        Assert.Equal("HR Onboarding Agent", definition.Name);
        Assert.Equal(2, definition.Skills!.Count);
        Assert.NotNull(definition.WorkdayConfig);
        Assert.Equal(2, definition.WorkdayConfig.Count);
        Assert.Equal(ExecutionMode.Delegate, definition.WorkdayConfig[0].ExecutionMode);
        Assert.Equal("Get_Documents", definition.WorkdayConfig[0].WorkdayResources![0].ToolName);
    }

    [Fact]
    public void Roundtrip_sample_payload_preserves_structure()
    {
        var json = File.ReadAllText(
            Path.Combine(GetSamplesDir(), "agent-card-full.json"));
        var definition = AgentDefinitionSerializer.Deserialize(json);
        var reserialized = AgentDefinitionSerializer.Serialize(definition!);
        var roundtripped = AgentDefinitionSerializer.Deserialize(reserialized);

        Assert.Equal(definition!.Name, roundtripped!.Name);
        Assert.Equal(definition.WorkdayConfig!.Count, roundtripped.WorkdayConfig!.Count);
        Assert.Equal(
            definition.WorkdayConfig[0].WorkdayResources![0].ToolName,
            roundtripped.WorkdayConfig[0].WorkdayResources![0].ToolName);
    }

    private static string GetSamplesDir()
    {
        // Walk up from bin/Debug/net10.0 to repo root
        var dir = AppContext.BaseDirectory;
        while (dir is not null && !File.Exists(Path.Combine(dir, "AsorAssistant.slnx")))
            dir = Directory.GetParent(dir)?.FullName;

        return Path.Combine(dir!, "docs", "samples");
    }
}
