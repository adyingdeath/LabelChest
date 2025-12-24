using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace LabelChest.Managers {
    /// <summary>
    /// Manages caching of label textures for efficient rendering.
    /// </summary>
    public class LabelCacheManager : IDisposable {
        private readonly Dictionary<string, Texture2D> _labelCache = new();
        private readonly HashSet<string> _pendingLabels = new();
        private SpriteBatch? _spriteBatch;

        // Configuration constants
        private float WorldFontScale = 0.8f;
        private const int WorldMaxWidth = 70;
        private const int Padding = 4;

        public LabelCacheManager(LocalizedContentManager.LanguageCode languageCode) {
            WorldFontScale = languageCode switch {
                LocalizedContentManager.LanguageCode.en => 0.65f,
                _ => 0.85f
            };
        }

        /// <summary>
        /// Adds a label to be processed in the next update tick.
        /// </summary>
        public void QueueLabel(string text) {
            if (!string.IsNullOrWhiteSpace(text)) {
                _pendingLabels.Add(text);
            }
        }

        /// <summary>
        /// Checks if a label texture exists and is valid.
        /// </summary>
        public bool TryGetTexture(string text, out Texture2D? texture) {
            if (_labelCache.TryGetValue(text, out texture) && !texture.IsDisposed)
                return true;

            texture = null;
            return false;
        }

        /// <summary>
        /// Processes pending labels and generates their textures.
        /// </summary>
        public void ProcessPendingLabels(GraphicsDevice device) {
            if (_pendingLabels.Count == 0 || device == null || device.IsDisposed)
                return;

            _spriteBatch ??= new SpriteBatch(device);
            var previousRenderTargets = device.GetRenderTargets();

            var tasks = new List<string>(_pendingLabels);
            _pendingLabels.Clear();

            foreach (string text in tasks) {
                if (TryGetTexture(text, out _) && !_labelCache[text].IsDisposed)
                    continue;

                GenerateLabelTexture(device, text);
            }

            device.SetRenderTargets(previousRenderTargets);
        }

        /// <summary>
        /// Generates a texture for a label text with proper formatting.
        /// </summary>
        private void GenerateLabelTexture(GraphicsDevice device, string text) {
            SpriteFont font = Game1.smallFont;
            List<string> lines = TextUtils.WrapText(
                font, text, WorldMaxWidth / WorldFontScale,
                out Vector2 textBoxSize
            );

            if (lines.Count == 0) return;

            int width = (int)textBoxSize.X + Padding * 2;
            int height = (int)textBoxSize.Y + Padding * 2;

            // Create and draw to render target
            RenderTarget2D target = new RenderTarget2D(
                device,
                width,
                height,
                false,
                SurfaceFormat.Color,
                DepthFormat.None,
                0,
                RenderTargetUsage.PreserveContents
            );

            device.SetRenderTarget(target);
            device.Clear(Color.Transparent);

            _spriteBatch!.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);

            float currentY = Padding;
            foreach (string line in lines) {
                Vector2 lineSize = font.MeasureString(line) * WorldFontScale;
                float x = (width - lineSize.X) / 2f;
                Vector2 pos = new Vector2(x, currentY);

                // Draw outline
                DrawTextOutline(font, line, pos);

                // Draw text
                _spriteBatch.DrawString(font, line, pos, Color.White, 0f, Vector2.Zero, WorldFontScale, SpriteEffects.None, 1f);

                currentY += font.MeasureString(line).Y * WorldFontScale;
            }

            _spriteBatch.End();
            _labelCache[text] = target;
        }

        private void DrawTextOutline(SpriteFont font, string text, Vector2 position) {
            const float offset = 2.5f;
            const float TWO_PI = (float)(2 * Math.PI);
            const float TENTH_PI = (float)(Math.PI / 10);
            for (float theta = 0; theta <= TWO_PI; theta += TENTH_PI) {
                for (float radius = offset; radius >= 0; radius -= 0.25f) {
                    float x = (float)(Math.Cos(theta) * radius);
                    float y = (float)(Math.Sin(theta) * radius);
                    _spriteBatch!.DrawString(font, text, position + new Vector2(x, y), Color.Black, 0f, Vector2.Zero, WorldFontScale, SpriteEffects.None, 1f);
                }
            }
        }

        /// <summary>
        /// Cleans up all cached textures.
        /// </summary>
        public void ClearCache() {
            foreach (var texture in _labelCache.Values) {
                if (texture != null && !texture.IsDisposed)
                    texture.Dispose();
            }

            _labelCache.Clear();
            _pendingLabels.Clear();
        }

        public void Dispose() {
            ClearCache();
            _spriteBatch?.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}