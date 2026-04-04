using System.Windows;
using Tellma.Backdoor.Models;
using Tellma.Backdoor.Services;
using Tellma.Backdoor.ViewModels;
using Tellma.Backdoor.Views;

namespace Tellma.Backdoor;

public partial class App : Application
{
    private BulkExecuteViewModel? _bulkExecuteVm;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Services
        var settingsService = new SettingsService();
        var appSettings = settingsService.Load();
        var discoveryService = new TenantDiscoveryService();
        var executionService = new ScriptExecutionService();

        // ViewModels
        _bulkExecuteVm = new BulkExecuteViewModel(discoveryService, executionService, settingsService, appSettings);

        var settingsVm = new SettingsViewModel(settingsService, appSettings, () =>
        {
            // When instances change in settings, refresh the bulk execute dropdown
            _bulkExecuteVm.RefreshInstances(appSettings.Instances);
        });

        var mainVm = new MainViewModel(_bulkExecuteVm, settingsVm);

        // Initialize bulk execute with instances
        _bulkExecuteVm.RefreshInstances(appSettings.Instances);
        _bulkExecuteVm.RestoreSelectedInstance(appSettings.SelectedInstanceId);

        // Main window
        var mainWindow = new MainWindow
        {
            DataContext = mainVm
        };

        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _bulkExecuteVm?.SaveState();
        base.OnExit(e);
    }
}
