using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace v2rayN.Base;

public class PageBase<TViewModel> : ReactiveUserControl<TViewModel>, IWorkspacePage where TViewModel : class
{
    private readonly WorkspacePageLifetime _lifetime = new();
    private bool _cancelBound;

    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title), typeof(string), typeof(PageBase<TViewModel>),
        new PropertyMetadata(string.Empty, (sender, _) =>
        {
            var page = (PageBase<TViewModel>)sender;
            page.TitleChanged?.Invoke(page, EventArgs.Empty);
        }));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    public event EventHandler? TitleChanged;
    public event Action<bool>? CloseRequested { add => _lifetime.Closed += value; remove => _lifetime.Closed -= value; }
    public event CancelEventHandler? Closing { add => _lifetime.Closing += value; remove => _lifetime.Closing -= value; }
    public bool? DialogResult { set => Close(value); }

    public PageBase()
    {
        Loaded += (_, _) =>
        {
            if (!_cancelBound && FindName("btnCancel") is ButtonBase cancel)
            {
                cancel.Click += (_, _) => Close(false);
                _cancelBound = true;
            }
        };
        PreviewKeyDown += (_, e) =>
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
