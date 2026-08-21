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

    private static readonly string[] SpellNames =
    {
        "餘燼飛彈",
        "霜花禁錮",
        "雷鳴裁決",
        "荊棘新生",
    };

    private readonly string[] _spellActions =
    {
        InputSettings.Spell1,
        InputSettings.Spell2,
        InputSettings.Spell3,
        InputSettings.Spell4,
    };

    private readonly double[] _spellTimers = new double[GameBalance.SpellCount];

    public float Mana { get; private set; } = GameBalance.PlayerMaxMana;
    public float MaxMana { get; private set; } = GameBalance.PlayerMaxMana;

    private double _attackTimer;
    private Vector2 _facing = Vector2.Right;

    public override void _Ready()
    {
        AddToGroup("player");
        QueueRedraw();
    }

    public override void _Process(double delta)
    {
        ProcessMovement(delta);
        UpdateTimers(delta);
        RegenerateMana(delta);
        ProcessAutoAttack();
        ProcessSpellInputs();

        QueueRedraw();
    }

    private void ProcessMovement(double delta)
    {
        Vector2 movement = Input.GetVector(
            InputSettings.MoveLeft,
            InputSettings.MoveRight,
            InputSettings.MoveUp,
            InputSettings.MoveDown);

        if (movement.LengthSquared() > 0.01f)
        {
            _facing = movement.Normalized();
            GlobalPosition += movement.Normalized() * GameBalance.PlayerMoveSpeed * (float)delta;
        }

        GlobalPosition = new Vector2(
            Mathf.Clamp(GlobalPosition.X, 80f, 1200f),
            Mathf.Clamp(GlobalPosition.Y, 100f, 650f));
    }

    private void UpdateTimers(double delta)
    {
        _attackTimer -= delta;
        for (int index = 0; index < _spellTimers.Length; index++)
        {
            if (_spellTimers[index] > 0)
                _spellTimers[index] -= delta;
        }
    }

    private void RegenerateMana(double delta)
    {
        Mana = Mathf.Min(MaxMana, Mana + GameBalance.PlayerManaRegenPerSecond * (float)delta);
        EmitSignal(SignalName.ManaChanged, Mana, MaxMana);
    }

    private void ProcessAutoAttack()
    {
        if (_attackTimer > 0)
            return;

        Enemy? target = FindNearestEnemy(GameBalance.PlayerAutoAttackRange);
        if (target != null)
        {
            FireAutoAttack(target);
            _attackTimer = GameBalance.PlayerAutoAttackCooldown;
        }
    }

    private void ProcessSpellInputs()
    {
        for (int slot = 0; slot < _spellActions.Length; slot++)
        {
            if (Input.IsActionJustPressed(_spellActions[slot]))
                TryCastSpell(slot);
        }
    }

    public void TryCastSpell(int slot)
    {
        if (slot < 0 || slot >= _spellActions.Length || _spellTimers[slot] > 0)
            return;

        float cost = GameBalance.SpellCosts[slot];
        if (Mana < cost)
        {
            EmitSignal(SignalName.CombatMessage, $"法力不足，無法施放「{SpellNames[slot]}」");
            return;
        }

        Mana -= cost;
        _spellTimers[slot] = GameBalance.SpellCooldowns[slot];
        EmitSignal(SignalName.SpellCast, slot, SpellNames[slot]);
        EmitSignal(SignalName.ManaChanged, Mana, MaxMana);

        ExecuteSpellEffect(slot);
    }

    private void ExecuteSpellEffect(int slot)
    {
        switch (slot)
        {
            case 0: CastEmberMissile(); break;
            case 1: CastFrostPrison(); break;
            case 2: CastThunderJudgement(); break;
            case 3: CastThornBloom(); break;
        }
    }

    public double GetSpellCooldown(int slot)
    {
        return slot >= 0 && slot < _spellTimers.Length
            ? Mathf.Max(0, (float)_spellTimers[slot])
            : 0;
    }

    private void FireAutoAttack(Enemy target)
    {
        _facing = GlobalPosition.DirectionTo(target.GlobalPosition);
        Projectile projectile = new()
        {
            Target = target,
            Damage = GameBalance.PlayerAutoAttackDamage,
            ProjectileColor = new Color("#ffe38b"),
            TravelSpeed = 430f,
        };
        GetTree().CurrentScene.AddChild(projectile);
        projectile.GlobalPosition = GlobalPosition + _facing * 20f;
    }

    private void CastEmberMissile()
    {
        Enemy? target = FindNearestEnemy(GameBalance.EmberMissileRange);
        if (target == null)
            return;

        Projectile projectile = new()
        {
            Target = target,
            Damage = GameBalance.EmberMissileDamage,
            ProjectileColor = new Color("#ff9d70"),
            TravelSpeed = GameBalance.EmberMissileTravelSpeed,
            Radius = GameBalance.EmberMissileRadius,
        };
        GetTree().CurrentScene.AddChild(projectile);
        projectile.GlobalPosition = GlobalPosition;
    }

    private void CastFrostPrison()
    {
        float range = GameBalance.FrostPrisonRange;
        foreach (Node node in GetTree().GetNodesInGroup("enemies"))
        {
            if (node is Enemy enemy && GlobalPosition.DistanceTo(enemy.GlobalPosition) <= range)
            {
                enemy.TakeDamage(GameBalance.FrostPrisonDamage);
                enemy.ApplySlow(GameBalance.FrostPrisonSlowMultiplier, GameBalance.FrostPrisonSlowDuration);
            }
        }
        SpawnBurst(new Color("#b2e9ff"), range);
    }

    private void CastThunderJudgement()
    {
        Enemy? target = FindNearestEnemy(GameBalance.ThunderJudgementRange);
        if (target == null)
            return;

        target.TakeDamage(GameBalance.ThunderJudgementDamage);
        float splashRange = GameBalance.ThunderJudgementSplashRange;
        foreach (Node node in GetTree().GetNodesInGroup("enemies"))
        {
            if (node is Enemy enemy && enemy != target && target.GlobalPosition.DistanceTo(enemy.GlobalPosition) <= splashRange)
            {
                enemy.TakeDamage(GameBalance.ThunderJudgementSplashDamage);
            }
        }
        SpawnBurst(new Color("#d4bcff"), 120f);
    }

    private void CastThornBloom()
    {
        float range = GameBalance.ThornBloomRange;
        foreach (Node node in GetTree().GetNodesInGroup("enemies"))
        {
            if (node is Enemy enemy && GlobalPosition.DistanceTo(enemy.GlobalPosition) <= range)
            {
                enemy.TakeDamage(GameBalance.ThornBloomDamage);
                enemy.ApplySlow(GameBalance.ThornBloomSlowMultiplier, GameBalance.ThornBloomSlowDuration);
            }
        }
        SpawnBurst(new Color("#9de89b"), range);
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
