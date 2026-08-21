using Godot;
using System.Collections.Generic;

public static class GameBalance
{
    public const ulong RandomSeed = 87321;
    public const int ViewportWidth = 1280;
    public const int ViewportHeight = 720;
    public const int HeaderHeight = 86;
    public const int FooterY = 665;
    public const int FloatingTextFontSize = 18;
    public const float FloatingTextRiseSpeed = 36f;
    public const double FloatingTextDuration = 0.78;
    public const double WarningMessageDuration = 2.2;
    public const double TutorialDuration = 8.0;
    public const float ThreeStarCoreHealthRatio = 0.75f;
    public const float TwoStarCoreHealthRatio = 0.45f;
    public const int SpellCount = 4;
    public const int TowerTypeCount = 4;

    public const float PlayerMaxMana = 100f;
    public const float PlayerMoveSpeed = 210f;
    public const float PlayerAutoAttackRange = 260f;
    public const float PlayerAutoAttackDamage = 12f;
    public const double PlayerAutoAttackCooldown = 0.62;
    public const float PlayerManaRegenPerSecond = 7f;

    public const float EmberMissileRange = 420f;
    public const float EmberMissileDamage = 34f;
    public const float EmberMissileTravelSpeed = 610f;
    public const float EmberMissileRadius = 9f;
    public const float FrostPrisonRange = 170f;
    public const float FrostPrisonDamage = 12f;
    public const float FrostPrisonSlowMultiplier = 0.35f;
    public const float FrostPrisonSlowDuration = 4.5f;
    public const float ThunderJudgementRange = 430f;
    public const float ThunderJudgementDamage = 68f;
    public const float ThunderJudgementSplashRange = 105f;
    public const float ThunderJudgementSplashDamage = 24f;
    public const float ThornBloomRange = 145f;
    public const float ThornBloomDamage = 26f;
    public const float ThornBloomSlowMultiplier = 0.6f;
    public const float ThornBloomSlowDuration = 2.5f;
    public const double ThornBloomAuraDuration = 3.5;
    public const float FireDamageVsFrozenMultiplier = 1.5f;
    public const float NatureCooldownInsideThornMultiplier = 0.65f;

    public static readonly float[] SpellCosts = { 18f, 26f, 34f, 22f };
    public static readonly double[] SpellCooldowns = { 0.8, 5.5, 8.0, 6.0 };

    public const int StartingEssence = 180;
    public const int TowerBuildCost = 40;
    public const int EssencePerEnemy = 14;
    public const float StartingCoreHealth = 100f;
    public const float CoreBuildRadius = 82f;
    public const float BuildAreaMinY = 100f;
    public const float BuildAreaMaxY = 625f;

    public const int BaseEnemiesPerWave = 5;
    public const int AdditionalEnemiesPerWave = 2;
    public const float BaseEnemyHealth = 28f;
    public const float EnemyHealthPerWave = 8f;
    public const float BaseEnemySpeed = 35f;
    public const float EnemySpeedPerWave = 2.5f;
    public const float BaseEnemyDamageToCore = 8f;
    public const float InitialSpawnDelay = 0.6f;
    public const float SpawnIntervalStart = 1.25f;
    public const float SpawnIntervalFloor = 0.38f;
    public const float SpawnIntervalReductionPerWave = 0.08f;
    public const float FrostSlowMultiplier = 0.65f;
    public const float FrostSlowDuration = 1.2f;
    public const float NatureSlowMultiplier = 0.82f;
    public const float NatureSlowDuration = 1.5f;
    public static readonly Color HudTextColor = new("#d7c6e9");
    public static readonly Color MessageTextColor = new("#ffe7b0");
    public static readonly Color DamageTextColor = new("#fff1b8");
    public static readonly Color EssenceTextColor = new("#b8ffcf");
    public static readonly Color WarningColor = new("#ff6b8a");
    public const double InitialWaveDelay = 1.5;
    public const double InterWaveDelay = 3.2;

    public static readonly Vector2[] SpawnPoints =
    {
        new(1120, 165),
        new(1120, 290),
        new(1120, 470),
        new(1120, 575),
    };
}

public sealed class LevelDefinition
{
    public int Id { get; }
    public string Title { get; }
    public string Subtitle { get; }
    public string Description { get; }
    public Color Accent { get; }
    public int Waves { get; }
    public float EnemyHealthBonus { get; }
    public float EnemySpeedBonus { get; }
    public float CoreDamageBonus { get; }
    public IReadOnlyList<Vector2> SpawnPoints { get; }
    public float StartingCoreHealth { get; }
    public int StartingEssence { get; }
    public int EliteWave { get; }
    public float EliteHealthMultiplier { get; }
    public float EliteSpeedMultiplier { get; }
    public float EliteDamageToCoreBonus { get; }
    public int EliteEssenceReward { get; }

