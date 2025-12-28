using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using LabelChest.Managers;

namespace LabelChest.Rendering {
    /// <summary>
    /// Renders chest labels in the game world.
    /// </summary>
    public class WorldLabelRenderer {
        private readonly LabelCacheManager _cacheManager;

        public WorldLabelRenderer(LabelCacheManager cacheManager) {
            _cacheManager = cacheManager;
        }

        /// <summary>
        /// Draws a label at the specified absolute screen position (center of the text texture).
        /// </summary>
        public void DrawLabel(SpriteBatch spriteBatch, Vector2 position, string text) {
            if (!_cacheManager.TryGetTexture(text, out Texture2D? texture) || texture == null || texture.IsDisposed) {
                _cacheManager.QueueLabel(text);
                return;
            }

            // Skip rendering if off-screen
            if (position.X < -texture.Width / 2f || position.X > Game1.viewport.Width + texture.Width / 2f ||
                position.Y < -texture.Height / 2f || position.Y > Game1.viewport.Height + texture.Height / 2f)
                return;

            spriteBatch.Draw(
                texture,
                position,
                null,
                Color.White,
                0f,
                new Vector2(texture.Width / 2f, texture.Height / 2f),
                1.0f,
                SpriteEffects.None,
                1f
            );
        }

        /// <summary>
        /// Draws a label above a chest at the specified tile position.
        /// </summary>
        public void DrawLabelWithTile(SpriteBatch spriteBatch, Vector2 tileLocation, string text) {
            Vector2 screenPos = Game1.GlobalToLocal(Game1.viewport, tileLocation * Game1.tileSize);
            Vector2 centerPos = new Vector2(screenPos.X + Game1.tileSize / 2f, screenPos.Y);
            DrawLabel(spriteBatch, centerPos, text);
        }
    }
}
