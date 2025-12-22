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
        /// Gets the category of a character: 0 for digit, 1 for letter, 2 for other.
        /// </summary>
        private static int GetCategory(char c)
        {
            if (char.IsWhiteSpace(c)) return 0;
            if (char.IsDigit(c)) return 1;
            if (char.IsLetter(c)) return 2;
            return 3;
        }

        /// <summary>
        /// Finds the last break point in the string where character categories change.
        /// Returns the index after which to break, or -1 if no break point found.
        /// </summary>
        private static int FindLastBreakPoint(string str)
        {
            for (int i = str.Length - 1; i > 0; i--)
            {
                if (GetCategory(str[i]) != GetCategory(str[i - 1]))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Adds a line to the list.
        /// </summary>
        private static void AddLine(
            List<string> lines, string line,
            ref float finalWidth, ref float finalHeight, SpriteFont font
        ) {
            Vector2 size = font.MeasureString(line);
            finalWidth = Math.Max(finalWidth, size.X);
            finalHeight += size.Y;
            lines.Add(line);
        }

        /// <summary>
        /// Wraps text to fit within a maximum width by character. Use
        /// font.MeasureString() char by char to find break points.
        /// If a threshold is provided (0 to 1), attempts to break at category boundaries
        /// if the distance from the break point to the nearest previous boundary
        /// is less than threshold * maxLineWidth.
        /// </summary>
        public static List<string> WrapText(
            SpriteFont font, string text, float maxLineWidth,
            out Vector2 finalSize, float threshold = 0.5f
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
                    // Check for break point based breaking
                    string currentStr = currentLine.ToString();
                    int lastBreakIndex = FindLastBreakPoint(currentStr);
                    if (lastBreakIndex >= 0)
                    {
                        string afterBreak = currentStr.Substring(lastBreakIndex) + c;
                        float afterBreakWidth = font.MeasureString(afterBreak).X;
                        if (afterBreakWidth < threshold * maxLineWidth)
                        {
                            // Break at break point
                            string beforeBreak = currentStr.Substring(0, lastBreakIndex);
                            AddLine(lines, beforeBreak, ref finalWidth, ref finalHeight, font);
                            currentLine.Clear();
                            currentLine.Append(afterBreak);
                            continue;
                        }
                    }

                    // Normal break (no suitable break point or threshold not met)
                    AddLine(lines, currentLine.ToString(), ref finalWidth, ref finalHeight, font);
                    currentLine.Clear();
                    currentLine.Append(c);
                }
                else
                {
                    currentLine.Append(c);
                }
            }

            if (currentLine.Length > 0) {
                AddLine(lines, currentLine.ToString(), ref finalWidth, ref finalHeight, font);
            }

            finalSize = new Vector2(finalWidth, finalHeight);
            return lines;
        }
    }
}
