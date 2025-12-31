using AddonsMobile.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace AddonsMobile.Menus.Components
{
    public class ButtonGridItem : ClickableComponent
    {
        public const int ItemWidth = 300;
        public const int ItemHeight = 80;
        public const int IconSize = 48;
        public const int Padding = 12;

        public ModKeyButton ButtonData { get; }
        public bool IsHovered { get; private set; }
        public int GridRow { get; set; }
        public int GridColumn { get; set; }

        // Visual State
        private float _hoverScale = 1f;
        private float _hoverAlpha = 0f;
        private const float HoverScaleTarget = 1.02f;
        private const float AnimationSpeed = 8f;

        public ButtonGridItem(ModKeyButton button, Rectangle bounds, int row, int column)
            : base(bounds, button.UniqueId)
        {
            ButtonData = button ?? throw new ArgumentNullException(nameof(button));
            GridRow = row;
            GridColumn = column;
        }

        // ═══════════════════════════════════════════════════════════
        // Update section
        // ═══════════════════════════════════════════════════════════

        public void Update(GameTime gameTime, int mouseX, int mouseY)
        {
            IsHovered = bounds.Contains(mouseX, mouseY);

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Animation hover effects
            float targetScale = IsHovered ? HoverScaleTarget : 1f;
            float targetAlpha = IsHovered ? 1f : 0f;

            _hoverScale = MathHelper.Lerp(_hoverScale, targetScale, AnimationSpeed * deltaTime);
            _hoverAlpha = MathHelper.Lerp(_hoverAlpha, targetAlpha, AnimationSpeed * deltaTime);
        }

        // ═══════════════════════════════════════════════════════════
        // Drawing section
        // ═══════════════════════════════════════════════════════════

        public void Draw(SpriteBatch b, int offsetY = 0)
        {
            Rectangle drawBounds = new Rectangle(
                bounds.X,
                bounds.Y - offsetY,
                bounds.Width,
                bounds.Height
            );

            // Skip if outside visible area
            if (drawBounds.Bottom < 0 || drawBounds.Top > Game1.uiViewport.Height)
            {
                return;
            }

            // Calculate scaled bounds for hover effect
            if (_hoverScale != 1f)
            {
                int widthDiff = (int)(drawBounds.Width * (_hoverScale - 1f));
                int heightDiff = (int)(drawBounds.Height * (_hoverScale - 1f));
                drawBounds.Inflate(widthDiff / 2, heightDiff / 2);
            }

            // Draw Background
            DrawBackground(b, drawBounds);

            // Draw Content
            DrawIcon(b, drawBounds);
            DrawText(b, drawBounds);
            DrawStatusIndicators(b, drawBounds);

            // Draw hover highlight
            if (_hoverAlpha > 0.01f)
            {
                DrawHoverHighlight(b, drawBounds);
            }
        }

        private void DrawBackground(SpriteBatch b, Rectangle bounds)
        {
            Color bgColor = GetBackgroundColor();

            // Draw rounded rectangle background using game texture
            IClickableMenu.drawTextureBox(
                b,
                Game1.menuTexture,
                new Rectangle(0, 256, 60, 60),
                bounds.X,
                bounds.Y,
                bounds.Width,
                bounds.Height,
                bgColor,
                1f,
                drawShadow: false
            );
        }

        /// <summary>
        /// Tentukan warna latar belakang menurut status
        /// </summary>
        /// <returns></returns>
        private Color GetBackgroundColor()
        {
            if (!ButtonData.IsEnabled)
            {
                return new Color(80, 80, 80);
            }

            if (!ButtonData.ShouldShow())
                return new Color(100, 80, 80);

            if (ButtonData.Type == ButtonType.Toggle && ButtonData.IsToggled)
                return new Color(80, 120, 80);

            return IsHovered ? new Color(100, 100, 120) : new Color(60, 60, 80);
        }

        private void DrawIcon(SpriteBatch b, Rectangle bounds)
        {
            Rectangle iconBounds = new Rectangle(
                bounds.X + Padding,
                bounds.Y + (bounds.Height - IconSize) / 2,
                IconSize,
                IconSize
            );

            if (ButtonData.IconTexture != null)
            {
                Rectangle sourceRect = ButtonData.IconSourceRect ??
                    new Rectangle(0, 0, ButtonData.IconTexture.Width, ButtonData.IconTexture.Height);

                b.Draw(
                    ButtonData.IconTexture,
                    iconBounds,
                    sourceRect,
                    ButtonData.GetCurrentTintColor()
                );
            }
            else
            {
                // Draw placeholder icon (category-based)
                DrawPlaceholderIcon(b, iconBounds);
            }
        }

        private void DrawPlaceholderIcon(SpriteBatch b, Rectangle iconBounds)
        {
            // Gunakan warna kategori sebagai placeholder
            Color categoryColor = ButtonData.Category.GetThemeColor();

            // Gambar rectangle simple berwarna dengan border
            b.Draw(
                Game1.staminaRect,
                iconBounds,
                categoryColor * 0.8f
            );

            // Draw first letter of display name
            string initial = ButtonData.DisplayName.Length > 0
                ? ButtonData.DisplayName[0].ToString().ToUpper()
                : "?";

            Vector2 textSize = Game1.smallFont.MeasureString(initial);
            Vector2 textPos = new Vector2(
                iconBounds.X + (iconBounds.Width - textSize.X) / 2,
                iconBounds.Y + (iconBounds.Height - textSize.Y) / 2
            );

            b.DrawString(
                Game1.smallFont,
                initial,
                textPos,
                Color.White
            );
        }

        private void DrawText(SpriteBatch b, Rectangle bounds)
        {
            int textX = bounds.X + Padding + IconSize + Padding;
            int textWidth = bounds.Width - Padding * 3 - IconSize - 60; // Reserve space for indicators

            // Display name
            string displayName = ButtonData.DisplayName;
            if (Game1.smallFont.MeasureString(displayName).X > textWidth)
            {
                displayName = TruncateText(displayName, textWidth, Game1.smallFont);
            }

            b.DrawString(
                Game1.smallFont,
                displayName,
                new Vector2(textX, bounds.Y + Padding),
                ButtonData.IsEnabled ? Color.White : Color.Gray
            );

            // Category and Type info
            string subtitle = $"{ButtonData.Category.GetDisplayName()} • {ButtonData.Type}";
            b.DrawString(
                Game1.smallFont,
                subtitle,
                new Vector2(textX, bounds.Y + Padding + 24),
                Color.LightGray * 0.8f,
                0f,
                Vector2.Zero,
                0.8f,
                SpriteEffects.None,
                0f
            );

            // Mod ID (smaller)
            string modInfo = $"by {ButtonData.ModId}";
            if (Game1.smallFont.MeasureString(modInfo).X * 0.6f > textWidth)
            {
                modInfo = TruncateText(modInfo, (int)(textWidth / 0.6f), Game1.smallFont);
            }

            b.DrawString(
                Game1.smallFont,
                modInfo,
                new Vector2(textX, bounds.Y + Padding + 44),
                Color.Gray * 0.6f,
                0f,
                Vector2.Zero,
                0.6f,
                SpriteEffects.None,
                0f
            );
        }


        private void DrawStatusIndicators(SpriteBatch b, Rectangle bounds)
        {
            int indicatorX = bounds.Right - Padding - 20;
            int indicatorY = bounds.Y + Padding;

            // Enabled/Disabled indicator
            Color enabledColor = ButtonData.IsEnabled ? Color.LimeGreen : Color.Red;
            b.Draw(
                Game1.staminaRect,
                new Rectangle(indicatorX, indicatorY, 12, 12),
                enabledColor
            );

            // Toggle state indicator
            if (ButtonData.Type == ButtonType.Toggle)
            {
                indicatorY += 18;
                Color toggleColor = ButtonData.IsToggled ? Color.Cyan : Color.DarkGray;
                b.Draw(
                    Game1.staminaRect,
                    new Rectangle(indicatorX, indicatorY, 12, 12),
                    toggleColor
                );
            }

            // Visibility indicator
            indicatorY += 18;
            bool isVisible = ButtonData.ShouldShow();
            Color visColor = isVisible ? Color.White : Color.DarkRed;
            b.Draw(
                Game1.staminaRect,
                new Rectangle(indicatorX, indicatorY, 12, 12),
                visColor * 0.7f
            );
        }

        private void DrawHoverHighlight(SpriteBatch b, Rectangle bounds)
        {
            // Draw glowing border
            Color highlightColor = Color.White * (_hoverAlpha * 0.3f);

            int borderWidth = 2;

            // Top
            b.Draw(Game1.staminaRect, new Rectangle(bounds.X, bounds.Y, bounds.Width, borderWidth), highlightColor);
            // Bottom
            b.Draw(Game1.staminaRect, new Rectangle(bounds.X, bounds.Bottom - borderWidth, bounds.Width, borderWidth), highlightColor);
            // Left
            b.Draw(Game1.staminaRect, new Rectangle(bounds.X, bounds.Y, borderWidth, bounds.Height), highlightColor);
            // Right
            b.Draw(Game1.staminaRect, new Rectangle(bounds.Right - borderWidth, bounds.Y, borderWidth, bounds.Height), highlightColor);
        }

        // ═══════════════════════════════════════════════════════════
        // Helpers
        // ═══════════════════════════════════════════════════════════
        private string TruncateText(string text, int maxWidth, SpriteFont font)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            string ellipsis = "...";
            float ellipsisWidth = font.MeasureString(ellipsis).X;

            if (font.MeasureString(text).X <= maxWidth)
                return text;

            while (text.Length > 0 && font.MeasureString(text + ellipsis).X > maxWidth)
            {
                text = text.Substring(0, text.Length - 1);
            }

            return text + ellipsis;
        }

        /// <summary>
        /// Get detailed tooltip text for this button.
        /// </summary>
        /// <returns></returns>
        public string GetTooltipText()
        {
            var sb = new System.Text.StringBuilder();

            sb.AppendLine($"§b{ButtonData.DisplayName}§r");
            sb.AppendLine();

            if (!string.IsNullOrEmpty(ButtonData.Description))
            {
                sb.AppendLine(ButtonData.Description);
                sb.AppendLine();
            }

            sb.AppendLine($"§6ID:§r {ButtonData.UniqueId}");
            sb.AppendLine($"§6Mod:§r {ButtonData.ModId}");
            sb.AppendLine($"§6Category:§r {ButtonData.Category.GetDisplayName()}");
            sb.AppendLine($"§6Type:§r {ButtonData.Type}");
            sb.AppendLine($"§6Priority:§r {ButtonData.Priority}");
            sb.AppendLine($"§6Cooldown:§r {ButtonData.PressCooldown}ms");

            sb.AppendLine();
            sb.AppendLine("§e--- Status ---§r");
            sb.AppendLine($"§6Enabled:§r {(ButtonData.IsEnabled ? "§aYes§r" : "§cNo§r")}");
            sb.AppendLine($"§6Visible:§r {(ButtonData.ShouldShow() ? "§aYes§r" : "§cNo§r")}");

            if (!string.IsNullOrEmpty(ButtonData.OriginalKeybind))
            {
                sb.AppendLine($"§6Keybind:§r {ButtonData.OriginalKeybind}");
            }

            return sb.ToString();
        }
    }
}