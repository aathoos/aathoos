using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using aathoos.Core;

namespace aathoos.Views;

public partial class GoalsPage : UserControl
{
    private readonly GoalStore _store = new(AppDatabase.Instance.Bridge);

    private static readonly SolidColorBrush FgBrush      = new(Color.FromRgb(0xe2, 0xe3, 0xde));
    private static readonly SolidColorBrush MutedBrush   = new(Color.FromRgb(0x84, 0x85, 0xa0));
    private static readonly SolidColorBrush AccentBrush  = new(Color.FromRgb(0xc4, 0x26, 0x4d));
    private static readonly SolidColorBrush SurfaceBrush = new(Color.FromRgb(0x1d, 0x1a, 0x30));
    private static readonly SolidColorBrush SuccessBrush = new(Color.FromRgb(0x22, 0xc5, 0x5e));
    private static readonly SolidColorBrush TransBrush   = new(Colors.Transparent);

    public GoalsPage()
    {
        InitializeComponent();
        Loaded += (_, _) => Refresh();
    }

    private void Refresh()
    {
        _store.Refresh();
        GoalCards.Children.Clear();

        if (_store.Goals.Count == 0)
        {
            GoalCards.Children.Add(BuildEmptyState());
            return;
        }

        // Active first, then completed
        foreach (var g in _store.Goals.Where(g => !g.IsCompleted)) GoalCards.Children.Add(BuildGoalCard(g));
        foreach (var g in _store.Goals.Where(g =>  g.IsCompleted)) GoalCards.Children.Add(BuildGoalCard(g));
    }

    private UIElement BuildEmptyState()
    {
        var panel = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 80, 0, 0),
        };
        panel.Children.Add(new TextBlock
        {
            Text = "\uE879", FontFamily = new FontFamily("Segoe MDL2 Assets"),
            FontSize = 44, HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = new SolidColorBrush(Color.FromArgb(0x28, 0xe2, 0xe3, 0xde)),
            Margin = new Thickness(0, 0, 0, 14),
        });
        panel.Children.Add(new TextBlock
        {
            Text = "No goals yet",
            FontSize = 19, FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromArgb(0x44, 0xe2, 0xe3, 0xde)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 6),
        });
        panel.Children.Add(new TextBlock
        {
            Text = "Press New Goal to set your first target",
            FontSize = 13, Foreground = MutedBrush,
            HorizontalAlignment = HorizontalAlignment.Center,
        });
        return panel;
    }

    private UIElement BuildGoalCard(AGoal goal)
    {
        var isComplete = goal.IsCompleted;
        var progressBrush = isComplete ? SuccessBrush : AccentBrush;
        var pct = (int)(goal.Progress * 100);

        // Title
        var titleText = new TextBlock
        {
            Text = goal.Title, FontSize = 15, FontWeight = FontWeights.SemiBold,
            Foreground = FgBrush, TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 4),
        };

        // Description
        var descText = new TextBlock
        {
            Text = string.IsNullOrWhiteSpace(goal.Description) ? " " : goal.Description,
            FontSize = 12.5, Foreground = MutedBrush, TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 14),
        };

        // Progress bar + pct
        var barRow = new Grid { Margin = new Thickness(0, 0, 0, 4) };
        barRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        barRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
        barRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var bar = new ProgressBar
        {
            Value = pct, Maximum = 100, Height = 7,
            Foreground = progressBrush,
            VerticalAlignment = VerticalAlignment.Center,
        };
        Grid.SetColumn(bar, 0);

        var pctLabel = new TextBlock
        {
            Text = $"{pct}%", FontSize = 12, FontWeight = FontWeights.SemiBold,
            Foreground = progressBrush, VerticalAlignment = VerticalAlignment.Center,
        };
        Grid.SetColumn(pctLabel, 2);

        barRow.Children.Add(bar);
        barRow.Children.Add(pctLabel);

        // Completed badge
        var completedBadge = new Border
        {
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(7, 3, 7, 3),
            Background = new SolidColorBrush(Color.FromArgb(0x22, 0x22, 0xc5, 0x5e)),
            Margin = new Thickness(0, 0, 0, 10),
            Visibility = isComplete ? Visibility.Visible : Visibility.Collapsed,
            Child = new TextBlock
            {
                Text = "COMPLETED", FontSize = 10.5, FontWeight = FontWeights.Bold,
                Foreground = SuccessBrush,
            },
        };

        // Action buttons
        var btnMinus = new Button
        {
            Content = "-10%", Padding = new Thickness(10, 6, 10, 6),
            FontSize = 12, IsEnabled = !isComplete, Tag = goal,
        };
        btnMinus.Click += (_, _) =>
        {
            _store.SetProgress(goal.Id, goal.Progress - 0.1);
            Refresh();
        };

        var btnPlus = new Button
        {
            Content = "+10%", Style = Application.Current.Resources["AccentButton"] as Style,
            Padding = new Thickness(10, 6, 10, 6), FontSize = 12,
            IsEnabled = !isComplete, Tag = goal, Margin = new Thickness(6, 0, 0, 0),
        };
        btnPlus.Click += (_, _) =>
        {
            _store.SetProgress(goal.Id, goal.Progress + 0.1);
            Refresh();
        };

        var btnComplete = new Button
        {
            Content = "Mark 100%", Padding = new Thickness(10, 6, 10, 6),
            FontSize = 12, IsEnabled = !isComplete, Tag = goal,
            Margin = new Thickness(6, 0, 0, 0),
            Style = Application.Current.Resources["GhostButton"] as Style,
        };
        btnComplete.Click += (_, _) =>
        {
            _store.SetProgress(goal.Id, 1.0);
            Refresh();
        };

        var btnDelete = new Button
        {
            Style = Application.Current.Resources["DangerButton"] as Style,
            Padding = new Thickness(6, 6, 6, 6), FontSize = 12,
            Tag = goal, Margin = new Thickness(6, 0, 0, 0),
            Content = new TextBlock
            {
                Text = "\uE74D", FontFamily = new FontFamily("Segoe MDL2 Assets"),
                FontSize = 13, Foreground = new SolidColorBrush(Colors.White),
            },
        };
        btnDelete.Click += (_, _) => { _store.Delete(goal.Id); Refresh(); };

        var actions = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 12, 0, 0) };
        actions.Children.Add(btnMinus);
        actions.Children.Add(btnPlus);
        actions.Children.Add(btnComplete);
        actions.Children.Add(btnDelete);

        var content = new StackPanel { Width = 280 };
        content.Children.Add(titleText);
        content.Children.Add(descText);
        content.Children.Add(completedBadge);
        content.Children.Add(barRow);
        content.Children.Add(actions);

        var card = new Border
        {
            Background = SurfaceBrush,
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(20, 18, 20, 18),
            Margin = new Thickness(0, 0, 14, 14),
            Opacity = isComplete ? 0.65 : 1.0,
            Child = content,
        };

        return card;
    }

    private void OnAddGoal(object sender, RoutedEventArgs e)
    {
        var dialog = new AddGoalDialog { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() != true) return;
        _store.Add(dialog.GoalTitle, dialog.GoalDescription);
        Refresh();
    }
}
