using Godot;
using System.Collections.Generic;

/// <summary>
/// 全遊戲輸入設定服務。
/// 角色與 UI 只依賴 action 名稱，不直接寫死按鍵，方便未來擴充與重綁。
/// </summary>
public partial class InputSettings : Node
{
    public const string MoveUp = "move_up";
    public const string MoveDown = "move_down";
    public const string MoveLeft = "move_left";
    public const string MoveRight = "move_right";
    public const string Spell1 = "spell_1";
    public const string Spell2 = "spell_2";
    public const string Spell3 = "spell_3";
    public const string Spell4 = "spell_4";

    private const string BindingSection = "bindings";
    private const string BindingFile = "user://keybindings.cfg";

    private readonly Dictionary<string, Key> _defaults = new()
    {
        [MoveUp] = Key.W,
        [MoveDown] = Key.S,
        [MoveLeft] = Key.A,
        [MoveRight] = Key.D,
        [Spell1] = Key.Key1,
        [Spell2] = Key.Key2,
        [Spell3] = Key.Key3,
        [Spell4] = Key.Key4,
    };

    private readonly Dictionary<string, Key> _bindings = new();
    private ConfigFile _config = new();

    public override void _Ready()
    {
        LoadBindings();
    }

    public Key GetBinding(string actionName)
    {
        return _bindings.TryGetValue(actionName, out Key key)
            ? key
            : GetDefault(actionName);
    }

    public Key GetDefault(string actionName)
    {
        return _defaults.TryGetValue(actionName, out Key key)
            ? key
            : Key.None;
    }

    public string GetBindingText(string actionName)
    {
        Key key = GetBinding(actionName);
        return key == Key.None ? "未設定" : OS.GetKeycodeString(key);
    }

    public bool Rebind(string actionName, Key key)
    {
        if (!_defaults.ContainsKey(actionName) || key == Key.None)
            return false;

        EnsureAction(actionName);
        InputMap.ActionEraseEvents(actionName);

        InputEventKey keyEvent = new()
        {
            PhysicalKeycode = key,
            Pressed = false,
        };
        InputMap.ActionAddEvent(actionName, keyEvent);

        _bindings[actionName] = key;
        _config.SetValue(BindingSection, actionName, (long)key);
        _config.Save(BindingFile);
        return true;
    }

    public void ResetToDefaults()
    {
        _config = new ConfigFile();
        _bindings.Clear();

        foreach (KeyValuePair<string, Key> pair in _defaults)
        {
            EnsureAction(pair.Key);
            InputMap.ActionEraseEvents(pair.Key);

            InputEventKey keyEvent = new()
            {
                PhysicalKeycode = pair.Value,
                Pressed = false,
            };
            InputMap.ActionAddEvent(pair.Key, keyEvent);
            _bindings[pair.Key] = pair.Value;
        }

        _config.Save(BindingFile);
    }

    private void LoadBindings()
    {
        _config = new ConfigFile();
        _config.Load(BindingFile);

        foreach (KeyValuePair<string, Key> pair in _defaults)
        {
            Key key = pair.Value;
            Variant stored = _config.GetValue(BindingSection, pair.Key, (long)pair.Value);
            if (stored.VariantType == Variant.Type.Int)
                key = (Key)stored.AsInt64();

            _bindings[pair.Key] = key;
            EnsureAction(pair.Key);
            InputMap.ActionEraseEvents(pair.Key);

            InputEventKey keyEvent = new()
            {
                PhysicalKeycode = key,
                Pressed = false,
            };
            InputMap.ActionAddEvent(pair.Key, keyEvent);
        }
    }

    private static void EnsureAction(string actionName)
    {
        if (!InputMap.HasAction(actionName))
            InputMap.AddAction(actionName);
    }
}