    public LevelDefinition(
        int id,
        string title,
        string subtitle,
        string description,
        Color accent,
        int waves,
        float enemyHealthBonus,
        float enemySpeedBonus,
        IReadOnlyList<Vector2> spawnPoints,
        float coreDamageBonus = 0f,
        float startingCoreHealth = GameBalance.StartingCoreHealth,
        int startingEssence = GameBalance.StartingEssence,
        int eliteWave = 0,
        float eliteHealthMultiplier = 1f,
        float eliteSpeedMultiplier = 1f,
        float eliteDamageToCoreBonus = 0f,
        int eliteEssenceReward = GameBalance.EssencePerEnemy)
    {
        Id = id;
        Title = title;
        Subtitle = subtitle;
        Description = description;
        Accent = accent;
        Waves = waves;
        EnemyHealthBonus = enemyHealthBonus;
        EnemySpeedBonus = enemySpeedBonus;
        SpawnPoints = spawnPoints;
        CoreDamageBonus = coreDamageBonus;
        StartingCoreHealth = startingCoreHealth;
        StartingEssence = startingEssence;
        EliteWave = eliteWave;
        EliteHealthMultiplier = eliteHealthMultiplier;
        EliteSpeedMultiplier = eliteSpeedMultiplier;
        EliteDamageToCoreBonus = eliteDamageToCoreBonus;
        EliteEssenceReward = eliteEssenceReward;
    }
}

public static class LevelCatalog
{
    private static readonly IReadOnlyList<LevelDefinition> Definitions = new List<LevelDefinition>
    {
        new(
            1,
            "糖霜森林",
            "腐化初醒",
            "在還沒完全融化的糖霜森林守住希望篝火。",
            new Color("#ffd36e"),
            6,
            0f,
            0f,
            GameBalance.SpawnPoints,
            0f,
            100f,
            180,
            0),
        new(
            2,
            "倒影湖",
            "水面裂成兩半",
            "湖水映出不屬於童話的影子，敵人開始從側翼逼近。",
            new Color("#9be5ff"),
            7,
            10f,
            4f,
            new[]
            {
                new Vector2(1160, 130),
                new Vector2(1080, 235),
                new Vector2(1080, 495),
                new Vector2(1160, 610),
            },
            2f,
            95f,
            165,
            4,
            2.1f,
            1.1f,
            2f,
            28),
        new(
            3,
            "紙傘竹林",
            "風裡有人唱歌",
            "紙傘與竹影會交換位置，妖怪的路線變得難以預測。",
            new Color("#c69bff"),
            8,
            22f,
            7f,
            new[]
            {
                new Vector2(1140, 180),
                new Vector2(1060, 330),
                new Vector2(1140, 540),
            },
            4f,
            105f,
            190,
            5,
            2.2f,
            1.12f,
            3f,
            32),
        new(
            4,
            "失眠城堡",
            "鐘聲沒有停過",
            "沉睡的塔樓與不肯閉眼的縫合妖怪一起守望黑夜。",
            new Color("#ff9dca"),
            9,
            38f,
            10f,
            new[]
            {
                new Vector2(1180, 150),
                new Vector2(1100, 275),
                new Vector2(1100, 455),
                new Vector2(1180, 600),
            },
            7f,
            90f,
            175,
            6,
            2.4f,
            1.15f,
            4f,
            40),
        new(
            5,
            "腐化王國",
            "童話的最後一頁",
            "所有腐化力量匯聚於王城，活下來並迎接最後的夜幕。",
            new Color("#a875ff"),
            10,
            58f,
            13f,
            new[]
            {
                new Vector2(1180, 115),
                new Vector2(1100, 205),
                new Vector2(1060, 365),
                new Vector2(1100, 525),
                new Vector2(1180, 635),
            },
            12f,
            110f,
            210,
            7,
            2.6f,
            1.18f,
            6f,
            50),
    };

    public static IReadOnlyList<LevelDefinition> All => Definitions;

    public static LevelDefinition Get(int id)
    {
        foreach (LevelDefinition definition in Definitions)
        {
            if (definition.Id == id)
                return definition;
        }

        return Definitions[0];
    }
}
