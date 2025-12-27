using LabelChest.UI;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley.Menus;

namespace LabelChest.Managers {
    /// <summary>
    /// Manages chest menu button bounds.
    /// </summary>
    public class ChestMenuButtonManager {
        private const int ButtonGroupHeight = 48;
        private const int ButtonGroupMinButtonWidth = 150;
        private MenuLabelButton menuLabelButton;
        private ConfigButton configButton;

        public ChestMenuButtonManager(MenuLabelButton menuLabelButton, ConfigButton configButton) {
            this.menuLabelButton = menuLabelButton;
            this.configButton = configButton;
        }

        public (Rectangle, Rectangle) GetButtonGroupBounds(ItemGrabMenu menu) {
            // First calculate the whole group
            int totalWidth = Math.Max(ButtonGroupMinButtonWidth, menu.width / 3);
            int x = menu.xPositionOnScreen + (menu.width - totalWidth) / 2;
            int y = menu.ItemsToGrabMenu.yPositionOnScreen - ButtonGroupHeight - (
                menu.ItemsToGrabMenu.capacity switch {
                    36 => 24,
                    70 => 8,
                    _ => 24,
                }
            );
            
            /* Android specific: adjust the buttons' positions so that players 
            can see and click buttons*/
            if (Constants.TargetPlatform == GamePlatform.Android) {
                y = 0;
            }

            return (
                new Rectangle(x, y, totalWidth - ButtonGroupHeight, ButtonGroupHeight),
                new Rectangle(x + totalWidth - ButtonGroupHeight, y, ButtonGroupHeight, ButtonGroupHeight)
            );
        }

        /// <summary>
        /// Sets this button's own up and down neighbors.
        /// </summary>
        public void SetupNeighbors(ItemGrabMenu menu) {
            ClickableComponent? closestAbove = null;
            ClickableComponent? closestBelow = null;

            foreach (var component in menu.allClickableComponents) {
                if (component == menuLabelButton || component == configButton) continue;

                // Check for up neighbor (component above the button)
                if (component.bounds.Bottom <= menuLabelButton.bounds.Top) {
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
                if (component.bounds.Top >= menuLabelButton.bounds.Bottom) {
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
                menuLabelButton.upNeighborID = closestAbove.myID;
                configButton.upNeighborID = closestAbove.myID;
            }

            if (closestBelow != null) {
                menuLabelButton.downNeighborID = closestBelow.myID;
                configButton.downNeighborID = closestBelow.myID;
            }

            // Set buttons in our group to neighbour each others
            menuLabelButton.rightNeighborID = configButton.myID;
            configButton.leftNeighborID = menuLabelButton.myID;
        }

        /// <summary>
        /// Updates up/down neighbors for other components to include this button.
        /// </summary>
        public void UpdateOtherComponentsNeighbors(ItemGrabMenu menu) {
            /* [TODO]: For slots in the first row of the inventory, the 
            upNeighbor is currently set to menuLabelButton. This is incorrect — 
            it should point to the slot directly above it in the chest instead. 
            */
            foreach (var component in menu.allClickableComponents) {
                if (component == menuLabelButton || component == configButton) continue;

                // Rule 1: If component is below the button and its up neighbor is above the button
                if (component.bounds.Bottom >= menuLabelButton.bounds.Top) {
                    var currentUpNeighbor = FindComponentByID(menu, component.upNeighborID);
                    if (currentUpNeighbor == null || currentUpNeighbor.bounds.Top <= menuLabelButton.bounds.Bottom) {
                        component.upNeighborID = menuLabelButton.myID;
                    }
                }

                // Rule 2: If component is above the button and its down neighbor is below the button
                if (component.bounds.Top <= menuLabelButton.bounds.Bottom) {
                    var currentDownNeighbor = FindComponentByID(menu, component.downNeighborID);
                    if (currentDownNeighbor == null || currentDownNeighbor.bounds.Bottom >= menuLabelButton.bounds.Top) {
                        component.downNeighborID = menuLabelButton.myID;
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
    }
}