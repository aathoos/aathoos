using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using aathoos.Core;

namespace aathoos.Views;

public partial class TasksPage : UserControl
{
    private enum Filter { All, Active, Completed }
    private Filter _filter = Filter.Active;

    private readonly TaskStore _store = new(AppDatabase.Instance.Bridge);

    private static readonly SolidColorBrush FgBrush       = new(Color.FromRgb(0xe2, 0xe3, 0xde));
    private static readonly SolidColorBrush MutedBrush    = new(Color.FromRgb(0x84, 0x85, 0xa0));
    private static readonly SolidColorBrush AccentBrush   = new(Color.FromRgb(0xc4, 0x26, 0x4d));
    private static readonly SolidColorBrush SurfaceBrush  = new(Color.FromRgb(0x1d, 0x1a, 0x30));
    private static readonly SolidColorBrush Surface2Brush = new(Color.FromRgb(0x26, 0x22, 0x40));
    private static readonly SolidColorBrush TransBrush    = new(Colors.Transparent);
    private static readonly SolidColorBrush HighBrush     = new(Color.FromRgb(0xef, 0x44, 0x44));
    private static readonly SolidColorBrush MedBrush      = new(Color.FromRgb(0xf5, 0x9e, 0x0b));
    private static readonly SolidColorBrush LowBrush      = new(Color.FromRgb(0x60, 0xa5, 0xfa));
    private static readonly SolidColorBrush SuccessBrush  = new(Color.FromRgb(0x22, 0xc5, 0x5e));

