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

    public Func<string?>? BearerTokenProvider { get; set; }
    public Func<AsorRegion?>? RegionProvider { get; set; }

    /// <summary>
    /// Called before building the model to apply any pending JSON edits.
    /// Returns false if JSON is invalid and registration should be aborted.
    /// </summary>
    public Func<bool>? ApplyPendingJsonChanges { get; set; }

    [ObservableProperty]
    private string? _resolvedUrl;

    [ObservableProperty]
    private bool _isRegistering;

    [ObservableProperty]
    private string? _responseText;

    [ObservableProperty]
    private bool _isSuccess;

    public RegistrationViewModel(IAsorRegistrationClient registrationClient, DefinitionEditorViewModel editor)
    {
        _registrationClient = registrationClient;
        _editor = editor;
    }

    [RelayCommand]
    private async Task Register()
    {
        var token = BearerTokenProvider?.Invoke();
        var region = RegionProvider?.Invoke();

        if (region is null)
        {
            ResponseText = "Select a region in the connection bar.";
            IsSuccess = false;
            return;
        }

        ResolvedUrl = $"{region.BaseUrl}/asor/v1/agentDefinition";

        if (string.IsNullOrWhiteSpace(token))
        {
            ResponseText = "Set a bearer token in the connection bar at the top of the app.";
            IsSuccess = false;
            return;
        }

        if (ApplyPendingJsonChanges is not null && !ApplyPendingJsonChanges())
        {
            ResponseText = "Invalid JSON — fix errors before registering.";
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
                Region = region,
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
