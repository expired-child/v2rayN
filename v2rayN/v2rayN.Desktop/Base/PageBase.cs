using System.ComponentModel;

namespace v2rayN.Desktop.Base;

public class PageBase<TViewModel> : ReactiveUserControl<TViewModel>, IWorkspacePage where TViewModel : class
{
    private readonly WorkspacePageLifetime _lifetime = new();
    private string _title = string.Empty;

    public string Title
    {
        get => _title;
        set { _title = value; TitleChanged?.Invoke(this, EventArgs.Empty); }
    }
    public event EventHandler? TitleChanged;
    public event Action<bool>? CloseRequested { add => _lifetime.Closed += value; remove => _lifetime.Closed -= value; }
    public event CancelEventHandler? Closing { add => _lifetime.Closing += value; remove => _lifetime.Closing -= value; }

    public PageBase()
    {
        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
            {
                Close();
                e.Handled = true;
            }
        };
    }

    public bool Close(bool? saved = null) => _lifetime.Close(saved);
}
