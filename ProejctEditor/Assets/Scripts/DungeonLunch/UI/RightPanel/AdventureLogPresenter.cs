using System.Collections.Generic;

public class AdventureLogPresenter
{
    public AdventureLogView view;
    private readonly List<string> _logs = new List<string>();
    private const int MaxLogs = 50;

    public AdventureLogPresenter(AdventureLogView view)
    {
        this.view = view;
        view.presenter = this;
    }

    public void AddLog(string message)
    {
        _logs.Add(message);
        if (_logs.Count > MaxLogs) _logs.RemoveAt(0);
        view.Render(_logs);
    }
}
