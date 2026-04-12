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

    /// <summary>Provides the bearer token from the app-level connection bar.</summary>
    public Func<string?>? BearerTokenProvider { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsConnectionConfigured))]
    [NotifyPropertyChangedFor(nameof(ConnectionStatusText))]
    private AsorRegion? _selectedRegion;

    [ObservableProperty]
    private string? _resolvedUrl;

    [ObservableProperty]
    private bool _isRegistering;

    [ObservableProperty]
    private string? _responseText;

    [ObservableProperty]
    private bool _isSuccess;

    public bool IsConnectionConfigured =>
        SelectedRegion is not null && !string.IsNullOrWhiteSpace(BearerTokenProvider?.Invoke());

    public string ConnectionStatusText =>
        IsConnectionConfigured ? $"{SelectedRegion!.Name} region" : "Not configured";

    public IReadOnlyList<AsorRegion> Regions => AsorRegion.All;

    public RegistrationViewModel(IAsorRegistrationClient registrationClient, DefinitionEditorViewModel editor)
    {
        _registrationClient = registrationClient;
        _editor = editor;
        SelectedRegion = AsorRegion.All[0]; // Default to US
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
        var token = BearerTokenProvider?.Invoke();

        if (SelectedRegion is null || string.IsNullOrWhiteSpace(token))
        {
            ResponseText = "Region and bearer token are required. Set the token in the connection bar at the top of the app.";
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
