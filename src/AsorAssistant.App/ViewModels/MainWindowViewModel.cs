using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AsorAssistant.App.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditorPage))]
    [NotifyPropertyChangedFor(nameof(IsJsonPage))]
    [NotifyPropertyChangedFor(nameof(IsDraftsPage))]
    [NotifyPropertyChangedFor(nameof(IsLookupsPage))]
    private int _selectedPageIndex;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasToken))]
    [NotifyPropertyChangedFor(nameof(TokenStatusText))]
    private string? _bearerToken;

    [ObservableProperty]
    private bool _isConnectionBarExpanded = true; // Start expanded so users see it

    [ObservableProperty]
    private bool _isRegistrationPanelOpen;

    public bool IsEditorPage => SelectedPageIndex == 0;
    public bool IsJsonPage => SelectedPageIndex == 1;
    public bool IsDraftsPage => SelectedPageIndex == 2;
    public bool IsLookupsPage => SelectedPageIndex == 3;

    public bool HasToken => !string.IsNullOrWhiteSpace(BearerToken);
    public string TokenStatusText => HasToken ? "Token set" : "No token";

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
    private void OpenRegistrationPanel()
    {
        IsRegistrationPanelOpen = true;
    }

    [RelayCommand]
    private void CloseRegistrationPanel()
    {
        IsRegistrationPanelOpen = false;
    }

    [RelayCommand]
    private void ToggleRegistrationPanel()
    {
        IsRegistrationPanelOpen = !IsRegistrationPanelOpen;
    }

    [RelayCommand]
    private void ToggleConnectionBar()
    {
        IsConnectionBarExpanded = !IsConnectionBarExpanded;
    }
}
