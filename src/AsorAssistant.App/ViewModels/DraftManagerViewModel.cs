using System.Collections.ObjectModel;
using AsorAssistant.Application.Models;
using AsorAssistant.Application.Ports;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AsorAssistant.App.ViewModels;

public partial class DraftManagerViewModel : ObservableObject
{
    private readonly IDraftStore _draftStore;
    private readonly DefinitionEditorViewModel _editor;

    [ObservableProperty]
    private string? _draftName;

    [ObservableProperty]
    private DraftMetadata? _selectedDraft;

    [ObservableProperty]
    private string? _statusMessage;

    private string? _currentDraftId;

    public ObservableCollection<DraftMetadata> Drafts { get; } = [];

    public DraftManagerViewModel(IDraftStore draftStore, DefinitionEditorViewModel editor)
    {
        _draftStore = draftStore;
        _editor = editor;
    }

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
}
