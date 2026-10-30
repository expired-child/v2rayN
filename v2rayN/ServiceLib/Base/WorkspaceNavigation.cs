namespace ServiceLib.Base;

/// <summary>UI-only page history. Callers continue to await the original save/cancel result.</summary>
public sealed class WorkspaceNavigation : IDisposable
{
    private sealed record Entry(IWorkspacePage Page, TaskCompletionSource<bool> Completion, Action<bool> Closed, EventHandler TitleChanged);
    private readonly List<Entry> _entries = [];

    public IWorkspacePage? Current => _entries.LastOrDefault()?.Page;
    public IWorkspacePage? Root => _entries.FirstOrDefault()?.Page;
    public int Count => _entries.Count;
    public event Action? Changed;

    public Task<bool> OpenAsync(IWorkspacePage page)
    {
        var existing = _entries.FirstOrDefault(entry => ReferenceEquals(entry.Page, page));
        if (existing != null)
        {
            return existing.Completion.Task;
        }
        var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        Action<bool> closed = saved => Complete(page, saved);
        EventHandler titleChanged = (_, _) => Changed?.Invoke();
        _entries.Add(new Entry(page, completion, closed, titleChanged));
        page.CloseRequested += closed;
        page.TitleChanged += titleChanged;
        Changed?.Invoke();
        return completion.Task;
    }

    public bool Back() => Current?.Close() ?? true;

    public Task<bool>? ReturnTo(Type pageType)
    {
        var entry = _entries.LastOrDefault(item => item.Page.GetType() == pageType);
        if (entry == null)
        {
            return null;
        }
        foreach (var child in _entries.Skip(_entries.IndexOf(entry) + 1).ToArray().Reverse())
        {
            if (!child.Page.Close()) break;
        }
        return entry.Completion.Task;
    }

    public bool CloseAll()
    {
        foreach (var entry in _entries.ToArray().Reverse())
        {
            if (!entry.Page.Close())
            {
                return false;
            }
        }
        return true;
    }

    private void Complete(IWorkspacePage page, bool saved)
    {
        var entry = _entries.FirstOrDefault(item => ReferenceEquals(item.Page, page));
        if (entry == null)
        {
            return;
        }
        _entries.Remove(entry);
        page.CloseRequested -= entry.Closed;
        page.TitleChanged -= entry.TitleChanged;
        Changed?.Invoke();
        entry.Completion.TrySetResult(saved);
    }

    public void Dispose()
    {
        foreach (var entry in _entries.ToArray())
        {
            Complete(entry.Page, false);
        }
    }
}
