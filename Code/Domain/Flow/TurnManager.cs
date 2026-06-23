using System;

namespace RogueLike.Domain.Flow;

/// <summary>
/// A strict state-machine that tracks whose turn it is.
/// Pure C# logic, no Godot Node requirements.
/// </summary>
public class TurnManager
{
    public TurnState CurrentState { get; private set; } = TurnState.Player;

    /// <summary>
    /// Fired when the turn state changes. 
    /// Useful for UI updates or triggering generic game-loop ticks.
    /// </summary>
    public event Action<TurnState> OnTurnChanged;

    /// <summary>
    /// Signals that the player has completed their action and the turn should advance.
    /// </summary>
    public void EndPlayerTurn()
    {
        if (CurrentState != TurnState.Player)
            return;

        SetState(TurnState.Enemy);
    }

    /// <summary>
    /// Signals that all enemies are done moving, yielding control back to the player.
    /// </summary>
    public void EndEnemyTurn()
    {
        if (CurrentState != TurnState.Enemy)
            return;

        SetState(TurnState.Player);
    }

    private void SetState(TurnState newState)
    {
        CurrentState = newState;
        OnTurnChanged?.Invoke(newState);
    }
}
