using Godot;

public partial class Projectile : Node2D
{
    public Enemy? Target { get; set; }
    public float Damage { get; set; } = 10f;
    public float TravelSpeed { get; set; } = 420f;
    public float Radius { get; set; } = 5f;
    public float ImpactSlowMultiplier { get; set; } = 1f;
    public float ImpactSlowDuration { get; set; }
    public Color ProjectileColor { get; set; } = Colors.White;

        private double _lifeRemaining = 2.5;

    public override void _Ready()
    {
        AddToGroup("projectiles");
        QueueRedraw();
    }

    public override void _Process(double delta)

    {
        _lifeRemaining -= delta;
        if (_lifeRemaining <= 0 || Target == null || !IsInstanceValid(Target))
        {
            QueueFree();
            return;
        }

        Vector2 direction = GlobalPosition.DirectionTo(Target.GlobalPosition);
        GlobalPosition += direction * TravelSpeed * (float)delta;
                if (Target.HasReachedCore)
        {
            QueueFree();
            return;
        }

        if (GlobalPosition.DistanceTo(Target.GlobalPosition) <= Radius + 16f)
        {
            Target.TakeDamage(Damage);
            if (ImpactSlowDuration > 0 && IsInstanceValid(Target) && !Target.HasReachedCore)
                Target.ApplySlow(ImpactSlowMultiplier, ImpactSlowDuration);
            QueueFree();
        }

        QueueRedraw();
    }

    public override void _Draw()
    {
        DrawCircle(Vector2.Zero, Radius + 4f, new Color(ProjectileColor, 0.22f));
        DrawCircle(Vector2.Zero, Radius, ProjectileColor);
    }
}
