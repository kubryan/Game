using Godot;
using System.Collections.Generic;

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

    public LevelDefinition(int id, string title, string subtitle, string description, Color accent, int waves, float enemyHealthBonus, float enemySpeedBonus)
    {
        Id = id;
        Title = title;
        Subtitle = subtitle;
        Description = description;
        Accent = accent;
        Waves = waves;
        EnemyHealthBonus = enemyHealthBonus;
        EnemySpeedBonus = enemySpeedBonus;
    }
}

public static class LevelCatalog
{
    private static readonly IReadOnlyList<LevelDefinition> Definitions = new List<LevelDefinition>
    {
        new(1, "糖霜森林", "腐化初醒", "在還沒完全融化的糖霜森林守住希望篝火。", new Color("#ffd36e"), 6, 0f, 0f),
        new(2, "倒影湖", "水面裂成兩半", "湖水映出不屬於童話的影子，敵人開始從側翼逼近。", new Color("#9be5ff"), 7, 10f, 4f),
        new(3, "紙傘竹林", "風裡有人唱歌", "紙傘與竹影會交換位置，妖怪的路線變得難以預測。", new Color("#c69bff"), 8, 22f, 7f),
        new(4, "失眠城堡", "鐘聲沒有停過", "沉睡的塔樓與不肯閉眼的縫合妖怪一起守望黑夜。", new Color("#ff9dca"), 9, 38f, 10f),
        new(5, "腐化王國", "童話的最後一頁", "所有腐化力量匯聚於王城，活下來並迎接最後的夜幕。", new Color("#a875ff"), 10, 58f, 13f),
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
