using Godot;
using System;

public partial class Main : Node2D
{
    private static readonly string[] SpellNames =
    {
        "餘燼飛彈",
        "霜花禁錮",
        "雷鳴裁決",
        "荊棘新生",
    };

    private readonly RandomNumberGenerator _random = new();
    private readonly Vector2 _corePosition = new(640, 365);

    private Player _player = null!;
    private LevelDefinition _level = null!;
    private KeybindMenu _keybindMenu = null!;
    private Label _statusLabel = null!;
    private Label _waveLabel = null!;
    private Label _manaLabel = null!;
    private Label _coreLabel = null!;
    private Label _messageLabel = null!;
    private Label[] _spellLabels = Array.Empty<Label>();
    private PanelContainer _tutorialPanel = null!;
    private Label _tutorialLabel = null!;
    private PanelContainer _resultPanel = null!;
    private Label _resultTitleLabel = null!;
    private Label _resultStatsLabel = null!;
    private Texture2D? _backgroundTexture;

    private float _coreHealth = GameBalance.StartingCoreHealth;
    private int _wave;
    private int _spawnedThisWave;
    private int _enemiesPerWave;
    private int _essence = GameBalance.StartingEssence;
    private double _spawnTimer;
    private double _nextWaveTimer = GameBalance.InitialWaveDelay;
    private double _messageTimer;
    private double _tutorialTimer;
    private int _defeatedEnemies;
    private int _builtTowers;
    private bool _waveInProgress;
    private bool _finished;

    public override void _Ready()
    {
        _random.Seed = GameBalance.RandomSeed;
        ProgressManager progress = GetNode<ProgressManager>("/root/ProgressManager");
        _level = LevelCatalog.Get(progress.SelectedLevel);
        _coreHealth = _level.StartingCoreHealth;
        _essence = _level.StartingEssence;
        _backgroundTexture = GD.Load<Texture2D>("res://assets/frosting_forest_visual_target_1280.png");

        CreatePlayer();
        CreateInterface();
        StartNextWave();
        ShowTutorialIfNeeded();
        QueueRedraw();
    }

