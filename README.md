# 腐化童話：夜守者

Windows 2D 俯視角魔法塔防原型，使用 **Godot 4.7.2 .NET + C#** 製作。

## 目前已完成

目前版本包含角色 WASD 移動、自動鎖定最近敵人的普通攻擊、四種主動法術、法力值與冷卻時間、法術鍵位自訂與保存、妖怪波次、魔法塔放置、希望篝火生命值、關卡完成與失敗、關卡選擇、逐步解鎖、已解鎖關卡重玩，以及關卡結束時停止塔樓攻擊。

主動法術預設使用數字鍵 **1、2、3、4**。在遊戲中按 **F1** 開啟鍵位設定，點選要修改的法術後按下新按鍵，設定會保存到 `user://keybindings.cfg`。按 **Esc** 可以取消正在進行的重綁。

## 操作

| 操作 | 預設按鍵 |
|---|---|
| 移動 | W、A、S、D |
| 主動法術 1–4 | 1、2、3、4 |
| 建造魔法塔 | 滑鼠左鍵點擊戰場空地 |
| 開啟鍵位設定 | F1 |
| 重新開始目前關卡 | F2 |
| 返回關卡地圖 | F3 |

角色的普通攻擊不需要按鍵，會自動攻擊射程內最近的妖怪。四種魔法塔會依序建造為火焰、冰霜、雷電與自然塔。

## 專案結構

`project.godot` 保存 Godot 專案設定與啟動場景；`scenes/LevelSelect.tscn` 是關卡選擇畫面；`scenes/Main.tscn` 是戰鬥場景；`scripts/InputSettings.cs` 是全遊戲鍵位服務；`scripts/ProgressManager.cs` 保存關卡解鎖進度；`scripts/LevelData.cs` 集中管理關卡資料與遊戲平衡常數；`scripts/LevelSelect.cs` 管理關卡地圖介面；`scripts/Player.cs` 管理移動、自動普攻、法力與法術；`scripts/KeybindMenu.cs` 管理遊戲內重綁介面；`scripts/Enemy.cs`、`scripts/Projectile.cs`、`scripts/SpellBurst.cs` 與 `scripts/Tower.cs` 分別管理敵人、投射物、法術特效與魔法塔；`scripts/Main.cs` 負責波次、UI、建塔、通關與失敗流程。

## 執行環境

本儲存庫**不包含 Godot 編輯器**。`tools/` 目錄在本機 `.gitignore` 中被排除，因此從 GitHub clone 下來後，請自行安裝 **Godot 4.7.2 .NET／C# 版本**，不要使用標準版 Godot。可以從 [Godot 官方下載頁](https://godotengine.org/download/archive/4.7.2-stable/) 取得對應版本。

本機開發資料夾若已有可攜式編輯器，可以直接使用：

```text
C:\Game\tools\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64.exe
```

這個路徑只適用於已配置本機工具的環境，**不代表該檔案存在於 GitHub 儲存庫**。

## 開啟與建置

開啟 Godot 4 .NET 後，選擇本資料夾中的 `project.godot`。啟動遊戲時會先進入 `scenes/LevelSelect.tscn` 關卡選擇畫面；也可以在 Godot 中直接開啟 `scenes/Main.tscn` 測試戰鬥場景。

若要從命令列建置，Windows 需要安裝 .NET 8 SDK，並在專案根目錄執行：

```powershell
dotnet build FairyCorruptionTD.csproj
```

正式匯出時，請在 Godot 的 Project > Export 中安裝或指定 Windows Desktop 匯出模板，再輸出 x86_64 版本。

## 資產說明

第一關背景使用 `assets/frosting_forest_visual_target_1280.png`，尺寸為 1280×720，約 1.7 MB。它是原始高解析背景的原型最佳化版本，降低了儲存與載入成本；若未來需要高解析版本，可以另外放入未納入原型提交的外部美術資產，而不必把大型原圖放進主儲存庫。

## 重置解鎖進度

若要重新測試逐步解鎖，可以刪除 Godot 使用者存檔中的：

```text
fairy_corruption_progress.cfg
```

通常位於：

```text
%APPDATA%\Godot\app_userdata\腐化童話：夜守者\
```

刪除後重新啟動遊戲，應會恢復成只有第 1 關解鎖。
