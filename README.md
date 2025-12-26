# 📱 AddonsMobile Framework

[![SMAPI](https://img.shields.io/badge/SMAPI-4.0+-blue.svg)](https://smapi.io/)
[![Stardew Valley](https://img.shields.io/badge/Stardew%20Valley-1.6+-green.svg)](https://www.stardewvalley.net/)
[![Platform](https://img.shields.io/badge/Platform-Android-orange.svg)]()


A powerful framework for Stardew Valley mobile modding that provides a unified button/hotkey system for Android players. This framework allows mod developers to easily add touch-friendly buttons that replace keyboard shortcuts.

---

## 📋 Table of Contents

- [Features](#-features)
- [Installation](#-installation)
- [For Players](#-for-players)
- [For Mod Developers](#-for-mod-developers)
  - [Quick Start](#quick-start)
  - [API Reference](#api-reference)
  - [Button Types](#button-types)
  - [Advanced Usage](#advanced-usage)
  - [Best Practices](#best-practices)
- [Examples](#-examples)
- [Troubleshooting](#-troubleshooting)

---

## ✨ Features

### For Players
- 🎮 **Floating Action Button (FAB)** - Expandable button menu for quick access
- 📍 **Draggable Position** - Place the FAB anywhere on screen
- 🎨 **Customizable Appearance** - Adjust size, opacity, and colors
- 📂 **Category Grouping** - Buttons organized by function
- ⚙️ **GMCM Support** - Easy configuration via Generic Mod Config Menu

### For Mod Developers
- 🔌 **Simple API** - Register buttons with just a few lines of code
- 🏗️ **Builder Pattern** - Fluent interface for complex button configurations
- 🔄 **Three Button Types** - Momentary, Toggle, and Hold behaviors
- 🎯 **Conditional Visibility** - Show/hide buttons based on game state
- 📊 **Priority System** - Control button ordering
- 🔔 **Event System** - React to button registration/unregistration
- 🧵 **Thread-Safe** - Safe for async operations

---

## 📥 Installation

### Requirements
- Stardew Valley 1.6+ (Android)
- SMAPI 4.0+ for Android

### Steps
1. Download the latest release from [Releases](../../releases)
2. Extract `AddonsMobile` folder to your `Mods` directory
3. Launch the game - the FAB will appear when you load a save

---

## 🎮 For Players

### Basic Usage

1. **Open Button Menu**: Tap the floating button (FAB) on screen
2. **Use Buttons**: Tap any button in the expanded menu
3. **Move FAB**: Long-press and drag to reposition
4. **Close Menu**: Tap outside the menu or tap FAB again

### Configuration

Configuration can be done via:
- **Generic Mod Config Menu** (recommended)
- **config.json** file in the mod folder

| Setting | Description | Default |
|---------|-------------|---------|
| `ButtonSize` | Size of buttons in pixels | `64` |
| `Opacity` | Transparency (0.0 - 1.0) | `0.9` |
| `ExpandDirection` | Menu expansion direction | `Up` |
| `ShowTooltips` | Display button names on hover | `true` |

---

## 👨‍💻 For Mod Developers

### Quick Start

#### Step 1: Add Dependency

In your mod's `manifest.json`:

```json
{
  "Name": "Your Mod Name",
  "Author": "Your Name",
  "Version": "1.0.0",
  "UniqueID": "YourName.YourModName",
  "Dependencies": [
    {
      "UniqueID": "YourName.AddonsMobile",
      "MinimumVersion": "1.0.0",
      "IsRequired": false
    }
  ]
}
```

> **Note**: Set `IsRequired: false` if your mod should work on desktop without AddonsMobile.

#### Step 2: Get the API

```csharp
using StardewModdingAPI;

public class ModEntry : Mod
{
    private IMobileAddonsAPI? _mobileAPI;

    public override void Entry(IModHelper helper)
    {
        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        // Get the API
        _mobileAPI = Helper.ModRegistry.GetApi<IMobileAddonsAPI>("YourName.AddonsMobile");
        
        if (_mobileAPI == null)
        {
            Monitor.Log("AddonsMobile not installed - mobile buttons disabled", LogLevel.Info);
            return;
        }

        // Check if running on mobile
        if (!_mobileAPI.IsMobilePlatform)
        {
            Monitor.Log("Not on mobile platform - skipping button registration", LogLevel.Debug);
            return;
        }

        // Register your buttons here
        RegisterMobileButtons();
    }

    private void RegisterMobileButtons()
    {
        // Simple button example
        _mobileAPI!.RegisterSimpleButton(
            uniqueId: "YourName.YourModName.OpenMenu",
            modId: ModManifest.UniqueID,
            displayName: "Open Menu",
            onPress: () => OpenYourMenu(),
            category: KeyCategory.Menu
        );
    }
}
```

#### Step 3: Copy the Interface

Copy the `IMobileAddonsAPI` interface to your mod project:

<details>
<summary>📄 Click to expand IMobileAddonsAPI.cs</summary>

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace YourModNamespace
{
    /// <summary>
    /// Public API for AddonsMobile Framework.
    /// Copy this interface to your mod project.
    /// </summary>
    public interface IMobileAddonsAPI
    {
        // ═══════════════════════════════════════════════════════════
        // METADATA
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>API version (semantic versioning)</summary>
        string ApiVersion { get; }
        
        /// <summary>True if running on Android</summary>
        bool IsMobilePlatform { get; }
        
        /// <summary>True if button UI is currently visible</summary>
        bool IsVisible { get; }

        // ═══════════════════════════════════════════════════════════
        // SIMPLE REGISTRATION
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// Quick way to register a simple momentary button.
        /// </summary>
        bool RegisterSimpleButton(
            string uniqueId,
            string modId,
            string displayName,
            Action onPress,
            KeyCategory category = KeyCategory.Miscellaneous
        );

        // ═══════════════════════════════════════════════════════════
        // BUILDER REGISTRATION
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// Create a button builder for advanced configuration.
        /// </summary>
        IButtonBuilder CreateButton(string uniqueId, string modId);

        // ═══════════════════════════════════════════════════════════
        // MANAGEMENT
        // ═══════════════════════════════════════════════════════════
        
        bool UnregisterButton(string uniqueId);
        int UnregisterAllFromMod(string modId);
        bool SetButtonEnabled(string uniqueId, bool enabled);
        bool SetToggleState(string uniqueId, bool toggled, bool invokeCallback = false);

        // ═══════════════════════════════════════════════════════════
        // QUERIES
        // ═══════════════════════════════════════════════════════════
        
        bool TriggerButton(string uniqueId);
        bool IsButtonRegistered(string uniqueId);
        int GetRegisteredButtonCount();
        int GetButtonCountForMod(string modId);

        // ═══════════════════════════════════════════════════════════
        // UI CONTROL
        // ═══════════════════════════════════════════════════════════
        
        void RefreshUI();
        void SetVisible(bool visible);

        // ═══════════════════════════════════════════════════════════
        // EVENTS
        // ═══════════════════════════════════════════════════════════
        
        event EventHandler<ButtonRegisteredEventArgs> ButtonRegistered;
        event EventHandler<ButtonUnregisteredEventArgs> ButtonUnregistered;
    }

    /// <summary>
    /// Fluent builder for creating buttons with advanced options.
    /// </summary>
    public interface IButtonBuilder
    {
        // Basic
        IButtonBuilder WithDisplayName(string name);
        IButtonBuilder WithDescription(string description);
        IButtonBuilder WithCategory(KeyCategory category);
        IButtonBuilder WithPriority(int priority);
        IButtonBuilder WithKeybind(string keybind);

        // Visual
        IButtonBuilder WithIcon(Texture2D texture, Rectangle? sourceRect = null);
        IButtonBuilder WithTint(Color normalColor, Color? toggledColor = null);

        // Behavior
        IButtonBuilder WithType(ButtonType type);
        IButtonBuilder WithCooldown(int milliseconds);
        IButtonBuilder WithVisibilityCondition(Func<bool> condition);
        IButtonBuilder WithEnabledCondition(Func<bool> condition);

        // Actions
        IButtonBuilder OnPress(Action action);
        IButtonBuilder OnHold(Action<float> action);
        IButtonBuilder OnRelease(Action action);
        IButtonBuilder OnToggle(Action<bool> action);

        // Finalize
        bool Register();
    }

    // ═══════════════════════════════════════════════════════════════
    // ENUMS
    // ═══════════════════════════════════════════════════════════════

    public enum ButtonType
    {
        /// <summary>Single press, executes once</summary>
        Momentary,
        
        /// <summary>Toggle on/off state</summary>
        Toggle,
        
        /// <summary>Continuous action while held</summary>
        Hold
    }

    public enum KeyCategory
    {
        /// <summary>Movement and navigation</summary>
        Movement,
        
        /// <summary>Tools and items</summary>
        Tools,
        
        /// <summary>Menus and UI</summary>
        Menu,
        
        /// <summary>Social and multiplayer</summary>
        Social,
        
        /// <summary>Combat and actions</summary>
        Combat,
        
        /// <summary>Other functions</summary>
        Miscellaneous
    }

    // ═══════════════════════════════════════════════════════════════
    // EVENT ARGS
    // ═══════════════════════════════════════════════════════════════

    public class ButtonRegisteredEventArgs : EventArgs
    {
        public string UniqueId { get; }
        public string ModId { get; }
        public string DisplayName { get; }
        public bool IsUpdate { get; }

        public ButtonRegisteredEventArgs(string uniqueId, string modId, string displayName, bool isUpdate)
        {
            UniqueId = uniqueId;
            ModId = modId;
            DisplayName = displayName;
            IsUpdate = isUpdate;
        }
    }

    public class ButtonUnregisteredEventArgs : EventArgs
    {
        public string UniqueId { get; }
        public string ModId { get; }

        public ButtonUnregisteredEventArgs(string uniqueId, string modId)
        {
            UniqueId = uniqueId;
            ModId = modId;
        }
    }
}
```

</details>

---

### API Reference

#### Registration Methods

##### `RegisterSimpleButton()`

Quick registration for simple momentary buttons.

```csharp
bool RegisterSimpleButton(
    string uniqueId,      // Unique button ID (recommended: "ModId.ButtonName")
    string modId,         // Your mod's UniqueID
    string displayName,   // Text shown in UI
    Action onPress,       // Action when pressed
    KeyCategory category  // Category for grouping (default: Miscellaneous)
);
```

**Returns**: `true` if registration successful, `false` if ID already exists.

##### `CreateButton()`

Create a builder for advanced button configuration.

```csharp
IButtonBuilder CreateButton(string uniqueId, string modId);
```

---

#### IButtonBuilder Methods

| Method | Description |
|--------|-------------|
| `WithDisplayName(string)` | Set the display name (required) |
| `WithDescription(string)` | Set tooltip description |
| `WithCategory(KeyCategory)` | Set category for grouping |
| `WithPriority(int)` | Set sort priority (0-1000, higher = first) |
| `WithKeybind(string)` | Set original keybind for reference |
| `WithIcon(Texture2D, Rectangle?)` | Set custom icon texture |
| `WithTint(Color, Color?)` | Set normal and toggled colors |
| `WithType(ButtonType)` | Set button behavior type |
| `WithCooldown(int)` | Set press cooldown in milliseconds |
| `WithVisibilityCondition(Func<bool>)` | Set when button is visible |
| `WithEnabledCondition(Func<bool>)` | Set when button is enabled |
| `OnPress(Action)` | Set press action |
| `OnHold(Action<float>)` | Set hold action (receives delta time) |
| `OnRelease(Action)` | Set release action |
| `OnToggle(Action<bool>)` | Set toggle action (receives new state) |
| `Register()` | Register the button (call last) |

---

#### Management Methods

```csharp
// Remove a specific button
bool UnregisterButton(string uniqueId);

// Remove all buttons from a mod
int UnregisterAllFromMod(string modId);

// Enable/disable a button
bool SetButtonEnabled(string uniqueId, bool enabled);

// Set toggle state programmatically
bool SetToggleState(string uniqueId, bool toggled, bool invokeCallback = false);

// Trigger a button programmatically
bool TriggerButton(string uniqueId);
```

---

#### Query Methods

```csharp
// Check if button exists
bool IsButtonRegistered(string uniqueId);

// Get total button count
int GetRegisteredButtonCount();

// Get button count for a specific mod
int GetButtonCountForMod(string modId);
```

---

#### UI Control

```csharp
// Refresh button display (after bulk changes)
void RefreshUI();

// Show/hide all buttons
void SetVisible(bool visible);

// Check visibility
bool IsVisible { get; }
```

---

### Button Types

#### Momentary (Default)

Single press button. Action executes once per tap.

```csharp
api.CreateButton("MyMod.UseItem", manifest.UniqueID)
    .WithDisplayName("Use Item")
    .WithType(ButtonType.Momentary)  // Optional, this is default
    .OnPress(() => UseCurrentItem())
    .Register();
```

**Use cases**: Open menu, use item, trigger one-time action

---

#### Toggle

On/Off state button. Toggles between two states.

```csharp
api.CreateButton("MyMod.AutoRun", manifest.UniqueID)
    .WithDisplayName("Auto Run")
    .WithType(ButtonType.Toggle)
    .WithTint(Color.White, Color.LightGreen)  // Normal, Toggled
    .OnToggle(isOn => {
        if (isOn)
            StartAutoRun();
        else
            StopAutoRun();
    })
    .Register();
```

**Use cases**: Enable/disable features, toggle modes

---

#### Hold

Continuous action while held. Good for movement or charging.

```csharp
api.CreateButton("MyMod.Sprint", manifest.UniqueID)
    .WithDisplayName("Sprint")
    .WithType(ButtonType.Hold)
    .OnPress(() => StartSprinting())
    .OnHold(deltaTime => {
        // Called every frame while held
        DrainStamina(deltaTime * 2f);
    })
    .OnRelease(() => StopSprinting())
    .Register();
```

**Use cases**: Sprint, charge attack, continuous movement

---

### Advanced Usage

#### Conditional Visibility

Show buttons only when relevant:

```csharp
api.CreateButton("MyMod.FishingCast", manifest.UniqueID)
    .WithDisplayName("Cast")
    .WithCategory(KeyCategory.Tools)
    .WithVisibilityCondition(() => 
        Game1.player.CurrentTool is FishingRod)
    .OnPress(() => CastFishingRod())
    .Register();
```

#### Conditional Enabled State

Disable buttons based on conditions:

```csharp
api.CreateButton("MyMod.SpecialAbility", manifest.UniqueID)
    .WithDisplayName("Special")
    .WithEnabledCondition(() => 
        Game1.player.Stamina >= 20)
    .OnPress(() => UseSpecialAbility())
    .Register();
```

#### Custom Icons

Use your own textures:

```csharp
// Load texture in Entry or GameLaunched
var iconTexture = Helper.ModContent.Load<Texture2D>("assets/icons.png");

api.CreateButton("MyMod.MyButton", manifest.UniqueID)
    .WithDisplayName("My Button")
    .WithIcon(iconTexture, new Rectangle(0, 0, 16, 16))  // Source rect in spritesheet
    .OnPress(() => DoSomething())
    .Register();
```

#### Priority System

Control button order (higher priority = appears first):

```csharp
// This appears first
api.CreateButton("MyMod.Important", manifest.UniqueID)
    .WithDisplayName("Important")
    .WithPriority(100)
    .OnPress(() => ImportantAction())
    .Register();

// This appears after
api.CreateButton("MyMod.Secondary", manifest.UniqueID)
    .WithDisplayName("Secondary")
    .WithPriority(50)
    .OnPress(() => SecondaryAction())
    .Register();
```

#### Event Handling

React to button changes:

```csharp
api.ButtonRegistered += (sender, e) => {
    Monitor.Log($"Button registered: {e.DisplayName} from {e.ModId}");
};

api.ButtonUnregistered += (sender, e) => {
    Monitor.Log($"Button unregistered: {e.UniqueId}");
};
```

#### Dynamic Button Updates

Update buttons at runtime:

```csharp
// Disable during cutscenes
helper.Events.GameLoop.UpdateTicked += (s, e) => {
    if (Game1.eventUp)
        api.SetButtonEnabled("MyMod.MyButton", false);
    else
        api.SetButtonEnabled("MyMod.MyButton", true);
};
```

---

### Best Practices

#### ✅ DO

```csharp
// ✅ Use consistent naming: "ModId.ButtonName"
api.RegisterSimpleButton(
    uniqueId: "AuthorName.ModName.ButtonName",
    modId: ModManifest.UniqueID,
    // ...
);

// ✅ Check platform before registering
if (_mobileAPI?.IsMobilePlatform == true)
{
    RegisterMobileButtons();
}

// ✅ Use appropriate categories
.WithCategory(KeyCategory.Tools)    // For tool-related actions
.WithCategory(KeyCategory.Menu)     // For UI/menu actions
.WithCategory(KeyCategory.Movement) // For movement actions

// ✅ Provide meaningful descriptions
.WithDescription("Opens the cooking menu when near a kitchen")

// ✅ Set appropriate cooldowns for spam-prone actions
.WithCooldown(500)  // 500ms between presses

// ✅ Clean up when needed
helper.Events.GameLoop.ReturnedToTitle += (s, e) => {
    _mobileAPI?.UnregisterAllFromMod(ModManifest.UniqueID);
};

// ✅ Handle null API gracefully
if (_mobileAPI == null) return;
```

#### ❌ DON'T

```csharp
// ❌ Don't use duplicate IDs
api.RegisterSimpleButton("button1", ...);  // Too generic!

// ❌ Don't register before GameLaunched
public override void Entry(IModHelper helper)
{
    var api = Helper.ModRegistry.GetApi<IMobileAddonsAPI>(...);
    api.RegisterSimpleButton(...);  // Too early! API might not exist yet
}

// ❌ Don't forget to call Register() on builders
api.CreateButton("MyMod.Button", modId)
    .WithDisplayName("Button")
    .OnPress(() => DoSomething());
    // Missing .Register() - button won't be created!

// ❌ Don't block in callbacks
.OnHold(deltaTime => {
    Thread.Sleep(100);  // Never do this!
})

// ❌ Don't ignore exceptions in callbacks
.OnPress(() => {
    // Always handle potential errors
    try {
        RiskyOperation();
    } catch (Exception ex) {
        Monitor.Log($"Error: {ex.Message}", LogLevel.Error);
    }
})
```

---

## 📚 Examples

### Example 1: Simple Mod Integration

```csharp
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace MySimpleMod
{
    public class ModEntry : Mod
    {
        private IMobileAddonsAPI? _mobileAPI;

        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            _mobileAPI = Helper.ModRegistry.GetApi<IMobileAddonsAPI>("AuthorName.AddonsMobile");
            
            if (_mobileAPI?.IsMobilePlatform != true)
                return;

            // Register a simple button
            _mobileAPI.RegisterSimpleButton(
                uniqueId: $"{ModManifest.UniqueID}.ShowInfo",
                modId: ModManifest.UniqueID,
                displayName: "Show Info",
                onPress: ShowPlayerInfo,
                category: KeyCategory.Menu
            );
        }

        private void ShowPlayerInfo()
        {
            var player = StardewValley.Game1.player;
            StardewValley.Game1.addHUDMessage(
                new StardewValley.HUDMessage($"Money: {player.Money}g", 2)
            );
        }
    }
}
```

### Example 2: Toggle Feature

```csharp
private bool _autoEatEnabled = false;

private void RegisterMobileButtons()
{
    _mobileAPI!.CreateButton($"{ModManifest.UniqueID}.AutoEat", ModManifest.UniqueID)
        .WithDisplayName("Auto Eat")
        .WithDescription("Automatically eat food when health is low")
        .WithCategory(KeyCategory.Combat)
        .WithType(ButtonType.Toggle)
        .WithTint(Color.White, Color.LightGreen)
        .OnToggle(isEnabled => {
            _autoEatEnabled = isEnabled;
            string status = isEnabled ? "enabled" : "disabled";
            Game1.addHUDMessage(new HUDMessage($"Auto Eat {status}", 2));
        })
        .Register();
}
```

### Example 3: Context-Sensitive Buttons

```csharp
private void RegisterMobileButtons()
{
    // Only show when holding a tool
    _mobileAPI!.CreateButton($"{ModManifest.UniqueID}.UseTool", ModManifest.UniqueID)
        .WithDisplayName("Use Tool")
        .WithCategory(KeyCategory.Tools)
        .WithVisibilityCondition(() => Game1.player.CurrentTool != null)
        .WithEnabledCondition(() => Game1.player.canMove && !Game1.player.UsingTool)
        .OnPress(() => {
            // Simulate tool use
            Game1.pressUseToolButton();
        })
        .Register();

    // Only show near animals
    _mobileAPI.CreateButton($"{ModManifest.UniqueID}.PetAnimal", ModManifest.UniqueID)
        .WithDisplayName("Pet Animal")
        .WithCategory(KeyCategory.Social)
        .WithVisibilityCondition(() => IsNearAnimal())
        .OnPress(() => PetNearbyAnimal())
        .Register();
}

private bool IsNearAnimal()
{
    // Check if player is near any farm animal
    var playerTile = Game1.player.Tile;
    return Game1.currentLocation?.animals.Values
        .Any(a => Vector2.Distance(a.Tile, playerTile) < 2f) ?? false;
}
```

### Example 4: Hold Button for Charging

```csharp
private float _chargeLevel = 0f;

private void RegisterMobileButtons()
{
    _mobileAPI!.CreateButton($"{ModManifest.UniqueID}.ChargeAttack", ModManifest.UniqueID)
        .WithDisplayName("Charge")
        .WithCategory(KeyCategory.Combat)
        .WithType(ButtonType.Hold)
        .OnPress(() => {
            _chargeLevel = 0f;
            Game1.addHUDMessage(new HUDMessage("Charging...", 2));
        })
        .OnHold(deltaTime => {
            _chargeLevel = Math.Min(_chargeLevel + deltaTime, 3f);  // Max 3 seconds
            // Update UI or visual feedback here
        })
        .OnRelease(() => {
            PerformChargedAttack(_chargeLevel);
            _chargeLevel = 0f;
        })
        .Register();
}

private void PerformChargedAttack(float charge)
{
    int damage = (int)(charge * 10);  // 10 damage per second charged
    Game1.addHUDMessage(new HUDMessage($"Released! Damage: {damage}", 2));
}
```

### Example 5: Multiple Buttons with Categories

```csharp
private void RegisterMobileButtons()
{
    var modId = ModManifest.UniqueID;

    // Movement category
    _mobileAPI!.CreateButton($"{modId}.Sprint", modId)
        .WithDisplayName("Sprint")
        .WithCategory(KeyCategory.Movement)
        .WithPriority(100)
        .WithType(ButtonType.Hold)
        .OnPress(() => Game1.player.addedSpeed = 3)
        .OnRelease(() => Game1.player.addedSpeed = 0)
        .Register();

    // Tools category  
    _mobileAPI.CreateButton($"{modId}.QuickSlot1", modId)
        .WithDisplayName("Slot 1")
        .WithCategory(KeyCategory.Tools)
        .WithPriority(90)
        .OnPress(() => Game1.player.CurrentToolIndex = 0)
        .Register();

    _mobileAPI.CreateButton($"{modId}.QuickSlot2", modId)
        .WithDisplayName("Slot 2")
        .WithCategory(KeyCategory.Tools)
        .WithPriority(89)
        .OnPress(() => Game1.player.CurrentToolIndex = 1)
        .Register();

    // Menu category
    _mobileAPI.CreateButton($"{modId}.OpenInventory", modId)
        .WithDisplayName("Inventory")
        .WithCategory(KeyCategory.Menu)
        .WithPriority(80)
        .OnPress(() => Game1.activeClickableMenu = new InventoryPage(0, 0, 0, 0))
        .Register();
}
```

---

## ❓ Troubleshooting

### Button Not Appearing

1. **Check platform**: Buttons only appear on Android
   ```csharp
   if (!_mobileAPI.IsMobilePlatform) return;
   ```

2. **Verify registration timing**: Register in `GameLaunched` event
   ```csharp
   helper.Events.GameLoop.GameLaunched += OnGameLaunched;
   ```

3. **Check visibility condition**: If set, ensure it returns `true`

4. **Verify API is loaded**: Check SMAPI log for AddonsMobile initialization

### Button Not Responding

1. **Check enabled condition**: Ensure it returns `true`

2. **Check cooldown**: Default is 250ms, might need adjustment

3. **Verify action is set**: Builder requires at least one action
   ```csharp
   .OnPress(() => DoSomething())  // Required!
   .Register();
   ```

### Console Commands (Debug)

```
> addons_list          # List all registered buttons
> addons_trigger <id>  # Trigger a button by ID
> addons_reset         # Reset FAB position
```

### Common Errors

| Error | Cause | Solution |
|-------|-------|----------|
| `API is null` | AddonsMobile not installed | Check dependencies |
| `UniqueId already exists` | Duplicate button ID | Use unique IDs |
| `No action defined` | Missing OnPress/OnToggle/OnHold | Add at least one action |
| `ModId must be set` | Forgot WithModId() | Use modId parameter |

---

### Development Setup

```bash
git clone https://github.com/YourUsername/AddonsMobile.git
cd AddonsMobile
dotnet restore
dotnet build
```

### Code Style

- Follow C# naming conventions
- Use XML documentation for public APIs
- Add unit tests for new features
- Update README for API changes

---

## 🙏 Credits

- **SMAPI Team** - For the amazing modding API
- **ConcernedApe** - For creating Stardew Valley
- **Android SMAPI Porters** - For making mobile modding possible

---

## 📞 Support

- **Issues**: [GitHub Issues](../../issues)
- **Discussions**: [GitHub Discussions](../../discussions)
- **Discord**: [Stardew Valley Discord](https://discord.gg/stardewvalley) - #modding-mobile

---

<p align="center">
  Made with ❤️ for the Stardew Valley modding community
</p>
```

---
