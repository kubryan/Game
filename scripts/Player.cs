using Godot;
using System;

public partial class Player : Node2D
{
    [Signal]
    public delegate void SpellCastEventHandler(int slot, string spellName);

    [Signal]
    public delegate void ManaChangedEventHandler(float current, float maximum);

    [Signal]
    public delegate void CombatMessageEventHandler(string message);

    public float Mana { get; private set; } = 100f;
    public float MaxMana { get; private set; } = 100f;
    public float AutoAttackRange { get; set; } = 260f;
    public float AutoAttackDamage { get; set; } = 12f;

    private readonly string[] _spellActions =
    {
        InputSettings.Spell1,
        InputSettings.Spell2,
        InputSettings.Spell3,
        InputSettings.Spell4,
    };

    private readonly string[] _spellNames =
    {
        "餘燼飛彈",
        "霜花禁錮",
        "雷鳴裁決",
        "荊棘新生",
    };

    private readonly float[] _spellCosts = { 18f, 26f, 34f, 22f };
    private readonly double[] _spellCooldowns = { 0.8, 5.5, 8.0, 6.0 };
    private readonly double[] _spellTimers = { 0, 0, 0, 0 };

    private double _attackTimer;
    private Vector2 _facing = Vector2.Right;

    public override void _Ready()
    {
        AddToGroup("player");
        QueueRedraw();
    }

    public override void _Process(double delta)
    {
        Vector2 movement = Input.GetVector(InputSettings.MoveLeft, InputSettings.MoveRight, InputSettings.MoveUp, InputSettings.MoveDown);
        if (movement.LengthSquared() > 0.01f)
        {
            _facing = movement.Normalized();
            GlobalPosition += movement.Normalized() * 210f * (float)delta;
        }

        GlobalPosition = new Vector2(
            Mathf.Clamp(GlobalPosition.X, 80f, 1200f),
            Mathf.Clamp(GlobalPosition.Y, 100f, 650f));

        _attackTimer -= delta;
        for (int index = 0; index < _spellTimers.Length; index++)
            _spellTimers[index] -= delta;

        Mana = Mathf.Min(MaxMana, Mana + 7f * (float)delta);
        EmitSignal(SignalName.ManaChanged, Mana, MaxMana);

        if (_attackTimer <= 0)
        {
            Enemy? target = FindNearestEnemy(AutoAttackRange);
            if (target != null)
            {
                FireAutoAttack(target);
                _attackTimer = 0.62;
            }
        }

        for (int slot = 0; slot < _spellActions.Length; slot++)
        {
            if (Input.IsActionJustPressed(_spellActions[slot]))
                TryCastSpell(slot);
        }
        QueueRedraw();
    }

    public void TryCastSpell(int slot)
    {
        if (slot < 0 || slot >= _spellActions.Length || _spellTimers[slot] > 0)
            return;
        if (Mana < _spellCosts[slot])
        {
            EmitSignal(SignalName.CombatMessage, "法力不足，無法施放「" + _spellNames[slot] + "」");
            return;
        }

        Mana -= _spellCosts[slot];
        _spellTimers[slot] = _spellCooldowns[slot];
        EmitSignal(SignalName.SpellCast, slot, _spellNames[slot]);
        EmitSignal(SignalName.ManaChanged, Mana, MaxMana);

        switch (slot)
        {
            case 0:
                CastEmberMissile();
                break;
            case 1:
                CastFrostPrison();
                break;
            case 2:
                CastThunderJudgement();
                break;
            case 3:
                CastThornBloom();
                break;
        }
    }

    public double GetSpellCooldown(int slot)
    {
        return slot >= 0 && slot < _spellTimers.Length ? Mathf.Max(0, (float)_spellTimers[slot]) : 0;
    }

