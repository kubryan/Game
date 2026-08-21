using Godot;

public partial class Enemy : Node2D
{
    [Signal]
    public delegate void DefeatedEventHandler(Enemy enemy);

    [Signal]
    public delegate void DamageTakenEventHandler(Enemy enemy, float amount);

    public float Health { get; private set; } = 42f;
    public float MaxHealth { get; private set; } = 42f;
    public float Speed { get; set; } = 38f;
    public float DamageToCore { get; private set; } = GameBalance.BaseEnemyDamageToCore;
    public Color BodyColor { get; set; } = new("#6b3f86");
    public string DisplayName { get; set; } = "紙眼童";

    private Vector2 _targetPosition;
    private double _slowTimeRemaining;
    private float _slowMultiplier = 1f;
    private bool _reachedCore;

    public void Configure(
        Vector2 targetPosition,
        float health,
        float speed,
        Color bodyColor,
        string displayName,
        float damageToCore = GameBalance.BaseEnemyDamageToCore)
    {
        _targetPosition = targetPosition;
        MaxHealth = health;
        Health = health;
        Speed = speed;
        DamageToCore = damageToCore;
        BodyColor = bodyColor;
        DisplayName = displayName;
        AddToGroup("enemies");
        QueueRedraw();
    }

    public override void _Process(double delta)
    {
        if (_reachedCore)
            return;

        if (_slowTimeRemaining > 0)
            _slowTimeRemaining -= delta;
        else
            _slowMultiplier = 1f;

        float distance = GlobalPosition.DistanceTo(_targetPosition);
        if (distance <= 28f)
        {
            _reachedCore = true;
            EmitSignal(SignalName.Defeated, this);
            QueueFree();
            return;
        }

        Vector2 direction = GlobalPosition.DirectionTo(_targetPosition);
        GlobalPosition += direction * Speed * _slowMultiplier * (float)delta;
        QueueRedraw();
    }

    public void TakeDamage(float amount)
    {
        if (_reachedCore || !IsInsideTree() || amount <= 0)
            return;

        float actualDamage = Mathf.Min(amount, Health);
        Health = Mathf.Max(0f, Health - amount);
        EmitSignal(SignalName.DamageTaken, this, actualDamage);

        if (Health <= 0f)
        {
            EmitSignal(SignalName.Defeated, this);
            QueueFree();
        }

        QueueRedraw();
    }

    public void ApplySlow(float multiplier, float duration)
    {
        _slowMultiplier = Mathf.Clamp(multiplier, 0.1f, 1f);
        _slowTimeRemaining = Mathf.Max(_slowTimeRemaining, duration);
        QueueRedraw();
    }

    public bool HasReachedCore => _reachedCore;

    public override void _Draw()
    {
        float pulse = 1f + Mathf.Sin((float)Time.GetTicksMsec() * 0.004f) * 0.04f;
        DrawCircle(Vector2.Zero, 18f * pulse, BodyColor);
        DrawCircle(new Vector2(-6, -3), 3f, Colors.White);
        DrawCircle(new Vector2(6, -3), 3f, Colors.White);
        DrawCircle(new Vector2(-6, -3), 1.5f, new Color("#281b3d"));
        DrawCircle(new Vector2(6, -3), 1.5f, new Color("#281b3d"));
        DrawArc(Vector2.Zero, 12f, 0.2f, Mathf.Pi - 0.2f, 14, new Color("#2b1b3a"), 2f);

        if (_slowTimeRemaining > 0)
            DrawArc(Vector2.Zero, 23f, 0, Mathf.Tau, 24, new Color("#aeeaff"), 3f);

        float healthRatio = MaxHealth <= 0 ? 0 : Health / MaxHealth;
        DrawRect(new Rect2(-20, -31, 40, 4), new Color("#2b1b3a"));
        DrawRect(new Rect2(-20, -31, 40 * healthRatio, 4), new Color("#ff91a8"));
    }
}
