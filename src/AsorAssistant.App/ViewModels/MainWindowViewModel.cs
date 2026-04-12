using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AsorAssistant.App.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditorPage))]
    [NotifyPropertyChangedFor(nameof(IsJsonPage))]
    [NotifyPropertyChangedFor(nameof(IsDraftsPage))]
    [NotifyPropertyChangedFor(nameof(IsWqlPage))]
    private int _selectedPageIndex;

    public bool IsEditorPage => SelectedPageIndex == 0;
    public bool IsJsonPage => SelectedPageIndex == 1;
    public bool IsDraftsPage => SelectedPageIndex == 2;
    public bool IsWqlPage => SelectedPageIndex == 3;

    [ObservableProperty]
    private string? _bearerToken;

    [ObservableProperty]
    private bool _isRegistrationPanelOpen;

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

    [RelayCommand]
    private void ToggleRegistrationPanel()
    {
        IsRegistrationPanelOpen = !IsRegistrationPanelOpen;
    }

    [RelayCommand]
    private void NavigateToEditor()
    {
        SelectedPageIndex = 0;
    }
}
