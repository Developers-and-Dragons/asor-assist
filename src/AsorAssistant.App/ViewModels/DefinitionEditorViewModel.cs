using System.Collections.ObjectModel;
using AsorAssistant.Domain.Models;
using AsorAssistant.Domain.Validation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AsorAssistant.App.ViewModels;

public partial class DefinitionEditorViewModel : ObservableObject
{
    // Required fields
    [ObservableProperty]
    private string? _name;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private string? _url;

    [ObservableProperty]
    private string? _version;

    // Provider — dropdown + manual entry
    [ObservableProperty]
    private string? _selectedProviderId;

    [ObservableProperty]
    private string? _customProviderId;

    // Platform — dropdown + manual entry
    [ObservableProperty]
    private string? _selectedPlatformId;

    [ObservableProperty]
    private string? _customPlatformId;

    // Capabilities
    [ObservableProperty]
    private bool _pushNotifications;

    [ObservableProperty]
    private bool _streaming;

    [ObservableProperty]
    private bool _stateTransitionHistory;

    // Optional fields
    [ObservableProperty]
    private string? _overview;

    [ObservableProperty]
    private string? _iconUrl;

    [ObservableProperty]
    private string? _documentationUrl;

    [ObservableProperty]
    private string? _externalAgentId;

    [ObservableProperty]
    private string? _externalTenantId;

    [ObservableProperty]
    private string? _defaultInputModesText;

    [ObservableProperty]
    private string? _defaultOutputModesText;

    [ObservableProperty]
    private bool _supportsAuthenticatedExtendedCard;

    // Skills
    public ObservableCollection<SkillViewModel> Skills { get; } = [];

    // Workday Config
    public ObservableCollection<AgentSkillResourceViewModel> WorkdayConfig { get; } = [];

    // Validation
    [ObservableProperty]
    private string? _validationErrors;

    [ObservableProperty]
    private bool _isValid;

