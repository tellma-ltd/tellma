using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.SqlClient;
using Tellma.Backdoor.Models;
using Tellma.Backdoor.Services;

namespace Tellma.Backdoor.ViewModels;

public partial class BulkExecuteViewModel : ObservableObject
{
    private readonly TenantDiscoveryService _discoveryService;
    private readonly ScriptExecutionService _executionService;
    private readonly SettingsService _settingsService;
    private readonly AppSettings _appSettings;

    private CancellationTokenSource? _cts;
    private DispatcherTimer? _saveTimer;

    public BulkExecuteViewModel(
        TenantDiscoveryService discoveryService,
        ScriptExecutionService executionService,
        SettingsService settingsService,
        AppSettings appSettings)
    {
        _discoveryService = discoveryService;
        _executionService = executionService;
        _settingsService = settingsService;
        _appSettings = appSettings;

        _scriptText = appSettings.LastScript ?? string.Empty;

        // Debounced save timer
        _saveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _saveTimer.Tick += (_, _) =>
        {
            _saveTimer.Stop();
            _appSettings.LastScript = ScriptText;
            _settingsService.Save(_appSettings);
        };
    }

    public ObservableCollection<TellmaInstance> Instances { get; } = [];

    [ObservableProperty]
    private TellmaInstance? _selectedInstance;

    [ObservableProperty]
    private string _scriptText = string.Empty;

    [ObservableProperty]
    private bool _isExecuting;

    [ObservableProperty]
    private bool _isLoadingTenants;

    [ObservableProperty]
    private string? _tenantLoadError;

    public ObservableCollection<TenantDatabaseRow> Tenants { get; } = [];

    partial void OnScriptTextChanged(string value)
    {
        _saveTimer?.Stop();
        _saveTimer?.Start();
    }

    partial void OnSelectedInstanceChanged(TellmaInstance? value)
    {
        _appSettings.SelectedInstanceId = value?.Id;
        _settingsService.Save(_appSettings);
        _ = LoadTenantsAsync();
    }

    public void RefreshInstances(IEnumerable<TellmaInstance> instances)
    {
        var selectedId = SelectedInstance?.Id;
        Instances.Clear();
        foreach (var inst in instances)
            Instances.Add(inst);

        SelectedInstance = Instances.FirstOrDefault(i => i.Id == selectedId)
                       ?? Instances.FirstOrDefault();
    }

    public async Task LoadTenantsAsync()
    {
        if (SelectedInstance == null || string.IsNullOrWhiteSpace(SelectedInstance.AdminConnection))
        {
            Tenants.Clear();
            return;
        }

        IsLoadingTenants = true;
        TenantLoadError = null;

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var adminConnBuilder = new SqlConnectionStringBuilder(SelectedInstance.AdminConnection);
            var tenants = await _discoveryService.DiscoverAsync(SelectedInstance.AdminConnection, cts.Token);

            Tenants.Clear();
            foreach (var t in tenants)
            {
                Tenants.Add(new TenantDatabaseRow
                {
                    DatabaseId = t.DatabaseId,
                    DatabaseName = t.DatabaseName,
                    ServerName = t.ServerName,
                    Description = t.Description,
                    ConnectionString = TenantDiscoveryService.BuildTenantConnectionString(t, adminConnBuilder),
                });
            }
        }
        catch (Exception ex)
        {
            Tenants.Clear();
            TenantLoadError = ex.Message;
        }
        finally
        {
            IsLoadingTenants = false;
        }
    }

    public void RestoreSelectedInstance(string? instanceId)
    {
        if (instanceId != null)
            SelectedInstance = Instances.FirstOrDefault(i => i.Id == instanceId);

        SelectedInstance ??= Instances.FirstOrDefault();
    }

    [RelayCommand(CanExecute = nameof(CanExecute))]
    private async Task ExecuteScriptAsync()
    {
        if (SelectedInstance == null || Tenants.Count == 0 || string.IsNullOrWhiteSpace(ScriptText))
            return;

        // Confirmation
        if (!SelectedInstance.SkipConfirmation)
        {
            var dialog = new Views.ConfirmationDialog
            {
                Owner = Application.Current.MainWindow
            };

            if (dialog.ShowDialog() != true)
                return;
        }

        // Reset statuses
        foreach (var row in Tenants)
        {
            row.Status = ExecutionStatus.NotStarted;
            row.ErrorMessage = null;
        }

        IsExecuting = true;
        ExecuteScriptCommand.NotifyCanExecuteChanged();

        _cts = new CancellationTokenSource();
        var progress = new Progress<(int Index, ExecutionStatus Status, string? Error)>(update =>
        {
            if (update.Index >= 0 && update.Index < Tenants.Count)
            {
                Tenants[update.Index].Status = update.Status;
                Tenants[update.Index].ErrorMessage = update.Error;
            }
        });

        try
        {
            await _executionService.ExecuteAsync(
                ScriptText,
                Tenants,
                SelectedInstance.BatchSize,
                progress,
                _cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected on cancel
        }
        finally
        {
            IsExecuting = false;
            _cts.Dispose();
            _cts = null;
            ExecuteScriptCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanExecute() => !IsExecuting;

    [RelayCommand]
    private void Cancel()
    {
        _cts?.Cancel();
    }

    public void SaveState()
    {
        _appSettings.LastScript = ScriptText;
        _appSettings.SelectedInstanceId = SelectedInstance?.Id;
        _settingsService.Save(_appSettings);
    }
}
