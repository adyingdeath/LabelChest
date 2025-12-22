using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewModdingAPI;
using LabelChest.Managers;
using Force.DeepCloner;

namespace LabelChest.UI
{
    /// <summary>
    /// Handles the label button UI in chest menus.
    /// </summary>
    public class MenuLabelButton
    {
        // UI Configuration
        private const int ButtonHeight = 48;
        private const int MinButtonWidth = 150;
        
        private readonly ITranslationHelper _translations;
        private readonly Action<string> _onLabelSet;

        public MenuLabelButton(ITranslationHelper translations, Action<string> onLabelSet)
        {
            _translations = translations;
            _onLabelSet = onLabelSet;
        }

        /// <summary>Gets the bounds of the label button for a given menu.</summary>
        public static Rectangle GetButtonBounds(ItemGrabMenu menu)
        {
            int width = Math.Max(MinButtonWidth, menu.width / 3);
            int x = menu.xPositionOnScreen + (menu.width - width) / 2;
            
            // Position above the menu content
            int y = menu.ItemsToGrabMenu.yPositionOnScreen - ButtonHeight - (
                menu.ItemsToGrabMenu.capacity switch
                {
                    36 => 24,
                    70 => 8,
                    _ => 24,
                }
            );

            return new Rectangle(x, y, width, ButtonHeight);
        }

        /// <summary>Draws the label button in the chest menu.</summary>
        public void Draw(SpriteBatch b, ItemGrabMenu menu, Chest chest)
        {
            Rectangle buttonRect = GetButtonBounds(menu);

            // Draw button background
            IClickableMenu.drawTextureBox(
                b,
                Game1.menuTexture,
                new Rectangle(0, 256, 60, 60),
                buttonRect.X,
                buttonRect.Y,
                buttonRect.Width,
                buttonRect.Height,
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
                buttonRect.X + (buttonRect.Width - textSize.X) / 2,
                buttonRect.Y + (buttonRect.Height - textSize.Y) / 2
            );

            // Draw hover effect
            bool isMouseHover = buttonRect.Contains(Game1.getMouseX(), Game1.getMouseY());
            float alpha = isMouseHover ? 0.5f : 1f;
            b.DrawString(Game1.smallFont, labelText, textPos, Game1.textColor * alpha);

            // Redraw mouse cursor
            menu.drawMouse(b);
        }

        /// <summary>
        /// Handles click events on the label button.
        /// </summary>
        public bool HandleClick(ItemGrabMenu menu, Chest chest, Vector2 cursorPosition)
        {
            Rectangle buttonRect = GetButtonBounds(menu);
            
            if (buttonRect.Contains((int)cursorPosition.X, (int)cursorPosition.Y))
            {
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