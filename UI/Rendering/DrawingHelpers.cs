using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace AddonsMobile.UI.Rendering
{
    /// <summary>
    /// Helper methods untuk operasi drawing dasar
    /// OPTIMIZED VERSION - menggunakan texture caching
    /// </summary>
    public class DrawingHelpers : IDisposable
    {
        // ══════════════════════════════════════════════════════════════════
        // EXISTING FIELDS
        // ══════════════════════════════════════════════════════════════════

        private Texture2D? _pixel;

        // ══════════════════════════════════════════════════════════════════
        // NEW: TEXTURE CACHES
        // ══════════════════════════════════════════════════════════════════

        private readonly Dictionary<int, Texture2D> _circleCache = new();
        private readonly Dictionary<string, Texture2D> _roundedRectCache = new();
        private GraphicsDevice? _graphicsDevice;

        private const int MAX_CACHE_SIZE = 20;  // Limit cache untuk memory

        // ══════════════════════════════════════════════════════════════════
        // PROPERTIES
        // ══════════════════════════════════════════════════════════════════

        public Texture2D? Pixel => _pixel;
        public bool IsReady => _pixel != null && _graphicsDevice != null;

        // ══════════════════════════════════════════════════════════════════
        // INITIALIZATION
        // ══════════════════════════════════════════════════════════════════

        public void EnsureTexture()
        {
            if (_pixel != null) return;

            try
            {
                _graphicsDevice = Game1.graphics.GraphicsDevice;
                _pixel = new Texture2D(_graphicsDevice, 1, 1);
                _pixel.SetData(new[] { Color.White });
            }
            catch
            {
                // Retry next frame
            }
        }

        // ══════════════════════════════════════════════════════════════════
        // BASIC DRAWING (unchanged)
        // ══════════════════════════════════════════════════════════════════

        public void DrawRectangle(SpriteBatch b, Rectangle rect, Color color)
        {
            if (_pixel == null) return;
            b.Draw(_pixel, rect, color);
        }

        public void DrawShadow(SpriteBatch b, Rectangle rect, int offset, float opacity)
        {
            if (_pixel == null) return;

            Rectangle shadowRect = new Rectangle(
                rect.X + offset,
                rect.Y + offset,
                rect.Width,
                rect.Height
            );

            b.Draw(_pixel, shadowRect, Color.Black * opacity);
        }

        public void DrawLine(SpriteBatch b, Vector2 start, Vector2 end, Color color, int thickness)
        {
            if (_pixel == null) return;

            Vector2 edge = end - start;
            float angle = MathF.Atan2(edge.Y, edge.X);
            float length = edge.Length();

            b.Draw(_pixel,
                new Rectangle((int)start.X, (int)start.Y, (int)length, thickness),
                null,
                color,
                angle,
                new Vector2(0, thickness / 2f),
                SpriteEffects.None,
                0);
        }

        // ══════════════════════════════════════════════════════════════════
        // OPTIMIZED: DrawCircle dengan texture caching
        // BEFORE: 64 draw calls per circle
        // AFTER: 1 draw call per circle
        // ══════════════════════════════════════════════════════════════════

        public void DrawCircle(SpriteBatch b, Vector2 center, int radius, Color color)
        {
            if (!IsReady || radius <= 0) return;

            // Get or create cached circle texture
            Texture2D circleTexture = GetOrCreateCircleTexture(radius);
            if (circleTexture == null) return;

            // Single draw call!
            Rectangle destRect = new Rectangle(
                (int)(center.X - radius),
                (int)(center.Y - radius),
                radius * 2,
                radius * 2
            );

            b.Draw(circleTexture, destRect, color);
        }

        /// <summary>
        /// Create or retrieve cached circle texture
        /// </summary>
        private Texture2D GetOrCreateCircleTexture(int radius)
        {
            // Check cache
            if (_circleCache.TryGetValue(radius, out var cached))
                return cached;

            // Limit cache size
            if (_circleCache.Count >= MAX_CACHE_SIZE)
            {
                ClearCircleCache();
            }

            // Create new texture
            int size = radius * 2;
            var texture = new Texture2D(_graphicsDevice, size, size);
            var pixels = new Color[size * size];

            float centerX = radius;
            float centerY = radius;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int index = y * size + x;
                    float dist = Vector2.Distance(
                        new Vector2(x, y),
                        new Vector2(centerX, centerY)
                    );

                    if (dist <= radius)
                    {
                        // Anti-aliased edge
                        float alpha = Math.Min(1f, radius - dist + 1f);
                        pixels[index] = Color.White * alpha;
                    }
                    else
                    {
                        pixels[index] = Color.Transparent;
                    }
                }
            }

            texture.SetData(pixels);
            _circleCache[radius] = texture;
            return texture;
        }

        // ══════════════════════════════════════════════════════════════════
        // OPTIMIZED: DrawRoundedRectangle dengan texture caching
        // BEFORE: Just drew a regular rectangle (not rounded!)
        // AFTER: Actually draws rounded rectangle with cached texture
        // ══════════════════════════════════════════════════════════════════

        public void DrawRoundedRectangle(SpriteBatch b, Rectangle rect, Color color, int radius)
        {
            if (!IsReady) return;

            // For very small radius, just draw rectangle
            if (radius <= 1)
            {
                b.Draw(_pixel, rect, color);
                return;
            }

            // Cache key based on dimensions and radius
            string cacheKey = $"{rect.Width}x{rect.Height}r{radius}";

            if (!_roundedRectCache.TryGetValue(cacheKey, out var texture))
            {
                // Limit cache size
                if (_roundedRectCache.Count >= MAX_CACHE_SIZE)
                {
                    ClearRoundedRectCache();
                }

                texture = CreateRoundedRectTexture(rect.Width, rect.Height, radius);
                if (texture != null)
                {
                    _roundedRectCache[cacheKey] = texture;
                }
            }

            if (texture != null)
            {
                b.Draw(texture, rect, color);
            }
            else
            {
                // Fallback to simple rectangle
                b.Draw(_pixel, rect, color);
            }
        }

        /// <summary>
        /// Create rounded rectangle texture
        /// </summary>
        private Texture2D? CreateRoundedRectTexture(int width, int height, int radius)
        {
            if (_graphicsDevice == null || width <= 0 || height <= 0) return null;

            // Clamp radius
            radius = Math.Min(radius, Math.Min(width, height) / 2);

            var texture = new Texture2D(_graphicsDevice, width, height);
            var pixels = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;

                    if (IsInsideRoundedRect(x, y, width, height, radius))
                    {
                        pixels[index] = Color.White;
                    }
                    else
                    {
                        pixels[index] = Color.Transparent;
                    }
                }
            }

            texture.SetData(pixels);
            return texture;
        }

        /// <summary>
        /// Check if point is inside rounded rectangle
        /// </summary>
        private bool IsInsideRoundedRect(int x, int y, int width, int height, int radius)
        {
            // Top-left corner
            if (x < radius && y < radius)
            {
                return Vector2.Distance(new Vector2(x, y), new Vector2(radius, radius)) <= radius;
            }
            // Top-right corner
            if (x >= width - radius && y < radius)
            {
                return Vector2.Distance(new Vector2(x, y), new Vector2(width - radius - 1, radius)) <= radius;
            }
            // Bottom-left corner
            if (x < radius && y >= height - radius)
            {
                return Vector2.Distance(new Vector2(x, y), new Vector2(radius, height - radius - 1)) <= radius;
            }
            // Bottom-right corner
            if (x >= width - radius && y >= height - radius)
            {
                return Vector2.Distance(new Vector2(x, y), new Vector2(width - radius - 1, height - radius - 1)) <= radius;
            }

            // Inside non-corner area
            return true;
        }

        // ══════════════════════════════════════════════════════════════════
        // IMPROVED: DrawRoundedBorder dengan corner arcs
        // ══════════════════════════════════════════════════════════════════

        public void DrawRoundedBorder(SpriteBatch b, Rectangle rect, Color color, int radius, int thickness)
        {
            if (_pixel == null) return;

            // Edges (tidak termasuk corners)
            // Top edge
            b.Draw(_pixel, new Rectangle(rect.X + radius, rect.Y, rect.Width - radius * 2, thickness), color);
            // Bottom edge
            b.Draw(_pixel, new Rectangle(rect.X + radius, rect.Bottom - thickness, rect.Width - radius * 2, thickness), color);
            // Left edge
            b.Draw(_pixel, new Rectangle(rect.X, rect.Y + radius, thickness, rect.Height - radius * 2), color);
            // Right edge
            b.Draw(_pixel, new Rectangle(rect.Right - thickness, rect.Y + radius, thickness, rect.Height - radius * 2), color);

            // Corner arcs
            if (radius > 0)
            {
                DrawCornerArc(b, new Vector2(rect.X + radius, rect.Y + radius), radius, 180, 270, color, thickness);
                DrawCornerArc(b, new Vector2(rect.Right - radius, rect.Y + radius), radius, 270, 360, color, thickness);
                DrawCornerArc(b, new Vector2(rect.X + radius, rect.Bottom - radius), radius, 90, 180, color, thickness);
                DrawCornerArc(b, new Vector2(rect.Right - radius, rect.Bottom - radius), radius, 0, 90, color, thickness);
            }
        }

        /// <summary>
        /// Draw arc for rounded corner
        /// </summary>
        private void DrawCornerArc(SpriteBatch b, Vector2 center, int radius,
            int startAngle, int endAngle, Color color, int thickness)
        {
            int segments = Math.Max(4, radius / 2);  // More segments for larger radius
            float startRad = MathHelper.ToRadians(startAngle);
            float endRad = MathHelper.ToRadians(endAngle);
            float angleStep = (endRad - startRad) / segments;

            for (int i = 0; i < segments; i++)
            {
                float angle1 = startRad + i * angleStep;
                float angle2 = startRad + (i + 1) * angleStep;

                Vector2 p1 = center + new Vector2(MathF.Cos(angle1), MathF.Sin(angle1)) * radius;
                Vector2 p2 = center + new Vector2(MathF.Cos(angle2), MathF.Sin(angle2)) * radius;

                DrawLine(b, p1, p2, color, thickness);
            }
        }

        // ══════════════════════════════════════════════════════════════════
        // CACHE MANAGEMENT
        // ══════════════════════════════════════════════════════════════════

        private void ClearCircleCache()
        {
            foreach (var texture in _circleCache.Values)
            {
                texture?.Dispose();
            }
            _circleCache.Clear();
        }

        private void ClearRoundedRectCache()
        {
            foreach (var texture in _roundedRectCache.Values)
            {
                texture?.Dispose();
            }
            _roundedRectCache.Clear();
        }

        /// <summary>
        /// Clear all cached textures
        /// Call when screen size changes significantly
        /// </summary>
        public void ClearAllCaches()
        {
            ClearCircleCache();
            ClearRoundedRectCache();
        }

        // ══════════════════════════════════════════════════════════════════
        // DISPOSE
        // ══════════════════════════════════════════════════════════════════

        public void Dispose()
        {
            ClearAllCaches();
            _pixel?.Dispose();
            _pixel = null;
            _graphicsDevice = null;
        }
    }
}