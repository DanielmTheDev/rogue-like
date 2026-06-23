using Xunit;
using RogueLike.Domain.Flow;

namespace RogueLike.Domain.Tests.Flow;

public class ExperienceSystemTest
{
    private readonly ExperienceSystem _exp;

    public ExperienceSystemTest()
    {
        _exp = new ExperienceSystem();
    }

    [Fact]
    public void InitialState_IsCorrect()
    {
        Assert.Equal(1, _exp.CurrentLevel);
        Assert.Equal(0, _exp.CurrentXP);
        Assert.Equal(100, _exp.XPForNextLevel);
    }

    [Fact]
    public void AddXP_NoLevelUp()
    {
        _exp.AddXP(50);
        Assert.Equal(1, _exp.CurrentLevel);
        Assert.Equal(50, _exp.CurrentXP);
    }

    [Fact]
    public void AddXP_ExactLevelUp()
    {
        _exp.AddXP(100);
        Assert.Equal(2, _exp.CurrentLevel);
        Assert.Equal(0, _exp.CurrentXP);
        Assert.Equal(200, _exp.XPForNextLevel);
    }

    [Fact]
    public void AddXP_LevelUpWithCarryOver()
    {
        _exp.AddXP(150);
        Assert.Equal(2, _exp.CurrentLevel);
        Assert.Equal(50, _exp.CurrentXP);
        Assert.Equal(200, _exp.XPForNextLevel);
    }

    [Fact]
    public void AddXP_MultiLevelUp()
    {
        // Level 1 needs 100 XP. Level 2 needs 200 XP. Total: 300 XP.
        // We add 350 XP.
        _exp.AddXP(350);
        Assert.Equal(3, _exp.CurrentLevel);
        Assert.Equal(50, _exp.CurrentXP);
        Assert.Equal(300, _exp.XPForNextLevel);
    }

    [Fact]
    public void Events_OnXPChanged_FiresCorrectly()
    {
        var receivedCurrentXP = -1;
        var receivedXPForNext = -1;

        _exp.OnXPChanged += (current, next) =>
        {
            receivedCurrentXP = current;
            receivedXPForNext = next;
        };

        _exp.AddXP(50);

        Assert.Equal(50, receivedCurrentXP);
        Assert.Equal(100, receivedXPForNext);
    }

    [Fact]
    public void Events_OnLevelUp_FiresCorrectly()
    {
        var receivedNewLevel = 0;
        _exp.OnLevelUp += (newLevel) =>
        {
            receivedNewLevel = newLevel;
        };

        _exp.AddXP(100);

        Assert.Equal(2, receivedNewLevel);
    }
}
