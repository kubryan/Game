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

    public LevelDefinition(
        int id,
        string title,
        string subtitle,
        string description,
        Color accent,
        int waves,
        float enemyHealthBonus,
        float enemySpeedBonus,
        float coreDamageBonus = 0f)
    {
        Id = id;
        Title = title;
        Subtitle = subtitle;
        Description = description;
        Accent = accent;
        Waves = waves;
        EnemyHealthBonus = enemyHealthBonus;
        EnemySpeedBonus = enemySpeedBonus;
        CoreDamageBonus = coreDamageBonus;
    }
}

public static class LevelCatalog
{
    private static readonly IReadOnlyList<LevelDefinition> Definitions = new List<LevelDefinition>
    {
        new(1, "糖霜森林", "腐化初醒", "在還沒完全融化的糖霜森林守住希望篝火。", new Color("#ffd36e"), 6, 0f, 0f, 0f),
        new(2, "倒影湖", "水面裂成兩半", "湖水映出不屬於童話的影子，敵人開始從側翼逼近。", new Color("#9be5ff"), 7, 10f, 4f, 2f),
        new(3, "紙傘竹林", "風裡有人唱歌", "紙傘與竹影會交換位置，妖怪的路線變得難以預測。", new Color("#c69bff"), 8, 22f, 7f, 4f),
        new(4, "失眠城堡", "鐘聲沒有停過", "沉睡的塔樓與不肯閉眼的縫合妖怪一起守望黑夜。", new Color("#ff9dca"), 9, 38f, 10f, 7f),
        new(5, "腐化王國", "童話的最後一頁", "所有腐化力量匯聚於王城，活下來並迎接最後的夜幕。", new Color("#a875ff"), 10, 58f, 13f, 12f),
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
