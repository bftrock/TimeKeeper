using System.Windows;

namespace TimeKeeper;

/// <summary>
/// Interaction logic for AddTask.xaml
/// </summary>
public partial class AddTask : Window
{
    public AddTask()
    {
        InitializeComponent();
    }

    private void OkBtn_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Hide();
    }

    private void CancelBtn_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Hide();
    }
}
