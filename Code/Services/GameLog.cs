using System;
using System.Collections.Generic;

namespace RogueLike.Code.Services;

/// <summary>
/// Pure C# service for managing the game's message log history.
/// Follows the decoupling rule: logic classes push messages here, 
/// and UI components listen for changes.
/// </summary>
public class GameLog
{
    private static GameLog _instance;
    public static GameLog Instance => _instance ??= new GameLog();

    public event Action<string> OnMessageLogged;
    public event Action OnLogCleared;

    private readonly List<string> _history = [];
    public IReadOnlyList<string> History => _history;

    public void Clear()
    {
        _history.Clear();
        OnLogCleared?.Invoke();
    }

    public void Log(string message)
    {
        _history.Add(message);
        OnMessageLogged?.Invoke(message);
    }

    public void LogCombat(string attacker, string target, int damage)
    {
        var color = attacker == "Player" ? "yellow" : "red";
        Log($"[color={color}]{attacker}[/color] hits [color=white]{target}[/color] for [color=orange]{damage}[/color] damage.");
    }

    public void LogDeath(string name)
    {
        Log($"[color=gray]The {name} dies.[/color]");
    }
}
