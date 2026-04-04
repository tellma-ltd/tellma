using CommunityToolkit.Mvvm.ComponentModel;

namespace Tellma.Backdoor.ViewModels;

public enum ExecutionStatus
{
    NotStarted,
    Executing,
    Completed,
    Failed,
    Cancelled
}

public partial class TenantDatabaseRow : ObservableObject
{
    public int DatabaseId { get; init; }
    public string DatabaseName { get; init; } = string.Empty;
    public string ServerName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string ConnectionString { get; init; } = string.Empty;

    [ObservableProperty]
    private ExecutionStatus _status = ExecutionStatus.NotStarted;

    [ObservableProperty]
    private string? _errorMessage;
}
