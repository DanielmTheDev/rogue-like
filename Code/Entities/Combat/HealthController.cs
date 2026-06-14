using System;

namespace RogueLike.Code.Entities.Combat;

/// <summary>
/// Pure C# class managing health state and events.
/// </summary>
public class HealthController
{
    public int MaxHp { get; private set; }
    public int CurrentHp { get; private set; }

    public event Action<int, int> OnHealthChanged;
    public event Action OnDied;

    public HealthController(int maxHp)
    {
        MaxHp = maxHp;
        CurrentHp = maxHp;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;

        CurrentHp -= amount;
        if (CurrentHp < 0) CurrentHp = 0;

        OnHealthChanged?.Invoke(CurrentHp, MaxHp);

        if (CurrentHp == 0)
        {
            OnDied?.Invoke();
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || CurrentHp <= 0) return; // Cannot heal dead entities

        CurrentHp += amount;
        if (CurrentHp > MaxHp) CurrentHp = MaxHp;

        OnHealthChanged?.Invoke(CurrentHp, MaxHp);
    }

    public void IncreaseMaxHp(int amount)
    {
        if (amount <= 0) return;
        MaxHp += amount;
        Heal(MaxHp); // Heal to full
    }
}