    public override void _Process(double delta)
    {
        if (_finished)
            return;

        UpdateMessageTimer(delta);
        UpdateTutorialTimer(delta);
        ProcessWave(delta);

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

            if (keyEvent.Keycode == Key.Escape && _tutorialPanel.Visible)
            {
                HideTutorial();
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

    private void UpdateMessageTimer(double delta)
    {
        if (_messageTimer <= 0)
            return;

        _messageTimer -= delta;
        if (_messageTimer <= 0)
        {
            _messageLabel.Text = "守住希望篝火，別讓夜色吞掉森林。";
            _messageLabel.AddThemeColorOverride("font_color", GameBalance.MessageTextColor);
        }
    }

    private void ShowTutorialIfNeeded()
    {
        if (_level.Id != 1)
            return;

        _tutorialTimer = GameBalance.TutorialDuration;
        _tutorialPanel.Visible = true;
    }

    private void UpdateTutorialTimer(double delta)
    {
        if (_tutorialTimer <= 0 || !_tutorialPanel.Visible)
            return;

        _tutorialTimer -= delta;
        if (_tutorialTimer <= 0)
            HideTutorial();
    }

    private void HideTutorial()
    {
        _tutorialTimer = 0;
        if (_tutorialPanel != null)
            _tutorialPanel.Visible = false;
    }

    private void ProcessWave(double delta)
    {
        if (_waveInProgress)
        {
            ProcessActiveWave(delta);
            return;
        }

        if (_wave >= _level.Waves)
            return;

        _nextWaveTimer -= delta;
        if (_nextWaveTimer <= 0)
            StartNextWave();
    }

    private void ProcessActiveWave(double delta)
    {
        _spawnTimer -= delta;
        if (_spawnedThisWave < _enemiesPerWave && _spawnTimer <= 0)
        {
            SpawnEnemy();
            _spawnedThisWave++;
            _spawnTimer = Mathf.Max(
                GameBalance.SpawnIntervalFloor,
                GameBalance.SpawnIntervalStart - _wave * GameBalance.SpawnIntervalReductionPerWave);
        }

        bool allEnemiesSpawned = _spawnedThisWave >= _enemiesPerWave;
        bool noEnemiesRemain = GetTree().GetNodesInGroup("enemies").Count == 0;
        if (!allEnemiesSpawned || !noEnemiesRemain)
            return;

        _waveInProgress = false;
        if (_wave >= _level.Waves)
        {
            CompleteLevel();
            return;
        }

        _nextWaveTimer = GameBalance.InterWaveDelay;
        ShowMessage($"第 {_wave} 波守住了。下一波妖氣將在 {GameBalance.InterWaveDelay:0} 秒後湧來。", GameBalance.InterWaveDelay);
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
            Position = Vector2.Zero,
            Size = new Vector2(GameBalance.ViewportWidth, GameBalance.HeaderHeight),
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        canvas.AddChild(topBar);

        _statusLabel = MakeLabel(
            $"{_level.Title} · {_level.Subtitle}",
            new Vector2(28, 14),
            22,
            new Color("#fff1b8"));
        canvas.AddChild(_statusLabel);

        _waveLabel = MakeLabel("", new Vector2(28, 49), 15, GameBalance.HudTextColor);
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

        _messageLabel = MakeLabel(
            "守住希望篝火，別讓夜色吞掉森林。",
            new Vector2(28, GameBalance.FooterY),
            16,
            GameBalance.MessageTextColor);
        canvas.AddChild(_messageLabel);

        _spellLabels = new Label[GameBalance.SpellCount];
        for (int index = 0; index < _spellLabels.Length; index++)
        {
            Label spell = MakeLabel(
                "",
                new Vector2(760 + index * 125, GameBalance.FooterY),
                14,
                new Color("#fff1b8"));
            spell.HorizontalAlignment = HorizontalAlignment.Center;
            spell.Size = new Vector2(115, 30);
            _spellLabels[index] = spell;
            canvas.AddChild(spell);
        }

        _keybindMenu = new KeybindMenu();
        canvas.AddChild(_keybindMenu);
        CreateTutorialPanel(canvas);
        CreateResultPanel(canvas);
    }

    private void CreateTutorialPanel(CanvasLayer canvas)
    {
        _tutorialPanel = new PanelContainer
        {
            Position = new Vector2(360, 180),
            Size = new Vector2(560, 220),
            Visible = false,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        _tutorialPanel.AddThemeStyleboxOverride(
            "panel",
            CreateOverlayStyle(new Color("#ffd36e")));

        MarginContainer margin = new();
        margin.AddThemeConstantOverride("margin_left", 28);
        margin.AddThemeConstantOverride("margin_top", 22);
        margin.AddThemeConstantOverride("margin_right", 28);
        margin.AddThemeConstantOverride("margin_bottom", 22);
        _tutorialPanel.AddChild(margin);

        _tutorialLabel = new Label
        {
            Text =
                "夜守者，歡迎來到糖霜森林。\n\n"
                + "WASD：移動角色\n"
                + "角色會自動攻擊附近的妖怪\n"
                + "左鍵：建造魔法塔\n"
                + "1～4：施放四種主動法術\n\n"
                + "提示會在幾秒後自動消失，按 Esc 可立即關閉。",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
        };
        _tutorialLabel.AddThemeFontSizeOverride("font_size", 20);
        _tutorialLabel.AddThemeColorOverride("font_color", new Color("#fff4bd"));
        margin.AddChild(_tutorialLabel);
        canvas.AddChild(_tutorialPanel);
    }

    private void CreateResultPanel(CanvasLayer canvas)
    {
        _resultPanel = new PanelContainer
        {
            Position = new Vector2(300, 125),
            Size = new Vector2(680, 455),
            Visible = false,
        };
        _resultPanel.AddThemeStyleboxOverride(
            "panel",
            CreateOverlayStyle(new Color("#a875ff")));

        MarginContainer margin = new();
        margin.AddThemeConstantOverride("margin_left", 42);
        margin.AddThemeConstantOverride("margin_top", 32);
        margin.AddThemeConstantOverride("margin_right", 42);
        margin.AddThemeConstantOverride("margin_bottom", 28);
        _resultPanel.AddChild(margin);

        VBoxContainer content = new();
        content.AddThemeConstantOverride("separation", 16);
        margin.AddChild(content);

        _resultTitleLabel = new Label
        {
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        _resultTitleLabel.AddThemeFontSizeOverride("font_size", 30);
        _resultTitleLabel.AddThemeColorOverride("font_color", new Color("#fff1b8"));
        content.AddChild(_resultTitleLabel);

        _resultStatsLabel = new Label
        {
            CustomMinimumSize = new Vector2(0, 210),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
        };
        _resultStatsLabel.AddThemeFontSizeOverride("font_size", 20);
        _resultStatsLabel.AddThemeColorOverride("font_color", new Color("#f0e6ff"));
        content.AddChild(_resultStatsLabel);

        HBoxContainer actions = new();
        actions.Alignment = BoxContainer.AlignmentMode.Center;
        actions.AddThemeConstantOverride("separation", 18);
        content.AddChild(actions);

        Button replayButton = new()
        {
            Text = "重新挑戰  F2",
            CustomMinimumSize = new Vector2(210, 48),
        };
        replayButton.Pressed += () => GetTree().ReloadCurrentScene();
        actions.AddChild(replayButton);

        Button mapButton = new()
        {
            Text = "返回關卡地圖  F3",
            CustomMinimumSize = new Vector2(210, 48),
        };
        mapButton.Pressed += () => GetTree().ChangeSceneToFile("res://scenes/LevelSelect.tscn");
        actions.AddChild(mapButton);
        canvas.AddChild(_resultPanel);
    }

    private StyleBoxFlat CreateOverlayStyle(Color borderColor)
    {
        StyleBoxFlat style = new()
        {
            BgColor = new Color(0.08f, 0.05f, 0.16f, 0.96f),
            BorderColor = borderColor,
            CornerRadiusTopLeft = 18,
            CornerRadiusTopRight = 18,
            CornerRadiusBottomLeft = 18,
            CornerRadiusBottomRight = 18,
        };
        style.SetBorderWidthAll(3);
        return style;
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
        _enemiesPerWave = GameBalance.BaseEnemiesPerWave + _wave * GameBalance.AdditionalEnemiesPerWave;
        _spawnTimer = GameBalance.InitialSpawnDelay;
        _waveInProgress = true;
        string waveMessage = _level.EliteWave == _wave
            ? $"第 {_wave} 波妖怪出現了，精英妖怪正在逼近。"
            : $"第 {_wave} 波妖怪出現了。";
        ShowMessage(waveMessage, 2.5);
    }

    private void SpawnEnemy()
    {
        Enemy enemy = new();
        AddChild(enemy);

        int lane = _random.RandiRange(0, _level.SpawnPoints.Count - 1);
        Vector2 spawnPosition = _level.SpawnPoints[lane];
        float health = GameBalance.BaseEnemyHealth
            + _wave * GameBalance.EnemyHealthPerWave
            + _level.EnemyHealthBonus;
        float speed = GameBalance.BaseEnemySpeed
            + _wave * GameBalance.EnemySpeedPerWave
            + _level.EnemySpeedBonus;
        float damageToCore = GameBalance.BaseEnemyDamageToCore + _level.CoreDamageBonus;
        bool isElite = _level.EliteWave == _wave
            && _spawnedThisWave == _enemiesPerWave / 2;

        Color[] enemyColors =
        {
            new Color("#7b4f96"),
            new Color("#5e739f"),
            new Color("#9c5676"),
            new Color("#567e67"),
        };

        Color enemyColor = enemyColors[_random.RandiRange(0, enemyColors.Length - 1)];
        if (isElite)
            enemyColor = new Color("#b85d81");

        enemy.GlobalPosition = spawnPosition;
        enemy.Configure(
            _corePosition,
            health,
            speed,
            enemyColor,
            isElite ? "腐化精英" : "縫合妖怪",
            damageToCore,
            isElite,
            isElite ? _level.EliteHealthMultiplier : 1f,
            isElite ? _level.EliteSpeedMultiplier : 1f,
            isElite ? _level.EliteDamageToCoreBonus : 0f,
            isElite ? _level.EliteEssenceReward : GameBalance.EssencePerEnemy);
        enemy.Defeated += OnEnemyDefeated;
        enemy.DamageTaken += OnEnemyDamageTaken;
    }

    private void OnEnemyDamageTaken(Enemy enemy, float amount)
    {
        SpawnFloatingText(
            enemy.GlobalPosition,
            $"-{amount:0.#}",
            GameBalance.DamageTextColor,
            new Vector2(0, -10));
    }

    private void OnEnemyDefeated(Enemy enemy)
    {
        if (enemy.HasReachedCore)
        {
            _coreHealth -= enemy.DamageToCore;
            SpawnFloatingText(
                enemy.GlobalPosition,
                $"-{enemy.DamageToCore:0.#} 篝火",
                GameBalance.WarningColor,
                new Vector2(0, -10));
            ShowMessage("妖怪碰到了希望篝火！", 2.2, GameBalance.WarningColor);
            if (_coreHealth <= 0)
                FailLevel();
            return;
        }

        _defeatedEnemies++;
        _essence += enemy.EssenceReward;
        SpawnFloatingText(
            enemy.GlobalPosition,
            $"+{enemy.EssenceReward} 星砂",
            GameBalance.EssenceTextColor,
            new Vector2(0, 12));
    }

    private void TryBuildTower(Vector2 position)
    {
        if (_essence < GameBalance.TowerBuildCost)
        {
            ShowMessage(
                $"星砂不足：建塔需要 {GameBalance.TowerBuildCost}，目前只有 {_essence}。",
                GameBalance.WarningMessageDuration,
                GameBalance.WarningColor);
            return;
        }

        bool outsideBuildArea = position.Y < GameBalance.BuildAreaMinY
            || position.Y > GameBalance.BuildAreaMaxY;
        bool tooCloseToCore = position.DistanceTo(_corePosition) < GameBalance.CoreBuildRadius;
        if (outsideBuildArea || tooCloseToCore)
        {
            ShowMessage("這裡不能建造魔法塔。", 1.6);
            return;
        }

        Tower tower = new();
        Tower.TowerType type = (Tower.TowerType)(
            GetTree().GetNodesInGroup("towers").Count % GameBalance.TowerTypeCount);
        tower.Configure(type);
        tower.GlobalPosition = position;
        AddChild(tower);
        _essence -= GameBalance.TowerBuildCost;
        _builtTowers++;
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

        string phase = _waveInProgress ? "" : "整備中";
        bool canAffordTower = _essence >= GameBalance.TowerBuildCost;
        string buildHint = canAffordTower
            ? $"左鍵：建造塔（{GameBalance.TowerBuildCost} 星砂）"
            : $"左鍵：建造塔（需要 {GameBalance.TowerBuildCost} 星砂）";
        _waveLabel.Text = _waveInProgress
            ? $"波次 {_wave} / {_level.Waves}    星砂 {_essence}    {buildHint}"
            : $"波次 {_wave} / {_level.Waves}    星砂 {_essence}    {phase}    {buildHint}";
        _waveLabel.AddThemeColorOverride(
            "font_color",
            canAffordTower ? GameBalance.HudTextColor : GameBalance.WarningColor);
        _coreLabel.Text = $"希望篝火  {Mathf.Max(0, Mathf.RoundToInt(_coreHealth))}%";
    }

    private void UpdateSpellLabels()
    {
        if (_spellLabels.Length != GameBalance.SpellCount)
            return;

        InputSettings settings = GetNode<InputSettings>("/root/InputSettings");
        for (int index = 0; index < GameBalance.SpellCount; index++)
        {
            double cooldown = _player.GetSpellCooldown(index);
            string cooldownText = cooldown > 0 ? $"{cooldown:0.0}s" : "就緒";
            _spellLabels[index].Text =
                $"{settings.GetBindingText($"spell_{index + 1}")}  {SpellNames[index]}\n{cooldownText}";
        }
    }

    private void ShowResultPanel(string title, string stats)
    {
        _resultTitleLabel.Text = title;
        _resultStatsLabel.Text = stats;
        _resultPanel.Visible = true;
    }

    private int CalculateStars()
    {
        float healthRatio = _level.StartingCoreHealth <= 0
            ? 0
            : _coreHealth / _level.StartingCoreHealth;
        if (healthRatio >= GameBalance.ThreeStarCoreHealthRatio)
            return 3;
        if (healthRatio >= GameBalance.TwoStarCoreHealthRatio)
            return 2;
        return 1;
    }

    private string BuildResultStats(bool completed)
    {
        int stars = completed ? CalculateStars() : 0;
        string rating = completed
            ? new string('★', stars) + new string('☆', 3 - stars)
            : "☆☆☆";
        string resultLine = completed ? "篝火仍在燃燒，這片童話暫時安全了。" : "希望篝火熄滅了，下一次請更早布置防線。";
        return $"{resultLine}\n\n"
            + $"評價   {rating}\n"
            + $"篝火   {Mathf.Max(0, Mathf.RoundToInt(_coreHealth))} / {Mathf.RoundToInt(_level.StartingCoreHealth)}\n"
            + $"擊退妖怪   {_defeatedEnemies}\n"
            + $"建造魔法塔   {_builtTowers}\n"
            + $"剩餘星砂   {_essence}\n\n"
            + "F2 重新挑戰　　F3 返回關卡地圖";
    }

    private void ShowMessage(string message, double duration, Color? color = null)
    {
        if (_messageLabel == null)
            return;

        _messageLabel.Text = message;
        _messageLabel.AddThemeColorOverride("font_color", color ?? GameBalance.MessageTextColor);
        _messageTimer = duration;
    }

    private void SpawnFloatingText(Vector2 position, string text, Color color, Vector2 offset = default)
    {
        FloatingText floatingText = new()
        {
            Text = text,
            TextColor = color,
            TextOffset = offset,
        };
        GetTree().CurrentScene.AddChild(floatingText);
        floatingText.GlobalPosition = position;
    }

    private void CompleteLevel()
    {
        StopAllTowers();
        _finished = true;
        HideTutorial();
        ProgressManager progress = GetNode<ProgressManager>("/root/ProgressManager");
        progress.CompleteLevel(_level.Id);
        _messageLabel.Text = $"{_level.Title} 完成！";
        ShowResultPanel($"{_level.Title} · 通關", BuildResultStats(true));
    }

    private void FailLevel()
    {
        StopAllTowers();
        _finished = true;
        HideTutorial();
        _messageLabel.Text = "希望篝火熄滅了。";
        ShowResultPanel($"{_level.Title} · 防線失守", BuildResultStats(false));
    }

    private void StopAllTowers()
    {
        foreach (Node node in GetTree().GetNodesInGroup("towers"))
        {
            if (node is Tower tower && IsInstanceValid(tower))
                tower.StopAttacking();
        }

        foreach (Node node in GetTree().GetNodesInGroup("projectiles"))
        {
            if (node is Projectile projectile && IsInstanceValid(projectile))
                projectile.QueueFree();
        }
    }

    public override void _Draw()
    {
        if (_backgroundTexture != null)
        {
            DrawTextureRect(
                _backgroundTexture,
                new Rect2(0, 0, GameBalance.ViewportWidth, GameBalance.ViewportHeight),
                false);
            DrawCircle(_corePosition, 62f, new Color("#3f2b5c", 0.82f));
            DrawCircle(_corePosition, 46f, new Color("#fff0a5", 0.84f));
            DrawCircle(_corePosition, 29f, new Color("#ffd36e", 0.95f));
            DrawArc(_corePosition, 70f, -0.8f, 1.2f, 18, new Color("#e889ad", 0.92f), 4f);
            return;
        }

        DrawRect(
            new Rect2(0, 0, GameBalance.ViewportWidth, GameBalance.ViewportHeight),
            new Color("#8dc9bb"));
        DrawRect(
            new Rect2(0, GameBalance.HeaderHeight, GameBalance.ViewportWidth, 550),
            new Color("#79b6a3"));

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
            DrawLine(
                new Vector2(x, y + 10),
                new Vector2(x + 8, y + 28),
                new Color("#3f765f"),
                4f);
        }

        for (int index = 0; index < 9; index++)
        {
            float x = 70 + index * 137;
            float y = 625 - (index % 3) * 26;
            DrawArc(
                new Vector2(x, y),
                26f,
                0.1f,
                2.8f,
                16,
                new Color("#403264", 0.38f),
                6f);
        }
    }
}
