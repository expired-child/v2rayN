using System.ComponentModel;

namespace ServiceLib.Base;

/// <summary>Preserves save/cancel results and closing hooks without a native dialog.</summary>
public sealed class WorkspacePageLifetime
{
    private CancelEventArgs? _closing;
    private bool _closed;
    private bool _saved;

    public event CancelEventHandler? Closing;
    public event Action<bool>? Closed;

    public bool Close(bool? saved = null)
    {
        if (_closed)
        {
            return true;
        }
        if (saved.HasValue)
        {
            _saved = saved.Value;
        }
        if (_closing != null)
        {
            // Legacy closing hooks may replace a cancel result with a saved result.
            _closing.Cancel = false;
            return true;
        }
        var args = new CancelEventArgs();
        _closing = args;
        try
        {
            Closing?.Invoke(this, args);
        }
        finally
        {
            _closing = null;
        }
        if (args.Cancel)
        {
            return false;
        }
        _closed = true;
        Closed?.Invoke(_saved);
        return true;
    }
}
