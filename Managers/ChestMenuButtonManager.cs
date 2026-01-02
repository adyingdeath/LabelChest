using LabelChest.UI;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
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
        public void SetupNeighbors() {
            // Fixed up/down neighbors
            menuLabelButton.upNeighborID = 4343;
            menuLabelButton.downNeighborID = 53910;
            configButton.upNeighborID = 4343;
            configButton.downNeighborID = 53910;

            // Set buttons in our group to neighbour each others
            menuLabelButton.rightNeighborID = configButton.myID;
            configButton.leftNeighborID = menuLabelButton.myID;
        }

        /// <summary>
        /// Updates up/down neighbors for other components to include this button.
        /// </summary>
        public void UpdateOtherComponentsNeighbors(ItemGrabMenu menu) {
            /* [TODO]: The chest menu in Android is different from the one in PC,
            so slots in the first row of chest have wrong neighbors calculation.
            */
            /* Different logic for different chests:
             * * Chest
             * * Stone Chest
             * * Big Chest
             * * Big Stone Chest
             * * Junimo Chest
             */
            int upperIndex = menu.ItemsToGrabMenu.capacity switch {
                9 => 53912, // Junimo Chest
                36 => 53921, // Chest, Stone Chest
                70 => 53923, // Big Chest, Big Stone Chest
                _ => 53912
            };
            menu.allClickableComponents.ForEach((component) => {
                if (53910 <= component.myID && component.myID <= upperIndex) {
                    component.upNeighborID = menuLabelButton.myID;
                } else if (4343 <= component.myID && component.myID <= 4363) {
                    // If Android, skip color palette.
                    if (Constants.TargetPlatform == GamePlatform.Android) return;
                    component.downNeighborID = menuLabelButton.myID;
                }
            });
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