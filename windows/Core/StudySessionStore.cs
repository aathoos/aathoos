using System.Collections.ObjectModel;

namespace aathoos.Core;

public sealed class StudySessionStore
{
    private readonly CoreBridge _bridge;

    public ObservableCollection<AStudySession> Sessions { get; } = [];

    public StudySessionStore(CoreBridge bridge)
    {
        _bridge = bridge;
        Refresh();
    }

    public void Refresh()
    {
        var sessions = _bridge.StudySessionListAll();
        Sessions.Clear();
        foreach (var s in sessions) Sessions.Add(s);
    }

    public void Add(string subject, long durationSecs, string? notes = null)
    {
        _bridge.StudySessionCreate(subject, durationSecs, notes);
        Refresh();
    }

    public void Delete(string id)
    {
        _bridge.StudySessionDelete(id);
        Refresh();
    }

    public long TotalDurationFor(string subject) =>
        _bridge.StudySessionTotalDuration(subject);
}
