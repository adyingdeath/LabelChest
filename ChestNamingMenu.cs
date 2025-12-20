using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace LabelChest
{
    public class ChestNamingMenu : IClickableMenu
    {
        private TextBox textBox;
        private ClickableTextureComponent cancelButton;
        private ClickableTextureComponent doneButton;
        private Action<string> onConfirm;
        private string title;

        public ChestNamingMenu(string title, string defaultText, Action<string> onConfirm)
        {
            this.title = title;
            this.onConfirm = onConfirm;

            // Compact menu size
            this.width = 400;
            this.height = 250;

            // Center on screen
            Vector2 pos = Utility.getTopLeftPositionForCenteringOnScreen(this.width, this.height);
            this.xPositionOnScreen = (int)pos.X;
            this.yPositionOnScreen = (int)pos.Y;

            // Setup TextBox (Centered)
            int textBoxWidth = 192; // Standard texture width
            this.textBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
            {
                X = this.xPositionOnScreen + (this.width - textBoxWidth) / 2,
                Y = this.yPositionOnScreen + 85,
                Text = defaultText ?? ""
            };
            this.textBox.SelectMe();

            // Setup OK Button (Centered below textbox)
            int buttonGap = 4;
            this.cancelButton = new ClickableTextureComponent(
                new Rectangle(this.xPositionOnScreen + this.width / 2 - (64 + buttonGap / 2), this.yPositionOnScreen + 160, 64, 64),
                Game1.mouseCursors,
                new Rectangle(192, 256, 64, 64), // CANCEL Icon sprite
                1f);
            this.doneButton = new ClickableTextureComponent(
                new Rectangle(this.xPositionOnScreen + this.width / 2 + buttonGap / 2, this.yPositionOnScreen + 160, 64, 64),
                Game1.mouseCursors,
                new Rectangle(128, 256, 64, 64), // OK Icon sprite
                1f);
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
        }

        public override void receiveKeyPress(Keys key)
        {
            if (key == Keys.Escape)
                Game1.exitActiveMenu();
            else if (key == Keys.Enter)
                Confirm(this.textBox.Text);
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
            Vector2 titleSize = Game1.dialogueFont.MeasureString(this.title);
            Utility.drawTextWithShadow(
                b,
                this.title,
                Game1.dialogueFont,
                new Vector2(this.xPositionOnScreen + (this.width - titleSize.X) / 2, this.yPositionOnScreen + 30),
                Game1.textColor
            );

            // Draw controls
            this.textBox.Draw(b);
            this.cancelButton.draw(b);
            this.doneButton.draw(b);
            this.drawMouse(b);
        }
    }
}