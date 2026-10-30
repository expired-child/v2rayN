using v2rayN.Base;

namespace v2rayN.Manager;

public class WindowDialog : IWindowDialog
{
    public async Task<bool> ShowDialogAsync<TViewModel>(TViewModel vm)
        where TViewModel : class
    {
        var viewFor = SimpleViewLocator.Instance.ResolveView(vm);
        if (viewFor is not PageBase<TViewModel> page || Application.Current.MainWindow is not Views.MainWindow main)
        {
            return false;
        }
        page.ViewModel = vm;
        var closeable = vm as ICloseable;
        EventHandler onSaved = (_, _) => Dispatch(() => page.Close(true));
        if (closeable != null)
        {
            closeable.RequestClose += onSaved;
        }
        try
        {
            return await main.ShowWorkspacePageAsync(page);
        }
        finally
        {
            if (closeable != null)
            {
                closeable.RequestClose -= onSaved;
            }
        }
    }

    private static void Dispatch(Action action)
    {
        Application.Current.Dispatcher.Invoke(action);
    }
}
