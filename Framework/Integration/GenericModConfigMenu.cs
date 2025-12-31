using AddonsMobile.Config;
using StardewModdingAPI;

namespace AddonsMobile.Framework.Integration
{
    public static class GenericModConfigMenu
    {
        public static void Register(IModHelper helper, IMonitor monitor, ModConfig Config, IManifest manifest)
        {
            var configMenu = helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

            if (configMenu == null)
            {
                monitor.Log("Generic Mod Config Menu not found. Config menu will not be available.", LogLevel.Debug);
                return;
            }

            RegisterMenu(configMenu, helper, Config, manifest);

            MenuSection(configMenu, helper, Config, manifest);

            monitor.Log("Config menu registered successfully!", LogLevel.Debug);
        }

        private static void RegisterMenu(IGenericModConfigMenuApi configMenu,
            IModHelper helper,
            ModConfig Config,
            IManifest manifest)
        {
            configMenu.Register(
                mod: manifest,
                reset: () => ResetConfig(Config),
                save: () => SaveConfig(helper, Config),
                titleScreenOnly: false
            );
        }

        private static void ResetConfig(ModConfig Config)
        {
            var defaultConfig = new ModConfig();

            Config.FabPositionX = defaultConfig.FabPositionX;
            Config.FabPositionY = defaultConfig.FabPositionY;
            Config.SafeAreaTop = defaultConfig.SafeAreaTop;
            Config.SafeAreaBottom = defaultConfig.SafeAreaBottom;
            Config.SafeAreaLeft = defaultConfig.SafeAreaLeft;
            Config.SafeAreaRight = defaultConfig.SafeAreaRight;
            Config.DragEnabled = defaultConfig.DragEnabled;
            Config.DragThreshold = defaultConfig.DragThreshold;
            Config.DragShowIndicator = defaultConfig.DragShowIndicator;
            Config.HapticFeedback = defaultConfig.HapticFeedback;
            Config.FabSize = defaultConfig.FabSize;
            Config.MenuButtonSize = defaultConfig.MenuButtonSize;
            Config.MenuButtonSpacing = defaultConfig.MenuButtonSpacing;
            Config.MaxButtonsPerRow = defaultConfig.MaxButtonsPerRow;
            Config.MenuPadding = defaultConfig.MenuPadding;
            Config.MenuButtonOpacity = defaultConfig.MenuButtonOpacity;
            Config.ShowButtonLabels = defaultConfig.ShowButtonLabels;
            Config.FabBackground = defaultConfig.FabBackground;
            Config.FabShowShadow = defaultConfig.FabShowShadow;
            Config.DragShowColor = defaultConfig.DragShowColor;
            Config.FabShowBadge = defaultConfig.FabShowBadge;
            Config.ShowTooltips = defaultConfig.ShowTooltips;
            Config.AutoHideInEvents = defaultConfig.AutoHideInEvents;
            Config.AnimationDuration = defaultConfig.AnimationDuration;
            Config.AutoCollapseDelay = defaultConfig.AutoCollapseDelay;
            Config.AutoCollapse = defaultConfig.AutoCollapse;
            Config.DoubleTapEnabled = defaultConfig.DoubleTapEnabled;
            Config.DoubleTapAction = defaultConfig.DoubleTapAction;
            Config.LongPressAction = defaultConfig.LongPressAction;
            Config.CategoriesEnabled = defaultConfig.CategoriesEnabled;
            Config.DefaultCategory = defaultConfig.DefaultCategory;
            Config.HideEmptyCategories = defaultConfig.HideEmptyCategories;
            Config.DebugVerboseLogging = defaultConfig.DebugVerboseLogging;
            Config.DebugShowBounds = defaultConfig.DebugShowBounds;
        }

        private static void SaveConfig(IModHelper helper, ModConfig Config)
        {
            helper.WriteConfig(Config);
        }

