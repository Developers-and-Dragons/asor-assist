using System.Collections.ObjectModel;
using AsorAssistant.Core.Models;
using AsorAssistant.Core.Services;
using Avalonia.Input.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AsorAssistant.App.ViewModels;

public partial class WqlLookupViewModel : ObservableObject
{
    private readonly WqlLookupService _lookupService;
    private readonly DefinitionEditorViewModel _editor;
    private IReadOnlyList<ServiceOperationLookup> _allResults = [];

    /// <summary>Provides the bearer token from the app-level connection bar.</summary>
    public Func<string?>? BearerTokenProvider { get; set; }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private string? _resultTypeLabel;

    [ObservableProperty]
    private bool _showServiceColumn;

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

    partial void OnFilterTextChanged(string? value)
    {
        ApplyFilter();
    }

    [RelayCommand]
    private async Task LookupSoap()
    {
        ShowServiceColumn = true;
        ResultTypeLabel = "SOAP Operations";
        await RunLookup(() => _lookupService.LookupSoapOperationsAsync(BearerTokenProvider?.Invoke()!));
    }

    [RelayCommand]
    private async Task LookupRest()
    {
        ShowServiceColumn = false;
        ResultTypeLabel = "REST Operations";
        await RunLookup(() => _lookupService.LookupRestOperationsAsync(BearerTokenProvider?.Invoke()!));
    }

    private async Task RunLookup(Func<Task<IReadOnlyList<ServiceOperationLookup>>> lookup)
    {
        var token = BearerTokenProvider?.Invoke();
        if (string.IsNullOrWhiteSpace(token))
        {
            StatusMessage = "No bearer token configured. Set it in the connection bar at the top.";
            return;
        }

        IsLoading = true;
        StatusMessage = null;
        _allResults = [];
        Results.Clear();

        try
        {
            _allResults = await lookup();
            ApplyFilter();
            StatusMessage = $"{_allResults.Count} operations found";
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

    private void ApplyFilter()
    {
        Results.Clear();
        var filter = FilterText?.Trim();
        var filtered = string.IsNullOrEmpty(filter)
            ? _allResults
            : _allResults.Where(r =>
                (r.Name?.Contains(filter, StringComparison.OrdinalIgnoreCase) == true) ||
                (r.WorkdayId?.Contains(filter, StringComparison.OrdinalIgnoreCase) == true) ||
                (r.ServiceName?.Contains(filter, StringComparison.OrdinalIgnoreCase) == true));

        foreach (var r in filtered)
            Results.Add(r);
    }

    [RelayCommand]
    private async Task CopyWid(IClipboard? clipboard)
    {
        if (SelectedResult is null)
        {
            StatusMessage = "Select an operation first.";
            return;
        }

        if (clipboard is not null)
            await clipboard.SetTextAsync(SelectedResult.WorkdayId);

        StatusMessage = $"Copied: {SelectedResult.WorkdayId}";
    }
}