    private void FireAutoAttack(Enemy target)
    {
        _facing = GlobalPosition.DirectionTo(target.GlobalPosition);
        Projectile projectile = new()
        {
            Target = target,
            Damage = AutoAttackDamage,
            ProjectileColor = new Color("#ffe38b"),
            TravelSpeed = 430f,
        };
        GetTree().CurrentScene.AddChild(projectile);
        projectile.GlobalPosition = GlobalPosition + _facing * 20f;
    }

    private void CastEmberMissile()
    {
        Enemy? target = FindNearestEnemy(420f);
        if (target == null)
            return;

        Projectile projectile = new()
        {
            Target = target,
            Damage = 34f,
            ProjectileColor = new Color("#ff9d70"),
            TravelSpeed = 610f,
            Radius = 9f,
        };
        GetTree().CurrentScene.AddChild(projectile);
        projectile.GlobalPosition = GlobalPosition;
    }

    private void CastFrostPrison()
    {
        foreach (Node node in GetTree().GetNodesInGroup("enemies"))
        {
            if (node is Enemy enemy && GlobalPosition.DistanceTo(enemy.GlobalPosition) <= 170f)
            {
                enemy.TakeDamage(12f);
                enemy.ApplySlow(0.35f, 4.5f);
            }
        }
        SpawnBurst(new Color("#b2e9ff"), 170f);
    }

    private void CastThunderJudgement()
    {
        Enemy? target = FindNearestEnemy(430f);
        if (target == null)
            return;

        target.TakeDamage(68f);
        foreach (Node node in GetTree().GetNodesInGroup("enemies"))
        {
            if (node is Enemy enemy && enemy != target && target.GlobalPosition.DistanceTo(enemy.GlobalPosition) <= 105f)
                enemy.TakeDamage(24f);
        }
        SpawnBurst(new Color("#d4bcff"), 120f);
    }

    private void CastThornBloom()
    {
        foreach (Node node in GetTree().GetNodesInGroup("enemies"))
        {
            if (node is Enemy enemy && GlobalPosition.DistanceTo(enemy.GlobalPosition) <= 145f)
            {
                enemy.TakeDamage(26f);
                enemy.ApplySlow(0.6f, 2.5f);
            }
        }
        SpawnBurst(new Color("#9de89b"), 145f);
    }

    private void SpawnBurst(Color color, float radius)
    {
        SpellBurst burst = new() { BurstColor = color, BurstRadius = radius };
        GetTree().CurrentScene.AddChild(burst);
        burst.GlobalPosition = GlobalPosition;
    }

    private Enemy? FindNearestEnemy(float range)
    {
        Enemy? closest = null;
        float closestDistance = range;
        foreach (Node node in GetTree().GetNodesInGroup("enemies"))
        {
            if (node is not Enemy enemy || !IsInstanceValid(enemy) || enemy.HasReachedCore)
                continue;

            float distance = GlobalPosition.DistanceTo(enemy.GlobalPosition);
            if (distance < closestDistance)
            {
                closest = enemy;
                closestDistance = distance;
            }
        }
        return closest;
    }

    public override void _Draw()
    {
        DrawCircle(Vector2.Zero, 22f, new Color("#fff2bc"));
        DrawCircle(Vector2.Zero, 17f, new Color("#f7a6c8"));
        DrawCircle(new Vector2(-7, -4), 3.5f, Colors.White);
        DrawCircle(new Vector2(7, -4), 3.5f, Colors.White);
        DrawCircle(new Vector2(-7, -4), 1.5f, new Color("#2c2142"));
        DrawCircle(new Vector2(7, -4), 1.5f, new Color("#2c2142"));
        DrawArc(new Vector2(0, 3), 8f, 0.25f, Mathf.Pi - 0.25f, 12, new Color("#6b3d65"), 2f);
        DrawLine(Vector2.Zero, _facing * 34f, new Color("#ffe38b"), 4f, true);
        DrawCircle(_facing * 38f, 5f, new Color("#fff4bd"));
    }
}
