using System.Collections.ObjectModel;
using AsorAssistant.Core.Models;
using AsorAssistant.Core.Ports;
using AsorAssistant.Core.Serialization;
using AsorAssistant.Domain.Models;
using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AsorAssistant.App.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly IDraftStore _draftStore;
    private readonly IAsorQueryClient _queryClient;

    // Navigation: 0 = Editor, 1 = Lookups
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditorPage))]
    [NotifyPropertyChangedFor(nameof(IsLookupsPage))]
    private int _selectedPageIndex;

    public bool IsEditorPage => SelectedPageIndex == 0;
    public bool IsLookupsPage => SelectedPageIndex == 1;

    // Bearer token (app-level)
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasToken))]
    private string? _bearerToken;

    [ObservableProperty]
    private bool _isConnectionBarExpanded = true;

    [ObservableProperty]
    private AsorRegion? _selectedRegion;

    public bool HasToken => !string.IsNullOrWhiteSpace(BearerToken);
    public IReadOnlyList<AsorRegion> Regions => AsorRegion.All;

    // Editor mode: Visual vs JSON
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsVisualMode))]
    [NotifyPropertyChangedFor(nameof(IsJsonMode))]
    private bool _jsonModeActive;

    public bool IsVisualMode => !JsonModeActive;
    public bool IsJsonMode => JsonModeActive;

    [ObservableProperty]
    private string? _jsonText;

    // Right panel
    [ObservableProperty]
    private bool _isRegistrationPanelOpen;

    // File drawer (left)
    [ObservableProperty]
    private bool _isFileDrawerOpen;

    // Draft state
    [ObservableProperty]
    private string? _currentDraftName;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private bool _showSaveSuccess;

    private string? _currentDraftId;

    // Draft lists
    public ObservableCollection<DraftMetadata> LocalDrafts { get; } = [];
    public ObservableCollection<AgentDefinition> RemoteAgents { get; } = [];

    [ObservableProperty]
    private AsorRegion? _remoteRegion;

    [ObservableProperty]
    private bool _isFetchingRemote;

    [ObservableProperty]
    private string? _remoteStatusMessage;

    // Child VMs
    public DefinitionEditorViewModel Editor { get; }
    public RegistrationViewModel Registration { get; }
    public WqlLookupViewModel WqlLookup { get; }

    public MainWindowViewModel(
        DefinitionEditorViewModel editor,
        RegistrationViewModel registration,
        WqlLookupViewModel wqlLookup,
        IDraftStore draftStore,
        IAsorQueryClient queryClient)
    {
        Editor = editor;
        Registration = registration;
        WqlLookup = wqlLookup;
        _draftStore = draftStore;
        _queryClient = queryClient;
        SelectedRegion = AsorRegion.All[0];
        RemoteRegion = AsorRegion.All[0];
    }

    // --- Mode toggle ---

    [RelayCommand]
    private void SwitchToVisual()
    {
        if (JsonModeActive && !string.IsNullOrWhiteSpace(JsonText))
        {
            try
            {
                var definition = AgentDefinitionSerializer.Deserialize(JsonText);
                if (definition is not null)
                    Editor.LoadFromModel(definition);
            }
            catch
            {
                StatusMessage = "⚠ Invalid JSON — fix errors before switching to Visual mode.";
                return;
            }
        }
        JsonModeActive = false;
        StatusMessage = null;
    }

    [RelayCommand]
    private void SwitchToJson()
    {
        JsonText = AgentDefinitionSerializer.Serialize(Editor.ToModel());
        JsonModeActive = true;
    }

    // --- Panels ---

    [RelayCommand]
    private void OpenRegistrationPanel()
    {
        IsRegistrationPanelOpen = true;
    }

    [RelayCommand]
    private void CloseRegistrationPanel()
    {
        IsRegistrationPanelOpen = false;
    }

    [RelayCommand]
    private void ToggleConnectionBar()
    {
        IsConnectionBarExpanded = !IsConnectionBarExpanded;
    }

    // --- Theme ---

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ThemeIcon))]
    private bool _isDarkMode = true;

    public string ThemeIcon => IsDarkMode ? "☀" : "🌙";

    [RelayCommand]
    private void ToggleTheme()
    {
        IsDarkMode = !IsDarkMode;
        if (Application.Current is not null)
        {
            Application.Current.RequestedThemeVariant = IsDarkMode
                ? ThemeVariant.Dark
                : ThemeVariant.Light;
        }
    }

    [RelayCommand]
    private void ToggleFileDrawer()
    {
        if (!IsFileDrawerOpen)
            _ = RefreshLocalDrafts();
        IsFileDrawerOpen = !IsFileDrawerOpen;
    }

    [RelayCommand]
    private void CloseFileDrawer()
    {
        IsFileDrawerOpen = false;
    }

    // --- File operations ---

    [RelayCommand]
    private void NewDefinition()
    {
        Editor.LoadFromModel(new AgentDefinition
        {
            Capabilities = new Capabilities(),
            Skills = [new AgentSkill()]
        });
        _currentDraftId = null;
        CurrentDraftName = null;
        StatusMessage = "New definition";
        IsFileDrawerOpen = false;
        if (JsonModeActive)
            SwitchToJson(); // refresh JSON view
    }

    [RelayCommand]
    private async Task Save()
    {
        if (string.IsNullOrWhiteSpace(CurrentDraftName))
        {
            StatusMessage = "Enter a name to save.";
            return;
        }

        if (JsonModeActive && !string.IsNullOrWhiteSpace(JsonText))
        {
            try
            {
                var parsed = AgentDefinitionSerializer.Deserialize(JsonText);
                if (parsed is not null) Editor.LoadFromModel(parsed);
            }
            catch
            {
                StatusMessage = "Invalid JSON — cannot save.";
                return;
            }
        }

        var definition = Editor.ToModel();
        var envelope = new DraftEnvelope
        {
            Metadata = new DraftMetadata
            {
                Id = _currentDraftId ?? Guid.NewGuid().ToString("N"),
                DisplayName = CurrentDraftName,
                Provider = definition.Provider?.Id,
                Version = definition.Version
            },
            Definition = definition
        };

        var saved = await _draftStore.SaveAsync(envelope);
        _currentDraftId = saved.Metadata.Id;
        StatusMessage = $"✓ Saved";
        await RefreshLocalDrafts();
        await FlashSaveSuccess();
    }

    private async Task FlashSaveSuccess()
    {
        ShowSaveSuccess = true;
        await Task.Delay(3000);
        ShowSaveSuccess = false;
    }

    [RelayCommand]
    private async Task LoadDraft(DraftMetadata? draft)
    {
        if (draft is null) return;

        var envelope = await _draftStore.LoadAsync(draft.Id);
        if (envelope is null)
        {
            StatusMessage = "Draft not found.";
            return;
        }

        Editor.LoadFromModel(envelope.Definition);
        _currentDraftId = envelope.Metadata.Id;
        CurrentDraftName = envelope.Metadata.DisplayName;
        StatusMessage = $"Loaded: {envelope.Metadata.DisplayName}";
        IsFileDrawerOpen = false;
        if (JsonModeActive)
            SwitchToJson();
    }

    [RelayCommand]
    private async Task DeleteDraft(DraftMetadata? draft)
    {
        if (draft is null) return;
        await _draftStore.DeleteAsync(draft.Id);
        if (_currentDraftId == draft.Id)
        {
            _currentDraftId = null;
            CurrentDraftName = null;
        }
        StatusMessage = $"Deleted: {draft.DisplayName}";
        await RefreshLocalDrafts();
    }

    [RelayCommand]
    private void LoadRemoteAgent(AgentDefinition? agent)
    {
        if (agent is null) return;
        Editor.LoadFromModel(agent);
        _currentDraftId = null;
        CurrentDraftName = agent.Name;
        StatusMessage = $"Loaded from tenant: {agent.Name}";
        IsFileDrawerOpen = false;
        if (JsonModeActive)
            SwitchToJson();
    }

    [RelayCommand]
    private async Task FetchRemoteAgents()
    {
        var token = BearerToken;
        if (RemoteRegion is null || string.IsNullOrWhiteSpace(token))
        {
            RemoteStatusMessage = "Set a region and bearer token.";
            return;
        }

        IsFetchingRemote = true;
        RemoteStatusMessage = null;
        RemoteAgents.Clear();

        try
        {
            var context = new RegistrationContext { Region = RemoteRegion, BearerToken = token };
            var agents = await _queryClient.ListDefinitionsAsync(context);
            foreach (var a in agents)
                RemoteAgents.Add(a);
            RemoteStatusMessage = $"{agents.Count} agent{(agents.Count == 1 ? "" : "s")} found";
        }
        catch (Exception ex)
        {
            RemoteStatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsFetchingRemote = false;
        }
    }

    private async Task RefreshLocalDrafts()
    {
        var drafts = await _draftStore.ListAsync();
        LocalDrafts.Clear();
        foreach (var d in drafts)
            LocalDrafts.Add(d);
    }
}
