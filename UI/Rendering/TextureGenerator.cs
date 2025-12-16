using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace AddonsMobile.UI.Rendering
{
    /// <summary>
    /// Generator untuk membuat texture UI secara programatik
    /// Berguna sebagai fallback atau untuk development
    /// </summary>
    public static class TextureGenerator
    {
        /// <summary>
        /// Membuat texture menu bar dengan style yang dipilih
        /// </summary>
        public static Texture2D CreateMenuBarTexture(GraphicsDevice device, int size = 48, MenuBarStyle style = MenuBarStyle.Dark)
        {
            Texture2D texture = new Texture2D(device, size, size);
            Color[] pixels = new Color[size * size];

            // Get colors based on style
            var colors = GetMenuBarColors(style);

            int border = size / 4; // 12px for 48px texture

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int index = y * size + x;

                    // Determine which zone we're in
                    bool isTop = y < border;
                    bool isBottom = y >= size - border;
                    bool isLeft = x < border;
                    bool isRight = x >= size - border;

                    // Distance from edges for gradient/rounded effect
                    int distFromEdge = GetDistanceFromEdge(x, y, size, size);

                    if (distFromEdge <= 1)
                    {
                        // Outer edge - border color
                        pixels[index] = colors.Border;
                    }
                    else if (distFromEdge <= 3)
                    {
                        // Border gradient
                        float t = (distFromEdge - 1) / 2f;
                        pixels[index] = Color.Lerp(colors.Border, colors.EdgeOuter, t);
                    }
                    else if (distFromEdge <= 5)
                    {
                        // Outer area
                        pixels[index] = colors.EdgeOuter;
                    }
                    else if (distFromEdge <= 7)
                    {
                        // Transition to center
                        float t = (distFromEdge - 5) / 2f;
                        pixels[index] = Color.Lerp(colors.EdgeOuter, colors.Center, t);
                    }
                    else
                    {
                        // Center area
                        pixels[index] = colors.Center;
                    }

                    // Add subtle corner rounding
                    if (IsInCornerRound(x, y, size, border / 2))
                    {
                        pixels[index] = Color.Transparent;
                    }
                }
            }

            texture.SetData(pixels);
            return texture;
        }

        /// <summary>
        /// Membuat texture button frame
        /// </summary>
        public static Texture2D CreateButtonFrameTexture(GraphicsDevice device, int size = 32, ButtonFrameStyle style = ButtonFrameStyle.Default)
        {
            Texture2D texture = new Texture2D(device, size, size);
            Color[] pixels = new Color[size * size];

            var colors = GetButtonColors(style);
            int border = size / 5; // ~6px for 32px texture

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int index = y * size + x;
                    int distFromEdge = GetDistanceFromEdge(x, y, size, size);

                    if (distFromEdge <= 1)
                    {
                        pixels[index] = colors.Border;
                    }
                    else if (distFromEdge <= 2)
                    {
                        pixels[index] = colors.Highlight;
                    }
                    else if (distFromEdge <= 4)
                    {
                        float t = (distFromEdge - 2) / 2f;
                        pixels[index] = Color.Lerp(colors.Highlight, colors.Center, t);
                    }
                    else
                    {
                        pixels[index] = colors.Center;
                    }

                    // Corner rounding
                    if (IsInCornerRound(x, y, size, border / 2))
                    {
                        pixels[index] = Color.Transparent;
                    }
                }
            }

            texture.SetData(pixels);
            return texture;
        }

        /// <summary>
        /// Membuat texture FAB (Floating Action Button)
        /// </summary>
        public static Texture2D CreateFabTexture(GraphicsDevice device, int size = 64, FabStyle style = FabStyle.Default)
        {
            Texture2D texture = new Texture2D(device, size, size);
            Color[] pixels = new Color[size * size];

            var colors = GetFabColors(style);
            float centerX = size / 2f;
            float centerY = size / 2f;
            float radius = size / 2f - 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int index = y * size + x;
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));

                    if (dist > radius + 1)
                    {
                        pixels[index] = Color.Transparent;
                    }
                    else if (dist > radius - 1)
                    {
                        // Anti-aliased edge
                        float alpha = 1f - (dist - (radius - 1)) / 2f;
                        pixels[index] = colors.Border * alpha;
                    }
                    else if (dist > radius - 3)
                    {
                        pixels[index] = colors.Border;
                    }
                    else if (dist > radius - 5)
                    {
                        float t = (radius - 3 - dist) / 2f;
                        pixels[index] = Color.Lerp(colors.Border, colors.Outer, t);
                    }
                    else if (dist > radius - 10)
                    {
                        pixels[index] = colors.Outer;
                    }
                    else
                    {
                        // Inner gradient for 3D effect
                        float t = dist / (radius - 10);
                        pixels[index] = Color.Lerp(colors.Center, colors.Outer, t * 0.3f);
                    }
                }
            }

            texture.SetData(pixels);
            return texture;
        }

        // ============================================
        // Helper Methods
        // ============================================

        private static int GetDistanceFromEdge(int x, int y, int width, int height)
        {
            int distLeft = x;
            int distRight = width - 1 - x;
            int distTop = y;
            int distBottom = height - 1 - y;

            return Math.Min(Math.Min(distLeft, distRight), Math.Min(distTop, distBottom));
        }

        private static bool IsInCornerRound(int x, int y, int size, int radius)
        {
            // Top-left corner
            if (x < radius && y < radius)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(radius, radius));
                return dist > radius;
            }
            // Top-right corner
            if (x >= size - radius && y < radius)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(size - radius - 1, radius));
                return dist > radius;
            }
            // Bottom-left corner
            if (x < radius && y >= size - radius)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(radius, size - radius - 1));
                return dist > radius;
            }
            // Bottom-right corner
            if (x >= size - radius && y >= size - radius)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(size - radius - 1, size - radius - 1));
                return dist > radius;
            }
            return false;
        }

        // ============================================
        // Color Schemes
        // ============================================

        private static MenuBarColors GetMenuBarColors(MenuBarStyle style)
        {
            return style switch
            {
                MenuBarStyle.Dark => new MenuBarColors
                {
                    Border = new Color(60, 70, 90),
                    EdgeOuter = new Color(45, 52, 70),
                    Center = new Color(35, 42, 55)
                },
                MenuBarStyle.Light => new MenuBarColors
                {
                    Border = new Color(180, 185, 195),
                    EdgeOuter = new Color(220, 225, 235),
                    Center = new Color(240, 242, 248)
                },
                MenuBarStyle.Blue => new MenuBarColors
                {
                    Border = new Color(40, 80, 140),
                    EdgeOuter = new Color(50, 100, 170),
                    Center = new Color(60, 120, 200)
                },
                MenuBarStyle.Wood => new MenuBarColors
                {
                    Border = new Color(80, 50, 30),
                    EdgeOuter = new Color(120, 80, 50),
                    Center = new Color(160, 110, 70)
                },
                MenuBarStyle.Stone => new MenuBarColors
                {
                    Border = new Color(70, 70, 75),
                    EdgeOuter = new Color(100, 100, 105),
                    Center = new Color(130, 130, 135)
                },
                _ => new MenuBarColors
                {
                    Border = new Color(60, 70, 90),
                    EdgeOuter = new Color(45, 52, 70),
                    Center = new Color(35, 42, 55)
                }
            };
        }

        private static ButtonColors GetButtonColors(ButtonFrameStyle style)
        {
            return style switch
            {
                ButtonFrameStyle.Default => new ButtonColors
                {
                    Border = new Color(80, 95, 120),
                    Highlight = new Color(100, 115, 145),
                    Center = new Color(70, 82, 105)
                },
                ButtonFrameStyle.Highlighted => new ButtonColors
                {
                    Border = new Color(100, 140, 180),
                    Highlight = new Color(130, 170, 210),
                    Center = new Color(90, 130, 170)
                },
                ButtonFrameStyle.Disabled => new ButtonColors
                {
                    Border = new Color(60, 60, 65),
                    Highlight = new Color(75, 75, 80),
                    Center = new Color(50, 50, 55)
                },
                _ => new ButtonColors
                {
                    Border = new Color(80, 95, 120),
                    Highlight = new Color(100, 115, 145),
                    Center = new Color(70, 82, 105)
                }
            };
        }

        private static FabColors GetFabColors(FabStyle style)
        {
            return style switch
            {
                FabStyle.Default => new FabColors
                {
                    Border = new Color(90, 105, 130),
                    Outer = new Color(60, 72, 95),
                    Center = new Color(50, 60, 80)
                },
                FabStyle.Accent => new FabColors
                {
                    Border = new Color(180, 100, 60),
                    Outer = new Color(200, 120, 80),
                    Center = new Color(220, 140, 100)
                },
                FabStyle.Green => new FabColors
                {
                    Border = new Color(60, 120, 80),
                    Outer = new Color(80, 150, 100),
                    Center = new Color(100, 180, 120)
                },
                _ => new FabColors
                {
                    Border = new Color(90, 105, 130),
                    Outer = new Color(60, 72, 95),
                    Center = new Color(50, 60, 80)
                }
            };
        }

        // ============================================
        // Color Structs
        // ============================================

        private struct MenuBarColors
        {
            public Color Border;
            public Color EdgeOuter;
            public Color Center;
        }

        private struct ButtonColors
        {
            public Color Border;
            public Color Highlight;
            public Color Center;
        }

        private struct FabColors
        {
            public Color Border;
            public Color Outer;
            public Color Center;
        }

        /// <summary>
        /// Export generated texture ke file PNG
        /// Panggil sekali untuk membuat template, lalu edit sesuai keinginan
        /// </summary>
        public static void ExportTexturesToFile(IModHelper helper, IMonitor monitor)
        {
            try
            {
                string assetPath = Path.Combine(helper.DirectoryPath, "assets");
                Directory.CreateDirectory(assetPath);

                var device = Game1.graphics.GraphicsDevice;

                // Export Menu Bar
                var menuBarTexture = CreateMenuBarTexture(device, 48, MenuBarStyle.Dark);
                string menuBarPath = Path.Combine(assetPath, "menu_bar_generated.png");
                using (var stream = File.Create(menuBarPath))
                {
                    menuBarTexture.SaveAsPng(stream, 48, 48);
                }
                monitor.Log($"Exported: {menuBarPath}", LogLevel.Info);

                // Export Button Frame
                var buttonTexture = CreateButtonFrameTexture(device, 32, ButtonFrameStyle.Default);
                string buttonPath = Path.Combine(assetPath, "button_frame_generated.png");
                using (var stream = File.Create(buttonPath))
                {
                    buttonTexture.SaveAsPng(stream, 32, 32);
                }
                monitor.Log($"Exported: {buttonPath}", LogLevel.Info);

                // Export FAB
                var fabTexture = CreateFabTexture(device, 64, FabStyle.Default);
                string fabPath = Path.Combine(assetPath, "fab_generated.png");
                using (var stream = File.Create(fabPath))
                {
                    fabTexture.SaveAsPng(stream, 64, 64);
                }
                monitor.Log($"Exported: {fabPath}", LogLevel.Info);

                // Export semua style variations
                ExportAllStyles(device, assetPath, monitor);
            }
            catch (Exception ex)
            {
                monitor.Log($"Failed to export textures: {ex.Message}", LogLevel.Error);
            }
        }

        private static void ExportAllStyles(GraphicsDevice device, string assetPath, IMonitor monitor)
        {
            // Menu bar styles
            foreach (MenuBarStyle style in Enum.GetValues(typeof(MenuBarStyle)))
            {
                var texture = CreateMenuBarTexture(device, 48, style);
                string path = Path.Combine(assetPath, $"menu_bar_{style.ToString().ToLower()}.png");
                using (var stream = File.Create(path))
                {
                    texture.SaveAsPng(stream, 48, 48);
                }
                monitor.Log($"Exported: menu_bar_{style.ToString().ToLower()}.png", LogLevel.Debug);
            }

            // Button styles
            foreach (ButtonFrameStyle style in Enum.GetValues(typeof(ButtonFrameStyle)))
            {
                var texture = CreateButtonFrameTexture(device, 32, style);
                string path = Path.Combine(assetPath, $"button_{style.ToString().ToLower()}.png");
                using (var stream = File.Create(path))
                {
                    texture.SaveAsPng(stream, 32, 32);
                }
                monitor.Log($"Exported: button_{style.ToString().ToLower()}.png", LogLevel.Debug);
            }
        }
    }

    // ============================================
    // Style Enums
    // ============================================

    public enum MenuBarStyle
    {
        Dark,
        Light,
        Blue,
        Wood,
        Stone
    }

    public enum ButtonFrameStyle
    {
        Default,
        Highlighted,
        Disabled
    }

    public enum FabStyle
    {
        Default,
        Accent,
        Green
    }
}