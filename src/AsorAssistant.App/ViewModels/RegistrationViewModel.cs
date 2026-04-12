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

    /// <summary>Provides the bearer token from the central app state.</summary>
    public Func<string?>? BearerTokenProvider { get; set; }

    [ObservableProperty]
    private AsorRegion? _selectedRegion;

    [ObservableProperty]
    private string? _tenantName;

    [ObservableProperty]
    private string? _resolvedUrl;

    [ObservableProperty]
    private bool _isRegistering;

    [ObservableProperty]
    private string? _responseText;

    [ObservableProperty]
    private bool _isSuccess;

    public IReadOnlyList<AsorRegion> Regions => AsorRegion.All;

    public RegistrationViewModel(IAsorRegistrationClient registrationClient, DefinitionEditorViewModel editor)
    {
        _registrationClient = registrationClient;
        _editor = editor;
    }

    partial void OnSelectedRegionChanged(AsorRegion? value)
    {
        UpdateResolvedUrl();
    }

    private void UpdateResolvedUrl()
    {
        ResolvedUrl = SelectedRegion is not null
            ? $"{SelectedRegion.BaseUrl}/asor/v1/agentDefinition"
            : null;
    }

    [RelayCommand]
    private async Task Register()
    {
        var token = BearerTokenProvider?.Invoke();

        if (SelectedRegion is null || string.IsNullOrWhiteSpace(token))
        {
            ResponseText = "Region and bearer token are required. Set the token via the 🔑 button in the nav rail.";
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
                BearerToken = token
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
