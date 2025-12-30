using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewModdingAPI;
using LabelChest.Managers;

namespace LabelChest.UI {
    /// <summary>
    /// Handles the label button UI in chest menus.
    /// </summary>
    public class MenuLabelButton : ClickableComponent {
        private const string ButtonName = "label-chest-button";
        private readonly ITranslationHelper _translations;
        private readonly Action<string> _onLabelSet;

        public MenuLabelButton(ITranslationHelper translations, Action<string> onLabelSet)
            : base(Rectangle.Empty, ButtonName) {
            _translations = translations;
            _onLabelSet = onLabelSet;
            this.myID = 9876543;
        }

        /// <summary>Draws the label button in the chest menu.</summary>
        public void Draw(SpriteBatch b, ItemGrabMenu menu, Chest chest) {
            Texture2D setLabelButton = Game1.content.Load<Texture2D>(ModEntry.MAIN_TEXTURE_PATH);

            // Draw button background
            IClickableMenu.drawTextureBox(
                b,
                setLabelButton,
                new Rectangle(0, 0, 60, 60),
                bounds.X,
                bounds.Y,
                bounds.Width,
                bounds.Height,
                Color.White,
                1f,
                false
            );

            // Determine button text
            string labelText = ChestLabelManager.HasLabel(chest) && !string.IsNullOrWhiteSpace(ChestLabelManager.GetLabel(chest))
                ? ChestLabelManager.GetLabel(chest)
                : _translations.Get("set-label-button");

            // Center text
            Vector2 textSize = Game1.smallFont.MeasureString(labelText);
            Vector2 textPos = new Vector2(
                bounds.X + (bounds.Width - textSize.X) / 2,
                bounds.Y + (bounds.Height - textSize.Y) / 2
            );

            // Draw hover effect or selection indicator
            bool isHover = (
                menu.currentlySnappedComponent == this
                || bounds.Contains(Game1.getMouseX(), Game1.getMouseY())
            );
            float alpha = isHover ? 0.5f : 1f;
            b.DrawString(Game1.smallFont, labelText, textPos, Game1.textColor * alpha);
            if (isHover) {
                IClickableMenu.drawHoverText(b, _translations.Get("set-label-button"), Game1.smallFont);
            }

            // Redraw mouse cursor
            menu.drawMouse(b);
        }

        /// <summary>
        /// Handles click events on the label button for both mouse and controller.
        /// </summary>
        public bool HandleClick(ItemGrabMenu menu, Chest chest, Vector2 cursorPosition) {
            // For mouse: check if cursor is within bounds
            // For controller: check if this component is currently selected
            if (bounds.Contains((int)cursorPosition.X, (int)cursorPosition.Y)
                || menu.currentlySnappedComponent == this) {
                Game1.playSound("drumkit6");
                string currentLabel = ChestLabelManager.GetLabel(chest);
                string title = _translations.Get("set-label-title");

                _onLabelSet?.Invoke(currentLabel);
                return true;
            }

            return false;
        }
    }
}