    public TasksPage()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            _filter = Filter.Active;
            ApplyTabStyle();
            Refresh();
        };
    }

    private void Refresh()
    {
        _store.Refresh();

        var tasks = _filter switch
        {
            Filter.Active    => _store.Tasks.Where(t => !t.IsCompleted).ToList(),
            Filter.Completed => _store.Tasks.Where(t =>  t.IsCompleted).ToList(),
            _                => _store.Tasks.ToList(),
        };

        TaskRows.Children.Clear();
        if (tasks.Count == 0) { TaskRows.Children.Add(BuildEmptyState()); return; }
        foreach (var t in tasks) TaskRows.Children.Add(BuildTaskRow(t));
    }

    private void ApplyTabStyle()
    {
        var active   = Application.Current.Resources["AccentButton"] as Style;
        var inactive = Application.Current.Resources[typeof(Button)] as Style;
        TabAll.Style       = _filter == Filter.All       ? active : inactive;
        TabActive.Style    = _filter == Filter.Active    ? active : inactive;
        TabCompleted.Style = _filter == Filter.Completed ? active : inactive;
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
            Text = "\uE73E", FontFamily = new FontFamily("Segoe MDL2 Assets"),
            FontSize = 44, HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = new SolidColorBrush(Color.FromArgb(0x28, 0xe2, 0xe3, 0xde)),
            Margin = new Thickness(0, 0, 0, 14),
        });
        panel.Children.Add(new TextBlock
        {
            Text = _filter == Filter.Completed ? "No completed tasks yet" : "All clear",
            FontSize = 19, FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromArgb(0x44, 0xe2, 0xe3, 0xde)),
            HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 6),
        });
        panel.Children.Add(new TextBlock
        {
            Text = _filter == Filter.Completed ? "Complete some tasks to see them here" : "Press New Task to add something",
            FontSize = 13, Foreground = MutedBrush,
            HorizontalAlignment = HorizontalAlignment.Center,
        });
        return panel;
    }

    private UIElement BuildTaskRow(ATask task)
    {
        var pBrush = task.Priority switch { 2 => HighBrush, 1 => MedBrush, _ => LowBrush };

        // Toggle
        var toggleBtn = new Button
        {
            Background = TransBrush, BorderThickness = new Thickness(0),
            Padding = new Thickness(0), Width = 26, Height = 26,
            VerticalAlignment = VerticalAlignment.Center,
            Cursor = Cursors.Hand, Tag = task,
            Content = new TextBlock
            {
                Text = task.IsCompleted ? "\uE73E" : "\uE739",
                FontFamily = new FontFamily("Segoe MDL2 Assets"), FontSize = 17,
                Foreground = task.IsCompleted ? SuccessBrush : AccentBrush,
                Opacity = task.IsCompleted ? 1.0 : 0.55,
            },
        };
        toggleBtn.Click += OnToggleClick;

        // Title + notes
        var titleText = new TextBlock
        {
            Text = task.Title, FontSize = 14,
            Foreground = task.IsCompleted ? MutedBrush : FgBrush,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        if (task.IsCompleted) titleText.TextDecorations = TextDecorations.Strikethrough;

        var textStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        textStack.Children.Add(titleText);
        if (!string.IsNullOrWhiteSpace(task.Notes))
        {
            textStack.Children.Add(new TextBlock
            {
                Text = task.Notes, FontSize = 12, Foreground = MutedBrush,
                TextTrimming = TextTrimming.CharacterEllipsis, Margin = new Thickness(0, 2, 0, 0),
            });
        }

        // Priority badge
        var pLabel = task.Priority switch { 2 => "HIGH", 1 => "MED", _ => "LOW" };
        var badge = new Border
        {
            CornerRadius = new CornerRadius(4), Padding = new Thickness(7, 3, 7, 3),
            Background = new SolidColorBrush(Color.FromArgb(0x22, pBrush.Color.R, pBrush.Color.G, pBrush.Color.B)),
            VerticalAlignment = VerticalAlignment.Center,
            Child = new TextBlock { Text = pLabel, FontSize = 11, FontWeight = FontWeights.Bold, Foreground = pBrush },
        };

        // Delete (hidden on idle)
        var deleteBtn = new Button
        {
            Background = TransBrush, BorderThickness = new Thickness(0),
            Padding = new Thickness(0), Width = 26, Height = 26,
            Opacity = 0, VerticalAlignment = VerticalAlignment.Center,
            Cursor = Cursors.Hand, Tag = task,
            Content = new TextBlock
            {
                Text = "\uE74D", FontFamily = new FontFamily("Segoe MDL2 Assets"),
                FontSize = 13, Foreground = new SolidColorBrush(Color.FromRgb(0xef, 0x44, 0x44)),
            },
        };
        deleteBtn.Click += OnDeleteClick;

        // Layout grid
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        Grid.SetColumn(toggleBtn, 0);
        Grid.SetColumn(textStack, 2);
        Grid.SetColumn(badge,     4);
        Grid.SetColumn(deleteBtn, 6);
        grid.Children.Add(toggleBtn);
        grid.Children.Add(textStack);
        grid.Children.Add(badge);
        grid.Children.Add(deleteBtn);

        var fadedBrush = new SolidColorBrush(Color.FromArgb(0x40, pBrush.Color.R, pBrush.Color.G, pBrush.Color.B));
        var card = new Border
        {
            BorderThickness = new Thickness(3, 0, 0, 0),
            BorderBrush     = task.IsCompleted ? fadedBrush : pBrush,
            Background      = SurfaceBrush,
            CornerRadius    = new CornerRadius(0, 9, 9, 0),
            Margin          = new Thickness(0, 3, 0, 3),
            Padding         = new Thickness(16, 12, 16, 12),
            Child           = grid,
        };

        card.MouseEnter += (_, _) => { card.Background = Surface2Brush; deleteBtn.Opacity = 1; };
        card.MouseLeave += (_, _) => { card.Background = SurfaceBrush;  deleteBtn.Opacity = 0; };

        return card;
    }

    private void OnToggleClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: ATask t }) { _store.ToggleCompleted(t); Refresh(); }
    }

    private void OnDeleteClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: ATask t }) { _store.Delete(t.Id); Refresh(); }
    }

    private void OnAddClick(object sender, RoutedEventArgs e)
    {
        var dialog = new AddTaskDialog { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() != true) return;
        _store.Add(dialog.TaskTitle, dialog.TaskNotes, dialog.TaskPriority);
        Refresh();
    }

    private void OnFilterAll(object sender, RoutedEventArgs e)      { _filter = Filter.All;       ApplyTabStyle(); Refresh(); }
    private void OnFilterActive(object sender, RoutedEventArgs e)    { _filter = Filter.Active;    ApplyTabStyle(); Refresh(); }
    private void OnFilterCompleted(object sender, RoutedEventArgs e) { _filter = Filter.Completed; ApplyTabStyle(); Refresh(); }
}
