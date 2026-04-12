using System.Collections.ObjectModel;
using AsorAssistant.Core.Models;
using AsorAssistant.Core.Ports;
using AsorAssistant.Domain.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AsorAssistant.App.ViewModels;

public partial class DraftManagerViewModel : ObservableObject
{
    private readonly IDraftStore _draftStore;
    private readonly IAsorQueryClient _queryClient;
    private readonly DefinitionEditorViewModel _editor;

    // Local drafts
    [ObservableProperty]
    private string? _draftName;

    [ObservableProperty]
    private DraftMetadata? _selectedDraft;

    [ObservableProperty]
    private string? _statusMessage;

    private string? _currentDraftId;

    public ObservableCollection<DraftMetadata> Drafts { get; } = [];

    // Remote agents
    [ObservableProperty]
    private AsorRegion? _selectedRegion;

    [ObservableProperty]
    private AgentDefinition? _selectedRemoteAgent;

    [ObservableProperty]
    private bool _isFetchingRemote;

    [ObservableProperty]
    private string? _remoteStatusMessage;

    public ObservableCollection<AgentDefinition> RemoteAgents { get; } = [];
    public IReadOnlyList<AsorRegion> Regions => AsorRegion.All;

    /// <summary>Provides the bearer token from the app-level connection bar.</summary>
    public Func<string?>? BearerTokenProvider { get; set; }

    public DraftManagerViewModel(IDraftStore draftStore, IAsorQueryClient queryClient, DefinitionEditorViewModel editor)
    {
        _draftStore = draftStore;
        _queryClient = queryClient;
        _editor = editor;
        SelectedRegion = AsorRegion.All[0]; // Default to US
    }

    // --- Local drafts ---

    [RelayCommand]
    private async Task RefreshList()
    {
        var drafts = await _draftStore.ListAsync();
        Drafts.Clear();
        foreach (var d in drafts)
            Drafts.Add(d);
    }

    [RelayCommand]
    private async Task Save()
    {
        var name = DraftName;
        if (string.IsNullOrWhiteSpace(name))
        {
            StatusMessage = "Enter a draft name first.";
            return;
        }

        var definition = _editor.ToModel();
        var envelope = new DraftEnvelope
        {
            Metadata = new DraftMetadata
            {
                Id = _currentDraftId ?? Guid.NewGuid().ToString("N"),
                DisplayName = name,
                Provider = definition.Provider?.Id,
                Version = definition.Version
            },
            Definition = definition
        };

        var saved = await _draftStore.SaveAsync(envelope);
        _currentDraftId = saved.Metadata.Id;
        StatusMessage = $"Saved: {name}";
        await RefreshList();
    }

    [RelayCommand]
    private async Task Load()
    {
        if (SelectedDraft is null)
            return;

        var envelope = await _draftStore.LoadAsync(SelectedDraft.Id);
        if (envelope is null)
        {
            StatusMessage = "Draft not found.";
            return;
        }

        _editor.LoadFromModel(envelope.Definition);
        DraftName = envelope.Metadata.DisplayName;
        _currentDraftId = envelope.Metadata.Id;
        StatusMessage = $"Loaded: {envelope.Metadata.DisplayName}";
    }

    [RelayCommand]
    private async Task Delete()
    {
        if (SelectedDraft is null)
            return;

        var name = SelectedDraft.DisplayName;
        await _draftStore.DeleteAsync(SelectedDraft.Id);
        if (_currentDraftId == SelectedDraft.Id)
            _currentDraftId = null;
        StatusMessage = $"Deleted: {name}";
        await RefreshList();
    }

    [RelayCommand]
    private async Task Duplicate()
    {
        if (SelectedDraft is null)
            return;

        var newName = $"Copy of {SelectedDraft.DisplayName}";
        await _draftStore.DuplicateAsync(SelectedDraft.Id, newName);
        StatusMessage = $"Duplicated as: {newName}";
        await RefreshList();
    }

    // --- Remote agents ---

    [RelayCommand]
    private async Task FetchRemoteAgents()
    {
        var token = BearerTokenProvider?.Invoke();
        if (SelectedRegion is null || string.IsNullOrWhiteSpace(token))
        {
            RemoteStatusMessage = "Select a region and set a bearer token in the connection bar.";
            return;
        }

        IsFetchingRemote = true;
        RemoteStatusMessage = null;
        RemoteAgents.Clear();

        try
        {
            var context = new RegistrationContext
            {
                Region = SelectedRegion,
                BearerToken = token
            };

            var agents = await _queryClient.ListDefinitionsAsync(context);
            foreach (var a in agents)
                RemoteAgents.Add(a);

            RemoteStatusMessage = $"{agents.Count} registered agent{(agents.Count == 1 ? "" : "s")} found";
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

    [RelayCommand]
    private void LoadRemoteAgent()
    {
        if (SelectedRemoteAgent is null)
        {
            RemoteStatusMessage = "Select an agent first.";
            return;
        }

        _editor.LoadFromModel(SelectedRemoteAgent);
        DraftName = SelectedRemoteAgent.Name;
        _currentDraftId = null;
        RemoteStatusMessage = $"Loaded: {SelectedRemoteAgent.Name}";
    }
}
