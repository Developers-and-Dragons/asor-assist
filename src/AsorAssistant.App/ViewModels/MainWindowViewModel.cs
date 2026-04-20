using System.Collections.ObjectModel;
using AsorAssistant.Core.Models;
using AsorAssistant.Core.Ports;
using AsorAssistant.Core.Serialization;
using AsorAssistant.Domain.Models;
using Avalonia;
using Avalonia.Input.Platform;
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
    [NotifyPropertyChangedFor(nameof(WindowTitle))]
    private bool _isDirty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WindowTitle))]
    private string? _currentDraftName;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private bool _showSaveSuccess;

    private void SetStatus(string message, int dismissMs = 3000)
    {
        StatusMessage = message;
        if (dismissMs > 0)
            _ = AutoDismissStatus(dismissMs);
    }

    private async Task AutoDismissStatus(int ms)
    {
        await Task.Delay(ms);
        StatusMessage = null;
    }

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

    public string WindowTitle
    {
        get
        {
            var name = CurrentDraftName ?? "Untitled";
            return IsDirty ? $"● ASOR Assistant — {name}" : $"ASOR Assistant — {name}";
        }
    }

    /// <summary>
    /// Set by the view to show a confirmation dialog. Returns true if user confirms discard.
    /// </summary>
    public Func<string, Task<bool>>? ConfirmDiscardAsync { get; set; }

    private async Task<bool> CheckDiscardAsync()
    {
        if (!IsDirty || ConfirmDiscardAsync is null) return true;
        return await ConfirmDiscardAsync("You have unsaved changes. Discard and continue?");
    }

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

        SubscribeToEditorChanges();
    }

    private bool _suppressDirtyTracking;

    private void SubscribeToEditorChanges()
    {
        // Track property changes on the editor (Name, Description, Url, etc.)
        Editor.PropertyChanged += (_, e) =>
        {
            // Ignore validation/help state changes
            if (_suppressDirtyTracking) return;
            if (e.PropertyName is nameof(Editor.ValidationErrors) or nameof(Editor.IsValid)
                or nameof(Editor.ActiveSection) or nameof(Editor.HelpTitle)
                or nameof(Editor.HelpContent) or nameof(Editor.IsContextualHelp))
                return;
            IsDirty = true;
        };

        // Track collection changes (skills added/removed)
        Editor.Skills.CollectionChanged += (_, _) => { if (!_suppressDirtyTracking) IsDirty = true; };
    }

    /// <summary>
    /// If JSON mode is active, parse the JSON text and load it into the editor.
    /// Returns false if the JSON is invalid.
    /// </summary>
    public bool ApplyJsonToEditor()
    {
        if (!JsonModeActive || string.IsNullOrWhiteSpace(JsonText))
            return true;

        try
        {
            var parsed = AgentDefinitionSerializer.Deserialize(JsonText);
            if (parsed is not null) Editor.LoadFromModel(parsed);
            return true;
        }
        catch
        {
            SetStatus("Invalid JSON — fix errors first.", 5000);
            return false;
        }
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
                SetStatus("⚠ Invalid JSON — fix errors before switching to Visual mode.", 5000);
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

    [RelayCommand]
    private async Task CopyJson(IClipboard? clipboard)
    {
        var json = JsonModeActive && !string.IsNullOrWhiteSpace(JsonText)
            ? JsonText
            : AgentDefinitionSerializer.Serialize(Editor.ToModel());

        if (clipboard is not null)
        {
            await clipboard.SetTextAsync(json);
            SetStatus("✓ JSON copied to clipboard");
        }
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
    private async Task NewDefinition()
    {
        if (!await CheckDiscardAsync()) return;

        _suppressDirtyTracking = true;
        Editor.LoadFromModel(new AgentDefinition
        {
            Capabilities = new Capabilities(),
            Skills = [new AgentSkill()]
        });
        _currentDraftId = null;
        CurrentDraftName = null;
        _suppressDirtyTracking = false;
        IsDirty = false;
        SetStatus("New definition");
        IsFileDrawerOpen = false;
        if (JsonModeActive)
            SwitchToJson(); // refresh JSON view
    }

    [RelayCommand]
    private async Task Save()
    {
        if (string.IsNullOrWhiteSpace(CurrentDraftName))
        {
            // Fall back to the agent name so Ctrl+S works without opening the flyout
            if (!string.IsNullOrWhiteSpace(Editor.Name))
                CurrentDraftName = Editor.Name;
            else
            {
                SetStatus("Enter a name to save.", 5000);
                return;
            }
        }

        if (!ApplyJsonToEditor())
        {
            SetStatus("Invalid JSON — cannot save.", 5000);
            return;
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
        IsDirty = false;
        SetStatus("✓ Saved");
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
        if (!await CheckDiscardAsync()) return;

        var envelope = await _draftStore.LoadAsync(draft.Id);
        if (envelope is null)
        {
            SetStatus("Draft not found.", 5000);
            return;
        }

        _suppressDirtyTracking = true;
        Editor.LoadFromModel(envelope.Definition);
        _currentDraftId = envelope.Metadata.Id;
        CurrentDraftName = envelope.Metadata.DisplayName;
        _suppressDirtyTracking = false;
        IsDirty = false;
        SetStatus($"Loaded: {envelope.Metadata.DisplayName}");
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
        SetStatus($"Deleted: {draft.DisplayName}");
        await RefreshLocalDrafts();
    }

    [RelayCommand]
    private async Task LoadRemoteAgent(AgentDefinition? agent)
    {
        if (agent is null) return;
        if (!await CheckDiscardAsync()) return;
        _suppressDirtyTracking = true;
        Editor.LoadFromModel(agent);
        _currentDraftId = null;
        CurrentDraftName = agent.Name;
        _suppressDirtyTracking = false;
        IsDirty = false;
        SetStatus($"Loaded from tenant: {agent.Name}");
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
