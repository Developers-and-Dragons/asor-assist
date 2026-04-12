using System.Collections.ObjectModel;
using AsorAssistant.Core.Models;
using AsorAssistant.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AsorAssistant.App.ViewModels;

public partial class WqlLookupViewModel : ObservableObject
{
    private readonly WqlLookupService _lookupService;
    private readonly DefinitionEditorViewModel _editor;

    [ObservableProperty]
    private string? _bearerToken;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private ServiceOperationLookup? _selectedResult;

    [ObservableProperty]
    private string? _filterText;

    public ObservableCollection<ServiceOperationLookup> Results { get; } = [];

    public WqlLookupViewModel(WqlLookupService lookupService, DefinitionEditorViewModel editor)
    {
        _lookupService = lookupService;
        _editor = editor;
    }

    [RelayCommand]
    private async Task LookupSoap()
    {
        await RunLookup(() => _lookupService.LookupSoapOperationsAsync(BearerToken!));
    }

    [RelayCommand]
    private async Task LookupRest()
    {
        await RunLookup(() => _lookupService.LookupRestOperationsAsync(BearerToken!));
    }

    private async Task RunLookup(Func<Task<IReadOnlyList<ServiceOperationLookup>>> lookup)
    {
        if (string.IsNullOrWhiteSpace(BearerToken))
        {
            StatusMessage = "Bearer token is required.";
            return;
        }

        IsLoading = true;
        StatusMessage = null;
        Results.Clear();

        try
        {
            var results = await lookup();
            foreach (var r in results)
                Results.Add(r);
            StatusMessage = $"Found {results.Count} operations.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void CopyWid()
    {
        if (SelectedResult is null)
        {
            StatusMessage = "Select an operation first.";
            return;
        }

        StatusMessage = $"WID: {SelectedResult.WorkdayId} — use this in your workdayConfig tool definitions.";
    }
}
