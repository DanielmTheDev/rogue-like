using System;

namespace RogueLike.Code.Systems;

/// <summary>
/// A pure C# class to manage experience points, levels, and player progression.
/// It is decoupled from any Godot nodes and can be unit-tested independently.
/// </summary>
public class ExperienceSystem
{
    public int CurrentLevel { get; private set; }
    public int CurrentXP { get; private set; }
    public int XPForNextLevel { get; private set; }

    public event Action<int, int> OnXPChanged; // currentXP, xpForNextLevel
    public event Action<int> OnLevelUp; // newLevel

    private const int BaseXP = 100;

    public ExperienceSystem()
    {
        CurrentLevel = 1;
        CurrentXP = 0;
        XPForNextLevel = CalculateXPForLevel(CurrentLevel);
    }

    /// <summary>
    /// Adds experience points and checks for level-ups.
    /// </summary>
    public void AddXP(int amount)
    {
        if (amount <= 0) return;

        CurrentXP += amount;
        OnXPChanged?.Invoke(CurrentXP, XPForNextLevel);

        while (CurrentXP >= XPForNextLevel)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        CurrentXP -= XPForNextLevel;
        CurrentLevel++;
        XPForNextLevel = CalculateXPForLevel(CurrentLevel);
        
        OnLevelUp?.Invoke(CurrentLevel);
        OnXPChanged?.Invoke(CurrentXP, XPForNextLevel);
    }

    /// <summary>
    /// Calculates the XP required for a given level.
    /// The formula makes each level require more XP than the last.
    /// </summary>
    private int CalculateXPForLevel(int level)
    {
        // Simple scaling formula: Level 1 -> 100XP, Level 2 -> 200XP, etc.
        return level * BaseXP;
    }
}
