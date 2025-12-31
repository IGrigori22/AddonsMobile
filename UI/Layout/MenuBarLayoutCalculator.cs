using AddonsMobile.Config;
using AddonsMobile.UI.Data;
using Microsoft.Xna.Framework;

namespace AddonsMobile.UI.Layout
{
    /// <summary>
    /// Calculator untuk menu bar bounds dan button positions.
    /// Extracted dari MobileButtonManager untuk better separation.
    /// </summary>
    public sealed class MenuBarLayoutCalculator
    {
        // ═══════════════════════════════════════════════════════════════════════════
        // CONSTANTS
        // ═══════════════════════════════════════════════════════════════════════════

        private const int MenuBarPadding = 12;
        private const int MenuButtonMargin = 8;
        private const int FABMenuSpacing = 15;

        // ═══════════════════════════════════════════════════════════════════════════
        // FIELDS
        // ═══════════════════════════════════════════════════════════════════════════

        private readonly ModConfig _config;

        // ═══════════════════════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ═══════════════════════════════════════════════════════════════════════════

        public MenuBarLayoutCalculator(ModConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // PUBLIC METHODS
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Hitung target width untuk menu bar berdasarkan jumlah buttons.
        /// </summary>
        public float CalculateTargetWidth(int buttonCount)
        {
            if (buttonCount <= 0)
                return 0f;

            int buttonSize = _config.MenuButtonSize;
            int totalButtonsWidth = buttonCount * buttonSize;
            int totalMarginsWidth = (buttonCount - 1) * MenuButtonMargin;

            return totalButtonsWidth + totalMarginsWidth + (MenuBarPadding * 2);
        }

        /// <summary>
        /// Calculate menu bar bounds dan button positions.
        /// </summary>
        public MenuBarLayout Calculate(
            Vector2 fabPosition,
            int fabSize,
            int buttonCount,
            float currentWidth,
            int screenWidth,
            int screenHeight)
        {
            var layout = new MenuBarLayout();

            if (buttonCount <= 0)
            {
                layout.MenuBarBounds = Rectangle.Empty;
                return layout;
            }

            int buttonSize = _config.MenuButtonSize;
            int menuBarHeight = buttonSize + (MenuBarPadding * 2);
            float targetWidth = CalculateTargetWidth(buttonCount);

            // Determine left/right placement
            bool showOnLeft = (fabPosition.X + fabSize + targetWidth + FABMenuSpacing) > screenWidth;
            layout.IsLeftSide = showOnLeft;

            int menuBarX;
            int menuBarY = (int)fabPosition.Y + (fabSize - menuBarHeight) / 2;

            if (showOnLeft)
            {
                menuBarX = (int)fabPosition.X - (int)currentWidth - FABMenuSpacing;
            }
            else
            {
                menuBarX = (int)fabPosition.X + fabSize + FABMenuSpacing;
            }

            // Clamp to screen bounds
            menuBarX = Math.Max(PositionManager.SCREEN_EDGE_PADDING, menuBarX);
            menuBarY = MathHelper.Clamp(
                menuBarY,
                PositionManager.SCREEN_EDGE_PADDING,
                screenHeight - menuBarHeight - PositionManager.SCREEN_EDGE_PADDING
            );

            layout.MenuBarBounds = new Rectangle(
                menuBarX,
                menuBarY,
                (int)currentWidth,
                menuBarHeight
            );

            int buttonY = menuBarY + MenuBarPadding;

            if (showOnLeft)
            {
                int menuRightEdge = (int)fabPosition.X - FABMenuSpacing;

                for (int i = 0; i < buttonCount; i++)
                {
                    // Hitung X dari kanan ke kiri
                    // Button 0: paling kanan
                    // Button 1: di sebelah kiri Button 0
                    // dst...
                    int buttonX = menuRightEdge - MenuBarPadding - buttonSize
                                  - i * (buttonSize + MenuButtonMargin);

                    layout.ButtonBounds.Add(new Rectangle(
                        buttonX,
                        buttonY,
                        buttonSize,
                        buttonSize
                    ));
                }
            }
            else
            {
                // Left edge menu bar FIXED di: fabPosition.X + fabSize + FAB_MENU_SPACING
                int menuLeftEdge = (int)fabPosition.X + fabSize + FABMenuSpacing;

                for (int i = 0; i < buttonCount; i++)
                {
                    // Hitung X dari kiri ke kanan
                    int buttonX = menuLeftEdge + MenuBarPadding
                                  + i * (buttonSize + MenuButtonMargin);

                    layout.ButtonBounds.Add(new Rectangle(
                        buttonX,
                        buttonY,
                        buttonSize,
                        buttonSize
                    ));
                }
            }

            return layout;
        }
    }

    /// <summary>
    /// Result dari layout calculation.
    /// </summary>
    public class MenuBarLayout
    {
        public Rectangle MenuBarBounds { get; set; }
        public List<Rectangle> ButtonBounds { get; set; } = new();
        public bool IsLeftSide { get; set; }
    }
}