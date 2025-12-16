using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AddonsMobile.UI.Rendering
{
    /// <summary>
    /// Helper class untuk menggambar texture dengan 9-slice scaling
    /// </summary>
    public class NineSliceRenderer
    {
        private readonly Texture2D _texture;
        private readonly int _borderLeft;
        private readonly int _borderRight;
        private readonly int _borderTop;
        private readonly int _borderBottom;
        private readonly Rectangle _sourceRect;

        // Pre-calculated source rectangles
        private readonly Rectangle _srcTopLeft;
        private readonly Rectangle _srcTopCenter;
        private readonly Rectangle _srcTopRight;
        private readonly Rectangle _srcMiddleLeft;
        private readonly Rectangle _srcMiddleCenter;
        private readonly Rectangle _srcMiddleRight;
        private readonly Rectangle _srcBottomLeft;
        private readonly Rectangle _srcBottomCenter;
        private readonly Rectangle _srcBottomRight;

        public NineSliceRenderer(Texture2D texture, int borderSize)
            : this(texture, borderSize, borderSize, borderSize, borderSize,
                   new Rectangle(0, 0, texture.Width, texture.Height))
        {
        }

        public NineSliceRenderer(Texture2D texture, int borderSize, Rectangle sourceRect)
            : this(texture, borderSize, borderSize, borderSize, borderSize, sourceRect)
        {
        }

        public NineSliceRenderer(Texture2D texture, int borderLeft, int borderRight,
            int borderTop, int borderBottom, Rectangle sourceRect)
        {
            _texture = texture;
            _borderLeft = borderLeft;
            _borderRight = borderRight;
            _borderTop = borderTop;
            _borderBottom = borderBottom;
            _sourceRect = sourceRect;

            int centerWidth = sourceRect.Width - borderLeft - borderRight;
            int centerHeight = sourceRect.Height - borderTop - borderBottom;

            // Top row
            _srcTopLeft = new Rectangle(sourceRect.X, sourceRect.Y, borderLeft, borderTop);
            _srcTopCenter = new Rectangle(sourceRect.X + borderLeft, sourceRect.Y, centerWidth, borderTop);
            _srcTopRight = new Rectangle(sourceRect.X + sourceRect.Width - borderRight, sourceRect.Y, borderRight, borderTop);

            // Middle row
            _srcMiddleLeft = new Rectangle(sourceRect.X, sourceRect.Y + borderTop, borderLeft, centerHeight);
            _srcMiddleCenter = new Rectangle(sourceRect.X + borderLeft, sourceRect.Y + borderTop, centerWidth, centerHeight);
            _srcMiddleRight = new Rectangle(sourceRect.X + sourceRect.Width - borderRight, sourceRect.Y + borderTop, borderRight, centerHeight);

            // Bottom row
            _srcBottomLeft = new Rectangle(sourceRect.X, sourceRect.Y + sourceRect.Height - borderBottom, borderLeft, borderBottom);
            _srcBottomCenter = new Rectangle(sourceRect.X + borderLeft, sourceRect.Y + sourceRect.Height - borderBottom, centerWidth, borderBottom);
            _srcBottomRight = new Rectangle(sourceRect.X + sourceRect.Width - borderRight, sourceRect.Y + sourceRect.Height - borderBottom, borderRight, borderBottom);
        }

        public void Draw(SpriteBatch b, Rectangle destRect, Color color)
        {
            if (_texture == null) return;

            int destCenterWidth = destRect.Width - _borderLeft - _borderRight;
            int destCenterHeight = destRect.Height - _borderTop - _borderBottom;

            if (destCenterWidth <= 0 || destCenterHeight <= 0)
            {
                b.Draw(_texture, destRect, _sourceRect, color);
                return;
            }

            // Top row
            b.Draw(_texture, new Rectangle(destRect.X, destRect.Y, _borderLeft, _borderTop), _srcTopLeft, color);
            b.Draw(_texture, new Rectangle(destRect.X + _borderLeft, destRect.Y, destCenterWidth, _borderTop), _srcTopCenter, color);
            b.Draw(_texture, new Rectangle(destRect.Right - _borderRight, destRect.Y, _borderRight, _borderTop), _srcTopRight, color);

            // Middle row
            b.Draw(_texture, new Rectangle(destRect.X, destRect.Y + _borderTop, _borderLeft, destCenterHeight), _srcMiddleLeft, color);
            b.Draw(_texture, new Rectangle(destRect.X + _borderLeft, destRect.Y + _borderTop, destCenterWidth, destCenterHeight), _srcMiddleCenter, color);
            b.Draw(_texture, new Rectangle(destRect.Right - _borderRight, destRect.Y + _borderTop, _borderRight, destCenterHeight), _srcMiddleRight, color);

            // Bottom row
            b.Draw(_texture, new Rectangle(destRect.X, destRect.Bottom - _borderBottom, _borderLeft, _borderBottom), _srcBottomLeft, color);
            b.Draw(_texture, new Rectangle(destRect.X + _borderLeft, destRect.Bottom - _borderBottom, destCenterWidth, _borderBottom), _srcBottomCenter, color);
            b.Draw(_texture, new Rectangle(destRect.Right - _borderRight, destRect.Bottom - _borderBottom, _borderRight, _borderBottom), _srcBottomRight, color);
        }
    }
}