using Godot;
using RogueLike.Code.Domain.Flow;

namespace RogueLike.Code.View.UI;

public partial class ExperienceUI : Control
{
    [Export] public Label LevelLabel;
    [Export] public ProgressBar XpBar;

    private ExperienceSystem _expSystem;

    public void Initialize(ExperienceSystem expSystem)
    {
        _expSystem = expSystem;
        _expSystem.OnLevelUp += UpdateDisplay;
        _expSystem.OnXPChanged += UpdateDisplay;
        UpdateDisplay(_expSystem.CurrentLevel, _expSystem.CurrentXP, _expSystem.XPForNextLevel);
    }

    private void UpdateDisplay(int newLevel)
    {
        UpdateDisplay(newLevel, _expSystem.CurrentXP, _expSystem.XPForNextLevel);
    }

    private void UpdateDisplay(int currentXP, int xpForNextLevel)
    {
        UpdateDisplay(_expSystem.CurrentLevel, currentXP, xpForNextLevel);
    }

    private void UpdateDisplay(int level, int currentXP, int xpForNextLevel)
    {
        if (LevelLabel == null || XpBar == null) return;

        LevelLabel.Text = $"Level: {level}";
        XpBar.MaxValue = xpForNextLevel;
        XpBar.Value = currentXP;
    }

    public override void _ExitTree()
    {
        if (_expSystem != null)
        {
            _expSystem.OnLevelUp -= UpdateDisplay;
            _expSystem.OnXPChanged -= UpdateDisplay;
        }
    }
}
