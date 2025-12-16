using AddonsMobile.Config;
using AddonsMobile.UI.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace AddonsMobile.UI.Components
{
    /// <summary>
    /// Bertanggung jawab untuk menggambar FAB (Floating Action Button)
    /// </summary>
    public class FabRenderer
    {
        private readonly ModConfig _config;
        private readonly DrawingHelpers _drawingHelpers;
        private readonly TextureManager _textureManager;
        private readonly FabBackgroundRenderer _backgroundRenderer;

        private const float GEAR_DISPLAY_RATIO = 0.7f;

        public FabRenderer(ModConfig config, DrawingHelpers drawingHelpers, TextureManager textureManager)
        {
            _config = config;
            _drawingHelpers = drawingHelpers;
            _textureManager = textureManager;
            _backgroundRenderer = new FabBackgroundRenderer(drawingHelpers);
        }

        public void Draw(SpriteBatch b, Rectangle fabBounds, float gearRotation,
            bool isDragging, bool isHeldDown, bool isExpanded, int buttonCount)
        {
            float opacity = _config.ButtonOpacity;
            Vector2 center = fabBounds.Center.ToVector2();

            // Draw shadow jika enabled
            if (_config.ShowFabShadow && _config.FabBackground != FabBackgroundStyle.None)
            {
                DrawShadow(b, fabBounds, isDragging);
            }

            // Draw background sesuai style
            _backgroundRenderer.DrawBackground(
                b,
                fabBounds,
                _config.FabBackground,
                opacity,
                isHeldDown,
                isDragging,
                isExpanded
            );

            // Draw shadow untuk icon jika tanpa background
            if (_config.FabBackground == FabBackgroundStyle.None && _config.ShowFabShadow)
            {
                DrawIconShadow(b, center, opacity * 0.4f, GetIconScale(isDragging, isHeldDown), gearRotation);
            }

            // Draw gear icon
            Color iconTint = GetIconTint(isDragging, isHeldDown, isExpanded);
            float iconScale = GetIconScale(isDragging, isHeldDown);
            DrawGearIcon(b, center, opacity, iconScale, iconTint, gearRotation);

            // Badge
            if (!isExpanded && !isDragging && buttonCount > 0)
            {
                DrawBadge(b, fabBounds, buttonCount, opacity);
            }
        }

        private void DrawShadow(SpriteBatch b, Rectangle bounds, bool isDragging)
        {
            int offset = isDragging ? 4 : 2;
            float shadowOpacity = isDragging ? 0.4f : 0.25f;

            Rectangle shadowBounds = new Rectangle(
                bounds.X + offset,
                bounds.Y + offset,
                bounds.Width,
                bounds.Height
            );

            Vector2 center = shadowBounds.Center.ToVector2();
            int radius = shadowBounds.Width / 2;

            _drawingHelpers.DrawCircle(b, center, radius, Color.Black * shadowOpacity);
        }

        private float GetIconScale(bool isDragging, bool isHeldDown)
        {
            if (isDragging) return 1.1f;
            if (isHeldDown) return 0.9f;
            return 1f;
        }

        private Color GetIconTint(bool isDragging, bool isHeldDown, bool isExpanded)
        {
            if (isDragging) return new Color(180, 255, 180);
            if (isHeldDown) return new Color(200, 200, 220);
            if (isExpanded) return new Color(255, 240, 200);
            return Color.White;
        }

        private void DrawGearIcon(SpriteBatch b, Vector2 center, float opacity,
            float scale, Color tint, float rotation)
        {
            var gearTexture = _textureManager.GearTexture;

            if (gearTexture != null)
            {
                float targetSize = _config.ButtonSize * GEAR_DISPLAY_RATIO;
                float textureScale = (targetSize / gearTexture.Width) * scale;

                Vector2 origin = new Vector2(
                    gearTexture.Width / 2f,
                    gearTexture.Height / 2f
                );

                b.Draw(
                    gearTexture,
                    center,
                    null,
                    tint * opacity,
                    rotation,
                    origin,
                    textureScale,
                    SpriteEffects.None,
                    0f
                );
            }
            else
            {
                DrawProceduralGear(b, center, opacity, scale, rotation);
            }
        }

        private void DrawIconShadow(SpriteBatch b, Vector2 center, float opacity,
            float scale, float rotation)
        {
            var gearTexture = _textureManager.GearTexture;
            if (gearTexture == null) return;

            float targetSize = _config.ButtonSize * GEAR_DISPLAY_RATIO;
            float textureScale = (targetSize / gearTexture.Width) * scale;

            Vector2 origin = new Vector2(
                gearTexture.Width / 2f,
                gearTexture.Height / 2f
            );

            b.Draw(
                gearTexture,
                center + new Vector2(2, 2),
                null,
                Color.Black * opacity,
                rotation,
                origin,
                textureScale,
                SpriteEffects.None,
                0f
            );
        }

        private void DrawProceduralGear(SpriteBatch b, Vector2 center, float opacity,
            float scale, float rotation)
        {
            if (!_drawingHelpers.IsReady) return;

            Color gearColor = Color.White * opacity;
            int gearRadius = (int)(_config.ButtonSize * 0.25f * scale);
            int toothLength = (int)(_config.ButtonSize * 0.1f * scale);
            int toothWidth = (int)(5 * scale);
            int numTeeth = 8;

            // Center circle
            _drawingHelpers.DrawCircle(b, center, gearRadius - 3, gearColor);

            // Inner hole
            _drawingHelpers.DrawCircle(b, center, gearRadius / 3, new Color(60, 70, 90) * opacity);

            // Teeth
            for (int i = 0; i < numTeeth; i++)
            {
                float angle = rotation + (MathHelper.TwoPi / numTeeth) * i;

                Vector2 toothStart = center + new Vector2(
                    MathF.Cos(angle) * (gearRadius - 2),
                    MathF.Sin(angle) * (gearRadius - 2)
                );

                Vector2 toothEnd = center + new Vector2(
                    MathF.Cos(angle) * (gearRadius + toothLength),
                    MathF.Sin(angle) * (gearRadius + toothLength)
                );

                _drawingHelpers.DrawLine(b, toothStart, toothEnd, gearColor, toothWidth);
            }
        }

        private void DrawBadge(SpriteBatch b, Rectangle fabBounds, int count, float opacity)
        {
            if (!_drawingHelpers.IsReady) return;

            int size = 22;
            Rectangle rect = new Rectangle(
                fabBounds.Right - size / 2 - 2,
                fabBounds.Top - size / 3 + 2,
                size, size
            );

            // Badge background
            Vector2 badgeCenter = rect.Center.ToVector2();
            _drawingHelpers.DrawCircle(b, badgeCenter, size / 2, new Color(220, 60, 60) * opacity);

            // Badge text
            string text = count > 9 ? "9+" : count.ToString();
            if (Game1.tinyFont != null)
            {
                Vector2 textSize = Game1.tinyFont.MeasureString(text);
                Vector2 textPos = new Vector2(
                    rect.Center.X - textSize.X / 2,
                    rect.Center.Y - textSize.Y / 2
                );
                b.DrawString(Game1.tinyFont, text, textPos, Color.White * opacity);
            }
        }

        public void DrawDragIndicator(SpriteBatch b, Rectangle fabBounds)
        {
            if (!_drawingHelpers.IsReady || Game1.smallFont == null) return;

            string text = "Drag to move";
            Vector2 textSize = Game1.smallFont.MeasureString(text);
            Vector2 textPos = new Vector2(
                fabBounds.Center.X - textSize.X / 2,
                fabBounds.Top - textSize.Y - 12
            );

            // Background
            Rectangle bgRect = new Rectangle(
                (int)textPos.X - 8,
                (int)textPos.Y - 4,
                (int)textSize.X + 16,
                (int)textSize.Y + 8
            );
            _drawingHelpers.DrawRectangle(b, bgRect, Color.Black * 0.7f);

            // Text
            b.DrawString(Game1.smallFont, text, textPos, Color.White);
        }
    }
}