using System.Windows;

namespace aathoos.Views;

public partial class LogSessionDialog : Window
{
    public string  Subject     { get; private set; } = "";
    public long    DurationSecs { get; private set; }
    public string? Notes       { get; private set; }

    public LogSessionDialog() => InitializeComponent();

    private void OnLog(object sender, RoutedEventArgs e)
    {
        var subject = SubjectBox.Text.Trim();
        if (string.IsNullOrEmpty(subject)) return;

        if (!int.TryParse(HoursBox.Text, out var h)) h = 0;
        if (!int.TryParse(MinutesBox.Text, out var m)) m = 0;
        var secs = (long)(h * 3600) + (long)(m * 60);
        if (secs <= 0) return;

        Subject      = subject;
        DurationSecs = secs;
        Notes        = string.IsNullOrWhiteSpace(NotesBox.Text) ? null : NotesBox.Text.Trim();
        DialogResult = true;
    }

    private void OnCancel(object sender, RoutedEventArgs e) => DialogResult = false;
}
