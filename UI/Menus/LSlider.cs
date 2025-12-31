using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace LabelChest.UI.Menus;

public class LSlider : LComponent {
    public const int pixelsWide = 48;

    public const int pixelsHigh = 6;

    public const int sliderButtonWidth = 10;

    public float value;

    public static Rectangle sliderBGSource = new Rectangle(403, 383, 6, 6);

    public static Rectangle sliderButtonRect = new Rectangle(420, 441, 10, 6);

    private readonly Action<float> onChange;

    private float minValue = 0;

    private float maxValue = 1;

    private float valueRange = 1;

    public LSlider(string label, Action<float> onChange, int x = -1, int y = -1)
        : base(label, x, y, 192, 24) {
        this.onChange = onChange;
        value = 0;
    }

    public LSlider Min(float minValue) {
        this.minValue = minValue;
        valueRange = maxValue - minValue;
        return this;
    }
    public LSlider Max(float maxValue) {
        this.maxValue = maxValue;
        valueRange = maxValue - minValue;
        return this;
    }
    public LSlider DefaultValue(float value) {
        this.value = value;
        return this;
    }

    public override void leftClickHeld(int x, int y) {
        if (!greyedOut) {
            base.leftClickHeld(x, y);
            if (x < bounds.X) {
                value = minValue;
            } else if (x > bounds.Right - 40) {
                value = maxValue;
            } else {
                value = (maxValue - minValue) * (x - bounds.X) / (bounds.Width - 40) + minValue;
            }

            onChange(value);
        }
    }

    public override void receiveLeftClick(int x, int y) {
        if (!greyedOut) {
            base.receiveLeftClick(x, y);
            leftClickHeld(x, y);
        }
    }

    public override void receiveKeyPress(Keys key) {
        base.receiveKeyPress(key);
        if (Game1.options.snappyMenus && Game1.options.gamepadControls && !greyedOut) {
            if (Game1.options.doesInputListContain(Game1.options.moveRightButton, key)) {
                value = Math.Min(value + 10, 100);
                onChange(value);
            } else if (Game1.options.doesInputListContain(Game1.options.moveLeftButton, key)) {
                value = Math.Max(value - 10, 0);
                onChange(value);
            }
        }
    }

    public override void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null) {
        base.draw(b, slotX, slotY, context);
        IClickableMenu.drawTextureBox(b, Game1.mouseCursors, sliderBGSource, slotX + bounds.X, slotY + bounds.Y, bounds.Width, bounds.Height, Color.White, 4f, drawShadow: false);
        b.Draw(Game1.mouseCursors, new Vector2(slotX + bounds.X + (bounds.Width - 40) * ((value - minValue) / valueRange), slotY + bounds.Y), sliderButtonRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
    }
}