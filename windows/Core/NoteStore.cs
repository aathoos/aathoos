using System.Collections.ObjectModel;

namespace aathoos.Core;

public sealed class NoteStore
{
    private readonly CoreBridge _bridge;

    public ObservableCollection<ANote> Notes { get; } = [];

    public NoteStore(CoreBridge bridge)
    {
        _bridge = bridge;
        Refresh();
    }

    public void Refresh()
    {
        var notes = _bridge.NoteListAll();
        Notes.Clear();
        foreach (var n in notes) Notes.Add(n);
    }

    public ANote? Add(string title, string body, string? subject)
    {
        var note = _bridge.NoteCreate(title, body, subject);
        Refresh();
        return note;
    }

    public bool UpdateBody(string id, string body)
    {
        var ok = _bridge.NoteUpdateBody(id, body);
        Refresh();
        return ok;
    }

    public void Delete(string id)
    {
        _bridge.NoteDelete(id);
        Refresh();
    }
}
