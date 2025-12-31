using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace AddonsMobile.Menus.Components
{
    /// <summary>
    /// Tooltip renderer dengan dukungan format warna sederhana.
    ///  §c = color code, §r = reset
    /// </summary>
    public static class ButtonInfoTooltip
    {
        private static readonly Dictionary<char, Color> ColorCodes = new()
        {
            ['r'] = Color.White,
            ['0'] = Color.Black,
            ['1'] = Color.DarkBlue,
            ['2'] = Color.DarkGreen,
            ['3'] = Color.DarkCyan,
            ['4'] = Color.DarkRed,
            ['5'] = Color.Purple,
            ['6'] = Color.Gold,
            ['7'] = Color.LightGray,
            ['8'] = Color.DarkGray,
            ['9'] = Color.Blue,
            ['a'] = Color.LimeGreen,
            ['b'] = Color.Cyan,
            ['c'] = Color.Red,
            ['d'] = Color.Magenta,
            ['e'] = Color.Yellow,
            ['f'] = Color.White
        };

        private const int Padding = 16;
        private const int LineSpacing = 4;
        private const int MaxWidth = 400;
        private const int MinWidth = 200;

        // ═══════════════════════════════════════════════════════════
        // Draw
        // ═══════════════════════════════════════════════════════════

        public static void Draw(SpriteBatch b, string text, int x, int y)
        {
            if (!string.IsNullOrEmpty(text))
                return;

            // Parsing dan measuring teks
            var lines = ParseColoredText(text);
            Vector2 textSize = MeasureText(lines);

            // Calculate tooltip bounds
            int width = Math.Clamp((int)textSize.X + Padding * 2, MinWidth, MaxWidth);
            int height = (int)textSize.Y + Padding * 2;

            // Adjust posisi untuk tetap pada layar
            Rectangle tooltipBounds = AdjustPosition(x, y, width, height);

            // Draw background
            DrawBackground(b, tooltipBounds);

            // Draw teks
            DrawText(b, lines, tooltipBounds.X + Padding, tooltipBounds.Y + Padding);
        }

        /// <summary>
        /// Draw tooltip dekat mouse kursor
        /// </summary>
        /// <param name="b"></param>
        /// <param name="text"></param>
        public static void DrawAtCursor(SpriteBatch b, string text)
        {
            int mouseX = Game1.getMouseX() + 32;
            int mouseY = Game1.getMouseY() + 32;
            Draw(b, text, mouseX, mouseY);
        }

        // ═══════════════════════════════════════════════════════════
        // Parsing
        // ═══════════════════════════════════════════════════════════

        private struct TextSegment
        {
            public string Text;
            public Color Color;
        }

        private struct ParsedLine
        {
            public List<TextSegment> Segments;
            public float Width;
            public float Height;
        }

        private static List<ParsedLine> ParseColoredText(string text)
        {
            var lines = new List<ParsedLine>();
            var textLines = text.Split('\n');
            var font = Game1.smallFont;

            foreach (var line in textLines)
            {
                var parsedLine = new ParsedLine
                {
                    Segments = new List<TextSegment>(),
                    Height = font.LineSpacing
                };

                // Parse color codes
                Color currentColor = Color.White;
                int lastIndex = 0;

                for (int i = 0; i < line.Length; i++)
                {
                    if (line[i] == '§' && i + 1 < line.Length)
                    {
                        // Add previous segment
                        if (i > lastIndex)
                        {
                            string segmentText = line.Substring(lastIndex, i - lastIndex);
                            parsedLine.Segments.Add(new TextSegment
                            {
                                Text = segmentText,
                                Color = currentColor
                            });
                            parsedLine.Width += font.MeasureString(segmentText).X;
                        }

                        // Get new color 
                        char colorCode = char.ToLower(line[i + 1]);
                        if (ColorCodes.TryGetValue(colorCode, out Color newColor))
                        {
                            currentColor = newColor;
                        }

                        i++; // Skip color code
                        lastIndex = i + 1;
                    }
                }

                // Add remaining text
                if (lastIndex < line.Length)
                {
                    string remainingText = line.Substring(lastIndex);
                    parsedLine.Segments.Add(new TextSegment
                    {
                        Text = remainingText,
                        Color = currentColor
                    });
                    parsedLine.Width += font.MeasureString(remainingText).X;
                }

                // Handle empty lines
                if (parsedLine.Segments.Count == 0)
                {
                    parsedLine.Segments.Add(new TextSegment { Text = " ", Color = Color.White });
                    parsedLine.Height = font.LineSpacing * 0.5f;
                }

                lines.Add(parsedLine);
            }

            return lines;
        }

        private static Vector2 MeasureText(List<ParsedLine> lines)
        {
            float maxWidth = 0;
            float totalHeight = 0;

            foreach (var line in lines)
            {
                maxWidth = Math.Max(maxWidth, line.Width);
                totalHeight += line.Height + LineSpacing;
            }

            return new Vector2(maxWidth, totalHeight);
        }

        // ═══════════════════════════════════════════════════════════
        // Drawing Helpers
        // ═══════════════════════════════════════════════════════════

        private static Rectangle AdjustPosition(int x, int y, int width, int height)
        {
            int viewWidth = Game1.uiViewport.Width;
            int viewHeight = Game1.uiViewport.Height;

            // Adjust horizontal
            if (x + width > viewWidth - 16)
            {
                x = viewWidth - width - 16;
            }
            if (x < 16)
            {
                x = 16;
            }

            // Adjust vertical
            if (y + height > viewHeight - 16)
            {
                y = viewHeight - height - 16;
            }
            if (y < 16)
            {
                y = 16;
            }

            return new Rectangle(x, y, width, height);
        }

        private static void DrawBackground(SpriteBatch b, Rectangle bounds)
        {
            // Draw shadow
            IClickableMenu.drawTextureBox(
                b,
                Game1.menuTexture,
                new Rectangle(0, 256, 60, 60),
                bounds.X + 4,
                bounds.Y + 4,
                bounds.Width,
                bounds.Height,
                Color.Black * 0.5f,
                1f,
                drawShadow: false
            );

            // Draw background
            IClickableMenu.drawTextureBox(
                b,
                Game1.menuTexture,
                new Rectangle(0, 256, 60, 60),
                bounds.X,
                bounds.Y,
                bounds.Width,
                bounds.Height,
                new Color(40, 40, 60),
                1f,
                drawShadow: false
            );
        }

        private static void DrawText(SpriteBatch b, List<ParsedLine> lines, int startX, int startY)
        {
            var font = Game1.smallFont;
            float currentY = startY;

            foreach (var line in lines)
            {
                float currentX = startX;

                foreach (var segment in line.Segments)
                {
                    // Shadow
                    b.DrawString(
                        font,
                        segment.Text,
                        new Vector2(currentX + 1, currentY + 1),
                        Color.Black * 0.5f
                    );

                    // Draw text
                    b.DrawString(
                        font,
                        segment.Text,
                        new Vector2(currentX, currentY),
                        segment.Color
                    );

                    currentX += font.MeasureString(segment.Text).X;
                }

                currentY += line.Height + LineSpacing;
            }
        }
    }
}