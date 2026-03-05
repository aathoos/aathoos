using System.Collections.ObjectModel;

namespace aathoos.Core;

public sealed class GoalStore
{
    private readonly CoreBridge _bridge;

    public ObservableCollection<AGoal> Goals { get; } = [];

    public GoalStore(CoreBridge bridge)
    {
        _bridge = bridge;
        Refresh();
    }

    public void Refresh()
    {
        var goals = _bridge.GoalListAll();
        Goals.Clear();
        foreach (var g in goals) Goals.Add(g);
    }

    public void Add(string title, string? description)
    {
        _bridge.GoalCreate(title, description);
        Refresh();
    }

    public void SetProgress(string id, double progress)
    {
        var clamped = Math.Clamp(progress, 0.0, 1.0);
        _bridge.GoalSetProgress(id, clamped);
        Refresh();
    }

    public void Delete(string id)
    {
        _bridge.GoalDelete(id);
        Refresh();
    }
}
