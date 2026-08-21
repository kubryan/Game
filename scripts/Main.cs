using Godot;
using System;

public partial class Main : Node2D
{
        private Player _player = null!;
    private LevelDefinition _level = null!;

    private KeybindMenu _keybindMenu = null!;
    private Label _statusLabel = null!;
    private Label _waveLabel = null!;
    private Label _manaLabel = null!;
    private Label _coreLabel = null!;
    private Label _messageLabel = null!;
    private Label[] _spellLabels = Array.Empty<Label>();

    private readonly RandomNumberGenerator _random = new();
    private Texture2D? _backgroundTexture;
    private readonly Vector2 _corePosition = new(640, 365);
    private float _coreHealth = 100f;
    private int _wave;
    private int _spawnedThisWave;
    private int _enemiesPerWave;
    private double _spawnTimer;
    private double _nextWaveTimer = 1.5;
    private double _messageTimer;
    private bool _waveInProgress;
    private bool _finished;
    private int _essence = 180;

        public override void _Ready()
    {
        _random.Seed = 87321;
        ProgressManager progress = GetNode<ProgressManager>("/root/ProgressManager");
        _level = LevelCatalog.Get(progress.SelectedLevel);
        _backgroundTexture = GD.Load<Texture2D>("res://assets/frosting_forest_visual_target.png");

        CreatePlayer();
        CreateInterface();
        StartNextWave();
        QueueRedraw();
    }

    public override void _Process(double delta)
    {
        if (_finished)
            return;

        if (_messageTimer > 0)
        {
            _messageTimer -= delta;
            if (_messageTimer <= 0)
                _messageLabel.Text = "守住希望篝火，別讓夜色吞掉森林。";
        }

        if (_waveInProgress)
        {
            _spawnTimer -= delta;
            if (_spawnedThisWave < _enemiesPerWave && _spawnTimer <= 0)
            {
                SpawnEnemy();
                _spawnedThisWave++;
                _spawnTimer = Mathf.Max(0.38f, 1.25f - _wave * 0.08f);
            }

            if (_spawnedThisWave >= _enemiesPerWave && GetTree().GetNodesInGroup("enemies").Count == 0)
            {
                _waveInProgress = false;
                if (_wave >= _level.Waves)
                {
                    CompleteLevel();
                    return;
                }

                _nextWaveTimer = 3.2;
                ShowMessage($"第 {_wave} 波守住了。下一波妖氣將在 3 秒後湧來。", 3.2);
            }
        }
        else if (_wave < _level.Waves)
        {
            _nextWaveTimer -= delta;
            if (_nextWaveTimer <= 0)
                StartNextWave();
        }

        if (_player != null)
            UpdateSpellLabels();
        UpdateStatusLabels();
        QueueRedraw();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
        {
            if (keyEvent.Keycode == Key.F1)
            {
                _keybindMenu.Toggle();
                GetViewport().SetInputAsHandled();
                return;
            }

                        if (keyEvent.Keycode == Key.F2)
            {
                GetTree().ReloadCurrentScene();
                GetViewport().SetInputAsHandled();
                return;
            }

            if (keyEvent.Keycode == Key.F3)
            {
                GetTree().ChangeSceneToFile("res://scenes/LevelSelect.tscn");
                GetViewport().SetInputAsHandled();
                return;
            }

            if (keyEvent.Keycode == Key.Escape && _keybindMenu.Visible)

            {
                _keybindMenu.Visible = false;
                GetViewport().SetInputAsHandled();
                return;
            }
        }

        if (_keybindMenu.Visible || _finished)
            return;

        if (@event is InputEventMouseButton mouse && mouse.Pressed && mouse.ButtonIndex == MouseButton.Left)
        {
            TryBuildTower(GetGlobalMousePosition());
            GetViewport().SetInputAsHandled();
        }
    }

    private void CreatePlayer()
    {
        _player = new Player
        {
            GlobalPosition = new Vector2(310, 365),
        };
        AddChild(_player);
        _player.ManaChanged += OnManaChanged;
        _player.SpellCast += OnSpellCast;
        _player.CombatMessage += OnCombatMessage;
    }

