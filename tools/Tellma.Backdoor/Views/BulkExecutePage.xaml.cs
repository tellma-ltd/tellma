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

    private void ScriptEditor_SelectionChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is BulkExecuteViewModel vm && sender is TextBox textBox)
        {
            if (textBox.SelectionLength > 0)
            {
                vm.SelectedText = textBox.Text.Substring(textBox.SelectionStart, textBox.SelectionLength);
            }
            else
            {
                vm.SelectedText = string.Empty;
            }
        }
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
