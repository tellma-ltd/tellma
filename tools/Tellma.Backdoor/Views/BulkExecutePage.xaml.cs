using System.Windows;
using System.Windows.Controls;
using Tellma.Backdoor.ViewModels;

namespace Tellma.Backdoor.Views;

public partial class BulkExecutePage : UserControl
{
    public BulkExecutePage()
    {
        InitializeComponent();
    }

    private void ShowError_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: TenantDatabaseRow row } && row.ErrorMessage != null)
        {
            MessageBox.Show(
                row.ErrorMessage,
                $"Error — {row.DatabaseName}",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
