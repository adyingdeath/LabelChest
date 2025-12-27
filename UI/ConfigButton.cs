using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewModdingAPI;

namespace LabelChest.UI {
    /// <summary>
    /// Handles the config button UI in chest menus.
    /// </summary>
    public class ConfigButton : ClickableComponent {
        private const string ButtonName = "config-button";
        private readonly ITranslationHelper _translations;
        private readonly Action _onConfigClicked;

        public ConfigButton(ITranslationHelper translations, Action onConfigClicked)
            : base(Rectangle.Empty, ButtonName) {
            _translations = translations;
            _onConfigClicked = onConfigClicked;
            this.myID = 99910525; // Different ID from MenuLabelButton
        }

        /// <summary>Draws the config button in the chest menu.</summary>
        public void Draw(SpriteBatch b, ItemGrabMenu menu) {
            Texture2D texture = Game1.content.Load<Texture2D>(ModEntry.MAIN_TEXTURE_PATH);

            // Draw hover effect
            bool isHover = (
                menu.currentlySnappedComponent == this
                || bounds.Contains(Game1.getMouseX(), Game1.getMouseY())
            );

            // Draw config button icon
            b.Draw(
                texture,
                new Vector2(bounds.X, bounds.Y),
                new Rectangle(64, 0, 60, 60),
                Color.White,
                0f,
                Vector2.Zero,
                bounds.Height / 60.0f,
                SpriteEffects.None,
                1.0f
            );

            // Draw gear icon overlay
            b.Draw(
                texture,
                new Vector2(bounds.X, bounds.Y),
                new Rectangle(128, 0, 60, 60),
                Color.White * (isHover ? 0.8f : 1.0f),
                0f,
                Vector2.Zero,
                bounds.Height / 60.0f,
                SpriteEffects.None,
                1.0f
            );
            
            if (isHover) {
                // Tooltip
                IClickableMenu.drawHoverText(b, "Config", Game1.smallFont);
            }

            // Redraw mouse cursor
            menu.drawMouse(b);
        }

        /// <summary>
        /// Handles click events on the config button.
        /// </summary>
        public bool HandleClick(ItemGrabMenu menu, Vector2 cursorPosition) {
            if (bounds.Contains((int)cursorPosition.X, (int)cursorPosition.Y)
                || menu.currentlySnappedComponent == this) {
                Game1.playSound("drumkit6");
                _onConfigClicked?.Invoke();
                return true;
            }

            return false;
        }
    }
}