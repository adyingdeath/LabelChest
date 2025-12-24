using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace LabelChest.UI
{
    public record ChestNamingMenuTranslation(
        string SetLabelTitle = "Set Chest Label",
        string CancelButton = "Cancel",
        string OkButton = "OK"
    );

    public class ChestNamingMenu : IClickableMenu
    {
        private const int TEXTBOX_WIDTH = 300;
        private const int BUTTON_GAP = 4;
        private const int BUTTON_SIZE = 64;
        private const int TEXTBOX_HEIGHT = 48;
        
        private TextBox textBox;
        private ClickableTextureComponent cancelButton;
        private ClickableTextureComponent doneButton;
        private Action<string> onConfirm;
        private ChestNamingMenuTranslation translations;
        
        // Component IDs for navigation
        private const int TEXTBOX_ID = 104;
        private const int DONE_BUTTON_ID = 102;
        private const int CANCEL_BUTTON_ID = 103;

        private string originalText;

        public ChestNamingMenu(ChestNamingMenuTranslation translations, string defaultText, Action<string> onConfirm)
        {
            this.translations = translations;
            this.originalText = defaultText;
            this.onConfirm = onConfirm;

            // Calculate window size based on content
            string title = translations.SetLabelTitle;
            Vector2 titleSize = Game1.dialogueFont.MeasureString(title);
            
            this.width = Math.Max(400, (int)titleSize.X + 100);
            this.width = Math.Max(this.width, TEXTBOX_WIDTH + 100);
            
            this.height = 250;

            // Center on screen
            Vector2 pos = Utility.getTopLeftPositionForCenteringOnScreen(this.width, this.height);
            this.xPositionOnScreen = (int)pos.X;
            this.yPositionOnScreen = (int)pos.Y;

            // Setup TextBox (Centered and wider)
            int textBoxX = this.xPositionOnScreen + (this.width - TEXTBOX_WIDTH) / 2;
            int textBoxY = this.yPositionOnScreen + 85;
            
            this.textBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
            {
                X = textBoxX,
                Y = textBoxY,
                Width = TEXTBOX_WIDTH,
                Height = TEXTBOX_HEIGHT,
                Text = defaultText ?? ""
            };
            
            // Create clickable component for textbox for navigation
            ClickableComponent textBoxCC = new ClickableComponent(
                new Rectangle(textBoxX, textBoxY, TEXTBOX_WIDTH, TEXTBOX_HEIGHT), "")
            {
                myID = TEXTBOX_ID,
                upNeighborID = -1,
                downNeighborID = DONE_BUTTON_ID,
                leftNeighborID = CANCEL_BUTTON_ID,
                rightNeighborID = DONE_BUTTON_ID,
                fullyImmutable = true, // Use fullyImmutable = true to disable auto neighbour changes.
            };

            // Setup Buttons (Centered below textbox)
            int totalButtonsWidth = (BUTTON_SIZE * 2) + BUTTON_GAP;
            int buttonsStartX = this.xPositionOnScreen + (this.width - totalButtonsWidth) / 2;
            int buttonsY = this.yPositionOnScreen + 160;
            
            // Cancel Button (left)
            this.cancelButton = new ClickableTextureComponent(
                new Rectangle(buttonsStartX, buttonsY, BUTTON_SIZE, BUTTON_SIZE),
                Game1.mouseCursors,
                new Rectangle(192, 256, 64, 64), // CANCEL Icon sprite
                1f)
            {
                myID = CANCEL_BUTTON_ID,
                upNeighborID = TEXTBOX_ID,
                downNeighborID = -1,
                leftNeighborID = -1,
                rightNeighborID = DONE_BUTTON_ID,
                fullyImmutable = true,
            };

            // Done Button (right)
            this.doneButton = new ClickableTextureComponent(
                new Rectangle(buttonsStartX + BUTTON_SIZE + BUTTON_GAP, buttonsY, BUTTON_SIZE, BUTTON_SIZE),
                Game1.mouseCursors,
                new Rectangle(128, 256, 64, 64), // OK Icon sprite
                1f)
            {
                myID = DONE_BUTTON_ID,
                upNeighborID = TEXTBOX_ID,
                downNeighborID = -1,
                leftNeighborID = CANCEL_BUTTON_ID,
                rightNeighborID = -1,
                fullyImmutable = true,
            };

            // Add components to menu
            this.allClickableComponents = new List<ClickableComponent>
            {
                textBoxCC,
                this.cancelButton,
                this.doneButton
            };

            this.textBox.SelectMe();
            
            // Enable controller navigation
            if (Game1.options.SnappyMenus)
            {
                this.snapToDefaultClickableComponent();
            }
        }

        public override void snapToDefaultClickableComponent()
        {
            this.currentlySnappedComponent = this.getComponentWithID(TEXTBOX_ID);
            if (this.currentlySnappedComponent != null)
            {
                this.snapCursorToCurrentSnappedComponent();
            }
        }

        public override void receiveGamePadButton(Buttons b)
        {
            base.receiveGamePadButton(b);
            
            if (this.textBox.Selected && (
                b == Buttons.DPadUp
                || b == Buttons.DPadDown
                || b == Buttons.DPadLeft
                || b == Buttons.DPadRight
                || b == Buttons.LeftThumbstickLeft
                || b == Buttons.LeftThumbstickUp
                || b == Buttons.LeftThumbstickDown
                || b == Buttons.LeftThumbstickRight
                )
            )
            {
                this.textBox.Selected = false;
            }
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
            this.textBox.Update();

            if (this.cancelButton.containsPoint(x, y))
            {
                Confirm("");
            }
            if (this.doneButton.containsPoint(x, y))
            {
                Confirm(this.textBox.Text);
            }
            
            // Handle textbox click
            Rectangle textBoxBounds = new Rectangle(this.textBox.X, this.textBox.Y, this.textBox.Width, this.textBox.Height);
            if (textBoxBounds.Contains(x, y))
            {
                this.textBox.Selected = true;
            }
            else
            {
                this.textBox.Selected = false;
            }
        }

        public override void receiveKeyPress(Keys key)
        {
            if (key == Keys.Escape)
            {
                // Use original text for Esc
                Confirm(originalText);
            }
            else if (key == Keys.Enter)
            {
                Confirm(this.textBox.Text);
            }
            else if (!this.textBox.Selected && !Game1.options.doesInputListContain(Game1.options.menuButton, key))
            {
                base.receiveKeyPress(key);
            }
        }

        private void Confirm(string text)
        {
            Game1.playSound("smallSelect");
            onConfirm?.Invoke(text);
            Game1.exitActiveMenu();
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            
            // Scale button on hover
            this.cancelButton.scale = this.cancelButton.containsPoint(x, y) ? 1.1f : 1.0f;
            this.doneButton.scale = this.doneButton.containsPoint(x, y) ? 1.1f : 1.0f;
        }

        public override void draw(SpriteBatch b)
        {
            // Draw background dim
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);

            // Draw menu box
            IClickableMenu.drawTextureBox(
                b,
                Game1.mouseCursors,
                new Rectangle(384, 373, 18, 18),
                this.xPositionOnScreen,
                this.yPositionOnScreen,
                this.width,
                this.height,
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
                new Vector2(this.xPositionOnScreen + (this.width - titleSize.X) / 2, this.yPositionOnScreen + 30),
                Game1.textColor
            );

            // Draw controls
            this.textBox.Draw(b);
            this.cancelButton.draw(b);
            this.doneButton.draw(b);
            
            // Draw button tooltips on hover
            if (this.cancelButton.containsPoint(Game1.getMouseX(), Game1.getMouseY()))
            {
                drawHoverText(b, translations.CancelButton, Game1.smallFont);
            }
            else if (this.doneButton.containsPoint(Game1.getMouseX(), Game1.getMouseY()))
            {
                drawHoverText(b, translations.OkButton, Game1.smallFont);
            }
            
            this.drawMouse(b);
        }
    }
}