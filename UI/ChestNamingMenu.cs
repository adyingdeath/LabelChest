using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace LabelChest.UI {
    public record ChestNamingMenuTranslation(
        string SetLabelTitle = "Set Chest Label",
        string CancelButton = "Cancel",
        string OkButton = "OK"
    );

    public class ChestNamingMenu : IClickableMenu {
        private const int TEXTBOX_WIDTH = 300;
        private const int BUTTON_GAP = 4;
        private const int BUTTON_SIZE = 64;
        private const int TEXTBOX_HEIGHT = 48;

        public TextBox textBox;
        public ClickableComponent textBoxCC;
        public ClickableTextureComponent cancelButton;
        public ClickableTextureComponent doneButton;
        private Action<string> onConfirm;
        private ChestNamingMenuTranslation translations;

        // Component IDs for navigation
        private const int TEXTBOX_CC_ID = 104;
        private const int DONE_BUTTON_ID = 102;
        private const int CANCEL_BUTTON_ID = 103;

        private readonly string originalText;

        public ChestNamingMenu(
            ChestNamingMenuTranslation translations,
            string originalText,
            Action<string> onConfirm
        ) {
            this.translations = translations;
            this.originalText = originalText;
            this.onConfirm = onConfirm;

            // Calculate window size based on content
            string title = translations.SetLabelTitle;
            Vector2 titleSize = Game1.dialogueFont.MeasureString(title);

            width = Math.Max(400, (int)titleSize.X + 100);
            width = Math.Max(width, TEXTBOX_WIDTH + 100);

            height = 250;

            // Center on screen
            Vector2 pos = Utility.getTopLeftPositionForCenteringOnScreen(width, height);
            xPositionOnScreen = (int)pos.X;
            yPositionOnScreen = (int)pos.Y;

            // Setup TextBox (Centered and wider)
            int textBoxX = xPositionOnScreen + (width - TEXTBOX_WIDTH) / 2;
            int textBoxY = yPositionOnScreen + 85;

            textBox = new TextBox(
                Game1.content.Load<Texture2D>("LooseSprites\\textBox"),
                null, Game1.smallFont, Game1.textColor
            ) {
                X = textBoxX,
                Y = textBoxY,
                Width = TEXTBOX_WIDTH,
                Height = TEXTBOX_HEIGHT,
                Text = originalText ?? ""
            };

            // Create clickable component for textbox for navigation
            textBoxCC = new ClickableComponent(
                new Rectangle(textBoxX, textBoxY, (int)(TEXTBOX_WIDTH * 0.6), (int)(TEXTBOX_HEIGHT * 0.6)), "") {
                myID = TEXTBOX_CC_ID,
                upNeighborID = -1,
                downNeighborID = DONE_BUTTON_ID,
                leftNeighborID = -1,
                rightNeighborID = -1,
            };

            // Setup Buttons (Centered below textbox)
            int totalButtonsWidth = (BUTTON_SIZE * 2) + BUTTON_GAP;
            int buttonsStartX = xPositionOnScreen + (width - totalButtonsWidth) / 2;
            int buttonsY = yPositionOnScreen + 160;

            // Cancel Button (left)
            cancelButton = new ClickableTextureComponent(
                new Rectangle(buttonsStartX, buttonsY, BUTTON_SIZE, BUTTON_SIZE),
                Game1.mouseCursors,
                new Rectangle(192, 256, 64, 64), // CANCEL Icon sprite
                1f) {
                myID = CANCEL_BUTTON_ID,
                upNeighborID = TEXTBOX_CC_ID,
                downNeighborID = -1,
                leftNeighborID = -1,
                rightNeighborID = DONE_BUTTON_ID,
            };

            // Done Button (right)
            doneButton = new ClickableTextureComponent(
                new Rectangle(buttonsStartX + BUTTON_SIZE + BUTTON_GAP, buttonsY, BUTTON_SIZE, BUTTON_SIZE),
                Game1.mouseCursors,
                new Rectangle(128, 256, 64, 64), // OK Icon sprite
                1f) {
                myID = DONE_BUTTON_ID,
                upNeighborID = TEXTBOX_CC_ID,
                downNeighborID = -1,
                leftNeighborID = CANCEL_BUTTON_ID,
                rightNeighborID = -1,
            };

            Game1.keyboardDispatcher.Subscriber = textBox;
            //textBox.Selected = false;

            // Enable controller navigation
            if (Game1.options.SnappyMenus) {
                populateClickableComponentList();
                snapToDefaultClickableComponent();
            }
        }

        public override void snapToDefaultClickableComponent() {
            currentlySnappedComponent = getComponentWithID(DONE_BUTTON_ID);
            if (currentlySnappedComponent != null) {
                snapCursorToCurrentSnappedComponent();
            }
        }

        public override void receiveGamePadButton(Buttons b) {
            base.receiveGamePadButton(b);

            SButton button = b.ToSButton();

            if (textBox.Selected && (
                button.Equals(SButton.DPadUp)
                || button.Equals(SButton.DPadDown)
                || button.Equals(SButton.DPadLeft)
                || button.Equals(SButton.DPadRight)
                || button.Equals(SButton.LeftThumbstickUp)
                || button.Equals(SButton.LeftThumbstickDown)
                || button.Equals(SButton.LeftThumbstickLeft)
                || button.Equals(SButton.LeftThumbstickRight)
                )
            ) {
                textBox.Selected = false;
            }

            if (button.Equals(SButton.ControllerB)) {
                // Use original text for Esc
                Confirm(originalText);
            }
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true) {
            base.receiveLeftClick(x, y, playSound);
            textBox.Update();

            if (cancelButton.containsPoint(x, y)) {
                Confirm("");
            }
            if (doneButton.containsPoint(x, y)) {
                Confirm(textBox.Text);
            }
        }

        public override void receiveKeyPress(Keys key) {
            if (key == Keys.Escape) {
                // Use original text for Esc
                Confirm(originalText);
            } else if (key == Keys.Enter) {
                Confirm(textBox.Text);
            } else if (!textBox.Selected && !Game1.options.doesInputListContain(Game1.options.menuButton, key)) {
                base.receiveKeyPress(key);
            }
        }

        private void Confirm(string text) {
            Game1.playSound("smallSelect");
            onConfirm?.Invoke(text);
            Game1.exitActiveMenu();
        }

        public override void performHoverAction(int x, int y) {
            base.performHoverAction(x, y);

            // Scale button on hover
            cancelButton.scale = cancelButton.containsPoint(x, y) ? 1.1f : 1.0f;
            doneButton.scale = doneButton.containsPoint(x, y) ? 1.1f : 1.0f;
        }

        public override void draw(SpriteBatch b) {
            // Draw background dim
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);

            // Draw menu box
            IClickableMenu.drawTextureBox(
                b,
                Game1.mouseCursors,
                new Rectangle(384, 373, 18, 18),
                xPositionOnScreen,
                yPositionOnScreen,
                width,
                height,
                Color.White,
                4f
            );

            // Draw Title (Centered)
            string title = translations.SetLabelTitle;
            Vector2 titleSize = Game1.dialogueFont.MeasureString(title);
            Utility.drawTextWithShadow(
                b,
                title,
                Game1.dialogueFont,
                new Vector2(xPositionOnScreen + (width - titleSize.X) / 2, yPositionOnScreen + 30),
                Game1.textColor
            );

            // Draw controls
            textBox.Draw(b);
            cancelButton.draw(b);
            doneButton.draw(b);

            // Draw button tooltips on hover
            if (cancelButton.containsPoint(Game1.getMouseX(), Game1.getMouseY())) {
                drawHoverText(b, translations.CancelButton, Game1.smallFont);
            } else if (doneButton.containsPoint(Game1.getMouseX(), Game1.getMouseY())) {
                drawHoverText(b, translations.OkButton, Game1.smallFont);
            }

            drawMouse(b);
        }
    }
}