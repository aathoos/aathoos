using System.Windows;

namespace aathoos.Views;

public partial class AddGoalDialog : Window
{
    public string  GoalTitle       { get; private set; } = "";
    public string? GoalDescription { get; private set; }

    public AddGoalDialog() => InitializeComponent();

    private void OnAdd(object sender, RoutedEventArgs e)
    {
        var title = TitleBox.Text.Trim();
        if (string.IsNullOrEmpty(title)) return;

        GoalTitle       = title;
        GoalDescription = string.IsNullOrWhiteSpace(DescBox.Text) ? null : DescBox.Text.Trim();
        DialogResult = true;
    }

    private void OnCancel(object sender, RoutedEventArgs e) => DialogResult = false;
}
