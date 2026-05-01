using Godot;
using RogueLike.Code.Services;

namespace RogueLike.Code.UI;

/// <summary>
/// Godot controller for the message log UI.
/// Listens to the GameLog service and updates a RichTextLabel.
/// </summary>
public partial class GameLogController : Control
{
    [Export]
    public RichTextLabel LogLabel;

    public override void _Ready()
    {
        if (LogLabel == null)
        {
            LogLabel = GetNode<RichTextLabel>("Panel/RichTextLabel");
        }

        // Subscribe to log events
        GameLog.Instance.OnMessageLogged += AppendMessage;
        GameLog.Instance.OnLogCleared += ClearLog;
        
        // Initial welcome
        GameLog.Instance.Log("[color=green]Welcome to the Dungeon, seeker.[/color]");
    }

    private void ClearLog()
    {
        LogLabel.Clear();
    }

    private void AppendMessage(string message)
    {
        LogLabel.AppendText(message + "\n");
    }

    public override void _ExitTree()
    {
        // Cleanup event subscription to prevent memory leaks
        GameLog.Instance.OnMessageLogged -= AppendMessage;
        GameLog.Instance.OnLogCleared -= ClearLog;
    }
}