    private void CreateInterface()
    {
        CanvasLayer canvas = new();
        AddChild(canvas);

        ColorRect topBar = new()
        {
            Color = new Color("#211734"),
            Position = new Vector2(0, 0),
            Size = new Vector2(1280, 86),
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        canvas.AddChild(topBar);

                _statusLabel = MakeLabel($"{_level.Title} · {_level.Subtitle}", new Vector2(28, 14), 22, new Color("#fff1b8"));

        canvas.AddChild(_statusLabel);
        _waveLabel = MakeLabel("", new Vector2(28, 49), 15, new Color("#d7c6e9"));
        canvas.AddChild(_waveLabel);
        _manaLabel = MakeLabel("", new Vector2(450, 20), 17, new Color("#aeeaff"));
        canvas.AddChild(_manaLabel);
        _coreLabel = MakeLabel("", new Vector2(450, 50), 15, new Color("#ffb6c8"));
        canvas.AddChild(_coreLabel);

                Button mapButton = new()
        {
            Text = "關卡地圖  F3",
            Position = new Vector2(880, 22),
            Size = new Vector2(170, 42),
        };
        mapButton.Pressed += () => GetTree().ChangeSceneToFile("res://scenes/LevelSelect.tscn");
        canvas.AddChild(mapButton);

        Button settingsButton = new()
        {
            Text = "鍵位設定  F1",
            Position = new Vector2(1060, 22),
            Size = new Vector2(190, 42),
        };
        settingsButton.Pressed += () => _keybindMenu.Toggle();
        canvas.AddChild(settingsButton);

        _messageLabel = MakeLabel("守住希望篝火，別讓夜色吞掉森林。", new Vector2(28, 665), 16, new Color("#ffe7b0"));
        canvas.AddChild(_messageLabel);

        _spellLabels = new Label[4];
        for (int index = 0; index < _spellLabels.Length; index++)
        {
            Label spell = MakeLabel("", new Vector2(760 + index * 125, 665), 14, new Color("#fff1b8"));
            spell.HorizontalAlignment = HorizontalAlignment.Center;
            spell.Size = new Vector2(115, 30);
            _spellLabels[index] = spell;
            canvas.AddChild(spell);
        }

        _keybindMenu = new KeybindMenu();
        canvas.AddChild(_keybindMenu);
    }

    private Label MakeLabel(string text, Vector2 position, int fontSize, Color color)
    {
        Label label = new()
        {
            Text = text,
            Position = position,
        };
        label.AddThemeFontSizeOverride("font_size", fontSize);
        label.AddThemeColorOverride("font_color", color);
        return label;
    }

    private void StartNextWave()
    {
                if (_wave >= _level.Waves)

        {
            CompleteLevel();
            return;
        }

        _wave++;
        _spawnedThisWave = 0;
                _enemiesPerWave = 5 + _wave * 2;

        _spawnTimer = 0.6;
        _waveInProgress = true;
        ShowMessage($"第 {_wave} 波妖怪出現了。", 2.5);
    }

    private void SpawnEnemy()
    {
        Enemy enemy = new();
        AddChild(enemy);
        int lane = _random.RandiRange(0, 3);
        Vector2 spawnPosition = lane switch
        {
            0 => new Vector2(1120, 165),
            1 => new Vector2(1120, 290),
            2 => new Vector2(1120, 470),
            _ => new Vector2(1120, 575),
        };

                float health = 28f + _wave * 8f + _level.EnemyHealthBonus;
        float speed = 35f + _wave * 2.5f + _level.EnemySpeedBonus;

        Color color = new[]
        {
            new Color("#7b4f96"),
            new Color("#5e739f"),
            new Color("#9c5676"),
            new Color("#567e67"),
        }[_random.RandiRange(0, 3)];
        enemy.GlobalPosition = spawnPosition;
        enemy.Configure(_corePosition, health, speed, color, "縫合妖怪");
        enemy.Defeated += OnEnemyDefeated;
    }

    private void OnEnemyDefeated(Enemy enemy)
    {
        if (enemy.HasReachedCore)
        {
            _coreHealth -= enemy.DamageToCore;
            ShowMessage("妖怪碰到了希望篝火！", 2.2);
            if (_coreHealth <= 0)
                FailLevel();
        }
        else
        {
            _essence += 14;
        }
    }

    private void TryBuildTower(Vector2 position)
    {
        if (_essence < 40)
        {
            ShowMessage("星砂不足，還需要更多妖怪掉落的光屑。", 2.2);
            return;
        }
        if (position.Y < 100 || position.Y > 625 || position.DistanceTo(_corePosition) < 82f)
        {
            ShowMessage("這裡不能建造魔法塔。", 1.6);
            return;
        }

        Tower tower = new();
        Tower.TowerType type = (Tower.TowerType)(GetTree().GetNodesInGroup("towers").Count % 4);
        tower.Configure(type);
        tower.GlobalPosition = position;
        AddChild(tower);
        _essence -= 40;
        ShowMessage("魔法塔已建立。繼續守住這片還沒腐爛的森林。", 2.2);
    }

    private void OnManaChanged(float current, float maximum)
    {
        _manaLabel.Text = $"法力  {Mathf.RoundToInt(current)} / {Mathf.RoundToInt(maximum)}";
    }

    private void OnSpellCast(int slot, string spellName)
    {
        ShowMessage($"施放：{spellName}。腐化的夜色被撕開一道縫。", 1.8);
    }

    private void OnCombatMessage(string message)
    {
        ShowMessage(message, 1.8);
    }

    private void UpdateStatusLabels()
    {
        if (_waveLabel == null)
            return;
                _waveLabel.Text = _waveInProgress ? $"波次 {_wave} / {_level.Waves}    星砂 {_essence}    左鍵：建造塔" : $"波次 {_wave} / {_level.Waves}    星砂 {_essence}    整備中";

        _coreLabel.Text = $"希望篝火  {Mathf.Max(0, Mathf.RoundToInt(_coreHealth))}%";
    }

    private void UpdateSpellLabels()
    {
        if (_spellLabels.Length != 4)
            return;
        InputSettings settings = GetNode<InputSettings>("/root/InputSettings");
        string[] names = { "餘燼飛彈", "霜花禁錮", "雷鳴裁決", "荊棘新生" };
        for (int index = 0; index < 4; index++)
        {
            double cooldown = _player.GetSpellCooldown(index);
            string cooldownText = cooldown > 0 ? $"{cooldown:0.0}s" : "就緒";
            _spellLabels[index].Text = $"{settings.GetBindingText($"spell_{index + 1}")}  {names[index]}\n{cooldownText}";
        }
    }

    private void ShowMessage(string message, double duration)
    {
        if (_messageLabel == null)
            return;
        _messageLabel.Text = message;
        _messageTimer = duration;
    }

    private void CompleteLevel()
    {
        StopAllTowers();
        _finished = true;
        ProgressManager progress = GetNode<ProgressManager>("/root/ProgressManager");
        progress.CompleteLevel(_level.Id);
        _messageLabel.Text = $"{_level.Title} 完成！下一個區域已解鎖。按 F2 重玩，按 F3 返回關卡地圖。";
    }

    private void FailLevel()
    {
        StopAllTowers();
        _finished = true;
        _messageLabel.Text = "希望篝火熄滅了。按 F2 重新挑戰，按 F3 返回關卡地圖。";
    }

        private void StopAllTowers()
    {
        foreach (Node node in GetTree().GetNodesInGroup("towers"))
        {
            if (node is Tower tower && IsInstanceValid(tower))
                tower.StopAttacking();
        }
    }

    public override void _Draw()

    {
        if (_backgroundTexture != null)
        {
            DrawTextureRect(_backgroundTexture, new Rect2(0, 0, 1280, 720), false);
            DrawCircle(_corePosition, 62f, new Color("#3f2b5c", 0.82f));
            DrawCircle(_corePosition, 46f, new Color("#fff0a5", 0.84f));
            DrawCircle(_corePosition, 29f, new Color("#ffd36e", 0.95f));
            DrawArc(_corePosition, 70f, -0.8f, 1.2f, 18, new Color("#e889ad", 0.92f), 4f);
            return;
        }

        DrawRect(new Rect2(0, 0, 1280, 720), new Color("#8dc9bb"));
        DrawRect(new Rect2(0, 86, 1280, 550), new Color("#79b6a3"));

        // 童話道路逐漸被暗色藤蔓侵蝕。
        DrawLine(new Vector2(1120, 165), _corePosition, new Color("#eed8a2"), 52f, true);
        DrawLine(new Vector2(1120, 290), _corePosition, new Color("#f0dca8"), 48f, true);
        DrawLine(new Vector2(1120, 470), _corePosition, new Color("#e8d29c"), 48f, true);
        DrawLine(new Vector2(1120, 575), _corePosition, new Color("#e2c893"), 52f, true);
        DrawCircle(_corePosition, 62f, new Color("#3f2b5c"));
        DrawCircle(_corePosition, 46f, new Color("#fff0a5"));
        DrawCircle(_corePosition, 29f, new Color("#ffd36e"));
        DrawArc(_corePosition, 70f, -0.8f, 1.2f, 18, new Color("#e889ad"), 4f);

        for (int index = 0; index < 16; index++)
        {
            float x = 46 + (index * 83) % 1150;
            float y = 120 + (index * 67) % 480;
            DrawCircle(new Vector2(x, y), 16f, new Color("#5d987f"));
            DrawLine(new Vector2(x, y + 10), new Vector2(x + 8, y + 28), new Color("#3f765f"), 4f);
        }

        for (int index = 0; index < 9; index++)
        {
            float x = 70 + index * 137;
            float y = 625 - (index % 3) * 26;
            DrawArc(new Vector2(x, y), 26f, 0.1f, 2.8f, 16, new Color("#403264", 0.38f), 6f);
        }
    }
}
