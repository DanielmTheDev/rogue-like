using GdUnit4;
using Godot;
using RogueLike.Code.TurnContext;
using static GdUnit4.Assertions;

namespace RogueLike.Tests.TurnContext;

/// <summary>
/// Verifies the correct behavior of the state-machine Turn Manager.
/// </summary>
[TestSuite]
public class TurnManagerTest
{
    [TestCase]
    public void InitialState_IsPlayerTurn()
    {
        var turnManager = new TurnManager();
        AssertInt((int)turnManager.CurrentState).IsEqual((int)TurnState.Player);
    }

    [TestCase]
    public void EndPlayerTurn_ShiftsToEnemyTurn_ThenBackToPlayer()
    {
        var turnManager = new TurnManager();

        // As enemies are currently instantly resolving their turn,
        // it should complete a full cycle back to Player.
        turnManager.EndPlayerTurn();

        AssertInt((int)turnManager.CurrentState).IsEqual((int)TurnState.Player);
    }

    [TestCase]
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

        AssertInt(enemyTurnFiredCount).IsEqual(1);
        AssertInt(playerTurnFiredCount).IsEqual(1);
    }
}
