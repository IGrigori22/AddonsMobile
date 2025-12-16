using AddonsMobile.API;
using AddonsMobile.Config;
using AddonsMobile.Framework;
using AddonsMobile.UI;
using AddonsMobile.UI.Rendering;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace AddonsMobile
{
    public sealed class ModEntry : Mod
    {
        // ════════════════════════════════════════════════════════════════
        // STATIC PROPERTIES
        // ════════════════════════════════════════════════════════════════

        internal static ModConfig Config { get; private set; } = null!;
        internal static IModHelper SHelper { get; private set; } = null!;
        internal static IMonitor SMonitor { get; private set; } = null!;
        internal static IManifest SManifest { get; private set; } = null!;
        internal static KeyRegistry Registry { get; private set; } = null!;

        // ════════════════════════════════════════════════════════════════
        // INSTANCE FIELDS
        // ════════════════════════════════════════════════════════════════

        private MobileButtonManager _mobileButtonManager = null!;
        private MobileAddonsAPI _addonsAPI = null!;
        private bool _isInitialized = false;

        // ════════════════════════════════════════════════════════════════
        // ENTRY POINT
        // ════════════════════════════════════════════════════════════════

        public override void Entry(IModHelper helper)
        {
            // Set static references
            SHelper = helper;
            SMonitor = Monitor;

            // Load and validate config
            Config = helper.ReadConfig<ModConfig>();
            Config.Validate();

            // Initialize registry
            Registry = new KeyRegistry(Monitor);

            // Initialize managers
            _mobileButtonManager = new MobileButtonManager(helper, Monitor);
            _addonsAPI = new MobileAddonsAPI(Registry, _mobileButtonManager, Monitor);

            // Register events
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;

            // Export textures hanya dalam mode debug
#if DEBUG
            TextureGenerator.ExportTexturesToFile(helper, Monitor);
#endif

            Monitor.Log("AddonsMobile initialized", LogLevel.Info);
            Monitor.Log($"Platform: {Constants.TargetPlatform}", LogLevel.Debug);
        }

        /// <summary>
        /// Expose API untuk mod lain
        /// </summary>
        public override object GetApi()
        {
            Monitor.Log($"GetApi() called - API is {(_addonsAPI != null ? "ready" : "NULL")}", LogLevel.Trace);
            return _addonsAPI;
        }

        // ════════════════════════════════════════════════════════════════
        // EVENT HANDLERS
        // ════════════════════════════════════════════════════════════════

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            //SetupGenericModConfigMenu();
            GenericModConfigMenu.Register(Helper, Monitor, Config, ModManifest);
            RegisterConsoleCommands();

            _isInitialized = true;
            Monitor.Log("Mobile button manager ready", LogLevel.Debug);
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            _mobileButtonManager.UpdatePosition();
            _mobileButtonManager.RefreshButtons();
            _mobileButtonManager.SetVisible(true);

            Monitor.Log($"Loaded with {Registry.Count} registered buttons", LogLevel.Info);
        }

        private void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            _mobileButtonManager.SetVisible(false);
        }

        /// <summary>
        /// Reset posisi FAB ke default
        /// </summary>
        public void ResetButtonPosition()
        {
            _mobileButtonManager?.ResetPosition();
        }

        // ════════════════════════════════════════════════════════════════
        // CONSOLE COMMANDS
        // ════════════════════════════════════════════════════════════════

        private void RegisterConsoleCommands()
        {
            Helper.ConsoleCommands.Add(
                "addons_reset",
                "Reset FAB position to default",
                (_, _) =>
                {
                    ResetButtonPosition();
                    Monitor.Log("FAB position reset to default", LogLevel.Info);
                }
            );

            Helper.ConsoleCommands.Add(
                "addons_list",
                "List all registered buttons",
                (_, _) =>
                {
                    var buttons = Registry.GetAllButtonsIncludingHidden();
                    Monitor.Log($"=== Registered Buttons ({Registry.Count}) ===", LogLevel.Info);
                    foreach (var button in buttons)
                    {
                        string status = button.ShouldShow() ? "✓" : "✗";
                        Monitor.Log($"  [{status}] {button.DisplayName} ({button.UniqueId}) - {button.ModId}", LogLevel.Info);
                    }
                }
            );

            Helper.ConsoleCommands.Add(
                "addons_toggle",
                "Toggle FAB visibility",
                (_, _) =>
                {
                    bool newState = !_mobileButtonManager.IsVisible;
                    _mobileButtonManager.SetVisible(newState);
                    Monitor.Log($"FAB visibility: {newState}", LogLevel.Info);
                }
            );

            Helper.ConsoleCommands.Add(
                "addons_trigger",
                "Trigger a button by ID. Usage: addons_trigger <uniqueId>",
                (_, args) =>
                {
                    if (args.Length == 0)
                    {
                        Monitor.Log("Usage: addons_trigger <uniqueId>", LogLevel.Error);
                        return;
                    }

                    string buttonId = args[0];
                    bool result = Registry.TriggerButton(buttonId, true);
                    Monitor.Log(result
                        ? $"Triggered button '{buttonId}'"
                        : $"Failed to trigger button '{buttonId}'",
                        result ? LogLevel.Info : LogLevel.Error);
                }
            );
        }

        // ════════════════════════════════════════════════════════════════
        // GENERIC MOD CONFIG MENU
        // ════════════════════════════════════════════════════════════════

        private void SetupGenericModConfigMenu()
        {
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>(
                "spacechase0.GenericModConfigMenu");

            if (configMenu is null)
            {
                Monitor.Log("Generic Mod Config Menu not available", LogLevel.Trace);
                return;
            }

            // Register mod
            configMenu.Register(
                mod: ModManifest,
                reset: () =>
                {
                    Config = new ModConfig();
                    Config.Validate();
                },
                save: () =>
                {
                    Config.Validate();
                    Helper.WriteConfig(Config);
                    _mobileButtonManager?.UpdatePosition();
                }
            );

            // ─────────────────────────────────────────────────────────
            // FAB Appearance Section
            // ─────────────────────────────────────────────────────────
            configMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => "FAB Appearance"
            );

            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Background Style",
                tooltip: () => "Choose the background style for the floating action button",
                getValue: () => Config.FabBackground.ToString(),
                setValue: value => Config.FabBackground = Enum.Parse<FabBackgroundStyle>(value),
                allowedValues: Enum.GetNames<FabBackgroundStyle>(),
                formatAllowedValue: FormatEnumName
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Show Shadow",
                tooltip: () => "Display a shadow behind the FAB",
                getValue: () => Config.ShowFabShadow,
                setValue: value => Config.ShowFabShadow = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Show Button Count Badge",
                tooltip: () => "Display the number of registered buttons on the FAB",
                getValue: () => Config.ShowButtonCountBadge,
                setValue: value => Config.ShowButtonCountBadge = value
            );

            // ─────────────────────────────────────────────────────────
            // Size Section
            // ─────────────────────────────────────────────────────────
            configMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => "Size"
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "FAB Size",
                tooltip: () => "Size of the floating action button (pixels)",
                getValue: () => Config.ButtonSize,
                setValue: value => Config.ButtonSize = value,
                min: 40,
                max: 100,
                interval: 4
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Menu Button Size",
                tooltip: () => "Size of menu buttons when expanded (pixels)",
                getValue: () => Config.MenuButtonSize,
                setValue: value => Config.MenuButtonSize = value,
                min: 30,
                max: 80,
                interval: 4
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Button Spacing",
                tooltip: () => "Space between menu buttons (pixels)",
                getValue: () => Config.ButtonSpacing,
                setValue: value => Config.ButtonSpacing = value,
                min: 4,
                max: 20,
                interval: 2
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Max Buttons Per Row",
                tooltip: () => "Maximum number of buttons in a single row",
                getValue: () => Config.MaxButtonsPerRow,
                setValue: value => Config.MaxButtonsPerRow = value,
                min: 3,
                max: 10,
                interval: 1
            );

            // ─────────────────────────────────────────────────────────
            // Appearance Section
            // ─────────────────────────────────────────────────────────
            configMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => "Appearance"
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Opacity",
                tooltip: () => "Opacity of all buttons (0.3 - 1.0)",
                getValue: () => Config.ButtonOpacity,
                setValue: value => Config.ButtonOpacity = value,
                min: 0.3f,
                max: 1.0f,
                interval: 0.05f
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Show Button Labels",
                tooltip: () => "Show text labels below menu buttons",
                getValue: () => Config.ShowButtonLabels,
                setValue: value => Config.ShowButtonLabels = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Show Tooltips",
                tooltip: () => "Show tooltip on long press",
                getValue: () => Config.ShowTooltips,
                setValue: value => Config.ShowTooltips = value
            );

            // ─────────────────────────────────────────────────────────
            // Position Section
            // ─────────────────────────────────────────────────────────
            configMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => "Position"
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Default X Position (%)",
                tooltip: () => "Default horizontal position as percentage of screen width",
                getValue: () => Config.ButtonPositionX,
                setValue: value => Config.ButtonPositionX = value,
                min: 5f,
                max: 95f,
                interval: 5f
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Default Y Position (%)",
                tooltip: () => "Default vertical position as percentage of screen height",
                getValue: () => Config.ButtonPositionY,
                setValue: value => Config.ButtonPositionY = value,
                min: 5f,
                max: 95f,
                interval: 5f
            );

            configMenu.AddParagraph(
                mod: ModManifest,
                text: () => "Tip: Long-press the FAB to reset position, or use console command: addons_reset"
            );

            // ─────────────────────────────────────────────────────────
            // Safe Area Section (untuk Android)
            // ─────────────────────────────────────────────────────────
            configMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => "Safe Area (Android)"
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Top Margin",
                tooltip: () => "Top safe area margin for notch (pixels)",
                getValue: () => Config.SafeAreaTop,
                setValue: value => Config.SafeAreaTop = value,
                min: 0,
                max: 100,
                interval: 5
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Bottom Margin",
                tooltip: () => "Bottom safe area margin for navigation bar (pixels)",
                getValue: () => Config.SafeAreaBottom,
                setValue: value => Config.SafeAreaBottom = value,
                min: 0,
                max: 100,
                interval: 5
            );

            // ─────────────────────────────────────────────────────────
            // Drag Section
            // ─────────────────────────────────────────────────────────
            configMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => "Drag Settings"
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Enable Dragging",
                tooltip: () => "Allow dragging the FAB to reposition it",
                getValue: () => Config.EnableDragging,
                setValue: value => Config.EnableDragging = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Drag Threshold",
                tooltip: () => "Distance to move before drag starts (pixels)",
                getValue: () => Config.DragThreshold,
                setValue: value => Config.DragThreshold = value,
                min: 5f,
                max: 50f,
                interval: 5f
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Show Drag Indicator",
                tooltip: () => "Show visual indicator while dragging",
                getValue: () => Config.ShowDragIndicator,
                setValue: value => Config.ShowDragIndicator = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Haptic Feedback",
                tooltip: () => "Vibrate on interaction (Android)",
                getValue: () => Config.HapticFeedback,
                setValue: value => Config.HapticFeedback = value
            );

            // ─────────────────────────────────────────────────────────
            // Behavior Section
            // ─────────────────────────────────────────────────────────
            configMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => "Behavior"
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Auto Hide in Events",
                tooltip: () => "Automatically hide during cutscenes",
                getValue: () => Config.AutoHideInEvents,
                setValue: value => Config.AutoHideInEvents = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Auto Collapse After Press",
                tooltip: () => "Collapse menu after pressing a button",
                getValue: () => Config.AutoCollapseAfterPress,
                setValue: value => Config.AutoCollapseAfterPress = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Animation Duration",
                tooltip: () => "Duration of expand/collapse animation (ms)",
                getValue: () => Config.AnimationDuration,
                setValue: value => Config.AnimationDuration = value,
                min: 100,
                max: 500,
                interval: 50
            );

            // ─────────────────────────────────────────────────────────
            // Gesture Section
            // ─────────────────────────────────────────────────────────
            configMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => "Gestures"
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Enable Double Tap",
                tooltip: () => "Enable double-tap gesture on FAB",
                getValue: () => Config.EnableDoubleTap,
                setValue: value => Config.EnableDoubleTap = value
            );

            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Double Tap Action",
                tooltip: () => "Action when double-tapping the FAB",
                getValue: () => Config.FabDoubleTapAction.ToString(),
                setValue: value => Config.FabDoubleTapAction = Enum.Parse<DoubleTapAction>(value),
                allowedValues: Enum.GetNames<DoubleTapAction>(),
                formatAllowedValue: FormatEnumName
            );

            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Long Press Action",
                tooltip: () => "Action when long-pressing the FAB",
                getValue: () => Config.FabLongPressAction.ToString(),
                setValue: value => Config.FabLongPressAction = Enum.Parse<LongPressAction>(value),
                allowedValues: Enum.GetNames<LongPressAction>(),
                formatAllowedValue: FormatEnumName
            );

            // ─────────────────────────────────────────────────────────
            // Debug Section
            // ─────────────────────────────────────────────────────────
            configMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => "Debug"
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Verbose Logging",
                tooltip: () => "Enable detailed logging for debugging",
                getValue: () => Config.VerboseLogging,
                setValue: value => Config.VerboseLogging = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Show Debug Bounds",
                tooltip: () => "Show debug rectangles for touch areas",
                getValue: () => Config.ShowDebugBounds,
                setValue: value => Config.ShowDebugBounds = value
            );

            Monitor.Log("Generic Mod Config Menu registered", LogLevel.Trace);
        }

        /// <summary>
        /// Format enum name untuk display yang lebih baik
        /// </summary>
        private static string FormatEnumName(string enumName)
        {
            // Konversi PascalCase ke spaced words
            var result = new System.Text.StringBuilder();

            foreach (char c in enumName)
            {
                if (char.IsUpper(c) && result.Length > 0)
                    result.Append(' ');
                result.Append(c);
            }

            return result.ToString();
        }
    }
}