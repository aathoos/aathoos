using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using aathoos.Core;

namespace aathoos.Views;

public partial class StudyPlannerPage : UserControl
{
    private readonly StudySessionStore _store = new(AppDatabase.Instance.Bridge);

    private static readonly SolidColorBrush FgBrush      = new(Color.FromRgb(0xe2, 0xe3, 0xde));
    private static readonly SolidColorBrush MutedBrush   = new(Color.FromRgb(0x84, 0x85, 0xa0));
    private static readonly SolidColorBrush AccentBrush  = new(Color.FromRgb(0xc4, 0x26, 0x4d));
    private static readonly SolidColorBrush SurfaceBrush = new(Color.FromRgb(0x1d, 0x1a, 0x30));
    private static readonly SolidColorBrush ErrorBrush   = new(Color.FromRgb(0xef, 0x44, 0x44));
    private static readonly SolidColorBrush TransBrush   = new(Colors.Transparent);

    private static readonly SolidColorBrush[] SubjectColors =
    [
        new(Color.FromRgb(0xc4, 0x26, 0x4d)),
        new(Color.FromRgb(0x60, 0xa5, 0xfa)),
        new(Color.FromRgb(0x22, 0xc5, 0x5e)),
        new(Color.FromRgb(0xf5, 0x9e, 0x0b)),
        new(Color.FromRgb(0xa7, 0x8b, 0xfa)),
        new(Color.FromRgb(0x34, 0xd3, 0x99)),
    ];

    public StudyPlannerPage()
    {
        InitializeComponent();
        Loaded += (_, _) => Refresh();
    }

    private void Refresh()
    {
        _store.Refresh();
        BuildSubjectTotals();
        BuildSessionList();
    }

    private void BuildSubjectTotals()
    {
        SubjectTotals.Children.Clear();

        var groups = _store.Sessions
            .GroupBy(s => s.Subject)
            .Select(g => (Subject: g.Key, TotalSecs: g.Sum(s => s.DurationSecs)))
            .OrderByDescending(x => x.TotalSecs)
            .ToList();

        if (groups.Count == 0) return;

        var maxSecs = groups.Max(x => x.TotalSecs);
        int ci = 0;

        foreach (var (subject, totalSecs) in groups)
        {
            var color = SubjectColors[ci++ % SubjectColors.Length];
            var fillRatio = maxSecs > 0 ? totalSecs / (double)maxSecs : 0;

            var label = new TextBlock
            {
                Text = subject, FontSize = 13, Foreground = FgBrush,
                TextTrimming = TextTrimming.CharacterEllipsis, Margin = new Thickness(0, 0, 0, 4),
            };

            var trackBorder = new Border
            {
                Height = 6, CornerRadius = new CornerRadius(3),
                Background = new SolidColorBrush(Color.FromArgb(0x22, color.Color.R, color.Color.G, color.Color.B)),
            };

            var fillBorder = new Border
            {
                Height = 6, CornerRadius = new CornerRadius(3),
                Background = color, HorizontalAlignment = HorizontalAlignment.Left,
            };

            var barGrid = new Grid { Margin = new Thickness(0, 0, 0, 3) };
            barGrid.Children.Add(trackBorder);
            barGrid.SizeChanged += (_, _) =>
            {
                if (barGrid.ActualWidth > 0)
                    fillBorder.Width = barGrid.ActualWidth * fillRatio;
            };
            barGrid.Children.Add(fillBorder);

            var timeText = new TextBlock
            {
                Text = FormatDuration(totalSecs), FontSize = 11.5, Foreground = MutedBrush,
            };

            var item = new StackPanel { Margin = new Thickness(18, 0, 18, 12) };
            item.Children.Add(label);
            item.Children.Add(barGrid);
            item.Children.Add(timeText);
            SubjectTotals.Children.Add(item);
        }
    }

