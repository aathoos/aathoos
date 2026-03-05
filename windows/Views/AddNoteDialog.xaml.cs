using System.Windows;

namespace aathoos.Views;

public partial class AddNoteDialog : Window
{
    public string  NoteTitle   { get; private set; } = "";
    public string  NoteBody    { get; private set; } = "";
    public string? NoteSubject { get; private set; }

    public AddNoteDialog() => InitializeComponent();

    private void OnAdd(object sender, RoutedEventArgs e)
    {
        var title = TitleBox.Text.Trim();
        if (string.IsNullOrEmpty(title)) return;

        NoteTitle   = title;
        NoteBody    = BodyBox.Text.Trim();
        NoteSubject = string.IsNullOrWhiteSpace(SubjectBox.Text) ? null : SubjectBox.Text.Trim();
        DialogResult = true;
    }

    private void OnCancel(object sender, RoutedEventArgs e) => DialogResult = false;
}