        private static void MenuSection(IGenericModConfigMenuApi configMenu,
            IModHelper helper,
            ModConfig Config,
            IManifest manifest)
        {
            configMenu.AddSectionTitle(
                mod: manifest,
                text: () => "FAB Appearance"
            );

            configMenu.AddTextOption(
                mod: manifest,
                name: () => "Background Style",
                tooltip: () => "Choose the background style for the floating action button",
                getValue: () => Config.FabBackground.ToString(),
                setValue: value => Config.FabBackground = Enum.Parse<FabBackgroundStyle>(value),
                allowedValues: Enum.GetNames<FabBackgroundStyle>(),
                formatAllowedValue: FormatEnumName
            );

            configMenu.AddBoolOption(
                mod: manifest,
                name: () => "Show Shadow",
                tooltip: () => "Display a shadow behind the FAB",
                getValue: () => Config.FabShowShadow,
                setValue: value => Config.FabShowShadow = value
            );

            configMenu.AddBoolOption(
                mod: manifest,
                name: () => "Show Button Count Badge",
                tooltip: () => "Display the number of registered buttons on the FAB",
                getValue: () => Config.FabShowBadge,
                setValue: value => Config.FabShowBadge = value
            );

            // ─────────────────────────────────────────────────────────
            // Size Section
            // ─────────────────────────────────────────────────────────
            configMenu.AddSectionTitle(
                mod: manifest,
                text: () => "Size"
            );

            configMenu.AddNumberOption(
                mod: manifest,
                name: () => "FAB Size",
                tooltip: () => "Size of the floating action button (pixels)",
                getValue: () => Config.FabSize,
                setValue: value => Config.FabSize = value,
                min: 40,
                max: 100,
                interval: 4
            );

            configMenu.AddNumberOption(
                mod: manifest,
                name: () => "Menu Button Size",
                tooltip: () => "Size of menu buttons when expanded (pixels)",
                getValue: () => Config.MenuButtonSize,
                setValue: value => Config.MenuButtonSize = value,
                min: 30,
                max: 80,
                interval: 4
            );

            configMenu.AddNumberOption(
                mod: manifest,
                name: () => "Button Spacing",
                tooltip: () => "Space between menu buttons (pixels)",
                getValue: () => Config.MenuButtonSpacing,
                setValue: value => Config.MenuButtonSpacing = value,
                min: 4,
                max: 20,
                interval: 2
            );

            configMenu.AddNumberOption(
                mod: manifest,
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
                mod: manifest,
                text: () => "Appearance"
            );

            configMenu.AddNumberOption(
                mod: manifest,
                name: () => "Opacity",
                tooltip: () => "Opacity of all buttons (0.3 - 1.0)",
                getValue: () => Config.MenuButtonOpacity,
                setValue: value => Config.MenuButtonOpacity = value,
                min: 0.3f,
                max: 1.0f,
                interval: 0.05f
            );

            configMenu.AddBoolOption(
                mod: manifest,
                name: () => "Show Button Labels",
                tooltip: () => "Show text labels below menu buttons",
                getValue: () => Config.ShowButtonLabels,
                setValue: value => Config.ShowButtonLabels = value
            );

            configMenu.AddBoolOption(
                mod: manifest,
                name: () => "Show Tooltips",
                tooltip: () => "Show tooltip on long press",
                getValue: () => Config.ShowTooltips,
                setValue: value => Config.ShowTooltips = value
            );

            // ─────────────────────────────────────────────────────────
            // Position Section
            // ─────────────────────────────────────────────────────────
            configMenu.AddSectionTitle(
                mod: manifest,
                text: () => "Position"
            );

            configMenu.AddNumberOption(
                mod: manifest,
                name: () => "Default X Position (%)",
                tooltip: () => "Default horizontal position as percentage of screen width",
                getValue: () => Config.FabPositionX,
                setValue: value => Config.FabPositionX = value,
                min: 5f,
                max: 95f,
                interval: 5f
            );

            configMenu.AddNumberOption(
                mod: manifest,
                name: () => "Default Y Position (%)",
                tooltip: () => "Default vertical position as percentage of screen height",
                getValue: () => Config.FabPositionY,
                setValue: value => Config.FabPositionY = value,
                min: 5f,
                max: 95f,
                interval: 5f
            );

            configMenu.AddParagraph(
                mod: manifest,
                text: () => "Tip: Long-press the FAB to reset position, or use console command: addons_reset"
            );

            // ─────────────────────────────────────────────────────────
            // Safe Area Section (untuk Android)
            // ─────────────────────────────────────────────────────────
            configMenu.AddSectionTitle(
                mod: manifest,
                text: () => "Safe Area (Android)"
            );

            configMenu.AddNumberOption(
                mod: manifest,
                name: () => "Top Margin",
                tooltip: () => "Top safe area margin for notch (pixels)",
                getValue: () => Config.SafeAreaTop,
                setValue: value => Config.SafeAreaTop = value,
                min: 0,
                max: 100,
                interval: 5
            );

            configMenu.AddNumberOption(
                mod: manifest,
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
                mod: manifest,
                text: () => "Drag Settings"
            );

            configMenu.AddBoolOption(
                mod: manifest,
                name: () => "Enable Dragging",
                tooltip: () => "Allow dragging the FAB to reposition it",
                getValue: () => Config.DragEnabled,
                setValue: value => Config.DragEnabled = value
            );

            configMenu.AddNumberOption(
                mod: manifest,
                name: () => "Drag Threshold",
                tooltip: () => "Distance to move before drag starts (pixels)",
                getValue: () => Config.DragThreshold,
                setValue: value => Config.DragThreshold = value,
                min: 5f,
                max: 50f,
                interval: 5f
            );

            configMenu.AddBoolOption(
                mod: manifest,
                name: () => "Show Drag Indicator",
                tooltip: () => "Show visual indicator while dragging",
                getValue: () => Config.DragShowIndicator,
                setValue: value => Config.DragShowIndicator = value
            );

            configMenu.AddBoolOption(
                mod: manifest,
                name: () => "Haptic Feedback",
                tooltip: () => "Vibrate on interaction (Android)",
                getValue: () => Config.HapticFeedback,
                setValue: value => Config.HapticFeedback = value
            );

            // ─────────────────────────────────────────────────────────
            // Behavior Section
            // ─────────────────────────────────────────────────────────
            configMenu.AddSectionTitle(
                mod: manifest,
                text: () => "Behavior"
            );

            configMenu.AddBoolOption(
                mod: manifest,
                name: () => "Auto Hide in Events",
                tooltip: () => "Automatically hide during cutscenes",
                getValue: () => Config.AutoHideInEvents,
                setValue: value => Config.AutoHideInEvents = value
            );

            configMenu.AddBoolOption(
                mod: manifest,
                name: () => "Auto Collapse After Press",
                tooltip: () => "Collapse menu after pressing a button",
                getValue: () => Config.AutoCollapse,
                setValue: value => Config.AutoCollapse = value
            );

            configMenu.AddNumberOption(
                mod: manifest,
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
                mod: manifest,
                text: () => "Gestures"
            );

            configMenu.AddBoolOption(
                mod: manifest,
                name: () => "Enable Double Tap",
                tooltip: () => "Enable double-tap gesture on FAB",
                getValue: () => Config.DoubleTapEnabled,
                setValue: value => Config.DoubleTapEnabled = value
            );

            configMenu.AddTextOption(
                mod: manifest,
                name: () => "Double Tap Action",
                tooltip: () => "Action when double-tapping the FAB",
                getValue: () => Config.DoubleTapAction.ToString(),
                setValue: value => Config.DoubleTapAction = Enum.Parse<DoubleTapAction>(value),
                allowedValues: Enum.GetNames<DoubleTapAction>(),
                formatAllowedValue: FormatEnumName
            );

            configMenu.AddTextOption(
                mod: manifest,
                name: () => "Long Press Action",
                tooltip: () => "Action when long-pressing the FAB",
                getValue: () => Config.LongPressAction.ToString(),
                setValue: value => Config.LongPressAction = Enum.Parse<LongPressAction>(value),
                allowedValues: Enum.GetNames<LongPressAction>(),
                formatAllowedValue: FormatEnumName
            );

            // ─────────────────────────────────────────────────────────
            // Debug Section
            // ─────────────────────────────────────────────────────────
            configMenu.AddSectionTitle(
                mod: manifest,
                text: () => "Debug"
            );

            configMenu.AddBoolOption(
                mod: manifest,
                name: () => "Verbose Logging",
                tooltip: () => "Enable detailed logging for debugging",
                getValue: () => Config.DebugVerboseLogging,
                setValue: value => Config.DebugVerboseLogging = value
            );

            configMenu.AddBoolOption(
                mod: manifest,
                name: () => "Show Debug Bounds",
                tooltip: () => "Show debug rectangles for touch areas",
                getValue: () => Config.DebugShowBounds,
                setValue: value => Config.DebugShowBounds = value
            );
        }

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
