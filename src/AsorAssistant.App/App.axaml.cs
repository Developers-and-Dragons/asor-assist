using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AsorAssistant.App.ViewModels;
using AsorAssistant.Core.Ports;
using AsorAssistant.Core.Services;
using AsorAssistant.Infrastructure.Http;
using AsorAssistant.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace AsorAssistant.App;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainVm = Services.GetRequiredService<MainWindowViewModel>();

            mainVm.Registration.BearerTokenProvider = () => mainVm.BearerToken;
            mainVm.WqlLookup.BearerTokenProvider = () => mainVm.BearerToken;

            desktop.MainWindow = new MainWindow { DataContext = mainVm };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Infrastructure
        var draftsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "asor-assist", "drafts");
        services.AddSingleton<IDraftStore>(new FileDraftStore(draftsDir));
        services.AddSingleton<HttpClient>();
        services.AddSingleton<IAsorRegistrationClient, AsorRegistrationClient>();
        services.AddSingleton<IAsorQueryClient, AsorQueryClient>();
        services.AddSingleton<IWqlClient, WqlClient>();
        services.AddSingleton<WqlLookupService>();

        // ViewModels
        services.AddSingleton<DefinitionEditorViewModel>();
        services.AddSingleton<RegistrationViewModel>();
        services.AddSingleton<WqlLookupViewModel>();
        services.AddSingleton<MainWindowViewModel>();
    }
}
