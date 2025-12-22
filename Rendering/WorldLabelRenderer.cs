using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using LabelChest.Managers;

namespace LabelChest.Rendering
{
    /// <summary>
    /// Renders chest labels in the game world.
    /// </summary>
    public class WorldLabelRenderer
    {
        private readonly LabelCacheManager _cacheManager;
        
        // World rendering configuration
        private const int WorldTileSize = 64;

        public WorldLabelRenderer(LabelCacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        /// <summary>
        /// Draws a label above a chest at the specified tile position.
        /// </summary>
        public void DrawLabel(SpriteBatch spriteBatch, Vector2 tileLocation, string text)
        {
            if (!_cacheManager.TryGetTexture(text, out Texture2D? texture) || texture == null || texture.IsDisposed)
            {
                _cacheManager.QueueLabel(text);
                return;
            }

            Vector2 screenPos = Game1.GlobalToLocal(Game1.viewport, tileLocation * WorldTileSize);
            
            // Skip rendering if off-screen
            if (screenPos.X < -64 || screenPos.X > Game1.viewport.Width || 
                screenPos.Y < -64 || screenPos.Y > Game1.viewport.Height)
                return;

            // Center above chest
            float startX = screenPos.X + (WorldTileSize / 2f) - (texture.Width / 2f);
            float startY = screenPos.Y - (texture.Height / 2f);

            spriteBatch.Draw(
                texture, 
                new Vector2(startX, startY), 
                null, 
                Color.White, 
                0f, 
                Vector2.Zero,
                1.0f, 
                SpriteEffects.None, 
                1f
            );
        }
    }
}