using AsorAssistant.Core.Models;
using AsorAssistant.Core.Ports;
using AsorAssistant.Domain.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AsorAssistant.App.ViewModels;

public partial class RegistrationViewModel : ObservableObject
{
    private readonly IAsorRegistrationClient _registrationClient;
    private readonly DefinitionEditorViewModel _editor;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsConnectionConfigured))]
    [NotifyPropertyChangedFor(nameof(ConnectionStatusText))]
    private AsorRegion? _selectedRegion;

    [ObservableProperty]
    private string? _tenantName;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsConnectionConfigured))]
    [NotifyPropertyChangedFor(nameof(ConnectionStatusText))]
    private string? _bearerToken;

    [ObservableProperty]
    private string? _resolvedUrl;

    [ObservableProperty]
    private bool _isRegistering;

    [ObservableProperty]
    private string? _responseText;

    [ObservableProperty]
    private bool _isSuccess;

    public bool IsConnectionConfigured =>
        SelectedRegion is not null && !string.IsNullOrWhiteSpace(BearerToken);

    public string ConnectionStatusText =>
        IsConnectionConfigured ? $"{SelectedRegion!.Name} region" : "Not configured";

    public IReadOnlyList<AsorRegion> Regions => AsorRegion.All;

    public RegistrationViewModel(IAsorRegistrationClient registrationClient, DefinitionEditorViewModel editor)
    {
        _registrationClient = registrationClient;
        _editor = editor;
    }

    partial void OnSelectedRegionChanged(AsorRegion? value)
    {
        ResolvedUrl = value is not null
            ? $"{value.BaseUrl}/asor/v1/agentDefinition"
            : null;
    }

    [RelayCommand]
    private async Task Register()
    {
        if (!IsConnectionConfigured)
        {
            ResponseText = "Select a region and enter a bearer token above.";
            IsSuccess = false;
            return;
        }

        _editor.Validate();
        if (!_editor.IsValid)
        {
            ResponseText = $"Validation errors:\n{_editor.ValidationErrors}";
            IsSuccess = false;
            return;
        }

        IsRegistering = true;
        ResponseText = null;

        try
        {
            var context = new RegistrationContext
            {
                Region = SelectedRegion,
                TenantName = TenantName,
                BearerToken = BearerToken
            };

            var definition = _editor.ToModel();
            var result = await _registrationClient.RegisterAsync(context, definition);

            IsSuccess = result.Success;
            if (result.Success)
            {
                ResponseText = $"Success ({result.StatusCode})\n\n{result.RawResponseBody}";
            }
            else
            {
                var errors = string.Join("\n", result.Errors.Select(e =>
                {
                    var parts = new List<string>();
                    if (e.Error is not null) parts.Add(e.Error);
                    if (e.Field is not null) parts.Add($"Field: {e.Field}");
                    if (e.Code is not null) parts.Add($"Code: {e.Code}");
                    return string.Join(" | ", parts);
                }));
                ResponseText = $"Failed ({result.StatusCode})\n\n{errors}";
            }
        }
        catch (Exception ex)
        {
            IsSuccess = false;
            ResponseText = $"Error: {ex.Message}";
        }
        finally
        {
            IsRegistering = false;
        }
    }
}
