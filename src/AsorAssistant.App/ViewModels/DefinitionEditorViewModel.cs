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

    // Inline field validation errors
    [ObservableProperty]
    private string? _nameError;

    [ObservableProperty]
    private string? _descriptionError;

    [ObservableProperty]
    private string? _urlError;

    [ObservableProperty]
    private string? _versionError;

    [ObservableProperty]
    private string? _providerError;

    [ObservableProperty]
    private string? _platformError;

    // Help panel
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HelpTitle))]
    [NotifyPropertyChangedFor(nameof(HelpContent))]
    [NotifyPropertyChangedFor(nameof(IsContextualHelp))]
    private string _activeSection = "default";

    public bool IsContextualHelp => ActiveSection != "default";

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
        📌 What this is
        Core identity fields shown in the Workday agent catalog.

        ✅ Required
        • Name — displayed to admins and users
        • Description — what the agent does
        • URL — where the agent is hosted
        • Version — any format you choose

        ⚠️ Watch out
        • Name + Provider + Version must be unique
        • Duplicates overwrite the existing registration
        • URL must be reachable by Workday

        💡 Tip
        Keep names clear and descriptive — they appear in the Workday catalog for discovery.
        """,

        "provider" => """
        📌 What this is
        Who built the agent and what platform it runs on.

        ✅ Required
        • Provider — your organization
        • Platform — the runtime (e.g., Copilot Studio, Bedrock, Other)

        ⚠️ Watch out
        • IDs use reference format: Provider=VALUE, Platform=VALUE
        • If yours isn't in the list, select "Custom..." and type the ID
        """,

        "capabilities" => """
        📌 What this is
        Feature flags telling Workday what your agent supports.

        ✅ Options
        • Push Notifications — async updates without polling
        • Streaming — Server-Sent Events (SSE)
        • State Transition History — task state tracking

        ⚠️ Watch out
        Leave unchecked unless your agent actively implements these. All three always appear in the JSON as true/false.
        """,

        "skills" => """
        📌 What this is
        Skills define what your agent can do. At least one is required.

        ✅ Required per skill
        • ID — unique, stable (referenced by Workday Config)
        • Name — human-readable label
        • Description — helps clients understand the skill

        📎 Optional
        • Tags — classification keywords
        • Input/Output Modes — MIME types like application/json

        ⚠️ Watch out
        • Don't change Skill IDs after registration — they're used as references
        • Workday Config entries link to skills by ID
        """,

        "workdayconfig" => """
        📌 What this is
        Maps your skills to Workday resources and access modes.

        ✅ Per mapping
        • Skill — select from skills you've defined above
        • Execution Mode:
          · Delegate = human-triggered (interactive)
          · Ambient = background (no human needed)
          · This determines the OAuth grant type

        ✅ Per resource
        • Tool Name — the operation name (endpoint or task)
        • Agent Resource ID — Workday tool WID
        • Securable Items — related data sources, CRF fields

        💡 Tip
        Use the Lookups tab to search for WIDs by operation name, then paste them here.
        """,

        "optional" => """
        📌 What this is
        Extra metadata — not required, but helpful.

        📎 Fields
        • Overview — shown when summarizing or invoking the agent
        • Icon URL — visual in the Workday catalog
        • Docs URL — link to external documentation
        • External Agent ID — useful for Workday callbacks
        • External Tenant ID — locates agent in partner system
        • Default Input/Output Modes — MIME types for all skills
        • Extended Card — for authenticated user experiences
        """,

        _ => """
        👋 Welcome to ASOR Assistant

        Create and register agent definitions for the Workday Agent System of Record (ASOR).

        ──────────────────────

        🚀 Quick start
        • Open — load a draft or fetch from tenant
        • New — blank definition
        • Visual / JSON — toggle editing modes
        • Save — save as a local draft
        • Register — POST to Workday ASOR

        ──────────────────────

        📋 Typical workflow
        1. Set region + bearer token above
        2. Open existing or create new
        3. Fill required fields (Identity, Provider, Skills)
        4. Add Workday Config mappings if needed
        5. Validate → Save → Register

        ──────────────────────

        🔍 Need WIDs?
        Use the Lookups tab to search for Workday service operation IDs.

        👆 Click any section in the editor to see specific guidance here.
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
        ClearFieldErrors();

        var definition = ToModel();
        var result = AgentDefinitionValidator.Validate(definition);
        IsValid = result.IsValid;
        ValidationErrors = result.IsValid ? null : string.Join("\n", result.Errors);

        if (!result.IsValid)
            ApplyFieldErrors(result.FieldErrors);

        if (IsValid)
            _ = AutoDismissValidation();
    }

    private void ApplyFieldErrors(Dictionary<string, string> fieldErrors)
    {
        // Top-level fields
        fieldErrors.TryGetValue("Name", out var nameErr); NameError = nameErr;
        fieldErrors.TryGetValue("Description", out var descErr); DescriptionError = descErr;
        fieldErrors.TryGetValue("Url", out var urlErr); UrlError = urlErr;
        fieldErrors.TryGetValue("Version", out var verErr); VersionError = verErr;
        fieldErrors.TryGetValue("Provider", out var provErr); ProviderError = provErr;
        fieldErrors.TryGetValue("Platform", out var platErr); PlatformError = platErr;

        // Skill-level errors
        for (var i = 0; i < Skills.Count; i++)
        {
            var skill = Skills[i];
            fieldErrors.TryGetValue($"Skills[{i}].Id", out var idErr); skill.IdError = idErr;
            fieldErrors.TryGetValue($"Skills[{i}].Name", out var snErr); skill.NameError = snErr;
            fieldErrors.TryGetValue($"Skills[{i}].Description", out var sdErr); skill.DescriptionError = sdErr;
        }
    }

    private void ClearFieldErrors()
    {
        NameError = null;
        DescriptionError = null;
        UrlError = null;
        VersionError = null;
        ProviderError = null;
        PlatformError = null;

        foreach (var skill in Skills)
            skill.ClearErrors();
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
        ClearFieldErrors();
    }

    [RelayCommand]
    public void ResetHelp()
    {
        ActiveSection = "default";
    }

    [RelayCommand]
    public void ShowHelp(string section)
    {
        ActiveSection = section;
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
