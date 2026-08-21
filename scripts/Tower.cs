using Godot;

public partial class Tower : Node2D
{
    public enum TowerType
    {
        Fire,
        Frost,
        Thunder,
        Nature,
    }

    public TowerType Type { get; set; } = TowerType.Fire;
    public float Range { get; set; } = 175f;
    public float Damage { get; set; } = 8f;
    public double Cooldown { get; set; } = 1.25;

    private double _timer;
    private bool _combatActive = true;
    private Color _color = new("#ff9d70");

    public void Configure(TowerType type)
    {
        Type = type;
        switch (Type)
        {
            case TowerType.Fire:
                _color = new Color("#ff9d70");
                Damage = 12f;
                Cooldown = 1.05;
                break;
            case TowerType.Frost:
                _color = new Color("#9be5ff");
                Damage = 7f;
                Cooldown = 0.9;
                break;
            case TowerType.Thunder:
                _color = new Color("#d4bcff");
                Damage = 20f;
                Cooldown = 2.2;
                break;
            case TowerType.Nature:
                _color = new Color("#a4e59d");
                Damage = 9f;
                Cooldown = 1.4;
                break;
        }

        QueueRedraw();
    }

    public override void _Ready()
    {
        AddToGroup("towers");
        QueueRedraw();
    }

    public void StopAttacking()
    {
        _combatActive = false;
    }

    public override void _Process(double delta)
    {
        if (!_combatActive)
            return;

        _timer -= delta;
        if (_timer <= 0)
        {
            Enemy? target = FindNearestEnemy();
            if (target != null)
            {
                Fire(target);
                _timer = Cooldown;
            }
        }
    }

    private Enemy? FindNearestEnemy()
    {
        Enemy? closest = null;
        float distance = Range;
        foreach (Node node in GetTree().GetNodesInGroup("enemies"))
        {
            if (node is not Enemy enemy || !IsInstanceValid(enemy) || enemy.HasReachedCore)
                continue;

            float current = GlobalPosition.DistanceTo(enemy.GlobalPosition);
            if (current < distance)
            {
                closest = enemy;
                distance = current;
            }
        }

        return closest;
    }

    private void Fire(Enemy target)
    {
        Projectile projectile = new()
        {
            Target = target,
            Damage = Damage,
            TravelSpeed = 350f,
            Radius = Type == TowerType.Thunder ? 7f : 5f,
            ProjectileColor = _color,
            ImpactSlowMultiplier = Type == TowerType.Frost
                ? GameBalance.FrostSlowMultiplier
                : Type == TowerType.Nature
                    ? GameBalance.NatureSlowMultiplier
                    : 1f,
            ImpactSlowDuration = Type == TowerType.Frost
                ? GameBalance.FrostSlowDuration
                : Type == TowerType.Nature
                    ? GameBalance.NatureSlowDuration
                    : 0f,
        };
        GetTree().CurrentScene.AddChild(projectile);
        projectile.GlobalPosition = GlobalPosition;
    }

    public override void _Draw()
    {
        DrawCircle(Vector2.Zero, 20f, new Color("#362850"));
        DrawCircle(Vector2.Zero, 15f, _color);
        DrawCircle(Vector2.Zero, 8f, new Color("#fff4bd"));
        DrawArc(Vector2.Zero, Range, 0, Mathf.Tau, 64, new Color(_color, 0.09f), 1.5f);
    }
}
