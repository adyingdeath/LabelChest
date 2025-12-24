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
        // UI Configuration
        private const int ButtonHeight = 48;
        private const int MinButtonWidth = 150;

        private readonly ITranslationHelper _translations;
        private readonly Action<string> _onLabelSet;

        public MenuLabelButton(ITranslationHelper translations, Action<string> onLabelSet)
            : base(Rectangle.Empty, ButtonName) {
            _translations = translations;
            _onLabelSet = onLabelSet;
            this.myID = 99910524;
        }

        /// <summary>
        /// Updates the button bounds for a given menu.
        /// </summary>
        public void UpdateBounds(ItemGrabMenu menu) {
            int width = Math.Max(MinButtonWidth, menu.width / 3);
            int x = menu.xPositionOnScreen + (menu.width - width) / 2;

            // Position above the menu content
            int y = menu.ItemsToGrabMenu.yPositionOnScreen - ButtonHeight - (
                menu.ItemsToGrabMenu.capacity switch {
                    36 => 24,
                    70 => 8,
                    _ => 24,
                }
            );

            this.bounds = new Rectangle(x, y, width, ButtonHeight);
        }

        /// <summary>
        /// Sets up neighbors for this button and other components in the menu.
        /// </summary>
        public void SetupNeighbors(ItemGrabMenu menu) {
            UpdateBounds(menu);

            // Clear existing neighbors to avoid conflicts
            this.upNeighborID = -1;
            this.downNeighborID = -1;
            this.leftNeighborID = -1;
            this.rightNeighborID = -1;

            // First, set this button's neighbors
            SetButtonNeighbors(menu);

            // Then update neighbors for other components
            UpdateOtherComponentsNeighbors(menu);

        }

        /// <summary>
        /// Sets this button's own up and down neighbors.
        /// </summary>
        private void SetButtonNeighbors(ItemGrabMenu menu) {
            ClickableComponent? closestAbove = null;
            ClickableComponent? closestBelow = null;

            foreach (var component in menu.allClickableComponents) {
                if (component == this) continue;

                // Check for up neighbor (component above the button)
                if (component.bounds.Bottom <= this.bounds.Top) {
                    if (closestAbove == null
                        || component.bounds.Bottom > closestAbove.bounds.Bottom
                        || (
                            component.bounds.Bottom == closestAbove.bounds.Bottom
                            && component.bounds.Left < closestAbove.bounds.Left
                            )
                        ) {
                        closestAbove = component;
                    }
                }

                // Check for down neighbor (component below the button)
                if (component.bounds.Top >= this.bounds.Bottom) {
                    if (closestBelow == null
                        || component.bounds.Top < closestBelow.bounds.Top
                        || (
                            component.bounds.Top == closestBelow.bounds.Top
                            && component.bounds.Left < closestBelow.bounds.Left
                            )
                        ) {
                        closestBelow = component;
                    }
                }
            }

            if (closestAbove != null) {
                this.upNeighborID = closestAbove.myID;
            }

            if (closestBelow != null) {
                this.downNeighborID = closestBelow.myID;
            }
        }

        /// <summary>
        /// Updates up/down neighbors for other components to include this button.
        /// </summary>
        private void UpdateOtherComponentsNeighbors(ItemGrabMenu menu) {
            foreach (var component in menu.allClickableComponents) {
                if (component == this) continue;

                // Rule 1: If component is below the button and its up neighbor is above the button
                if (component.bounds.Bottom >= this.bounds.Top) {
                    var currentUpNeighbor = FindComponentByID(menu, component.upNeighborID);
                    if (currentUpNeighbor == null || currentUpNeighbor.bounds.Top <= this.bounds.Bottom) {
                        component.upNeighborID = this.myID;
                    }
                }

                // Rule 2: If component is above the button and its down neighbor is below the button
                if (component.bounds.Top <= this.bounds.Bottom) {
                    var currentDownNeighbor = FindComponentByID(menu, component.downNeighborID);
                    if (currentDownNeighbor == null || currentDownNeighbor.bounds.Bottom >= this.bounds.Top) {
                        component.downNeighborID = this.myID;
                    }
                }
            }
        }

        /// <summary>
        /// Finds a component by its ID in the menu.
        /// </summary>
        private static ClickableComponent? FindComponentByID(ItemGrabMenu menu, int id) {
            if (id == -1) return null;

            return menu.allClickableComponents.Find((comp) => comp.myID == id);
        }

        /// <summary>Draws the label button in the chest menu.</summary>
        public void Draw(SpriteBatch b, ItemGrabMenu menu, Chest chest) {
            // Draw button background
            IClickableMenu.drawTextureBox(
                b,
                Game1.menuTexture,
                new Rectangle(0, 256, 60, 60),
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