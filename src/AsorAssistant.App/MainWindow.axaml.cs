using System.Diagnostics;
using System.Runtime.InteropServices;
using AsorAssistant.App.ViewModels;
using AsorAssistant.App.Views;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AsorAssistant.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        DataContextChanged += (_, _) =>
        {
            if (DataContext is MainWindowViewModel vm)
                vm.ConfirmDiscardAsync = ShowConfirmDiscardAsync;
        };
    }

    private async Task<bool> ShowConfirmDiscardAsync(string message)
    {
        var dialog = new ConfirmDialog(message);
        await dialog.ShowDialog(this);
        return dialog.Confirmed;
    }

    private void OnSpecLinkClick(object? sender, RoutedEventArgs e)
    {
        var url = "https://github.com/Workday/asor/blob/main/versions/v1.2.md";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            Process.Start("open", url);
        else
            Process.Start("xdg-open", url);
    }
}
