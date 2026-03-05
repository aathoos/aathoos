using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using aathoos.Core;

namespace aathoos.Views;

public partial class DashboardPage : UserControl
{
    private static readonly SolidColorBrush FgBrush      = new(Color.FromRgb(0xe2, 0xe3, 0xde));
    private static readonly SolidColorBrush MutedBrush   = new(Color.FromRgb(0x84, 0x85, 0xa0));
    private static readonly SolidColorBrush AccentBrush  = new(Color.FromRgb(0xc4, 0x26, 0x4d));
    private static readonly SolidColorBrush SuccessBrush = new(Color.FromRgb(0x22, 0xc5, 0x5e));
    private static readonly SolidColorBrush SurfaceBrush = new(Color.FromRgb(0x1d, 0x1a, 0x30));
    private static readonly SolidColorBrush BorderBrush2 = new(Color.FromArgb(0x28, 0xe2, 0xe3, 0xde));

    public DashboardPage()
    {
        InitializeComponent();
        Loaded += (_, _) => Refresh();
    }

    private void Refresh()
    {
        var bridge = AppDatabase.Instance.Bridge;

        // Greeting
        var hour = DateTime.Now.Hour;
        GreetingText.Text = hour < 12 ? "Good morning" : hour < 17 ? "Good afternoon" : "Good evening";
        DateText.Text = DateTime.Now.ToString("dddd, MMMM d, yyyy");

        // Data
        var tasks    = bridge.TaskListAll();
        var notes    = bridge.NoteListAll();
        var goals    = bridge.GoalListAll();
        var sessions = bridge.StudySessionListAll();

        // Stat cards
        var pending = tasks.Count(t => !t.IsCompleted);
        TaskCountText.Text = pending.ToString();
        NoteCountText.Text = notes.Count.ToString();
        GoalCountText.Text = goals.Count(g => !g.IsCompleted).ToString();

        var todayStart   = new DateTimeOffset(DateTime.Today).ToUnixTimeSeconds();
        var todaySeconds = sessions.Where(s => s.StartedAt >= todayStart).Sum(s => s.DurationSecs);
        var h = todaySeconds / 3600;
        var m = (todaySeconds % 3600) / 60;
        StudyTimeText.Text = h > 0 ? $"{h}h {m}m" : $"{m}m";

        // Task list
        TodayTaskRows.Children.Clear();
        var incomplete = tasks.Where(t => !t.IsCompleted).Take(6).ToList();
        if (incomplete.Count == 0)
        {
            NoTasksText.Visibility = Visibility.Visible;
        }
        else
        {
            NoTasksText.Visibility = Visibility.Collapsed;
            foreach (var t in incomplete)
                TodayTaskRows.Children.Add(BuildMiniTask(t));
        }

        // Goal list
        GoalRows.Children.Clear();
        var activeGoals = goals.Where(g => !g.IsCompleted).Take(5).ToList();
        if (activeGoals.Count == 0)
        {
            NoGoalsText.Visibility = Visibility.Visible;
        }
        else
        {
            NoGoalsText.Visibility = Visibility.Collapsed;
            foreach (var g in activeGoals)
                GoalRows.Children.Add(BuildMiniGoal(g));
        }
    }

    private UIElement BuildMiniTask(ATask task)
    {
        var grid = new Grid { Margin = new Thickness(0, 6, 0, 6) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var priorityColor = task.Priority switch
        {
            2 => new SolidColorBrush(Color.FromRgb(0xef, 0x44, 0x44)),
            1 => new SolidColorBrush(Color.FromRgb(0xf5, 0x9e, 0x0b)),
            _ => new SolidColorBrush(Color.FromRgb(0x60, 0xa5, 0xfa)),
        };

        var dot = new Border
        {
            Width = 7, Height = 7,
            CornerRadius = new CornerRadius(4),
            Background = priorityColor,
            VerticalAlignment = VerticalAlignment.Center,
        };
        Grid.SetColumn(dot, 0);

        var title = new TextBlock
        {
            Text = task.Title,
            FontSize = 13,
            Foreground = FgBrush,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        Grid.SetColumn(title, 2);

        var badge = task.Priority switch
        {
            2 => "HIGH",
            1 => "MED",
            _ => "LOW",
        };
        var priorityTag = new Border
        {
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(6, 2, 6, 2),
            Background = new SolidColorBrush(Color.FromArgb(0x22, priorityColor.Color.R, priorityColor.Color.G, priorityColor.Color.B)),
            VerticalAlignment = VerticalAlignment.Center,
            Child = new TextBlock { Text = badge, FontSize = 10, FontWeight = FontWeights.Bold, Foreground = priorityColor },
        };
        Grid.SetColumn(priorityTag, 3);

        grid.Children.Add(dot);
        grid.Children.Add(title);
        grid.Children.Add(priorityTag);

        var wrapper = new Border
        {
            BorderThickness = new Thickness(0, 0, 0, 1),
            BorderBrush = BorderBrush2,
            Padding = new Thickness(0, 0, 0, 6),
            Margin = new Thickness(0, 0, 0, 0),
            Child = grid,
        };
        return wrapper;
    }

    private UIElement BuildMiniGoal(AGoal goal)
    {
        var panel = new StackPanel { Margin = new Thickness(0, 0, 0, 14) };

        var headerRow = new Grid();
        headerRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        headerRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var titleText = new TextBlock
        {
            Text = goal.Title,
            FontSize = 13,
            FontWeight = FontWeights.SemiBold,
            Foreground = FgBrush,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        Grid.SetColumn(titleText, 0);

        var pctText = new TextBlock
        {
            Text = $"{goal.Progress:P0}",
            FontSize = 12,
            Foreground = MutedBrush,
            VerticalAlignment = VerticalAlignment.Center,
        };
        Grid.SetColumn(pctText, 1);

        headerRow.Children.Add(titleText);
        headerRow.Children.Add(pctText);
        panel.Children.Add(headerRow);

        var bar = new ProgressBar
        {
            Value = goal.Progress * 100,
            Maximum = 100,
            Height = 5,
            Margin = new Thickness(0, 6, 0, 0),
            Foreground = goal.IsCompleted ? SuccessBrush : AccentBrush,
        };
        panel.Children.Add(bar);

        return panel;
    }
}
