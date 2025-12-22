using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LabelChest.Managers
{
    /// <summary>
    /// Provides utility methods for text processing.
    /// </summary>
    public static class TextUtils
    {
        /// <summary>
        /// Wraps text to fit within a maximum width by character. Use
        /// font.MeasureString() char by char to find break points.
        /// </summary>
        public static List<string> WrapText(
            SpriteFont font, string text, float maxLineWidth,
            out Vector2 finalSize
        ) {
            if (string.IsNullOrEmpty(text))
            {
                finalSize = Vector2.Zero;
                return new List<string>();
            }
            
            List<string> lines = new List<string>();
            StringBuilder currentLine = new StringBuilder();
            float finalWidth = 0f;
            float finalHeight = 0f;

            foreach (char c in text)
            {
                string testString = currentLine.ToString() + c;
                Vector2 size = font.MeasureString(testString);
                
                if (size.X > maxLineWidth && currentLine.Length > 0)
                {
                    // Accumulate the size to the final size.
                    Vector2 currentSize = font.MeasureString(currentLine.ToString());
                    finalWidth = Math.Max(finalWidth, currentSize.X);
                    finalHeight += currentSize.Y;
                    // Add this new line to the array.
                    lines.Add(currentLine.ToString());
                    currentLine.Clear();
                    currentLine.Append(c);
                }
                else
                {
                    currentLine.Append(c);
                }
            }

            if (currentLine.Length > 0) {
                // Accumulate the size to the final size.
                Vector2 currentSize = font.MeasureString(currentLine.ToString());
                finalWidth = Math.Max(finalWidth, currentSize.X);
                finalHeight += currentSize.Y;
                // Add this new line to the array.
                lines.Add(currentLine.ToString());
            }

            finalSize = new Vector2(finalWidth, finalHeight);
            return lines;
        }
    }
}