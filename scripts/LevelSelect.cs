using Godot;
using System.Collections.Generic;

public partial class LevelSelect : Control
{
    private ProgressManager _progress = null!;
    private Label _progressLabel = null!;
    private HBoxContainer _cards = null!;
    private Texture2D? _backgroundTexture;

    public override void _Ready()
    {
        _progress = GetNode<ProgressManager>("/root/ProgressManager");
        _backgroundTexture = GD.Load<Texture2D>("res://assets/frosting_forest_visual_target_1280.png");
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        BuildInterface();
        RefreshCards();
        QueueRedraw();
    }

    private void BuildInterface()
    {
        VBoxContainer layout = new()
        {
            Position = new Vector2(46, 34),
            Size = new Vector2(1188, 650),
        };
        AddChild(layout);

        Label title = new()
        {
            Text = "腐化童話：夜守者",
            CustomMinimumSize = new Vector2(0, 52),
        };
        title.AddThemeFontSizeOverride("font_size", 34);
        title.AddThemeColorOverride("font_color", new Color("#fff1b8"));
        layout.AddChild(title);

        Label subtitle = new()
        {
            Text = "選擇要踏入的夜晚。完成前一關後，下一段童話才會亮起。",
            CustomMinimumSize = new Vector2(0, 36),
        };
        subtitle.AddThemeFontSizeOverride("font_size", 17);
        subtitle.AddThemeColorOverride("font_color", new Color("#eadcf3"));
        layout.AddChild(subtitle);

        _cards = new HBoxContainer
        {
            CustomMinimumSize = new Vector2(0, 455),
            SizeFlagsVertical = SizeFlags.ExpandFill,
        };
        _cards.AddThemeConstantOverride("separation", 14);
        layout.AddChild(_cards);

        _progressLabel = new Label
        {
            CustomMinimumSize = new Vector2(0, 34),
        };
        _progressLabel.AddThemeFontSizeOverride("font_size", 16);
        _progressLabel.AddThemeColorOverride("font_color", new Color("#ffe7b0"));
        layout.AddChild(_progressLabel);

        Label hint = new()
        {
            Text = "完成關卡後按 F2 可以重新挑戰；目前的鍵位設定會跨關卡保存。",
            CustomMinimumSize = new Vector2(0, 28),
        };
        hint.AddThemeColorOverride("font_color", new Color("#d7c6e9"));
        layout.AddChild(hint);
    }

    private void RefreshCards()
    {
        foreach (Node child in _cards.GetChildren())
            child.QueueFree();

        foreach (LevelDefinition definition in LevelCatalog.All)
        {
            bool unlocked = _progress.IsUnlocked(definition.Id);
            Button card = CreateCard(definition, unlocked);
            _cards.AddChild(card);
        }

        _progressLabel.Text = $"已解鎖 {_progress.HighestUnlockedLevel} / {LevelCatalog.All.Count}    ·    點擊已亮起的區域開始，已完成區域可重玩";
    }

    private Button CreateCard(LevelDefinition definition, bool unlocked)
    {
        Button card = new()
        {
            CustomMinimumSize = new Vector2(220, 420),
            Text = unlocked
                ? $"{definition.Id}\n\n{definition.Title}\n{definition.Subtitle}\n\n{definition.Description}\n\n{(definition.Id < _progress.HighestUnlockedLevel ? "已完成 · 可重玩" : "尚未完成")}"
                : $"{definition.Id}\n\n尚未解鎖\n\n完成前一個區域後\n這裡才會亮起",
            Disabled = !unlocked,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            Alignment = HorizontalAlignment.Center,
            VerticalIconAlignment = VerticalAlignment.Center,
        };
        card.AddThemeFontSizeOverride("font_size", 17);
        card.AddThemeColorOverride("font_color", unlocked ? new Color("#fff1b8") : new Color("#867d99"));
        card.AddThemeStyleboxOverride("normal", MakeCardStyle(definition.Accent, unlocked, false));
        card.AddThemeStyleboxOverride("hover", MakeCardStyle(definition.Accent, unlocked, true));
        card.AddThemeStyleboxOverride("disabled", MakeCardStyle(definition.Accent, false, false));
        if (unlocked)
            card.Pressed += () => SelectLevel(definition.Id);
        return card;
    }

    private void SelectLevel(int levelId)
    {
        if (!_progress.SelectLevel(levelId))
            return;
        GetTree().ChangeSceneToFile("res://scenes/Main.tscn");
    }

    private static StyleBoxFlat MakeCardStyle(Color accent, bool unlocked, bool hovered)
    {
        Color background = unlocked
            ? new Color("#2d2147", hovered ? 0.96f : 0.88f)
            : new Color("#171323", 0.78f);
        StyleBoxFlat style = new()
        {
            BgColor = background,
            BorderColor = unlocked ? new Color(accent, hovered ? 0.95f : 0.62f) : new Color("#4d455d", 0.72f),
            BorderWidthLeft = hovered ? 3 : 2,
            BorderWidthTop = hovered ? 3 : 2,
            BorderWidthRight = hovered ? 3 : 2,
            BorderWidthBottom = hovered ? 3 : 2,
            CornerRadiusTopLeft = 18,
            CornerRadiusTopRight = 18,
            CornerRadiusBottomLeft = 18,
            CornerRadiusBottomRight = 18,
            ContentMarginLeft = 14,
            ContentMarginTop = 18,
            ContentMarginRight = 14,
            ContentMarginBottom = 18,
        };
        return style;
    }

    public override void _Draw()
    {
        if (_backgroundTexture != null)
            DrawTextureRect(_backgroundTexture, new Rect2(0, 0, 1280, 720), false);
        else
            DrawRect(new Rect2(0, 0, 1280, 720), new Color("#211734"));

        DrawRect(new Rect2(0, 0, 1280, 720), new Color("#160f27", 0.55f));
        DrawCircle(new Vector2(640, 365), 260f, new Color("#ffe09a", 0.07f));
    }
}