    // Help panel
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HelpTitle))]
    [NotifyPropertyChangedFor(nameof(HelpContent))]
    private string _activeSection = "default";

    public string HelpTitle => ActiveSection switch
    {
        "identity" => "Identity",
        "provider" => "Provider & Platform",
        "capabilities" => "Capabilities",
        "skills" => "Skills",
        "workdayconfig" => "Workday Config",
        "optional" => "Optional Fields",
        _ => "Getting Started"
    };

    public string HelpContent => ActiveSection switch
    {
        "identity" => """
        These fields identify your agent in the Workday catalog.

        Required fields:
        • Name — human-readable, shown to admins
        • Description — helps users understand what the agent does
        • URL — where the agent is hosted
        • Version — format is up to you

        Tips:
        • Name + Provider + Version must be unique — duplicates update the existing registration
        • URL should be publicly reachable by Workday
        """,

        "provider" => """
        Provider identifies who built the agent. Platform identifies where it runs.

        • Select from the known list, or choose "Custom..." to enter a reference ID
        • IDs use the format Provider=VALUE and Platform=VALUE
        • If your provider isn't listed, use "Custom..." and enter the ID manually

        Common platforms:
        • Microsoft Copilot Studio, Azure AI Foundry
        • Amazon Bedrock AgentCore
        • Salesforce Agentforce
        • Use "Other" if none match
        """,

        "capabilities" => """
        Feature flags that tell Workday what your agent supports.

        • Push Notifications — agent can send async updates without polling
        • Streaming — supports Server-Sent Events (SSE)
        • State Transition History — tracks and exposes task state changes

        Leave disabled unless your agent actively implements these. All three are included in the JSON with true/false values.
        """,

        "skills" => """
        Skills define what your agent can do. At least one is required.

        Required per skill:
        • ID — unique, stable identifier (used in API calls and Workday Config references)
        • Name — human-readable
        • Description — helps clients understand the skill

        Optional:
        • Tags — classification keywords
        • Input/Output Modes — MIME types (e.g., application/json)

        Tips:
        • Skill IDs are referenced by Workday Config entries
        • Keep IDs stable across versions
        """,

        "workdayconfig" => """
        Maps skills to Workday resources and execution modes.

        Each entry links a Skill ID to:
        • Execution Mode
          - Delegate: requires human invocation (interactive)
          - Ambient: runs without human trigger (background)
          - This determines the OAuth grant type
        • Workday Resources — the operations this skill uses

        Per resource:
        • Tool Name — the endpoint or task name
        • Agent Resource ID — Workday tool WID (use Lookups to find)
        • Securable Items — related data sources, CRF fields, etc.

        Tip: Use the Lookups tab to search for WIDs by operation name.
        """,

        "optional" => """
        These fields are not required but add useful metadata.

        • Overview — shown when summarizing or initially invoking the agent
        • Icon URL — visual representation in the Workday catalog
        • Documentation URL — link to external docs
        • External Agent ID — partner system identifier, useful for Workday callbacks
        • External Tenant ID — locates the agent in the partner system
        • Default Input/Output Modes — MIME types applied to all skills (overridable per skill)
        • Authenticated Extended Card — set true if the agent provides an extended card for authenticated users
        """,

        _ => """
        Welcome to ASOR Assistant. This tool helps you create and register agent definitions for the Workday Agent System of Record.

        Quick start:
        • Open — load a saved draft or fetch agents from a tenant
        • New — start a blank definition
        • Visual / JSON — switch between form and code editing
        • Save — save your work as a local draft
        • Register — POST to the Workday ASOR API

        Typical workflow:
        1. Set your region and bearer token in the connection bar above
        2. Open an existing definition or create new
        3. Fill in required fields (Identity, Provider, Skills)
        4. Add Workday Config mappings if needed
        5. Validate the definition
        6. Save as draft and/or register with Workday

        Use the Lookups tab to search for Workday service operation WIDs to use in your Workday Config.

        Click on any section in the editor to see specific guidance here.
        """
    };

    // Lookup data (with Custom option at the end)
    public IReadOnlyList<LookupOption> PlatformOptions { get; } =
        KnownPlatforms.All.Select(p => new LookupOption { Name = p.Name, Id = p.Id })
            .Append(new LookupOption { Name = "Custom...", Id = "__CUSTOM__" }).ToList();

    public IReadOnlyList<LookupOption> ProviderOptions { get; } =
        KnownProviders.All.Select(p => new LookupOption { Name = p.Name, Id = p.Id })
            .Append(new LookupOption { Name = "Custom...", Id = "__CUSTOM__" }).ToList();

    public bool IsCustomProvider => SelectedProviderId == "__CUSTOM__";
    public bool IsCustomPlatform => SelectedPlatformId == "__CUSTOM__";

    partial void OnSelectedProviderIdChanged(string? value) => OnPropertyChanged(nameof(IsCustomProvider));
    partial void OnSelectedPlatformIdChanged(string? value) => OnPropertyChanged(nameof(IsCustomPlatform));

    public DefinitionEditorViewModel()
    {
        AddDefaultSkill();
    }

    [RelayCommand]
    private void AddSkill()
    {
        Skills.Add(new SkillViewModel());
    }

    [RelayCommand]
    private void RemoveSkill(SkillViewModel skill)
    {
        Skills.Remove(skill);
    }

    [RelayCommand]
    private void AddWorkdayConfigEntry()
    {
        var entry = new AgentSkillResourceViewModel { AvailableSkills = Skills };
        entry.WorkdayResources.Add(new WorkdayResourceViewModel());
        WorkdayConfig.Add(entry);
    }

    [RelayCommand]
    private void RemoveWorkdayConfigEntry(AgentSkillResourceViewModel entry)
    {
        WorkdayConfig.Remove(entry);
    }

    [RelayCommand]
    public void Validate()
    {
        var definition = ToModel();
        var result = AgentDefinitionValidator.Validate(definition);
        IsValid = result.IsValid;
        ValidationErrors = result.IsValid ? null : string.Join("\n", result.Errors);

        if (IsValid)
            _ = AutoDismissValidation();
    }

    private async Task AutoDismissValidation()
    {
        await Task.Delay(3000);
        IsValid = false;
    }

    [RelayCommand]
    public void DismissErrors()
    {
        ValidationErrors = null;
        IsValid = false;
    }

    public AgentDefinition ToModel()
    {
        var providerId = SelectedProviderId == "__CUSTOM__" ? CustomProviderId : SelectedProviderId;
        var platformId = SelectedPlatformId == "__CUSTOM__" ? CustomPlatformId : SelectedPlatformId;

        return new AgentDefinition
        {
            Name = Name,
            Description = Description,
            Url = Url,
            Version = Version,
            Provider = new Provider { Id = providerId },
            Platform = new Platform { Id = platformId },
            Capabilities = new Capabilities
            {
                PushNotifications = PushNotifications,
                Streaming = Streaming,
                StateTransitionHistory = StateTransitionHistory
            },
            Skills = Skills.Select(s => s.ToModel()).ToList(),
            Overview = NullIfEmpty(Overview),
            IconUrl = NullIfEmpty(IconUrl),
            DocumentationUrl = NullIfEmpty(DocumentationUrl),
            ExternalAgentID = NullIfEmpty(ExternalAgentId),
            ExternalTenantID = NullIfEmpty(ExternalTenantId),
            DefaultInputModes = SkillViewModel.ParseMimeTypes(DefaultInputModesText),
            DefaultOutputModes = SkillViewModel.ParseMimeTypes(DefaultOutputModesText),
            SupportsAuthenticatedExtendedCard = SupportsAuthenticatedExtendedCard ? true : null,
            WorkdayConfig = WorkdayConfig.Count > 0
                ? WorkdayConfig.Select(c => c.ToModel()).ToList()
                : null
        };
    }

    public void LoadFromModel(AgentDefinition definition)
    {
        Name = definition.Name;
        Description = definition.Description;
        Url = definition.Url;
        Version = definition.Version;

        // Provider — match by full ref ID, WID, bare reference_id, or descriptor
        var prov = definition.Provider;
        if (prov?.Id is null && prov?.ReferenceId is null && prov?.Descriptor is null)
        {
            SelectedProviderId = null;
            CustomProviderId = null;
        }
        else
        {
            var providerMatch = MatchKnownValue(KnownProviders.All, prov?.Id, prov?.ReferenceId, prov?.Descriptor);
            if (providerMatch != default)
            {
                SelectedProviderId = providerMatch.Id;
                CustomProviderId = null;
            }
            else
            {
                SelectedProviderId = "__CUSTOM__";
                CustomProviderId = prov?.Descriptor ?? prov?.ReferenceId ?? prov?.Id;
            }
        }

        // Platform — match by full ref ID, WID, bare reference_id, or descriptor
        var plat = definition.Platform;
        if (plat?.Id is null && plat?.ReferenceId is null && plat?.Descriptor is null)
        {
            SelectedPlatformId = null;
            CustomPlatformId = null;
        }
        else
        {
            var platformMatch = MatchKnownValue(KnownPlatforms.All, plat?.Id, plat?.ReferenceId, plat?.Descriptor);
            if (platformMatch != default)
            {
                SelectedPlatformId = platformMatch.Id;
                CustomPlatformId = null;
            }
            else
            {
                SelectedPlatformId = "__CUSTOM__";
                CustomPlatformId = plat?.Descriptor ?? plat?.ReferenceId ?? plat?.Id;
            }
        }

        PushNotifications = definition.Capabilities?.PushNotifications == true;
        Streaming = definition.Capabilities?.Streaming == true;
        StateTransitionHistory = definition.Capabilities?.StateTransitionHistory == true;

        Overview = definition.Overview;
        IconUrl = definition.IconUrl;
        DocumentationUrl = definition.DocumentationUrl;
        ExternalAgentId = definition.ExternalAgentID;
        ExternalTenantId = definition.ExternalTenantID;
        DefaultInputModesText = definition.DefaultInputModes is not null
            ? string.Join(", ", definition.DefaultInputModes.Where(m => m.Type is not null).Select(m => m.Type))
            : null;
        DefaultOutputModesText = definition.DefaultOutputModes is not null
            ? string.Join(", ", definition.DefaultOutputModes.Where(m => m.Type is not null).Select(m => m.Type))
            : null;
        SupportsAuthenticatedExtendedCard = definition.SupportsAuthenticatedExtendedCard == true;

        Skills.Clear();
        if (definition.Skills is not null)
        {
            foreach (var skill in definition.Skills)
                Skills.Add(SkillViewModel.FromModel(skill));
        }

        WorkdayConfig.Clear();
        if (definition.WorkdayConfig is not null)
        {
            foreach (var entry in definition.WorkdayConfig)
                WorkdayConfig.Add(AgentSkillResourceViewModel.FromModel(entry, Skills));
        }

    }

    private void AddDefaultSkill()
    {
        if (Skills.Count == 0)
            Skills.Add(new SkillViewModel());
    }

    /// <summary>
    /// Match a known value list against an id (which may be a WID or a ref ID like "Provider=SELF-BUILT"),
    /// a bare reference_id (like "SELF-BUILT"), or a descriptor (like "Self-Built").
    /// </summary>
    private static (string Name, string Id) MatchKnownValue(
        IReadOnlyList<(string Name, string Id)> known, string? id, string? referenceId, string? descriptor)
    {
        // Direct match on full ref ID (e.g., "Provider=SELF-BUILT" == "Provider=SELF-BUILT")
        var byId = known.FirstOrDefault(k => k.Id == id);
        if (byId != default) return byId;

        // Match bare reference_id against the value part (e.g., "SELF-BUILT" matches "Provider=SELF-BUILT")
        if (referenceId is not null)
        {
            var byRef = known.FirstOrDefault(k => k.Id.EndsWith("=" + referenceId, StringComparison.Ordinal));
            if (byRef != default) return byRef;
        }

        // Match by descriptor (e.g., "Self-Built" matches name "Self-Built")
        if (descriptor is not null)
        {
            var byDesc = known.FirstOrDefault(k =>
                string.Equals(k.Name, descriptor, StringComparison.OrdinalIgnoreCase));
            if (byDesc != default) return byDesc;
        }

        return default;
    }

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}
