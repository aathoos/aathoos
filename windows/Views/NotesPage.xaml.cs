using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using aathoos.Core;

namespace aathoos.Views;

public partial class NotesPage : UserControl
{
    private readonly NoteStore _store = new(AppDatabase.Instance.Bridge);
    private ANote? _selected;

    private static readonly SolidColorBrush FgBrush      = new(Color.FromRgb(0xe2, 0xe3, 0xde));
    private static readonly SolidColorBrush MutedBrush   = new(Color.FromRgb(0x84, 0x85, 0xa0));
    private static readonly SolidColorBrush AccentBrush  = new(Color.FromRgb(0xc4, 0x26, 0x4d));
    private static readonly SolidColorBrush SurfaceBrush = new(Color.FromRgb(0x1d, 0x1a, 0x30));
    private static readonly SolidColorBrush HoverBrush   = new(Color.FromRgb(0x26, 0x22, 0x40));
    private static readonly SolidColorBrush SelBrush     = new(Color.FromArgb(0x1f, 0xc4, 0x26, 0x4d));

    public NotesPage()
    {
        InitializeComponent();
        Loaded += (_, _) => RebuildList();
    }

    private void RebuildList()
    {
        _store.Refresh();
        NoteList.Children.Clear();

        if (_store.Notes.Count == 0)
        {
            NoteList.Children.Add(new TextBlock
            {
                Text = "No notes yet. Press New Note.",
                FontSize = 12, Foreground = MutedBrush,
                Margin = new Thickness(16, 16, 16, 0),
                TextWrapping = TextWrapping.Wrap,
            });
            return;
        }

        foreach (var note in _store.Notes.OrderByDescending(n => n.UpdatedAt))
            NoteList.Children.Add(BuildNoteItem(note));
    }

    private UIElement BuildNoteItem(ANote note)
    {
        var isSelected = _selected?.Id == note.Id;

        var title = new TextBlock
        {
            Text = note.Title, FontSize = 13.5, FontWeight = FontWeights.SemiBold,
            Foreground = FgBrush, TextTrimming = TextTrimming.CharacterEllipsis,
        };

        var subjectRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 4) };
        if (!string.IsNullOrWhiteSpace(note.Subject))
        {
            subjectRow.Children.Add(new Border
            {
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(6, 2, 6, 2),
                Background = new SolidColorBrush(Color.FromArgb(0x22, 0xc4, 0x26, 0x4d)),
                Child = new TextBlock { Text = note.Subject, FontSize = 11, Foreground = AccentBrush },
                Margin = new Thickness(0, 0, 6, 0),
            });
        }
        var updated = DateTimeOffset.FromUnixTimeSeconds(note.UpdatedAt).LocalDateTime;
        subjectRow.Children.Add(new TextBlock
        {
            Text = FormatAge(updated), FontSize = 11, Foreground = MutedBrush,
            VerticalAlignment = VerticalAlignment.Center,
        });

        var preview = new TextBlock
        {
            Text = note.Body?.Replace('\n', ' '),
            FontSize = 12, Foreground = MutedBrush,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };

        var panel = new StackPanel();
        panel.Children.Add(title);
        panel.Children.Add(subjectRow);
        panel.Children.Add(preview);

        var card = new Border
        {
            Padding = new Thickness(16, 12, 16, 12),
            BorderThickness = new Thickness(0, 0, 0, 1),
            BorderBrush = new SolidColorBrush(Color.FromArgb(0x20, 0xe2, 0xe3, 0xde)),
            Background = isSelected ? SelBrush : SurfaceBrush,
            Child = panel, Cursor = Cursors.Hand, Tag = note,
        };

        card.MouseEnter       += (_, _) => { if (_selected?.Id != note.Id) card.Background = HoverBrush; };
        card.MouseLeave       += (_, _) => { if (_selected?.Id != note.Id) card.Background = SurfaceBrush; };
        card.MouseLeftButtonUp += (_, _) => SelectNote(note);

        return card;
    }

    private void SelectNote(ANote note)
    {
        _selected = note;
        RebuildList();

        EditorPanel.Visibility      = Visibility.Visible;
        EmptyEditorPanel.Visibility = Visibility.Collapsed;

        EditorTitle.Text   = note.Title;
        EditorSubject.Text = string.IsNullOrWhiteSpace(note.Subject) ? "No subject" : $"Subject: {note.Subject}";
        EditorBody.Text    = note.Body;

        var updated = DateTimeOffset.FromUnixTimeSeconds(note.UpdatedAt).LocalDateTime;
        EditorMeta.Text = $"Last edited {updated:MMM d, yyyy 'at' h:mm tt}";
    }

    private void OnEditorBodyLostFocus(object sender, RoutedEventArgs e)
    {
        if (_selected == null) return;
        _store.UpdateBody(_selected.Id, EditorBody.Text);
        _selected = _selected with { Body = EditorBody.Text };
        var fresh = _store.Notes.FirstOrDefault(n => n.Id == _selected.Id);
        if (fresh != null)
        {
            var dt = DateTimeOffset.FromUnixTimeSeconds(fresh.UpdatedAt).LocalDateTime;
            EditorMeta.Text = $"Last edited {dt:MMM d, yyyy 'at' h:mm tt}";
        }
    }

    private void OnAddNote(object sender, RoutedEventArgs e)
    {
        var dialog = new AddNoteDialog { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() != true) return;
        var note = _store.Add(dialog.NoteTitle, dialog.NoteBody, dialog.NoteSubject);
        if (note != null) SelectNote(note);
        else RebuildList();
    }

    private void OnDeleteNote(object sender, RoutedEventArgs e)
    {
        if (_selected == null) return;
        _store.Delete(_selected.Id);
        _selected = null;
        EditorPanel.Visibility      = Visibility.Collapsed;
        EmptyEditorPanel.Visibility = Visibility.Visible;
        RebuildList();
    }

    private static string FormatAge(DateTime dt)
    {
        var diff = DateTime.Now - dt;
        if (diff.TotalMinutes < 1) return "just now";
        if (diff.TotalHours   < 1) return $"{(int)diff.TotalMinutes}m ago";
        if (diff.TotalDays    < 1) return $"{(int)diff.TotalHours}h ago";
        if (diff.TotalDays    < 7) return $"{(int)diff.TotalDays}d ago";
        return dt.ToString("MMM d");
    }
}
