using Xunit;
using RogueLike.Domain.Flow;

namespace RogueLike.Domain.Tests.Flow;

/// <summary>
/// Verifies the correct behavior of the state-machine Turn Manager.
/// </summary>
public class TurnManagerTest
{
    [Fact]
    public void InitialState_IsPlayerTurn()
    {
        var turnManager = new TurnManager();
        Assert.Equal(TurnState.Player, turnManager.CurrentState);
    }

    [Fact]
    public void EndPlayerTurn_ShiftsToEnemyTurn()
    {
        var turnManager = new TurnManager();

        turnManager.EndPlayerTurn();

        Assert.Equal(TurnState.Enemy, turnManager.CurrentState);

        turnManager.EndEnemyTurn();

        Assert.Equal(TurnState.Player, turnManager.CurrentState);
    }

    [Fact]
    public void EndPlayerTurn_FiresTurnChangedEvents()
    {
        var turnManager = new TurnManager();

        var enemyTurnFiredCount = 0;
        var playerTurnFiredCount = 0;

        turnManager.OnTurnChanged += state =>
        {
            if (state == TurnState.Enemy) enemyTurnFiredCount++;
            if (state == TurnState.Player) playerTurnFiredCount++;
        };

        turnManager.EndPlayerTurn();
        turnManager.EndEnemyTurn();

        Assert.Equal(1, enemyTurnFiredCount);
        Assert.Equal(1, playerTurnFiredCount);
    }
}
