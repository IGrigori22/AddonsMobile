using AddonsMobile.Config;
using AddonsMobile.Framework;
using AddonsMobile.UI.Data;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

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

        private const int MENU_BAR_PADDING = 12;
        private const int MENU_BUTTON_MARGIN = 8;
        private const int FAB_MENU_SPACING = 15;

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
            int totalMarginsWidth = (buttonCount - 1) * MENU_BUTTON_MARGIN;

            return totalButtonsWidth + totalMarginsWidth + (MENU_BAR_PADDING * 2);
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
            int menuBarHeight = buttonSize + (MENU_BAR_PADDING * 2);
            float targetWidth = CalculateTargetWidth(buttonCount);

            // Determine left/right placement
            bool showOnLeft = (fabPosition.X + fabSize + targetWidth + FAB_MENU_SPACING) > screenWidth;

            int menuBarX;
            if (showOnLeft)
            {
                menuBarX = (int)fabPosition.X - (int)currentWidth - FAB_MENU_SPACING;
            }
            else
            {
                menuBarX = (int)fabPosition.X + fabSize + FAB_MENU_SPACING;
            }

            int menuBarY = (int)fabPosition.Y + (fabSize - menuBarHeight) / 2;

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

            layout.IsLeftSide = showOnLeft;

            // Calculate button bounds
            int buttonX = menuBarX + MENU_BAR_PADDING;
            int buttonY = menuBarY + MENU_BAR_PADDING;

            for (int i = 0; i < buttonCount; i++)
            {
                layout.ButtonBounds.Add(new Rectangle(
                    buttonX + i * (buttonSize + MENU_BUTTON_MARGIN),
                    buttonY,
                    buttonSize,
                    buttonSize
                ));
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