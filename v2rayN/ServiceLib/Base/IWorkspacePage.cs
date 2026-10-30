namespace ServiceLib.Base;

/// <summary>A management or editor page hosted inside the main workspace.</summary>
public interface IWorkspacePage
{
    string Title { get; }
    event EventHandler? TitleChanged;
    event Action<bool>? CloseRequested;
    bool Close(bool? saved = null);
}
