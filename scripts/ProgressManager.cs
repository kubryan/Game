using Godot;
using System;

public partial class ProgressManager : Node
{
    [Signal]
    public delegate void ProgressChangedEventHandler(int highestUnlockedLevel);

    private const string SavePath = "user://fairy_corruption_progress.cfg";
    private const string Section = "progress";

    private int[] _bestStars = Array.Empty<int>();
    private float[] _bestCoreHealthRatios = Array.Empty<float>();
    private double[] _bestTimes = Array.Empty<double>();

    public int HighestUnlockedLevel { get; private set; } = 1;
    public int SelectedLevel { get; private set; } = 1;

    public override void _Ready()
    {
        EnsureBestScoreStorage();
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

    public bool RecordBestScore(int levelId, int stars, float coreHealthRatio, double timeSeconds)
    {
        if (levelId < 1 || levelId > LevelCatalog.All.Count)
            return false;

        EnsureBestScoreStorage();
        int index = levelId - 1;
        bool improved = false;
        int clampedStars = Mathf.Clamp(stars, 0, 3);
        float clampedCoreRatio = Mathf.Clamp(coreHealthRatio, 0f, 1f);

        if (clampedStars > _bestStars[index])
        {
            _bestStars[index] = clampedStars;
            improved = true;
        }

        if (clampedCoreRatio > _bestCoreHealthRatios[index])
        {
            _bestCoreHealthRatios[index] = clampedCoreRatio;
            improved = true;
        }

        if (timeSeconds > 0 && (_bestTimes[index] <= 0 || timeSeconds < _bestTimes[index]))
        {
            _bestTimes[index] = timeSeconds;
            improved = true;
        }

        if (improved)
            SaveProgress();

        return improved;
    }

    public bool HasBestScore(int levelId)
    {
        return IsValidLevel(levelId) && _bestStars[levelId - 1] > 0;
    }

    public int GetBestStars(int levelId)
    {
        return IsValidLevel(levelId) ? _bestStars[levelId - 1] : 0;
    }

    public float GetBestCoreHealthRatio(int levelId)
    {
        return IsValidLevel(levelId) ? _bestCoreHealthRatios[levelId - 1] : 0f;
    }

    public double GetBestTimeSeconds(int levelId)
    {
        return IsValidLevel(levelId) ? _bestTimes[levelId - 1] : 0;
    }

    public void ResetProgress()
    {
        EnsureBestScoreStorage();
        HighestUnlockedLevel = 1;
        SelectedLevel = 1;
        Array.Clear(_bestStars, 0, _bestStars.Length);
        Array.Clear(_bestCoreHealthRatios, 0, _bestCoreHealthRatios.Length);
        Array.Clear(_bestTimes, 0, _bestTimes.Length);
        SaveProgress();
        EmitSignal(SignalName.ProgressChanged, HighestUnlockedLevel);
    }

    private void LoadProgress()
    {
        EnsureBestScoreStorage();
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

        for (int levelId = 1; levelId <= LevelCatalog.All.Count; levelId++)
        {
            int index = levelId - 1;
            _bestStars[index] = Mathf.Clamp(
                (int)config.GetValue(Section, $"level_{levelId}_best_stars", 0),
                0,
                3);
            _bestCoreHealthRatios[index] = Mathf.Clamp(
                config.GetValue(Section, $"level_{levelId}_best_core_ratio", 0f).As<float>(),
                0f,
                1f);
            _bestTimes[index] = Math.Max(
                0,
                config.GetValue(Section, $"level_{levelId}_best_time", 0d).As<double>());
        }
    }

    private void SaveProgress()
    {
        EnsureBestScoreStorage();
        ConfigFile config = new();
        config.SetValue(Section, "highest_unlocked_level", HighestUnlockedLevel);
        config.SetValue(Section, "selected_level", SelectedLevel);

        for (int levelId = 1; levelId <= LevelCatalog.All.Count; levelId++)
        {
            int index = levelId - 1;
            config.SetValue(Section, $"level_{levelId}_best_stars", _bestStars[index]);
            config.SetValue(Section, $"level_{levelId}_best_core_ratio", _bestCoreHealthRatios[index]);
            config.SetValue(Section, $"level_{levelId}_best_time", _bestTimes[index]);
        }

        config.Save(SavePath);
    }

    private void EnsureBestScoreStorage()
    {
        int levelCount = LevelCatalog.All.Count;
        if (_bestStars.Length == levelCount)
            return;

        _bestStars = new int[levelCount];
        _bestCoreHealthRatios = new float[levelCount];
        _bestTimes = new double[levelCount];
    }

    private bool IsValidLevel(int levelId)
    {
        return levelId >= 1
            && levelId <= LevelCatalog.All.Count
            && levelId <= _bestStars.Length;
    }
}
