using AddonsMobile.Config;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AddonsMobile.UI.Rendering
{
    /// <summary>
    /// Renderer untuk berbagai style background FAB
    /// </summary>
    public class FabBackgroundRenderer
    {
        private readonly DrawingHelpers _drawingHelpers;
        private readonly Dictionary<(FabBackgroundStyle style, int size), Texture2D> _sizedCache = new();
        private GraphicsDevice _graphicsDevice;

        public FabBackgroundRenderer(DrawingHelpers drawingHelpers)
        {
            _drawingHelpers = drawingHelpers;
        }

        /// <summary>
        /// Menggambar background FAB sesuai style yang dipilih
        /// </summary>
        public void DrawBackground(SpriteBatch b, Rectangle bounds, FabBackgroundStyle style,
            float opacity, bool isPressed, bool isDragging, bool isExpanded)
        {
            if (style == FabBackgroundStyle.None)
            {
                // Tidak menggambar background
                return;
            }

            // Get or create texture
            _graphicsDevice ??= b.GraphicsDevice;
            var texture = GetOrCreateTexture(style, bounds.Width);

            if (texture == null)
            {
                // Fallback ke procedural drawing
                DrawProceduralBackground(b, bounds, style, opacity, isPressed, isDragging, isExpanded);
                return;
            }

            // Apply state modifiers
            Color tint = GetStateTint(style, isPressed, isDragging, isExpanded);

            b.Draw(texture, bounds, tint * opacity);
        }

        private Texture2D GetOrCreateTexture(FabBackgroundStyle style, int size)
        {
            if (_graphicsDevice == null) return null;

            var cacheKey = (style, size);

            // Check cache with proper key
            if (_sizedCache.TryGetValue(cacheKey, out var cached))
            {
                return cached;
            }

            // Dispose old texture with same style but different size
            var keysToRemove = _sizedCache.Keys
                .Where(k => k.style == style && k.size != size)
                .ToList();

            foreach (var key in keysToRemove)
            {
                _sizedCache[key]?.Dispose();
                _sizedCache.Remove(key);
            }

            // Generate new texture
            var texture = GenerateBackgroundTexture(style, size);
            if (texture != null)
            {
                _sizedCache[cacheKey] = texture;
            }

            return texture;
        }

        private Texture2D? GenerateBackgroundTexture(FabBackgroundStyle style, int size)
        {
            return style switch
            {
                FabBackgroundStyle.CircleDark => GenerateCircleTexture(size,
                    new Color(50, 58, 75), new Color(70, 82, 100), new Color(90, 105, 130)),

                FabBackgroundStyle.CircleLight => GenerateCircleTexture(size,
                    new Color(220, 225, 235), new Color(240, 242, 248), new Color(180, 185, 195)),

                FabBackgroundStyle.RoundedSquare => GenerateRoundedSquareTexture(size,
                    new Color(50, 58, 75), new Color(90, 105, 130), 12),

                FabBackgroundStyle.Wood => GenerateWoodTexture(size),

                FabBackgroundStyle.Stone => GenerateStoneTexture(size),

                FabBackgroundStyle.Metal => GenerateMetalTexture(size),

                FabBackgroundStyle.StardewStyle => GenerateStardewStyleTexture(size),

                FabBackgroundStyle.GradientBlue => GenerateGradientCircleTexture(size,
                    new Color(40, 80, 160), new Color(80, 140, 220)),

                FabBackgroundStyle.GradientGreen => GenerateGradientCircleTexture(size,
                    new Color(40, 120, 60), new Color(80, 180, 100)),

                FabBackgroundStyle.GradientSunset => GenerateGradientCircleTexture(size,
                    new Color(180, 80, 60), new Color(220, 160, 80)),

                _ => null
            };
        }

        // ============================================
        // Texture Generators
        // ============================================

        private Texture2D GenerateCircleTexture(int size, Color center, Color outer, Color border)
        {
            var texture = new Texture2D(_graphicsDevice, size, size);
            var pixels = new Color[size * size];

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
                        pixels[index] = border * alpha;
                    }
                    else if (dist > radius - 3)
                    {
                        pixels[index] = border;
                    }
                    else
                    {
                        // Gradient from center to outer
                        float t = dist / (radius - 3);
                        pixels[index] = Color.Lerp(center, outer, t * 0.5f);
                    }
                }
            }

            texture.SetData(pixels);
            return texture;
        }

        private Texture2D GenerateGradientCircleTexture(int size, Color colorTop, Color colorBottom)
        {
            var texture = new Texture2D(_graphicsDevice, size, size);
            var pixels = new Color[size * size];

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
                        float alpha = 1f - (dist - (radius - 1)) / 2f;
                        float gradientT = y / (float)size;
                        pixels[index] = Color.Lerp(colorTop, colorBottom, gradientT) * alpha;
                    }
                    else
                    {
                        float gradientT = y / (float)size;
                        Color baseColor = Color.Lerp(colorTop, colorBottom, gradientT);

                        // Add slight radial highlight
                        float radialT = dist / radius;
                        pixels[index] = Color.Lerp(baseColor, baseColor * 0.8f, radialT * 0.3f);
                    }
                }
            }

            texture.SetData(pixels);
            return texture;
        }

        private Texture2D GenerateRoundedSquareTexture(int size, Color fill, Color border, int cornerRadius)
        {
            var texture = new Texture2D(_graphicsDevice, size, size);
            var pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int index = y * size + x;

                    // Check if in rounded corner area
                    bool inCorner = false;
                    float cornerDist = 0;

                    if (x < cornerRadius && y < cornerRadius)
                    {
                        cornerDist = Vector2.Distance(new Vector2(x, y), new Vector2(cornerRadius, cornerRadius));
                        inCorner = true;
                    }
                    else if (x >= size - cornerRadius && y < cornerRadius)
                    {
                        cornerDist = Vector2.Distance(new Vector2(x, y), new Vector2(size - cornerRadius - 1, cornerRadius));
                        inCorner = true;
                    }
                    else if (x < cornerRadius && y >= size - cornerRadius)
                    {
                        cornerDist = Vector2.Distance(new Vector2(x, y), new Vector2(cornerRadius, size - cornerRadius - 1));
                        inCorner = true;
                    }
                    else if (x >= size - cornerRadius && y >= size - cornerRadius)
                    {
                        cornerDist = Vector2.Distance(new Vector2(x, y), new Vector2(size - cornerRadius - 1, size - cornerRadius - 1));
                        inCorner = true;
                    }

                    if (inCorner && cornerDist > cornerRadius)
                    {
                        pixels[index] = Color.Transparent;
                    }
                    else
                    {
                        int distFromEdge = Math.Min(Math.Min(x, size - 1 - x), Math.Min(y, size - 1 - y));

                        if (distFromEdge <= 2)
                        {
                            pixels[index] = border;
                        }
                        else
                        {
                            pixels[index] = fill;
                        }
                    }
                }
            }

            texture.SetData(pixels);
            return texture;
        }

        private Texture2D GenerateWoodTexture(int size)
        {
            var texture = new Texture2D(_graphicsDevice, size, size);
            var pixels = new Color[size * size];

            Color woodDark = new Color(101, 67, 33);
            Color woodMid = new Color(139, 90, 43);
            Color woodLight = new Color(160, 120, 60);
            Color woodBorder = new Color(70, 45, 20);

            float centerX = size / 2f;
            float centerY = size / 2f;
            float radius = size / 2f - 2;

            Random rand = new Random(42); // Fixed seed for consistency

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
                    else if (dist > radius - 2)
                    {
                        float alpha = Math.Max(0, 1f - (dist - (radius - 2)) / 3f);
                        pixels[index] = woodBorder * alpha;
                    }
                    else
                    {
                        // Wood grain pattern
                        float grain = MathF.Sin(y * 0.3f + MathF.Sin(x * 0.1f) * 2) * 0.5f + 0.5f;
                        grain += (float)(rand.NextDouble() * 0.1f - 0.05f); // Noise

                        Color woodColor;
                        if (grain < 0.3f)
                            woodColor = woodDark;
                        else if (grain < 0.7f)
                            woodColor = Color.Lerp(woodDark, woodMid, (grain - 0.3f) / 0.4f);
                        else
                            woodColor = Color.Lerp(woodMid, woodLight, (grain - 0.7f) / 0.3f);

                        pixels[index] = woodColor;
                    }
                }
            }

            texture.SetData(pixels);
            return texture;
        }

        private Texture2D GenerateStoneTexture(int size)
        {
            var texture = new Texture2D(_graphicsDevice, size, size);
            var pixels = new Color[size * size];

            Color stoneDark = new Color(80, 80, 85);
            Color stoneMid = new Color(120, 120, 125);
            Color stoneLight = new Color(150, 150, 155);
            Color stoneBorder = new Color(60, 60, 65);

            float centerX = size / 2f;
            float centerY = size / 2f;
            float radius = size / 2f - 2;

            Random rand = new Random(123);

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
                    else if (dist > radius - 2)
                    {
                        float alpha = Math.Max(0, 1f - (dist - (radius - 2)) / 3f);
                        pixels[index] = stoneBorder * alpha;
                    }
                    else
                    {
                        // Stone texture noise
                        float noise = (float)(rand.NextDouble());

                        Color stoneColor;
                        if (noise < 0.2f)
                            stoneColor = stoneDark;
                        else if (noise < 0.6f)
                            stoneColor = stoneMid;
                        else
                            stoneColor = stoneLight;

                        // Add depth based on distance from center
                        float depthFactor = 1f - (dist / radius) * 0.2f;
                        pixels[index] = new Color(
                            (int)(stoneColor.R * depthFactor),
                            (int)(stoneColor.G * depthFactor),
                            (int)(stoneColor.B * depthFactor)
                        );
                    }
                }
            }

            texture.SetData(pixels);
            return texture;
        }

        private Texture2D GenerateMetalTexture(int size)
        {
            var texture = new Texture2D(_graphicsDevice, size, size);
            var pixels = new Color[size * size];

            Color metalDark = new Color(100, 105, 115);
            Color metalLight = new Color(180, 185, 195);
            Color metalBorder = new Color(70, 75, 85);

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
                    else if (dist > radius - 2)
                    {
                        float alpha = Math.Max(0, 1f - (dist - (radius - 2)) / 3f);
                        pixels[index] = metalBorder * alpha;
                    }
                    else
                    {
                        // Metallic gradient (highlight at top-left)
                        float highlightX = (centerX - x) / radius;
                        float highlightY = (centerY - y) / radius;
                        float highlight = Math.Max(0, (highlightX + highlightY) * 0.5f);

                        pixels[index] = Color.Lerp(metalDark, metalLight, highlight);
                    }
                }
            }

            texture.SetData(pixels);
            return texture;
        }

        private Texture2D GenerateStardewStyleTexture(int size)
        {
            var texture = new Texture2D(_graphicsDevice, size, size);
            var pixels = new Color[size * size];

            // Stardew Valley UI colors
            Color bgColor = new Color(232, 196, 148);      // Tan/beige
            Color borderOuter = new Color(92, 60, 36);     // Dark brown
            Color borderInner = new Color(156, 108, 60);   // Medium brown
            Color highlight = new Color(255, 232, 196);    // Light highlight

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
                    else if (dist > radius - 2)
                    {
                        float alpha = Math.Max(0, 1f - (dist - (radius - 2)) / 3f);
                        pixels[index] = borderOuter * alpha;
                    }
                    else if (dist > radius - 4)
                    {
                        pixels[index] = borderInner;
                    }
                    else
                    {
                        // Inner area with subtle gradient
                        float gradientT = y / (float)size;
                        Color innerColor = Color.Lerp(highlight, bgColor, gradientT * 0.5f + 0.3f);
                        pixels[index] = innerColor;
                    }
                }
            }

            texture.SetData(pixels);
            return texture;
        }

        // ============================================
        // Procedural Fallback
        // ============================================

        private void DrawProceduralBackground(SpriteBatch b, Rectangle bounds, FabBackgroundStyle style,
            float opacity, bool isPressed, bool isDragging, bool isExpanded)
        {
            Vector2 center = bounds.Center.ToVector2();
            int radius = bounds.Width / 2;

            Color bgColor = GetProceduralColor(style, isPressed, isDragging, isExpanded);
            Color borderColor = GetProceduralBorderColor(style);

            // Draw filled circle
            _drawingHelpers.DrawCircle(b, center, radius, bgColor * opacity);

            // Draw border
            DrawCircleBorder(b, center, radius, borderColor * opacity, 2);
        }

        private Color GetProceduralColor(FabBackgroundStyle style, bool isPressed, bool isDragging, bool isExpanded)
        {
            Color baseColor = style switch
            {
                FabBackgroundStyle.CircleDark => new Color(50, 58, 75),
                FabBackgroundStyle.CircleLight => new Color(220, 225, 235),
                FabBackgroundStyle.RoundedSquare => new Color(50, 58, 75),
                FabBackgroundStyle.Wood => new Color(139, 90, 43),
                FabBackgroundStyle.Stone => new Color(120, 120, 125),
                FabBackgroundStyle.Metal => new Color(140, 145, 155),
                FabBackgroundStyle.StardewStyle => new Color(232, 196, 148),
                FabBackgroundStyle.GradientBlue => new Color(60, 110, 190),
                FabBackgroundStyle.GradientGreen => new Color(60, 150, 80),
                FabBackgroundStyle.GradientSunset => new Color(200, 120, 70),
                _ => new Color(50, 58, 75)
            };

            if (isDragging)
                return new Color(100, 180, 100);
            if (isPressed)
                return Color.Lerp(baseColor, Color.Black, 0.2f);
            if (isExpanded)
                return Color.Lerp(baseColor, Color.White, 0.1f);

            return baseColor;
        }

        private Color GetProceduralBorderColor(FabBackgroundStyle style)
        {
            return style switch
            {
                FabBackgroundStyle.CircleDark => new Color(90, 105, 130),
                FabBackgroundStyle.CircleLight => new Color(180, 185, 195),
                FabBackgroundStyle.Wood => new Color(70, 45, 20),
                FabBackgroundStyle.Stone => new Color(60, 60, 65),
                FabBackgroundStyle.Metal => new Color(70, 75, 85),
                FabBackgroundStyle.StardewStyle => new Color(92, 60, 36),
                FabBackgroundStyle.GradientBlue => new Color(30, 60, 120),
                FabBackgroundStyle.GradientGreen => new Color(30, 90, 40),
                FabBackgroundStyle.GradientSunset => new Color(140, 60, 40),
                _ => new Color(90, 105, 130)
            };
        }

        private Color GetStateTint(FabBackgroundStyle style, bool isPressed, bool isDragging, bool isExpanded)
        {
            if (isDragging)
                return new Color(180, 255, 180); // Green tint
            if (isPressed)
                return new Color(200, 200, 200); // Darker
            if (isExpanded)
                return new Color(255, 240, 220); // Warm tint

            return Color.White;
        }

        private void DrawCircleBorder(SpriteBatch b, Vector2 center, int radius, Color color, int thickness)
        {
            int segments = 48;
            float angleStep = MathHelper.TwoPi / segments;

            for (int i = 0; i < segments; i++)
            {
                float angle1 = i * angleStep;
                float angle2 = (i + 1) * angleStep;

                Vector2 p1 = center + new Vector2(
                    MathF.Cos(angle1) * radius,
                    MathF.Sin(angle1) * radius
                );

                Vector2 p2 = center + new Vector2(
                    MathF.Cos(angle2) * radius,
                    MathF.Sin(angle2) * radius
                );

                _drawingHelpers.DrawLine(b, p1, p2, color, thickness);
            }
        }

        public void ClearCache()
        {
            foreach (var texture in _sizedCache.Values)
            {
                texture?.Dispose();
            }
            _sizedCache.Clear();
        }
    }
}