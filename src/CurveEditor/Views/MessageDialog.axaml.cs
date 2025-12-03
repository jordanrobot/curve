using Avalonia.Controls;
using Avalonia.Interactivity;

namespace CurveEditor.Views;

/// <summary>
/// Simple message dialog with OK/Cancel buttons.
/// </summary>
public partial class MessageDialog : Window
{
    /// <summary>
    /// Gets whether the user clicked OK.
    /// </summary>
    public bool IsConfirmed { get; private set; }

    public MessageDialog()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Sets the message to display.
    /// </summary>
    public void SetMessage(string message)
    {
        MessageText.Text = message;
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        IsConfirmed = false;
        Close();
    }

    private void OnOkClick(object? sender, RoutedEventArgs e)
    {
        IsConfirmed = true;
        Close();
    }
}
