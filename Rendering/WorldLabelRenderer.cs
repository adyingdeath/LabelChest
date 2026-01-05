using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;
using LabelChest.Managers;
using static StardewValley.LocalizedContentManager;

namespace LabelChest.Rendering {
    /// <summary>
    /// Renders chest labels in the game world.
    /// </summary>
    public class WorldLabelRenderer {
        // Configuration constants
        private static readonly float BaseFontScale = Game1.content.GetCurrentLanguage() switch {
            LanguageCode.en => 0.64631576f,
            LanguageCode.zh => 0.8f,
            _ => 0.8f
        };
        private static float FontScale => BaseFontScale * ModEntry.Config.FontSize;
        private const int WorldMaxWidth = 70;
        private const int Padding = 20;

        /// <summary>
        /// Draws a label above a chest at the specified tile position.
        /// </summary>
        public void DrawLabelWithTile(SpriteBatch spriteBatch, Vector2 tileLocation, string text, Chest chest) {
            Vector2 screenPos = Game1.GlobalToLocal(Game1.viewport, tileLocation * Game1.tileSize);
            Vector2 centerPos = new Vector2(screenPos.X + Game1.tileSize / 2f, screenPos.Y);
            DrawLabel(spriteBatch, centerPos, text, chest);
        }

        /// <summary>
        /// Draws a label at the specified absolute screen position with dynamic coloring based on chest.
        /// </summary>
        public void DrawLabel(SpriteBatch spriteBatch, Vector2 position, string text, Chest chest) {
            SpriteFont font = Game1.smallFont;
            List<string> lines = TextUtils.WrapText(
                font, text, WorldMaxWidth / FontScale,
                out Vector2 textBoxSize
            );

            if (lines.Count == 0) return;

            // Determine text color based on configuration and chest
            Color textColor = GetTextColor(chest);
            Color outlineColor = GetOutlineColor(textColor);

            // Calculate total dimensions
            int width = (int)textBoxSize.X + Padding * 2;
            int height = (int)textBoxSize.Y + Padding * 2;

            // Skip rendering if off-screen
            if (position.X < -width / 2f || position.X > Game1.viewport.Width + width / 2f ||
                position.Y < -height / 2f || position.Y > Game1.viewport.Height + height / 2f)
                return;

            float currentY = position.Y - height / 2f + Padding;
            foreach (string line in lines) {
                Vector2 lineSize = font.MeasureString(line) * FontScale;
                float x = position.X - lineSize.X / 2f;
                Vector2 pos = new Vector2(x, currentY);

                // Draw outline
                DrawTextOutline(spriteBatch, font, pos, line, outlineColor);

                // Draw text
                spriteBatch.DrawString(font, line, pos, textColor, 0f, Vector2.Zero, FontScale, SpriteEffects.None, 1f);

                currentY += font.MeasureString(line).Y * FontScale;
            }
        }

        private Color GetTextColor(Chest chest) {
            return ModEntry.Config.TextColorType switch {
                TextColorType.Fixed => ModEntry.Config.TextColor,
                TextColorType.Inverted => InvertColor(chest.playerChoiceColor.Value),
                TextColorType.FollowBox => chest.playerChoiceColor.Value,
                _ => Color.White,
            };
        }

        private Color GetOutlineColor(Color textColor) {
            return ModEntry.Config.OutlineColorType switch {
                OutlineColorType.Fixed => ModEntry.Config.OutlineColor,
                OutlineColorType.Inverted => InvertColor(textColor),
                _ => InvertColor(textColor),
            };
        }

        private Color InvertColor(Color color) {
            return new Color(255 - color.R, 255 - color.G, 255 - color.B, color.A);
        }

        private void DrawTextOutline(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, string text, Color color) {
            const float offset = 2.5f;
            const float TWO_PI = (float)(2 * Math.PI);
            const float MIN_DELTA_THETA = (float)(Math.PI / 10);
            const float MAX_DELTA_THETA = (float)(Math.PI / 3);
            float deltaTheta = MIN_DELTA_THETA;
            for (float theta = 0; theta <= TWO_PI; theta += deltaTheta) {
                for (float radius = offset; radius >= 0; radius -= 0.25f) {
                    float x = (float)(Math.Cos(theta) * radius);
                    float y = (float)(Math.Sin(theta) * radius);
                    spriteBatch.DrawString(font, text, position + new Vector2(x, y), color, 0f, Vector2.Zero, FontScale, SpriteEffects.None, 1f);
                }
                deltaTheta = 0.5f * deltaTheta + 0.5f * MAX_DELTA_THETA;
            }
        }
    }
}
