using CommunityToolkit.Mvvm.ComponentModel;

namespace Tellma.Backdoor.ViewModels;

public class NavItem(string label, object page)
{
    public string Label { get; } = label;
    public object Page { get; } = page;

    public override string ToString() => Label;
}

public partial class MainViewModel : ObservableObject
{
    public MainViewModel(BulkExecuteViewModel bulkExecuteVm, SettingsViewModel settingsVm)
    {
        BulkExecutePage = new NavItem("Bulk Execute Script", bulkExecuteVm);
        SettingsPage = new NavItem("Settings", settingsVm);

        NavigationItems = [BulkExecutePage, SettingsPage];
        _selectedNavItem = BulkExecutePage;
    }

    public NavItem BulkExecutePage { get; }
    public NavItem SettingsPage { get; }

    public List<NavItem> NavigationItems { get; }

    [ObservableProperty]
    private NavItem _selectedNavItem;

    public object? CurrentPage => SelectedNavItem?.Page;

    partial void OnSelectedNavItemChanged(NavItem value)
    {
        OnPropertyChanged(nameof(CurrentPage));
    }
}
