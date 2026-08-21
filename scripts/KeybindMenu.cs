using Godot;
using System.Collections.Generic;

public partial class KeybindMenu : Control
{
    private readonly string[] _actions =
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

    private readonly Dictionary<string, Button> _buttons = new();
    private InputSettings? _settings;
    private string? _waitingAction;
    private Label? _hint;
    private Panel? _panel;

    public override void _Ready()
    {
        _settings = GetNode<InputSettings>("/root/InputSettings");
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;
        BuildUi();
        Visible = false;
    }

    public void Toggle()
    {
        Visible = !Visible;
        if (Visible)
            RefreshButtons();
    }

    public override void _Input(InputEvent @event)
    {
        if (!Visible || _waitingAction == null || @event is not InputEventKey keyEvent || !keyEvent.Pressed || keyEvent.Echo)
            return;

        if (keyEvent.Keycode == Key.Escape)
        {
            _waitingAction = null;
            SetHint("尚未變更鍵位。");
            RefreshButtons();
            GetViewport().SetInputAsHandled();
            return;
        }

        if (_settings != null && _settings.Rebind(_waitingAction, keyEvent.PhysicalKeycode == Key.None ? keyEvent.Keycode : keyEvent.PhysicalKeycode))
        {
            SetHint("鍵位已保存。按下另一個按鈕可繼續調整。");
        }
        _waitingAction = null;
        RefreshButtons();
        GetViewport().SetInputAsHandled();
    }

    private void BuildUi()
    {
        ColorRect dim = new()
        {
            Color = new Color(0.06f, 0.04f, 0.12f, 0.78f),
            MouseFilter = MouseFilterEnum.Stop,
        };
        dim.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(dim);

        _panel = new Panel
        {
            CustomMinimumSize = new Vector2(460, 420),
            Position = new Vector2(410, 150),
        };
        _panel.AddThemeStyleboxOverride("panel", MakePanelStyle());
        AddChild(_panel);

        VBoxContainer layout = new()
        {
            Position = new Vector2(34, 24),
            Size = new Vector2(392, 370),
        };
        _panel.AddChild(layout);

        Label title = new()
        {
            Text = "法術鍵位設定",
            HorizontalAlignment = HorizontalAlignment.Center,
            CustomMinimumSize = new Vector2(0, 42),
        };
        title.AddThemeFontSizeOverride("font_size", 26);
        title.AddThemeColorOverride("font_color", new Color("#fff1b8"));
        layout.AddChild(title);

        Label description = new()
        {
            Text = "點選法術後按下新按鍵；按 Esc 取消。設定會自動保存。",
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            CustomMinimumSize = new Vector2(0, 44),
        };
        description.AddThemeColorOverride("font_color", new Color("#d8cce9"));
        layout.AddChild(description);

        for (int index = 0; index < _actions.Length; index++)
        {
            string action = _actions[index];
            Button button = new()
            {
                CustomMinimumSize = new Vector2(0, 48),
                Alignment = HorizontalAlignment.Left,
            };
            int capturedIndex = index;
            button.Pressed += () => BeginRebind(_actions[capturedIndex]);
            _buttons[action] = button;
            layout.AddChild(button);
        }

        _hint = new Label
        {
            Text = "",
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            CustomMinimumSize = new Vector2(0, 38),
        };
        _hint.AddThemeColorOverride("font_color", new Color("#ffcf94"));
        layout.AddChild(_hint);

        Button close = new()
        {
            Text = "返回遊戲",
            CustomMinimumSize = new Vector2(0, 44),
        };
        close.Pressed += () => Visible = false;
        layout.AddChild(close);
        RefreshButtons();
    }

    private void BeginRebind(string action)
    {
        _waitingAction = action;
        SetHint($"請按下「{GetSpellName(action)}」的新按鍵……");
        RefreshButtons();
    }

    private void RefreshButtons()
    {
        if (_settings == null)
            return;

        for (int index = 0; index < _actions.Length; index++)
        {
            string action = _actions[index];
            if (!_buttons.TryGetValue(action, out Button? button))
                continue;

            string prefix = _waitingAction == action ? "等待按鍵：" : $"法術 {index + 1}：";
            button.Text = $"{prefix}{_spellNames[index]}    [{_settings.GetBindingText(action)}]";
        }
    }

    private void SetHint(string text)
    {
        if (_hint != null)
            _hint.Text = text;
    }

    private string GetSpellName(string action)
    {
        int index = System.Array.IndexOf(_actions, action);
        return index >= 0 ? _spellNames[index] : action;
    }

    private static StyleBoxFlat MakePanelStyle()
    {
        StyleBoxFlat style = new()
        {
            BgColor = new Color("#291d45"),
            BorderColor = new Color("#e9a6c8"),
            BorderWidthLeft = 2,
            BorderWidthTop = 2,
            BorderWidthRight = 2,
            BorderWidthBottom = 2,
            CornerRadiusTopLeft = 18,
            CornerRadiusTopRight = 18,
            CornerRadiusBottomLeft = 18,
            CornerRadiusBottomRight = 18,
        };
        return style;
    }
}
