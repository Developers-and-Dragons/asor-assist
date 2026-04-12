using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AsorAssistant.App.ViewModels;

namespace AsorAssistant.App.Views;

public partial class DefinitionEditorView : UserControl
{
    public DefinitionEditorView()
    {
        InitializeComponent();
        AddHandler(InputElement.GotFocusEvent, OnChildGotFocus, RoutingStrategies.Bubble);
    }

    private void OnChildGotFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not DefinitionEditorViewModel vm)
            return;

        var source = e.Source as Control;
        while (source is not null && source != this)
        {
            if (source.Name is not null)
            {
                var section = source.Name switch
                {
                    "SectionIdentity" => "identity",
                    "SectionProvider" => "provider",
                    "SectionCapabilities" => "capabilities",
                    "SectionSkills" => "skills",
                    "SectionWorkdayConfig" => "workdayconfig",
                    "SectionOptional" => "optional",
                    _ => null
                };

                if (section is not null)
                {
                    vm.ActiveSection = section;
                    return;
                }
            }
            source = source.GetVisualParent() as Control;
        }
    }
}
