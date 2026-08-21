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
    public int EssenceReward { get; private set; } = GameBalance.EssencePerEnemy;
    public bool IsElite { get; private set; }
    public Color BodyColor { get; set; } = new("#6b3f86");
    public string DisplayName { get; set; } = "紙眼童";

    private Vector2 _targetPosition;
    private double _slowTimeRemaining;
    private double _freezeTimeRemaining;
    private float _slowMultiplier = 1f;
    private bool _reachedCore;

    public void Configure(
        Vector2 targetPosition,
        float health,
        float speed,
        Color bodyColor,
        string displayName,
        float damageToCore = GameBalance.BaseEnemyDamageToCore,
        bool isElite = false,
        float healthMultiplier = 1f,
        float speedMultiplier = 1f,
        float damageToCoreBonus = 0f,
        int essenceReward = GameBalance.EssencePerEnemy)
    {
        _targetPosition = targetPosition;
        MaxHealth = health * Mathf.Max(0.1f, healthMultiplier);
        Health = MaxHealth;
        Speed = speed * Mathf.Max(0.1f, speedMultiplier);
        DamageToCore = damageToCore + damageToCoreBonus;
        EssenceReward = essenceReward;
        IsElite = isElite;
        BodyColor = bodyColor;
        DisplayName = displayName;
        AddToGroup("enemies");
        QueueRedraw();
    }

    public override void _Process(double delta)
    {
        if (_reachedCore)
            return;

        if (_freezeTimeRemaining > 0)
        {
            _freezeTimeRemaining -= delta;
            QueueRedraw();
            return;
        }

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

    public void ApplyFreeze(float duration)
    {
        _freezeTimeRemaining = Mathf.Max(_freezeTimeRemaining, duration);
        QueueRedraw();
    }

    public bool IsFrozen => _freezeTimeRemaining > 0;
    public bool HasReachedCore => _reachedCore;

    public override void _Draw()
    {
        float pulse = 1f + Mathf.Sin((float)Time.GetTicksMsec() * 0.004f) * 0.04f;
        float bodyRadius = IsElite ? 25f : 18f;
        DrawCircle(Vector2.Zero, bodyRadius * pulse, BodyColor);
        DrawCircle(new Vector2(-6, -3), 3f, Colors.White);
        DrawCircle(new Vector2(6, -3), 3f, Colors.White);
        DrawCircle(new Vector2(-6, -3), 1.5f, new Color("#281b3d"));
        DrawCircle(new Vector2(6, -3), 1.5f, new Color("#281b3d"));
        DrawArc(Vector2.Zero, IsElite ? 17f : 12f, 0.2f, Mathf.Pi - 0.2f, 14, new Color("#2b1b3a"), 2f);

        if (IsElite)
            DrawArc(Vector2.Zero, 31f, 0, Mathf.Tau, 24, new Color("#ffd36e"), 4f);
        else if (IsFrozen)
            DrawArc(Vector2.Zero, 25f, 0, Mathf.Tau, 24, new Color("#e4fbff"), 4f);
        else if (_slowTimeRemaining > 0)
            DrawArc(Vector2.Zero, 23f, 0, Mathf.Tau, 24, new Color("#aeeaff"), 3f);

        float healthRatio = MaxHealth <= 0 ? 0 : Health / MaxHealth;
        float healthBarWidth = IsElite ? 54f : 40f;
        DrawRect(new Rect2(-healthBarWidth / 2f, -31, healthBarWidth, 4), new Color("#2b1b3a"));
        DrawRect(new Rect2(-healthBarWidth / 2f, -31, healthBarWidth * healthRatio, 4), new Color("#ff91a8"));
    }
}
