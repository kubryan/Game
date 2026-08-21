using Godot;

public partial class FloatingText : Node2D
{
    public string Text { get; set; } = string.Empty;
    public Color TextColor { get; set; } = GameBalance.DamageTextColor;
    public Vector2 TextOffset { get; set; } = Vector2.Zero;

    private double _life = GameBalance.FloatingTextDuration;
    private double _maxLife = GameBalance.FloatingTextDuration;
    private Label _label = null!;

    public override void _Ready()
    {
        _label = new Label
        {
            Text = Text,
            Position = new Vector2(-72, -18) + TextOffset,
            Size = new Vector2(144, 34),
            HorizontalAlignment = HorizontalAlignment.Center,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        _label.AddThemeFontSizeOverride("font_size", GameBalance.FloatingTextFontSize);
        _label.AddThemeColorOverride("font_color", TextColor);
        _label.AddThemeColorOverride("font_shadow_color", new Color("#211734", 0.9f));
        _label.AddThemeConstantOverride("shadow_offset_x", 2);
        _label.AddThemeConstantOverride("shadow_offset_y", 2);
        AddChild(_label);
    }

    public override void _Process(double delta)
    {
        _life -= delta;
        if (_life <= 0)
        {
            QueueFree();
            return;
        }

        GlobalPosition += Vector2.Up * GameBalance.FloatingTextRiseSpeed * (float)delta;
        float progress = 1f - (float)(_life / _maxLife);
        _label.Modulate = new Color(1f, 1f, 1f, Mathf.Clamp(1f - progress * 1.35f, 0f, 1f));
    }
}
