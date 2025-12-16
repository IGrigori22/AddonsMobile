# 📱 Addons Mobile - Virtual Button Manager for Stardew Valley Android

A centralized virtual button manager for Stardew Valley Android mods. Allows mod developers to register custom buttons without keybind conflicts.

![SMAPI](https://img.shields.io/badge/SMAPI-4.0%2B-green)
![Platform](https://img.shields.io/badge/Platform-Android-blue)
![Version](https://img.shields.io/badge/Version-1.0.0-orange)

---

## 📋 Table of Contents

- [For Players](#-for-players)
- [For Mod Developers](#-for-mod-developers)
  - [Quick Start](#-quick-start)
  - [Step-by-Step Guide](#-step-by-step-guide)
  - [API Reference](#-api-reference)
  - [Full Example](#-full-example)
- [Configuration](#-configuration)
- [FAQ](#-faq)

---

## 🎮 For Players

### What does this mod do?

This mod adds a **floating action button (FAB)** on your screen. When you tap it, a menu expands showing buttons from other mods that support Addons Mobile.






### Installation

1. Install [SMAPI](https://smapi.io/) for Android
2. Download **Addons Mobile** from Nexus Mods
3. Extract to `StardewValley/Mods/` folder
4. Done! The FAB will appear when you load a save

---

## 👨‍💻 For Mod Developers

### 🚀 Quick Start

**3 simple steps to add your button:**

```csharp
// 1. Get the API
var api = Helper.ModRegistry.GetApi<IMobileAddonsAPI>("Grigori22.AddonsMobile");

// 2. Create and register your button
api?.CreateButton("YourMod.OpenMenu", ModManifest.UniqueID)
    .WithDisplayName("My Menu")
    .OnPressed(() => OpenYourMenu())
    .Register();

// 3. That's it! ✅




📖 Step-by-Step Guide
Step 1: Add Dependency (Optional)
In your manifest.json, add Addons Mobile as an optional dependency:

{
    "Name": "Your Mod Name",
    "Author": "Your Name",
    "Version": "1.0.0",
    "UniqueID": "YourName.YourMod",
    "EntryDll": "YourMod.dll",
    "MinimumApiVersion": "4.0.0",
    "Dependencies": [
        {
            "UniqueID": "IGrigori22.AddonsMobile",
            "IsRequired": false
        }
    ]
}


Step 2: Copy the API Interface
Create a new file API/IMobileAddonsAPI.cs in your project:

namespace YourMod.API
{
    public interface IMobileAddonsAPI
    {
        string Version { get; }
        bool IsMobilePlatform { get; }
        
        IButtonBuilder CreateButton(string uniqueId, string modId);
        bool UnregisterButton(string uniqueId);
        void UnregisterAllFromMod(string modId);
        bool SetButtonEnabled(string uniqueId, bool enabled);
        bool TriggerButton(string uniqueId);
        int GetRegisteredButtonCount();
        bool IsButtonRegistered(string uniqueId);
        void RefreshUI();
        void SetVisible(bool visible);
    }

    public interface IButtonBuilder
    {
        IButtonBuilder WithDisplayName(string name);
        IButtonBuilder WithDescription(string description);
        IButtonBuilder WithCategory(KeyCategory category);
        IButtonBuilder WithPriority(int priority);
        IButtonBuilder WithCooldown(int milliseconds);
        IButtonBuilder WithVisibilityCondition(Func<bool> condition);
        IButtonBuilder WithOriginalKeybind(string keybind);
        IButtonBuilder OnPressed(Action action);
        bool Register();
    }

    public enum KeyCategory
    {
        Menu,
        Farming,
        Tools,
        Cheats,
        Information,
        Social,
        Inventory,
        Teleport,
        Miscellaneous
    }
}


Step 3: Get API and Register Buttons
In your ModEntry.cs:

using YourMod.API;

public class ModEntry : Mod
{
    private IMobileAddonsAPI? _mobileApi;

    public override void Entry(IModHelper helper)
    {
        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        // Get the API
        _mobileApi = Helper.ModRegistry.GetApi<IMobileAddonsAPI>("IGrigori22.AddonsMobile");

        if (_mobileApi != null)
        {
            RegisterMobileButtons();
            Monitor.Log($"Connected to Addons Mobile v{_mobileApi.Version}", LogLevel.Info);
        }
        else
        {
            Monitor.Log("Addons Mobile not installed", LogLevel.Debug);
        }
    }

    private void RegisterMobileButtons()
    {
        _mobileApi!.CreateButton("YourMod.OpenMenu", ModManifest.UniqueID)
            .WithDisplayName("My Menu")
            .WithDescription("Opens my custom menu")
            .WithCategory(KeyCategory.Menu)
            .WithPriority(50)
            .OnPressed(() => 
            {
                Game1.activeClickableMenu = new YourCustomMenu();
            })
            .Register();
    }
}


📚 API Reference
Button Builder Methods
Method	Description	Required
| `.WithDisplayName(string)` | `✅ Yes` | Button label shown in menu |
| `.WithDisplayName(string)` | `✅ Yes` | Button label shown in menu | 
| `.WithDescription(string)` | `No` | Tooltip description	|
| `.WithCategory(KeyCategory)` | `No` |	Category for grouping	|
| `.WithPriority(int)` | `No` |	Higher = shown first (default: 0)	|
| `.WithCooldown(int)` | `No` |	Milliseconds between presses (default: 250)	|
| `.WithVisibilityCondition(Func<bool>)` | `No` |	When to show button	|
| `.WithOriginalKeybind(string)` | `No` |	Desktop keybind reference	|
| `.OnPressed(Action)` | `✅ Yes` |	Action when tapped	|
| `.Register()` | `✅ Yes` |	Finalize registration	|





KeyCategory.Menu          // Menu/UI mods
KeyCategory.Farming       // Farming tools
KeyCategory.Tools         // Tool utilities  
KeyCategory.Cheats        // Cheat/debug mods
KeyCategory.Information   // Info overlays
KeyCategory.Social        // NPC/relationship mods
KeyCategory.Inventory     // Inventory management
KeyCategory.Teleport      // Warp/teleport mods
KeyCategory.Miscellaneous // Other (default)


// Check if on mobile
bool isMobile = api.IsMobilePlatform;

// Get API version
string version = api.Version;

// Check if button exists
bool exists = api.IsButtonRegistered("YourMod.ButtonId");

// Enable/disable button
api.SetButtonEnabled("YourMod.ButtonId", false);

// Remove button
api.UnregisterButton("YourMod.ButtonId");

// Remove all your buttons
api.UnregisterAllFromMod(ModManifest.UniqueID);

// Trigger button programmatically
api.TriggerButton("YourMod.ButtonId");

// Refresh UI after changes
api.RefreshUI();

// Show/hide the FAB
api.SetVisible(false);


💡 Full Example
Here's a complete example with multiple buttons and fallback support:

using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using YourMod.API;

namespace YourMod
{
    public class ModEntry : Mod
    {
        private IMobileAddonsAPI? _mobileApi;

        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            // Try to get Addons Mobile API
            _mobileApi = Helper.ModRegistry.GetApi<IMobileAddonsAPI>(
                "Grigori22.AddonsMobile"
            );

            if (_mobileApi != null)
            {
                RegisterMobileButtons();
                Monitor.Log("Mobile buttons registered!", LogLevel.Info);
            }
        }

        private void RegisterMobileButtons()
        {
            if (_mobileApi == null) return;

            // Button 1: Open Menu
            _mobileApi.CreateButton("MyMod.OpenMenu", ModManifest.UniqueID)
                .WithDisplayName("📋 Menu")
                .WithDescription("Open the main menu")
                .WithCategory(KeyCategory.Menu)
                .WithPriority(100)
                .WithOriginalKeybind("F5")
                .OnPressed(OpenMainMenu)
                .Register();

            // Button 2: Quick Action
            _mobileApi.CreateButton("MyMod.QuickHeal", ModManifest.UniqueID)
                .WithDisplayName("❤️ Heal")
                .WithDescription("Restore health")
                .WithCategory(KeyCategory.Cheats)
                .WithPriority(50)
                .WithCooldown(1000) // 1 second cooldown
                .OnPressed(() => 
                {
                    Game1.player.health = Game1.player.maxHealth;
                    Game1.addHUDMessage(new HUDMessage("Healed!", HUDMessage.health_type));
                })
                .Register();

            // Button 3: Conditional button (only shows at farm)
            _mobileApi.CreateButton("MyMod.FarmAction", ModManifest.UniqueID)
                .WithDisplayName("🌾 Farm")
                .WithDescription("Farm-only action")
                .WithCategory(KeyCategory.Farming)
                .WithPriority(30)
                .WithVisibilityCondition(() => 
                    Game1.player?.currentLocation?.IsFarm == true
                )
                .OnPressed(DoFarmAction)
                .Register();
        }

        // Fallback: Desktop keybind
        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady) return;

            if (e.Button == SButton.F5)
            {
                OpenMainMenu();
                Helper.Input.Suppress(e.Button);
            }
        }

        private void OpenMainMenu()
        {
            if (!Context.IsWorldReady) return;
            
            Game1.activeClickableMenu = new MyCustomMenu();
        }

        private void DoFarmAction()
        {
            Game1.addHUDMessage(new HUDMessage("Farm action!", HUDMessage.newQuest_type));
        }
    }
}



⚙️ Configuration
Players can configure the FAB in config.json:

{
    "ButtonPositionX": 95,
    "ButtonPositionY": 50,
    "ButtonSize": 64,
    "MenuButtonSize": 56,
    "ButtonSpacing": 8,
    "MaxButtonsPerRow": 4,
    "ButtonOpacity": 0.85,
    "ShowButtonLabels": true,
    "AutoHideInEvents": true,
    "AnimationDuration": 200,
    "VerboseLogging": false
}


Setting	Description	Default
ButtonPositionX	Horizontal position (0-100%)	95
ButtonPositionY	Vertical position (0-100%)	50
ButtonSize	FAB size in pixels	64
MenuButtonSize	Menu button size	56
ButtonOpacity	Transparency (0.3-1.0)	0.85
ShowButtonLabels	Show text labels	true
AutoHideInEvents	Hide during cutscenes	true


❓ FAQ
Q: My button doesn't appear!
A: Check these things:

Is Addons Mobile installed?
Did you call .Register() at the end?
Did you provide .WithDisplayName() and .OnPressed()?
Check SMAPI log for errors
Q: API returns null!
A: Make sure:

Addons Mobile is installed in Mods folder
UniqueID is exactly "Grigori22.AddonsMobile"
You're getting API in GameLaunched event, not Entry()
Q: Button shows but can't be clicked!
A: The FAB only works when:

No menu is open
No event/cutscene is playing
Player is in the world
Q: Can I use custom icons?
A: Yes! Use WithIcon():



.WithIcon(yourTexture, new Rectangle(0, 0, 16, 16))



Q: How do I update button visibility?
A: Use WithVisibilityCondition():

.WithVisibilityCondition(() => SomeCondition())

The condition is checked every time the menu expands.