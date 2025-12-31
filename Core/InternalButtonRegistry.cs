using AddonsMobile.Menus;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using AddonsMobile.Config;
using AddonsMobile.Framework.Data;
using AddonsMobile.Framework;

namespace AddonsMobile.Core
{
    /// <summary>
    /// Register button internal for AddonsMobile
    /// </summary>
    internal class InternalButtonRegistry
    {
        private const string ModId = "AddonsMobile";
        private const string ButtonGeneralMenuId = "AddonsMobile.GeneralMenu";
        private const string ToggleVisibilityId = "AddonsMobile.ToggleVisibility";
        private const string ResetPositionId = "AddonsMobile.ResetPosition";
        private const string GeneralMenuIcon = "assets/Android_White.png";
        private static Texture2D? _generalMenuIcon;
        private static Rectangle sourceRect;

        public InternalButtonRegistry() { }

        /// <summary>
        /// Register all internal buttons
        /// Dipanggil di OnGameLaunched setelah API tersedia
        /// </summary>
        /// <param name="api"></param>
        /// <param name="monitor"></param>
        public static void RegisterAll(IMobileAddonsAPI api, KeyRegistry registry, ModConfig config, IModHelper helper, IMonitor monitor)
        {
            if (api == null)
            {
                monitor.Log("Cannot register internal buttons: API is null", LogLevel.Error);
                return;
            }

            try
            {
                LoadCustomIcon(helper, monitor);
                RegisterButtonGeneralMenu(api, registry, config, monitor);
                //RegisterToggleVisibilityButton(api, monitor);
                //RegisterResetPositionButton(api, monitor);

                monitor.Log("Internal Buttons registered successfully", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                monitor.Log($"Failed to register internal buttons: {ex.Message}", LogLevel.Error);
            }
        }

        // ═══════════════════════════════════════════════════════════
        // Button: Open General Menu
        // ═══════════════════════════════════════════════════════════

        private static void LoadCustomIcon(IModHelper helper, IMonitor monitor)
        {
            try
            {
                _generalMenuIcon = helper.ModContent.Load<Texture2D>(GeneralMenuIcon);
                sourceRect = new Rectangle(0, 0, 24, 24);
                monitor.Log("Load geneeral menu icon form assets", LogLevel.Debug);
            }
            catch
            {
                _generalMenuIcon = Game1.mouseCursors;
                sourceRect = new Rectangle(211, 428, 9, 9);
                monitor.Log("Custom icon not found, use default icon as fallback", LogLevel.Debug);
            }
        }

        private static void RegisterButtonGeneralMenu(IMobileAddonsAPI api, KeyRegistry registry, ModConfig config, IMonitor monitor)
        {
            var button = api.CreateButton(ButtonGeneralMenuId, ModId);


            if (_generalMenuIcon != null)
            {
                button.WithDisplayName("General Addons Menu")
                .WithDescription("Open General menu on Addons mobile mods.")
                .WithCategory(KeyCategory.Menu)
                .WithPriority(1)
                .WithType(ButtonType.Momentary)
                .WithIcon(_generalMenuIcon, sourceRect)
                .WithVisibilityCondition(() => Context.IsWorldReady && Game1.activeClickableMenu == null)
                .OnPress(() =>
                {
                    try
                    {
                        if (Game1.activeClickableMenu == null)
                        {
                            Game1.activeClickableMenu = new GeneralMenu(registry, config, monitor);
                        }
                    }
                    catch (Exception ex)
                    {
                        monitor.Log($"Error opening GeneralMenu: {ex.Message}", LogLevel.Error);
                    }
                })
                .Register();
            }
            else
            {
                monitor.Log("Failed register button", LogLevel.Error);
            }

            monitor.Log($"Registered: {ButtonGeneralMenuId}", LogLevel.Trace);

        }

        // ═══════════════════════════════════════════════════════════
        // Unregister
        // ═══════════════════════════════════════════════════════════

        private static void UnregisterAll(IMobileAddonsAPI api, IMonitor monitor)
        {
            if (api == null)
                return;

            try
            {
                int count = api.UnregisterAllFromMod(ModId);
                monitor.Log($"Unregistered {count} internal button(s)", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                monitor.Log($"Error unregistering internal buttons: {ex.Message}", LogLevel.Error);
            }
        }
    }
}
