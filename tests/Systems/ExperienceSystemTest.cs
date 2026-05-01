using GdUnit4;
using RogueLike.Code.Systems;

namespace RogueLike.Code.Tests.Systems;

[TestSuite]
public class ExperienceSystemTest
{
    private ExperienceSystem _exp;

    [BeforeTest]
    public void Setup()
    {
        _exp = new ExperienceSystem();
    }

    [TestCase]
    public void InitialState_IsCorrect()
    {
        Assertions.AssertThat(_exp.CurrentLevel).IsEqual(1);
        Assertions.AssertThat(_exp.CurrentXP).IsEqual(0);
        Assertions.AssertThat(_exp.XPForNextLevel).IsEqual(100);
    }

    [TestCase]
    public void AddXP_NoLevelUp()
    {
        _exp.AddXP(50);
        Assertions.AssertThat(_exp.CurrentLevel).IsEqual(1);
        Assertions.AssertThat(_exp.CurrentXP).IsEqual(50);
    }

    [TestCase]
    public void AddXP_ExactLevelUp()
    {
        _exp.AddXP(100);
        Assertions.AssertThat(_exp.CurrentLevel).IsEqual(2);
        Assertions.AssertThat(_exp.CurrentXP).IsEqual(0);
        Assertions.AssertThat(_exp.XPForNextLevel).IsEqual(200);
    }

    [TestCase]
    public void AddXP_LevelUpWithCarryOver()
    {
        _exp.AddXP(150);
        Assertions.AssertThat(_exp.CurrentLevel).IsEqual(2);
        Assertions.AssertThat(_exp.CurrentXP).IsEqual(50);
        Assertions.AssertThat(_exp.XPForNextLevel).IsEqual(200);
    }

    [TestCase]
    public void AddXP_MultiLevelUp()
    {
        // Level 1 needs 100 XP. Level 2 needs 200 XP. Total: 300 XP.
        // We add 350 XP.
        _exp.AddXP(350);
        Assertions.AssertThat(_exp.CurrentLevel).IsEqual(3);
        Assertions.AssertThat(_exp.CurrentXP).IsEqual(50);
        Assertions.AssertThat(_exp.XPForNextLevel).IsEqual(300);
    }

    [TestCase]
    public void Events_OnXPChanged_FiresCorrectly()
    {
        int receivedCurrentXP = -1;
        int receivedXPForNext = -1;

        _exp.OnXPChanged += (current, next) =>
        {
            receivedCurrentXP = current;
            receivedXPForNext = next;
        };

        _exp.AddXP(50);

        Assertions.AssertThat(receivedCurrentXP).IsEqual(50);
        Assertions.AssertThat(receivedXPForNext).IsEqual(100);
    }

    [TestCase]
    public void Events_OnLevelUp_FiresCorrectly()
    {
        int receivedNewLevel = 0;
        _exp.OnLevelUp += (newLevel) =>
        {
            receivedNewLevel = newLevel;
        };
        
        _exp.AddXP(100);

        Assertions.AssertThat(receivedNewLevel).IsEqual(2);
    }
}
