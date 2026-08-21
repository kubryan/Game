# 腐化童話：夜守者

Windows 2D 俯視角魔法塔防原型，使用 **Godot 4.7.2 .NET + C#** 製作。

## 目前已完成

第一個核心系統已完成，包含角色的 WASD 移動、角色自動鎖定最近敵人的普通攻擊、四種主動法術、法力值與冷卻時間、法術鍵位自訂與保存、妖怪波次、魔法塔放置、希望篝火生命值、關卡完成與失敗，以及 F2 重新挑戰。

主動法術預設使用數字鍵 **1、2、3、4**。在遊戲中按 **F1** 開啟鍵位設定，點選要修改的法術後按下新按鍵，設定會保存到 `user://keybindings.cfg`。按 **Esc** 可以取消正在進行的重綁。

## 操作

| 操作 | 預設按鍵 |
|---|---|
| 移動 | W、A、S、D |
| 主動法術 1–4 | 1、2、3、4 |
| 建造魔法塔 | 滑鼠左鍵點擊戰場空地，每座消耗 40 星砂 |
| 開啟鍵位設定 | F1 |
| 重新開始目前關卡 | F2 |

角色的普通攻擊不需要按鍵，會自動攻擊射程內最近的妖怪。四種魔法塔會依序建造為火焰、冰霜、雷電與自然塔。

## 專案結構

`project.godot` 保存視窗與輸入動作設定；`scripts/InputSettings.cs` 是全遊戲鍵位服務；`scripts/Player.cs` 管理移動、自動普攻、法力與法術；`scripts/KeybindMenu.cs` 管理遊戲內重綁介面；`scripts/Enemy.cs`、`scripts/Projectile.cs`、`scripts/SpellBurst.cs` 與 `scripts/Tower.cs` 分別管理敵人、投射物、法術特效與魔法塔；`scripts/Main.cs` 負責測試關卡、波次、UI 與通關流程。

## 執行

請使用 **Godot 4 .NET** 開啟本資料夾，而不是標準版 Godot。專案內已放置可攜式編輯器於 `tools/Godot_v4.7.2-stable_mono_win64/`；也可以從官方網站下載同版本的 .NET 編輯器。開啟專案後按右上角 Play，或直接執行主場景 `scenes/Main.tscn`。

若要從命令列建置，Windows 需要安裝 .NET 8 SDK，並在專案根目錄執行：

```powershell
dotnet build FairyCorruptionTD.csproj
```

若 Windows 工作階段缺少 `ProgramFiles` 系統環境變數，請先修正系統環境或直接使用 Godot 編輯器的 Build 按鈕。正式匯出時，請在 Godot 的 Project > Export 中加入 Windows Desktop 預設，再輸出 x86_64 版本。
