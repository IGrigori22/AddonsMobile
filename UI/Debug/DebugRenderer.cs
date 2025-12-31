using AddonsMobile.UI.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AddonsMobile.UI.Debug
{
    /// <summary>
    /// Renderer untuk debug visualization.
    /// </summary>
    public sealed class DebugRenderer
    {
        private readonly DrawingHelpers _drawingHelpers;

        public DebugRenderer(DrawingHelpers drawingHelpers)
        {
            _drawingHelpers = drawingHelpers;
        }

        /// <summary>
        /// Draw debug outline untuk FAB.
        /// </summary>
        public void DrawFabBounds(SpriteBatch b, Rectangle fabBounds, bool isDragging)
        {
            if (!_drawingHelpers.IsReady)
                return;

            Color color = isDragging ? Color.Green * 0.8f : Color.Red * 0.8f;
            DrawBoundsOutline(b, fabBounds, color, thickness: 2);
        }

        /// <summary>
        /// Draw debug outline untuk menu bar.
        /// </summary>
        public void DrawMenuBarBounds(SpriteBatch b, Rectangle menuBarBounds)
        {
            if (!_drawingHelpers.IsReady)
                return;

            Color color = Color.Cyan * 0.8f;
            DrawBoundsOutline(b, menuBarBounds, color, thickness: 2);
        }

        /// <summary>
        /// Draw debug outline untuk menu buttons.
        /// </summary>
        public void DrawButtonBounds(SpriteBatch b, List<Rectangle> buttonBounds)
        {
            if (!_drawingHelpers.IsReady)
                return;

            Color color = Color.Yellow * 0.8f;

            foreach (var bounds in buttonBounds)
            {
                DrawBoundsOutline(b, bounds, color, thickness: 1);
            }
        }

        /// <summary>
        /// Helper untuk draw outline rectangle.
        /// </summary>
        private void DrawBoundsOutline(SpriteBatch b, Rectangle bounds, Color color, int thickness = 1)
        {
            // Top
            _drawingHelpers.DrawRectangle(b,
                new Rectangle(bounds.X, bounds.Y, bounds.Width, thickness),
                color);

            // Bottom
            _drawingHelpers.DrawRectangle(b,
                new Rectangle(bounds.X, bounds.Bottom - thickness, bounds.Width, thickness),
                color);

            // Left
            _drawingHelpers.DrawRectangle(b,
                new Rectangle(bounds.X, bounds.Y, thickness, bounds.Height),
                color);

            // Right
            _drawingHelpers.DrawRectangle(b,
                new Rectangle(bounds.Right - thickness, bounds.Y, thickness, bounds.Height),
                color);
        }
    }
}