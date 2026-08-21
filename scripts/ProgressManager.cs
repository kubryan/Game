using Godot;

public partial class ProgressManager : Node
{
    [Signal]
    public delegate void ProgressChangedEventHandler(int highestUnlockedLevel);

    private const string SavePath = "user://fairy_corruption_progress.cfg";
    private const string Section = "progress";

    public int HighestUnlockedLevel { get; private set; } = 1;
    public int SelectedLevel { get; private set; } = 1;

    public override void _Ready()
    {
        LoadProgress();
    }

    public bool IsUnlocked(int levelId)
    {
        return levelId >= 1 && levelId <= HighestUnlockedLevel && levelId <= LevelCatalog.All.Count;
    }

    public bool SelectLevel(int levelId)
    {
        if (!IsUnlocked(levelId))
            return false;

        SelectedLevel = levelId;
        SaveProgress();
        return true;
    }

    public void CompleteLevel(int levelId)
    {
        if (levelId < 1 || levelId > LevelCatalog.All.Count)
            return;

        if (levelId >= HighestUnlockedLevel && HighestUnlockedLevel < LevelCatalog.All.Count)
            HighestUnlockedLevel = levelId + 1;

        SelectedLevel = levelId;
        SaveProgress();
        EmitSignal(SignalName.ProgressChanged, HighestUnlockedLevel);
    }

    public void ResetProgress()
    {
        HighestUnlockedLevel = 1;
        SelectedLevel = 1;
        SaveProgress();
        EmitSignal(SignalName.ProgressChanged, HighestUnlockedLevel);
    }

    private void LoadProgress()
    {
        ConfigFile config = new();
        if (config.Load(SavePath) != Error.Ok)
        {
            SaveProgress();
            return;
        }

        HighestUnlockedLevel = Mathf.Clamp(
            (int)config.GetValue(Section, "highest_unlocked_level", 1),
            1,
            LevelCatalog.All.Count);
        SelectedLevel = Mathf.Clamp(
            (int)config.GetValue(Section, "selected_level", 1),
            1,
            HighestUnlockedLevel);
    }

    private void SaveProgress()
    {
        ConfigFile config = new();
        config.SetValue(Section, "highest_unlocked_level", HighestUnlockedLevel);
        config.SetValue(Section, "selected_level", SelectedLevel);
        config.Save(SavePath);
    }
}
