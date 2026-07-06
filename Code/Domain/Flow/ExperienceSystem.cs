using System;

namespace RogueLike.Domain.Flow;

/// <summary>
/// A pure C# class to manage experience points, levels, and player progression.
/// It is decoupled from any Godot nodes and can be unit-tested independently.
/// </summary>
public class ExperienceSystem
{
    public int CurrentLevel { get; private set; } = 1;
    public int CurrentXP { get; private set; }

    // Derived from the level (Level 1 -> 100 XP, Level 2 -> 200 XP, ...), so it never needs syncing.
    public int XPForNextLevel => CurrentLevel * BaseXP;

    public event Action<int, int> OnXPChanged; // currentXP, xpForNextLevel
    public event Action<int> OnLevelUp; // newLevel

    private const int BaseXP = 100;

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

    /// <summary>Reset progression to a fresh run (Level 1, 0 XP), raising the change event so the UI refreshes.</summary>
    public void Reset()
    {
        CurrentLevel = 1;
        CurrentXP = 0;
        OnXPChanged?.Invoke(CurrentXP, XPForNextLevel);
    }

    private void LevelUp()
    {
        CurrentXP -= XPForNextLevel; // reads the current level's cost before the level increments
        CurrentLevel++;

        OnLevelUp?.Invoke(CurrentLevel);
        OnXPChanged?.Invoke(CurrentXP, XPForNextLevel);
    }
}