    private void BuildSessionList()
    {
        SessionList.Children.Clear();

        var sessions = _store.Sessions.OrderByDescending(s => s.StartedAt).Take(30).ToList();

        if (sessions.Count == 0) { NoSessionsText.Visibility = Visibility.Visible; return; }
        NoSessionsText.Visibility = Visibility.Collapsed;

        foreach (var session in sessions)
            SessionList.Children.Add(BuildSessionRow(session));
    }

    private UIElement BuildSessionRow(AStudySession session)
    {
        var date = DateTimeOffset.FromUnixTimeSeconds(session.StartedAt).LocalDateTime;

        var badge = new Border
        {
            CornerRadius = new CornerRadius(4), Padding = new Thickness(7, 3, 7, 3),
            Background = new SolidColorBrush(Color.FromArgb(0x22, 0xc4, 0x26, 0x4d)),
            Child = new TextBlock { Text = session.Subject, FontSize = 11.5, Foreground = AccentBrush },
            VerticalAlignment = VerticalAlignment.Center,
        };

        var duration = new TextBlock
        {
            Text = FormatDuration(session.DurationSecs),
            FontSize = 14, FontWeight = FontWeights.SemiBold,
            Foreground = FgBrush, VerticalAlignment = VerticalAlignment.Center,
        };

        var dateText = new TextBlock
        {
            Text = date.ToString("MMM d, h:mm tt"),
            FontSize = 12, Foreground = MutedBrush, VerticalAlignment = VerticalAlignment.Center,
        };

        var deleteBtn = new Button
        {
            Background = TransBrush, BorderThickness = new Thickness(0),
            Padding = new Thickness(0), Width = 24, Height = 24,
            Opacity = 0, VerticalAlignment = VerticalAlignment.Center, Tag = session,
            Cursor = Cursors.Hand,
            Content = new TextBlock
            {
                Text = "\uE74D", FontFamily = new FontFamily("Segoe MDL2 Assets"),
                FontSize = 12, Foreground = ErrorBrush,
            },
        };
        deleteBtn.Click += (_, _) => { _store.Delete(session.Id); Refresh(); };

        var topRow = new StackPanel { Orientation = Orientation.Horizontal };
        topRow.Children.Add(badge);
        topRow.Children.Add(new Border { Width = 10 });
        topRow.Children.Add(duration);

        var leftCol = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        leftCol.Children.Add(topRow);
        if (!string.IsNullOrWhiteSpace(session.Notes))
        {
            leftCol.Children.Add(new TextBlock
            {
                Text = session.Notes, FontSize = 12, Foreground = MutedBrush,
                TextTrimming = TextTrimming.CharacterEllipsis, Margin = new Thickness(0, 3, 0, 0),
            });
        }

        var rowGrid = new Grid();
        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) });
        rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        Grid.SetColumn(leftCol,   0);
        Grid.SetColumn(dateText,  1);
        Grid.SetColumn(deleteBtn, 3);
        rowGrid.Children.Add(leftCol);
        rowGrid.Children.Add(dateText);
        rowGrid.Children.Add(deleteBtn);

        var card = new Border
        {
            Background = SurfaceBrush, CornerRadius = new CornerRadius(9),
            Margin = new Thickness(0, 0, 0, 6), Padding = new Thickness(16, 12, 16, 12),
            Child = rowGrid,
        };
        card.MouseEnter += (_, _) => { card.Background = new SolidColorBrush(Color.FromRgb(0x26, 0x22, 0x40)); deleteBtn.Opacity = 1; };
        card.MouseLeave += (_, _) => { card.Background = SurfaceBrush; deleteBtn.Opacity = 0; };

        return card;
    }

    private void OnLogSession(object sender, RoutedEventArgs e)
    {
        var dialog = new LogSessionDialog { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() != true) return;
        _store.Add(dialog.Subject, dialog.DurationSecs, dialog.Notes);
        Refresh();
    }

    private static string FormatDuration(long secs)
    {
        var h = secs / 3600;
        var m = (secs % 3600) / 60;
        if (h > 0 && m > 0) return $"{h}h {m}m";
        if (h > 0)           return $"{h}h";
        return $"{m}m";
    }
}
