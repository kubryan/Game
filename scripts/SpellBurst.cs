using Godot;

public partial class SpellBurst : Node2D
{
    public Color BurstColor { get; set; } = Colors.White;
    public float BurstRadius { get; set; } = 120f;

    private double _life = 0.42;
    private double _maxLife = 0.42;

    public override void _Process(double delta)
    {
        _life -= delta;
        if (_life <= 0)
        {
            QueueFree();
            return;
        }
        QueueRedraw();
    }

    public override void _Draw()
    {
        float progress = 1f - (float)(_life / _maxLife);
        float radius = Mathf.Lerp(18f, BurstRadius, progress);
        float alpha = Mathf.Lerp(0.68f, 0f, progress);
        DrawCircle(Vector2.Zero, radius, new Color(BurstColor, alpha * 0.18f));
        DrawArc(Vector2.Zero, radius, 0, Mathf.Tau, 48, new Color(BurstColor, alpha), 5f);
        DrawArc(Vector2.Zero, radius * 0.7f, progress, progress + Mathf.Pi * 1.4f, 24, new Color(Colors.White, alpha * 0.85f), 2f);
    }
}
