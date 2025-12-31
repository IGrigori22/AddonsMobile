using AddonsMobile.Config;
using AddonsMobile.Framework;
using AddonsMobile.UI.Animation;
using AddonsMobile.UI.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace AddonsMobile.UI.Components
{
    /// <summary>
    /// Bertanggung jawab untuk menggambar Menu Bar dan button-button di dalamnya
    /// </summary>
    public class MenuBarRenderer
    {
        private readonly ModConfig _config;
        private readonly DrawingHelpers _drawingHelpers;
        private readonly TextureManager _textureManager;

        private const int MENU_BAR_PADDING = 12;
        private const int MENU_BAR_CORNER_RADIUS = 8;
        private const int MENU_BUTTON_MARGIN = 8;

        public MenuBarRenderer(ModConfig config, DrawingHelpers drawingHelpers, TextureManager textureManager)
        {
            _config = config;
            _drawingHelpers = drawingHelpers;
            _textureManager = textureManager;
        }

        public void Draw(SpriteBatch b, Rectangle menuBarBounds, Vector2 fabPosition,
            float menuBarWidth, float menuBarOpacity, float expandProgress,
            List<Rectangle> buttonBounds, List<ModKeyButton> buttons)
        {
            float opacity = menuBarOpacity * _config.MenuButtonOpacity;

            int animatedWidth = (int)menuBarWidth;
            if (animatedWidth <= 0) return;

            bool growFromRight = menuBarBounds.X < fabPosition.X;

            Rectangle animatedBounds;
            if (growFromRight)
            {
                animatedBounds = new Rectangle(
                    menuBarBounds.Right - animatedWidth,
                    menuBarBounds.Y,
                    animatedWidth,
                    menuBarBounds.Height
                );
            }
            else
            {
                animatedBounds = new Rectangle(
                    menuBarBounds.X,
                    menuBarBounds.Y,
                    animatedWidth,
                    menuBarBounds.Height
                );
            }

            // Shadow
            _drawingHelpers.DrawShadow(b, animatedBounds, 4, 0.3f * opacity);

            // Background
            if (_textureManager.MenuBarRenderer != null)
            {
                _textureManager.MenuBarRenderer.Draw(b, animatedBounds, Color.White * opacity);
            }
            else
            {
                DrawDefaultBackground(b, animatedBounds, opacity);
            }

            // Buttons
            if (expandProgress > 0.3f)
            {
                DrawButtons(b, buttonBounds, buttons, expandProgress, opacity);
            }
        }

        private void DrawDefaultBackground(SpriteBatch b, Rectangle bounds, float opacity)
        {
            Color bgColor = new Color(40, 45, 55, (int)(230 * opacity));
            _drawingHelpers.DrawRoundedRectangle(b, bounds, bgColor, MENU_BAR_CORNER_RADIUS);

            Color borderColor = new Color(80, 90, 110) * opacity;
            _drawingHelpers.DrawRoundedBorder(b, bounds, borderColor, MENU_BAR_CORNER_RADIUS, 2);
        }

        private void DrawButtons(SpriteBatch b, List<Rectangle> buttonBounds,
            List<ModKeyButton> buttons, float expandProgress, float opacity)
        {
            float buttonOpacity = Math.Min(1f, (expandProgress - 0.3f) / 0.7f) * opacity;

            for (int i = 0; i < buttonBounds.Count && i < buttons.Count; i++)
            {
                var bounds = buttonBounds[i];
                var button = buttons[i];

                float delay = i * 0.1f;
                float progress = MathHelper.Clamp((expandProgress - delay) / (1f - delay), 0f, 1f);

                if (progress <= 0) continue;

                float easedProgress = EasingFunctions.EaseOutBack(progress);
                int size = (int)(bounds.Width * easedProgress);
                if (size <= 0) continue;

                Rectangle scaledBounds = new Rectangle(
                    bounds.Center.X - size / 2,
                    bounds.Center.Y - size / 2,
                    size, size
                );

                // Button frame
                float frameOpacity = button.IsEnabled ? buttonOpacity : buttonOpacity * 0.5f;

                if (_textureManager.ButtonFrameRenderer != null)
                {
                    _textureManager.ButtonFrameRenderer.Draw(b, scaledBounds, Color.White * frameOpacity);
                }
                else
                {
                    DrawDefaultButtonFrame(b, scaledBounds, button.IsEnabled, buttonOpacity);
                }

                // Content
                if (progress > 0.5f)
                {
                    DrawButtonContent(b, button, scaledBounds, buttonOpacity * progress);
                }
            }
        }

        private void DrawDefaultButtonFrame(SpriteBatch b, Rectangle bounds, bool isEnabled, float opacity)
        {
            Color buttonBgColor = isEnabled
                ? new Color(70, 80, 100, (int)(220 * opacity))
                : new Color(50, 55, 65, (int)(150 * opacity));

            _drawingHelpers.DrawRoundedRectangle(b, bounds, buttonBgColor, 6);

            Color buttonBorderColor = new Color(100, 115, 140) * opacity;
            _drawingHelpers.DrawRoundedBorder(b, bounds, buttonBorderColor, 6, 1);
        }

        private void DrawButtonContent(SpriteBatch b, ModKeyButton button, Rectangle bounds, float opacity)
        {
            if (button.IconTexture != null)
            {
                int iconSize = (int)(bounds.Width * 0.65f);
                Rectangle iconRect = new Rectangle(
                    bounds.Center.X - iconSize / 2,
                    bounds.Center.Y - iconSize / 2,
                    iconSize, iconSize
                );

                b.Draw(
                    button.IconTexture,
                    iconRect,
                    button.IconSourceRect,
                    button.TintColor * opacity
                );
            }
            else if (Game1.smallFont != null)
            {
                string text = button.DisplayName.Length > 0
                    ? button.DisplayName[0].ToString().ToUpper()
                    : "?";

                Vector2 textSize = Game1.smallFont.MeasureString(text);
                Vector2 textPos = new Vector2(
                    bounds.Center.X - textSize.X / 2,
                    bounds.Center.Y - textSize.Y / 2
                );

                b.DrawString(Game1.smallFont, text, textPos + new Vector2(1, 1), Color.Black * (opacity * 0.5f));
                b.DrawString(Game1.smallFont, text, textPos, Color.White * opacity);
            }

            // Label
            if (_config.ShowButtonLabels && Game1.tinyFont != null && opacity > 0.7f)
            {
                string label = button.DisplayName;
                if (label.Length > 8) label = label.Substring(0, 7) + "..";

                Vector2 labelSize = Game1.tinyFont.MeasureString(label);
                Vector2 labelPos = new Vector2(
                    bounds.Center.X - labelSize.X / 2,
                    bounds.Bottom + 3
                );

                _drawingHelpers.DrawRectangle(b, new Rectangle(
                    (int)labelPos.X - 3,
                    (int)labelPos.Y - 1,
                    (int)labelSize.X + 6,
                    (int)labelSize.Y + 2
                ), Color.Black * (opacity * 0.6f));

                b.DrawString(Game1.tinyFont, label, labelPos, Color.White * opacity);
            }
        }
    }
}