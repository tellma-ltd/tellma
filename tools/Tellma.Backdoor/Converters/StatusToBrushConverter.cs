using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Tellma.Backdoor.ViewModels;

namespace Tellma.Backdoor.Converters;

public class StatusToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is ExecutionStatus status ? status switch
        {
            ExecutionStatus.NotStarted => Brushes.Gray,
            ExecutionStatus.Executing => Brushes.DodgerBlue,
            ExecutionStatus.Completed => Brushes.Green,
            ExecutionStatus.Failed => Brushes.Red,
            ExecutionStatus.Cancelled => Brushes.Orange,
            _ => Brushes.Gray,
        } : Brushes.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class StatusToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is ExecutionStatus status ? status switch
        {
            ExecutionStatus.NotStarted => "Not Started",
            ExecutionStatus.Executing => "Executing...",
            ExecutionStatus.Completed => "Completed",
            ExecutionStatus.Failed => "Failed",
            ExecutionStatus.Cancelled => "Cancelled",
            _ => string.Empty,
        } : string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class StatusToFailedVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is ExecutionStatus.Failed ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? false : true;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? false : true;
}

public class NotNullToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value != null;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
