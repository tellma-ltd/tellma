using System.Windows;
using System.Windows.Input;

namespace Tellma.Backdoor.Views;

public partial class ConfirmationDialog : Window
{
    public ConfirmationDialog()
    {
        InitializeComponent();
        ConfirmTextBox.Focus();
    }

    private bool IsConfirmed =>
        string.Equals(ConfirmTextBox.Text.Trim(), "Confirmed", StringComparison.OrdinalIgnoreCase);

    private void ConfirmTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        OkButton.IsEnabled = IsConfirmed;
    }

    private void ConfirmTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && IsConfirmed)
        {
            DialogResult = true;
            Close();
        }
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}
