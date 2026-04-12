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

    // Lookup data
    public IReadOnlyList<LookupOption> PlatformOptions { get; } =
        KnownPlatforms.All.Select(p => new LookupOption { Name = p.Name, Id = p.Id }).ToList();

    public IReadOnlyList<LookupOption> ProviderOptions { get; } =
        KnownProviders.All.Select(p => new LookupOption { Name = p.Name, Id = p.Id }).ToList();

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
        var entry = new AgentSkillResourceViewModel();
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
                PushNotifications = PushNotifications ? true : null,
                Streaming = Streaming ? true : null,
                StateTransitionHistory = StateTransitionHistory ? true : null
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

        // Provider
        var providerMatch = KnownProviders.All.FirstOrDefault(p => p.Id == definition.Provider?.Id);
        if (providerMatch != default)
        {
            SelectedProviderId = providerMatch.Id;
            CustomProviderId = null;
        }
        else
        {
            SelectedProviderId = "__CUSTOM__";
            CustomProviderId = definition.Provider?.Id;
        }

        // Platform
        var platformMatch = KnownPlatforms.All.FirstOrDefault(p => p.Id == definition.Platform?.Id);
        if (platformMatch != default)
        {
            SelectedPlatformId = platformMatch.Id;
            CustomPlatformId = null;
        }
        else
        {
            SelectedPlatformId = "__CUSTOM__";
            CustomPlatformId = definition.Platform?.Id;
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
                WorkdayConfig.Add(AgentSkillResourceViewModel.FromModel(entry));
        }

        Validate();
    }

    private void AddDefaultSkill()
    {
        if (Skills.Count == 0)
            Skills.Add(new SkillViewModel());
    }

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}
