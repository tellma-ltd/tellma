using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tellma.Backdoor.Models;
using Tellma.Backdoor.Services;

namespace Tellma.Backdoor.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly SettingsService _settingsService;
    private readonly AppSettings _appSettings;
    private readonly Action _onInstancesChanged;

    public SettingsViewModel(SettingsService settingsService, AppSettings appSettings, Action onInstancesChanged)
    {
        _settingsService = settingsService;
        _appSettings = appSettings;
        _onInstancesChanged = onInstancesChanged;

        Instances = new ObservableCollection<TellmaInstance>(appSettings.Instances);
        if (Instances.Count > 0)
            SelectedInstance = Instances[0];
    }

    public ObservableCollection<TellmaInstance> Instances { get; }

    [ObservableProperty]
    private TellmaInstance? _selectedInstance;

    [ObservableProperty]
    private string? _testConnectionStatus;

    [ObservableProperty]
    private bool _isTestConnectionSuccess;

    // Bound fields for the selected instance
    [ObservableProperty]
    private string _editName = string.Empty;

    [ObservableProperty]
    private string _editAdminConnection = string.Empty;

    [ObservableProperty]
    private bool _editSkipConfirmation;

    [ObservableProperty]
    private int _editBatchSize = 100;

    partial void OnSelectedInstanceChanged(TellmaInstance? value)
    {
        if (value != null)
        {
            EditName = value.Name ?? string.Empty;
            EditAdminConnection = value.AdminConnection;
            EditSkipConfirmation = value.SkipConfirmation;
            EditBatchSize = value.BatchSize;
        }

        TestConnectionStatus = null;
    }

    [RelayCommand]
    private void AddInstance()
    {
        var instance = new TellmaInstance();
        Instances.Add(instance);
        SelectedInstance = instance;
    }

    [RelayCommand]
    private void RemoveInstance()
    {
        if (SelectedInstance != null)
        {
            var index = Instances.IndexOf(SelectedInstance);
            Instances.Remove(SelectedInstance);
            if (Instances.Count > 0)
                SelectedInstance = Instances[Math.Min(index, Instances.Count - 1)];
            else
                SelectedInstance = null;
        }
    }

    [RelayCommand]
    private void SaveInstance()
    {
        if (SelectedInstance == null)
            return;

        SelectedInstance.Name = string.IsNullOrWhiteSpace(EditName) ? null : EditName;
        SelectedInstance.AdminConnection = EditAdminConnection;
        SelectedInstance.SkipConfirmation = EditSkipConfirmation;
        SelectedInstance.BatchSize = Math.Max(EditBatchSize, 1);

        // Refresh the list item display by removing and re-inserting
        var instance = SelectedInstance;
        var index = Instances.IndexOf(instance);
        if (index >= 0)
        {
            Instances.RemoveAt(index);
            Instances.Insert(index, instance);
            SelectedInstance = instance;
        }

        // Persist
        _appSettings.Instances = [.. Instances];
        _settingsService.Save(_appSettings);
        _onInstancesChanged();
    }

    [RelayCommand]
    private async Task TestConnectionActionAsync()
    {
        if (string.IsNullOrWhiteSpace(EditAdminConnection))
        {
            TestConnectionStatus = "Connection string is empty.";
            IsTestConnectionSuccess = false;
            return;
        }

        TestConnectionStatus = "Testing...";
        IsTestConnectionSuccess = false;

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            await TenantDiscoveryService.TestConnectionAsync(EditAdminConnection, cts.Token);
            TestConnectionStatus = "Connection successful!";
            IsTestConnectionSuccess = true;
        }
        catch (Exception ex)
        {
            TestConnectionStatus = $"Failed: {ex.Message}";
            IsTestConnectionSuccess = false;
        }
    }
}
