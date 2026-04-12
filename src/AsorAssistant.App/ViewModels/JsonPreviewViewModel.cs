using AsorAssistant.Core.Serialization;
using Avalonia.Input.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AsorAssistant.App.ViewModels;

public partial class JsonPreviewViewModel : ObservableObject
{
    private readonly DefinitionEditorViewModel _editor;

    [ObservableProperty]
    private string? _jsonText;

    [ObservableProperty]
    private string? _statusMessage;

    public JsonPreviewViewModel(DefinitionEditorViewModel editor)
    {
        _editor = editor;
    }

    [RelayCommand]
    private void Refresh()
    {
        var definition = _editor.ToModel();
        JsonText = AgentDefinitionSerializer.Serialize(definition);
        StatusMessage = null;
    }

    [RelayCommand]
    private async Task CopyToClipboard(IClipboard? clipboard)
    {
        if (clipboard is null || string.IsNullOrEmpty(JsonText))
            return;

        await clipboard.SetTextAsync(JsonText);
        StatusMessage = "Copied to clipboard";
    }
}
