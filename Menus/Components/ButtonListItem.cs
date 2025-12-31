using AddonsMobile.Framework.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace AddonsMobile.Menus.Components
{
    /*
    ╔═══════════════════════════════════════════════════════════════════════════════╗
    ║                            BUTTON LIST ITEM                                    ║
    ║                                                                               ║
    ║  Komponen UI untuk menampilkan satu button terdaftar di Dashboard.            ║
    ║  Menampilkan icon, nama, mod source, dan toggle visibility.                   ║
    ║                                                                               ║
    ║  Layout:                                                                      ║
    ║  ┌────────────────────────────────────────────────────────────────────────┐  ║
    ║  │  ┌──────┐                                                              │  ║
    ║  │  │ ICON │  Button Name                              ┌─────────┐       │  ║
    ║  │  │      │  by ModName                               │ Toggle  │       │  ║
    ║  │  └──────┘                                           └─────────┘       │  ║
    ║  └────────────────────────────────────────────────────────────────────────┘  ║
    ║                                                                               ║
    ║  Jika hidden: icon akan grayscale dan ada indikator "HIDDEN"                 ║
    ╚═══════════════════════════════════════════════════════════════════════════════╝
    */

    /// <summary>
    /// A list item component displaying a registered button with visibility toggle.
    /// </summary>
    public class ButtonListItem
    {
        // ═══════════════════════════════════════════════════════════════════════════
        // CONSTANTS
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>Height of each list item</summary>
        public const int ITEM_HEIGHT = 72;

        /// <summary>Icon size</summary>
        private const int ICON_SIZE = 48;

        /// <summary>Padding inside the item</summary>
        private const int PADDING = 12;

        /// <summary>Toggle switch width</summary>
        private const int TOGGLE_WIDTH = 56;

        /// <summary>Toggle switch height</summary>
        private const int TOGGLE_HEIGHT = 28;

        // ═══════════════════════════════════════════════════════════════════════════
        // PROPERTIES
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>The button data being displayed</summary>
        public ModKeyButton ButtonData { get; }

        /// <summary>Bounds of this list item</summary>
        public Rectangle Bounds { get; private set; }

        /// <summary>Whether this item is currently hovered</summary>
        public bool IsHovered { get; private set; }

        /// <summary>Whether the button is visible (not force-hidden)</summary>
        public bool IsVisible { get; private set; }

        /// <summary>The visibility toggle switch</summary>
        public ToggleSwitch VisibilityToggle { get; }

        // ═══════════════════════════════════════════════════════════════════════════
        // PRIVATE FIELDS
        // ═══════════════════════════════════════════════════════════════════════════

        private Rectangle _iconBounds;
        private Rectangle _textBounds;
        private readonly string _displayName;
        private readonly string _modName;
        private readonly string _category;

        // Grayscale effect untuk hidden buttons
        //private readonly Effect? _grayscaleEffect;

        // ═══════════════════════════════════════════════════════════════════════════
        // EVENTS
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>Event fired when visibility toggle changes</summary>
        public event EventHandler<ButtonVisibilityToggledEventArgs>? VisibilityToggled;

        // ═══════════════════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Creates a new button list item.
        /// </summary>
        /// <param name="buttonData">The button data to display</param>
        /// <param name="bounds">The bounds of this item</param>
        /// <param name="isVisible">Initial visibility state</param>
        public ButtonListItem(ModKeyButton buttonData, Rectangle bounds, bool isVisible)
        {
            ButtonData = buttonData ?? throw new ArgumentNullException(nameof(buttonData));
            Bounds = bounds;
            IsVisible = isVisible;

            // Extract display info
            _displayName = buttonData.DisplayName;
            _modName = buttonData.ModId;
            _category = buttonData.Category.ToString();

            // Calculate sub-bounds
            CalculateBounds();

            // Create toggle switch
            int toggleX = Bounds.Right - TOGGLE_WIDTH - PADDING;
            int toggleY = Bounds.Y + (Bounds.Height - TOGGLE_HEIGHT) / 2;

            VisibilityToggle = new ToggleSwitch(
                id: $"visibility_{buttonData.UniqueId}",
                x: toggleX,
                y: toggleY,
                initialState: isVisible,
                width: TOGGLE_WIDTH,
                height: TOGGLE_HEIGHT
            );

            // Subscribe to toggle events
            VisibilityToggle.ValueChanged += OnToggleChanged;
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // PUBLIC METHODS
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Updates the bounds of this item.
        /// </summary>
        public void SetBounds(Rectangle newBounds)
        {
            Bounds = newBounds;
            CalculateBounds();

            // Update toggle position
            int toggleX = Bounds.Right - TOGGLE_WIDTH - PADDING;
            int toggleY = Bounds.Y + (Bounds.Height - TOGGLE_HEIGHT) / 2;
            VisibilityToggle.SetPosition(toggleX, toggleY);
        }

        /// <summary>
        /// Updates the visibility state.
        /// </summary>
        public void SetVisibility(bool visible, bool animate = true)
        {
            IsVisible = visible;
            VisibilityToggle.SetState(visible, animate);
        }

        /// <summary>
        /// Updates the item.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            VisibilityToggle.Update(gameTime);
        }

        /// <summary>
        /// Handles hover input.
        /// </summary>
        public void PerformHover(int x, int y)
        {
            IsHovered = Bounds.Contains(x, y);
            VisibilityToggle.PerformHover(x, y);
        }

        /// <summary>
        /// Handles click input.
        /// </summary>
        /// <returns>True if click was handled</returns>
        public bool ReceiveClick(int x, int y)
        {
            // Check toggle first
            if (VisibilityToggle.ReceiveClick(x, y))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Draws the list item.
        /// </summary>
        public void Draw(SpriteBatch b)
        {
            // ─────────────── BACKGROUND ───────────────

            Color bgColor = Color.White * 0.1f;

            if (IsHovered)
            {
                bgColor = Color.White * 0.2f;
            }

            if (!IsVisible)
            {
                bgColor = Color.Gray * 0.15f;
            }

            // Draw background
            b.Draw(
                Game1.staminaRect,
                Bounds,
                bgColor
            );

            // Draw border
            DrawBorder(b, Bounds, IsVisible ? Game1.textColor * 0.3f : Color.Gray * 0.2f);

            // ─────────────── ICON ───────────────

            DrawIcon(b);

            // ─────────────── TEXT ───────────────

            DrawText(b);

            // ─────────────── VISIBILITY INDICATOR ───────────────

            if (!IsVisible)
            {
                DrawHiddenIndicator(b);
            }

            // ─────────────── TOGGLE SWITCH ───────────────

            VisibilityToggle.Draw(b);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // PRIVATE METHODS
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Calculates sub-bounds for icon and text areas.
        /// </summary>
        private void CalculateBounds()
        {
            // Icon bounds (left side)
            _iconBounds = new Rectangle(
                Bounds.X + PADDING,
                Bounds.Y + (Bounds.Height - ICON_SIZE) / 2,
                ICON_SIZE,
                ICON_SIZE
            );

            // Text bounds (after icon, before toggle)
            int textX = _iconBounds.Right + PADDING;
            int textWidth = Bounds.Width - ICON_SIZE - TOGGLE_WIDTH - (PADDING * 4);

            _textBounds = new Rectangle(
                textX,
                Bounds.Y + PADDING,
                textWidth,
                Bounds.Height - (PADDING * 2)
            );
        }

        /// <summary>
        /// Draws the button icon.
        /// </summary>
        private void DrawIcon(SpriteBatch b)
        {
            // Determine tint based on visibility
            Color iconTint = IsVisible ? Color.White : Color.Gray * 0.5f;

            // Check if button has custom icon
            if (ButtonData.IconTexture != null)
            {
                Rectangle sourceRect = ButtonData.IconSourceRect ??
                    new Rectangle(0, 0, ButtonData.IconTexture.Width, ButtonData.IconTexture.Height);

                // Draw custom icon
                b.Draw(
                    ButtonData.IconTexture,
                    _iconBounds,
                    sourceRect,
                    iconTint
                );

                // Apply grayscale overlay if hidden
                if (!IsVisible)
                {
                    DrawGrayscaleOverlay(b, _iconBounds);
                }
            }
            else
            {
                // Draw default icon based on category
                DrawDefaultIcon(b, iconTint);
            }

            // Draw border around icon
            DrawBorder(b, _iconBounds, IsVisible ? Color.Brown * 0.5f : Color.Gray * 0.3f);
        }

        /// <summary>
        /// Draws a default icon based on button category.
        /// </summary>
        private void DrawDefaultIcon(SpriteBatch b, Color tint)
        {
            // Use vanilla game icons based on category
            Rectangle sourceRect = ButtonData.Category switch
            {
                KeyCategory.Tools => new Rectangle(128, 64, 16, 16),      // Tool icon
                KeyCategory.Menu => new Rectangle(128, 80, 16, 16),      // Menu icon
                KeyCategory.Information => new Rectangle(144, 64, 16, 16),    // Action icon
                KeyCategory.Miscellaneous => new Rectangle(144, 80, 16, 16),  // Movement icon
                _ => new Rectangle(160, 64, 16, 16)                      // Default icon
            };

            b.Draw(
                Game1.mouseCursors,
                _iconBounds,
                sourceRect,
                tint
            );

            if (!IsVisible)
            {
                DrawGrayscaleOverlay(b, _iconBounds);
            }
        }

        /// <summary>
        /// Draws a grayscale overlay effect for hidden buttons.
        /// </summary>
        private void DrawGrayscaleOverlay(SpriteBatch b, Rectangle bounds)
        {
            /*
            Karena SpriteBatch tidak mendukung shader secara langsung tanpa
            custom Effect, kita gunakan pendekatan visual alternatif:
            
            1. Overlay semi-transparent gray
            2. Desaturate dengan blending
            
            Ini memberikan efek visual "disabled" yang jelas.
            */

            // Draw gray overlay untuk efek desaturated
            b.Draw(
                Game1.staminaRect,
                bounds,
                Color.Gray * 0.4f
            );

            // Draw diagonal lines pattern untuk indikasi disabled
            DrawDisabledPattern(b, bounds);
        }

        /// <summary>
        /// Draws diagonal line pattern for disabled state.
        /// </summary>
        private void DrawDisabledPattern(SpriteBatch b, Rectangle bounds)
        {
            // Simple pattern: small X atau lines
            Color patternColor = Color.Black * 0.2f;
            int lineWidth = 2;

            // Draw beberapa garis diagonal
            for (int i = 0; i < bounds.Width + bounds.Height; i += 8)
            {
                int x1 = bounds.X + Math.Min(i, bounds.Width);
                int y1 = bounds.Y + Math.Max(0, i - bounds.Width);
                int x2 = bounds.X + Math.Max(0, i - bounds.Height);
                int y2 = bounds.Y + Math.Min(i, bounds.Height);

                // Simplified - just draw corner markers instead of full lines
                if (i == 0 || i == bounds.Width + bounds.Height - 8)
                {
                    b.Draw(Game1.staminaRect, new Rectangle(x1, y1, lineWidth, lineWidth), patternColor);
                }
            }
        }

        /// <summary>
        /// Draws the text labels.
        /// </summary>
        private void DrawText(SpriteBatch b)
        {
            Color textColor = IsVisible ? Game1.textColor : Game1.textColor * 0.5f;
            Color subTextColor = IsVisible ? Game1.textColor * 0.7f : Game1.textColor * 0.35f;

            // ─────────────── DISPLAY NAME ───────────────

            string displayName = _displayName;
            SpriteFont font = Game1.smallFont;
            Vector2 nameSize = font.MeasureString(displayName);

            // Truncate if too long
            int maxWidth = _textBounds.Width - 8;
            if (nameSize.X > maxWidth)
            {
                while (displayName.Length > 3 && font.MeasureString(displayName + "...").X > maxWidth)
                {
                    displayName = displayName.Substring(0, displayName.Length - 1);
                }
                displayName += "...";
            }

            Vector2 namePos = new Vector2(
                _textBounds.X,
                _textBounds.Y + 4
            );

            Utility.drawTextWithShadow(b, displayName, font, namePos, textColor);

            // ─────────────── MOD NAME & CATEGORY ───────────────

            string subText = $"by {_modName} • {_category}";
            Vector2 subTextSize = Game1.smallFont.MeasureString(subText);

            // Truncate if needed
            if (subTextSize.X > maxWidth)
            {
                subText = $"by {_modName}";
                if (Game1.smallFont.MeasureString(subText).X > maxWidth)
                {
                    while (subText.Length > 5 && Game1.smallFont.MeasureString(subText + "...").X > maxWidth)
                    {
                        subText = subText.Substring(0, subText.Length - 1);
                    }
                    subText += "...";
                }
            }

            Vector2 subTextPos = new Vector2(
                _textBounds.X,
                namePos.Y + nameSize.Y + 4
            );

            Utility.drawTextWithShadow(
                b,
                subText,
                Game1.smallFont,
                subTextPos,
                subTextColor,
                1f,
                -1f,
                -1,
                -1,
                0.5f
            );
        }

        /// <summary>
        /// Draws "HIDDEN" indicator for hidden buttons.
        /// </summary>
        private void DrawHiddenIndicator(SpriteBatch b)
        {
            string hiddenText = "HIDDEN";
            Vector2 textSize = Game1.smallFont.MeasureString(hiddenText);

            // Position above the toggle
            Vector2 position = new Vector2(
                VisibilityToggle.Bounds.X + (VisibilityToggle.Bounds.Width - textSize.X) / 2,
                VisibilityToggle.Bounds.Y - textSize.Y - 4
            );

            // Draw with red color
            Utility.drawTextWithShadow(
                b,
                hiddenText,
                Game1.smallFont,
                position,
                new Color(200, 80, 80), // Red-ish
                1f,
                -1f,
                -1,
                -1,
                0.5f
            );
        }

        /// <summary>
        /// Draws a simple border around a rectangle.
        /// </summary>
        private void DrawBorder(SpriteBatch b, Rectangle rect, Color color)
        {
            int thickness = 2;

            // Top
            b.Draw(Game1.staminaRect, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
            // Bottom
            b.Draw(Game1.staminaRect, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
            // Left
            b.Draw(Game1.staminaRect, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
            // Right
            b.Draw(Game1.staminaRect, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
        }

        /// <summary>
        /// Handles toggle value changes.
        /// </summary>
        private void OnToggleChanged(object? sender, ToggleChangedEventArgs e)
        {
            IsVisible = e.NewValue;
            VisibilityToggled?.Invoke(this, new ButtonVisibilityToggledEventArgs(
                ButtonData.UniqueId,
                e.NewValue
            ));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // EVENT ARGS
    // ═══════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Event arguments for button visibility toggle.
    /// </summary>
    public class ButtonVisibilityToggledEventArgs : EventArgs
    {
        /// <summary>The button's unique ID</summary>
        public string ButtonId { get; }

        /// <summary>New visibility state</summary>
        public bool IsVisible { get; }

        public ButtonVisibilityToggledEventArgs(string buttonId, bool isVisible)
        {
            ButtonId = buttonId;
            IsVisible = isVisible;
        }
    }
}