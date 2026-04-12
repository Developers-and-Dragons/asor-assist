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

    /// <summary>Provides the bearer token from the registration state.</summary>
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
            StatusMessage = "No bearer token configured. Set it in the Register panel on the Editor page.";
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
            StatusMessage = $"{results.Count} operations found";
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
