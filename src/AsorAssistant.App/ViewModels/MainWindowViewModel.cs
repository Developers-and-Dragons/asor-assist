using CommunityToolkit.Mvvm.ComponentModel;

namespace AsorAssistant.App.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private object? _currentView;

    [ObservableProperty]
    private int _selectedTabIndex;

    public DefinitionEditorViewModel Editor { get; }
    public JsonPreviewViewModel JsonPreview { get; }
    public DraftManagerViewModel DraftManager { get; }
    public RegistrationViewModel Registration { get; }
    public WqlLookupViewModel WqlLookup { get; }

    public MainWindowViewModel(
        DefinitionEditorViewModel editor,
        JsonPreviewViewModel jsonPreview,
        DraftManagerViewModel draftManager,
        RegistrationViewModel registration,
        WqlLookupViewModel wqlLookup)
    {
        Editor = editor;
        JsonPreview = jsonPreview;
        DraftManager = draftManager;
        Registration = registration;
        WqlLookup = wqlLookup;
    }
}
