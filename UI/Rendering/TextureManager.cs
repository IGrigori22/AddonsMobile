using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace AddonsMobile.UI.Rendering
{
    /// <summary>
    /// Mengelola loading dan caching texture
    /// </summary>
    public class TextureManager
    {
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;

        // Loaded textures
        public Texture2D? GearTexture { get; private set; }
        public Texture2D? MenuBarTexture { get; private set; }
        public Texture2D? ButtonFrameTexture { get; private set; }

        // 9-slice renderers
        public NineSliceRenderer? MenuBarRenderer { get; private set; }
        public NineSliceRenderer? ButtonFrameRenderer { get; private set; }

        // Constants
        private const int MENU_BAR_BORDER = 12;
        private const int BUTTON_FRAME_BORDER = 6;

        public TextureManager(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
        }

        public void LoadAllTextures()
        {
            LoadGearTexture();
            LoadMenuBarTextures();
        }

        private void LoadGearTexture()
        {
            try
            {
                GearTexture = _helper.ModContent.Load<Texture2D>("assets/gear_icon.png");
                _monitor.Log($"Gear icon loaded: {GearTexture.Width}x{GearTexture.Height}", LogLevel.Trace);
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed to load gear_icon.png: {ex.Message}", LogLevel.Warn);
                GearTexture = null;
            }
        }

        private void LoadMenuBarTextures()
        {
            // Menu bar texture
            try
            {
                MenuBarTexture = _helper.ModContent.Load<Texture2D>("assets/menu_bar.png");
                _monitor.Log("Loaded menu_bar.png from assets", LogLevel.Trace);
            }
            catch
            {
                MenuBarTexture = TextureGenerator.CreateMenuBarTexture(
                    Game1.graphics.GraphicsDevice,
                    48,
                    MenuBarStyle.Dark
                );
                _monitor.Log("Generated menu bar texture programmatically", LogLevel.Trace);
            }

            MenuBarRenderer = new NineSliceRenderer(MenuBarTexture, MENU_BAR_BORDER);

            // Button frame texture
            try
            {
                ButtonFrameTexture = _helper.ModContent.Load<Texture2D>("assets/button_frame.png");
                _monitor.Log("Loaded button_frame.png from assets", LogLevel.Trace);
            }
            catch
            {
                ButtonFrameTexture = TextureGenerator.CreateButtonFrameTexture(
                    Game1.graphics.GraphicsDevice,
                    32,
                    ButtonFrameStyle.Default
                );
                _monitor.Log("Generated button frame texture programmatically", LogLevel.Trace);
            }

            ButtonFrameRenderer = new NineSliceRenderer(ButtonFrameTexture, BUTTON_FRAME_BORDER);
        }
    }
}