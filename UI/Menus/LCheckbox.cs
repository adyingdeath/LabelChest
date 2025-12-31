using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace LabelChest.UI.Menus;

public class LCheckbox : LComponent
{
    public const int pixelsWide = 9;

    public static LCheckbox selected;

    public bool isChecked;

    public static Rectangle sourceRectUnchecked = new Rectangle(227, 425, 9, 9);

    public static Rectangle sourceRectChecked = new Rectangle(236, 425, 9, 9);

    private readonly Action<bool> onChange;

    public LCheckbox(string label, Action<bool> onChange, int x = -1, int y = -1)
        : base(label, x, y, 36, 36)
    {
        this.onChange = onChange;
        isChecked = false;
        labelOffset.X = bounds.Width + 10;
    }

    public override void receiveLeftClick(int x, int y)
    {
        if (!greyedOut)
        {
            Game1.playSound("drumkit6");
            selected = this;
            base.receiveLeftClick(x, y);
            isChecked = !isChecked;
            onChange(isChecked);
            selected = null;
        }
    }

    public override void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null)
    {
        b.Draw(Game1.mouseCursors, new Vector2(slotX + bounds.X, slotY + bounds.Y), isChecked ? sourceRectChecked : sourceRectUnchecked, Color.White * (greyedOut ? 0.33f : 1f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.4f);
        base.draw(b, slotX, slotY, context);
    }
}