using Godot;

public partial class ThornAura : Node2D
{
    public float Radius { get; set; } = GameBalance.ThornBloomRange;

    private double _life = GameBalance.ThornBloomAuraDuration;

    public override void _Ready()
    {
        AddToGroup("thorn_auras");
        QueueRedraw();
    }

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

    public bool Contains(Vector2 position)
    {
        return GlobalPosition.DistanceTo(position) <= Radius;
    }

    public override void _Draw()
    {
        float progress = Mathf.Clamp((float)(_life / GameBalance.ThornBloomAuraDuration), 0f, 1f);
        float pulse = 1f + Mathf.Sin((float)Time.GetTicksMsec() * 0.006f) * 0.04f;
        Color auraColor = new Color("#9de89b");
        DrawCircle(Vector2.Zero, Radius * pulse, new Color(auraColor, 0.08f * progress));
        DrawArc(Vector2.Zero, Radius * pulse, 0, Mathf.Tau, 64, new Color(auraColor, 0.7f * progress), 3f);
        DrawArc(Vector2.Zero, Radius * 0.82f, 0.4f, 2.4f, 20, new Color("#d9ffc5", 0.65f * progress), 2f);
    }
}
