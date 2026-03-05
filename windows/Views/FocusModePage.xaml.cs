using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using aathoos.Core;

namespace aathoos.Views;

public partial class FocusModePage : UserControl
{
    private const int WorkSecs      = 25 * 60;
    private const int BreakSecs     = 5  * 60;
    private const int LongBreakSecs = 15 * 60;
    private const int SessionsCycle = 4;
    private const double Circumference = 2 * Math.PI * 116;

    private readonly StudySessionStore _store = new(AppDatabase.Instance.Bridge);
    private readonly DispatcherTimer _timer;

    private bool _running;
    private bool _isBreak;
    private int  _remaining;
    private int  _total;
    private int  _completedToday;

    private static readonly SolidColorBrush AccentBrush = new(Color.FromRgb(0xc4, 0x26, 0x4d));
    private static readonly SolidColorBrush BreakBrush  = new(Color.FromRgb(0x22, 0xc5, 0x5e));

    public FocusModePage()
    {
        InitializeComponent();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += OnTick;

        Loaded += (_, _) =>
        {
            ResetToWork();
            CountTodaySessions();
        };
        Unloaded += (_, _) => _timer.Stop();
    }

    private void ResetToWork()
    {
        _isBreak = false;
        _remaining = _total = WorkSecs;
        var sessionNum = (_completedToday % SessionsCycle) + 1;
        TimerRing.Stroke = AccentBrush;
        PhaseLabel.Foreground = AccentBrush;
        PhaseLabel.Text = "WORK SESSION";
        SessionInfoText.Text = $"Session {sessionNum} of {SessionsCycle}";
        UpdateDisplay();
        UpdateButtons(running: false);
    }

    private void CountTodaySessions()
    {
        _store.Refresh();
        var todayStart = new DateTimeOffset(DateTime.Today).ToUnixTimeSeconds();
        _completedToday = _store.Sessions.Count(s => s.StartedAt >= todayStart);
        StatsLabel.Text = _completedToday switch
        {
            0 => "No sessions completed today",
            1 => "1 session completed today",
            _ => $"{_completedToday} sessions completed today",
        };
    }

    private void OnTick(object? sender, EventArgs e)
    {
        if (_remaining > 0) { _remaining--; UpdateDisplay(); return; }

        _timer.Stop();
        _running = false;

        if (!_isBreak)
        {
            var subject = string.IsNullOrWhiteSpace(SubjectBox.Text) ? "General" : SubjectBox.Text.Trim();
            _store.Add(subject, WorkSecs);
            _completedToday++;
            CountTodaySessions();

            _isBreak = true;
            var longBreak = (_completedToday % SessionsCycle) == 0;
            _remaining = _total = longBreak ? LongBreakSecs : BreakSecs;
            PhaseLabel.Text = longBreak ? "LONG BREAK" : "SHORT BREAK";
            PhaseLabel.Foreground = BreakBrush;
            TimerRing.Stroke = BreakBrush;
            SessionInfoText.Text = longBreak ? "Long break — well earned!" : "Short break";
        }
        else
        {
            ResetToWork();
        }

        UpdateDisplay();
        UpdateButtons(running: false);
    }

    private void UpdateDisplay()
    {
        TimerText.Text = $"{_remaining / 60:D2}:{_remaining % 60:D2}";
        var progress = _total > 0 ? (double)_remaining / _total : 1.0;
        TimerRing.StrokeDashOffset = Circumference * (1.0 - progress);
    }

    private void UpdateButtons(bool running)
    {
        StartBtnIcon.Text = running ? "\uE769" : "\uE768";
        StartBtnText.Text = running ? "Pause"  : "Start";
    }

    private void OnStartPause(object sender, RoutedEventArgs e)
    {
        if (_running) { _timer.Stop(); _running = false; }
        else          { _timer.Start(); _running = true; }
        UpdateButtons(_running);
    }

    private void OnReset(object sender, RoutedEventArgs e)
    {
        _timer.Stop();
        _running = false;
        ResetToWork();
    }
}
