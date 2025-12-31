using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace LabelChest.UI.Menus;

public class LComponent : IScreenReadable {
    public enum Style {
        Default,
        OptionLabel
    }

    public const int defaultX = 8;

    public const int defaultY = 4;

    public const int defaultPixelWidth = 9;

    public Rectangle bounds;

    public string label;

    public bool greyedOut;

    public Vector2 labelOffset = Vector2.Zero;

    public Style style;

    public string ScreenReaderText { get; set; }

    public string ScreenReaderDescription { get; set; }

    public bool ScreenReaderIgnore { get; set; }

    public LComponent(string label) {
        this.label = label;
        bounds = new Rectangle(32, 16, 36, 36);
    }

    public LComponent(string label, int x, int y, int width, int height) {
        if (x == -1) {
            x = 32;
        }

        if (y == -1) {
            y = 16;
        }

        bounds = new Rectangle(x, y, width, height);
        this.label = label;
    }

    public LComponent(string label, Rectangle bounds) {
        this.label = label;
        this.bounds = bounds;
    }

    //
    // Summary:
    //     Handle a user left-click on the element (including a 'click' through controller
    //     selection).
    //
    // Parameters:
    //   x:
    //     The pixel X coordinate that was clicked.
    //
    //   y:
    //     The pixel Y coordinate that was clicked.
    public virtual void receiveLeftClick(int x, int y) {
    }

    //
    // Summary:
    //     Handle the left-click button being held down (including a button resulting in
    //     a 'click' through controller selection). This is called each tick that it's held.
    //
    //
    // Parameters:
    //   x:
    //     The cursor's current pixel X coordinate.
    //
    //   y:
    //     The cursor's current pixel Y coordinate.
    public virtual void leftClickHeld(int x, int y) {
    }

    //
    // Summary:
    //     Handle the left-click button being released (including a button resulting in
    //     a 'click' through controller selection).
    //
    // Parameters:
    //   x:
    //     The cursor's current pixel X coordinate.
    //
    //   y:
    //     The cursor's current pixel Y coordinate.
    public virtual void leftClickReleased(int x, int y) {
    }

    //
    // Summary:
    //     Handle a keyboard button pressed.
    //
    // Parameters:
    //   key:
    //     The keyboard button that was pressed.
    public virtual void receiveKeyPress(Keys key) {
    }

    public void resetValue() {
    }

    //
    // Summary:
    //     Render the element.
    //
    // Parameters:
    //   b:
    //     The sprite batch being drawn.
    //
    //   slotX:
    //     The pixel X position at which to draw, relative to the bounds.
    //
    //   slotY:
    //     The pixel Y position at which to draw, relative to the bounds.
    //
    //   context:
    //     The menu which contains this element, if applicable.
    public virtual void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null) {
        if (style == Style.OptionLabel) {
            Utility.drawTextWithShadow(b, label, Game1.dialogueFont, new Vector2(slotX + bounds.X + (int)labelOffset.X, slotY + bounds.Y + (int)labelOffset.Y + 12), greyedOut ? (Game1.textColor * 0.33f) : Game1.textColor, 1f, 0.1f);
            return;
        }

        int num = slotX + bounds.X + bounds.Width + 8 + (int)labelOffset.X;
        int num2 = slotY + bounds.Y + (int)labelOffset.Y;
        string text = label;
        SpriteFont spriteFont = Game1.dialogueFont;
        if (context != null) {
            int num3 = context.width - 64;
            int xPositionOnScreen = context.xPositionOnScreen;
            if (spriteFont.MeasureString(label).X + (float)num > (float)(num3 + xPositionOnScreen)) {
                int width = num3 + xPositionOnScreen - num;
                spriteFont = Game1.smallFont;
                text = Game1.parseText(label, spriteFont, width);
                num2 -= (int)((spriteFont.MeasureString(text).Y - spriteFont.MeasureString("T").Y) / 2f);
            }
        }

        Utility.drawTextWithShadow(b, text, spriteFont, new Vector2(num, num2), greyedOut ? (Game1.textColor * 0.33f) : Game1.textColor, 1f, 0.1f);
    }
}