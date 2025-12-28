using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace LabelChest.UI.Components;

public class ConfigSlider : OptionsSlider {
    private readonly Action<int> onChange;
    public ConfigSlider(string label, Action<int> onChange, int x = -1, int y = -1) : base(label, -1, x, y) {
        this.onChange = onChange;
        labelOffset.X = bounds.Width + 10;
    }
    // Override events so we can listen to them
    public override void leftClickHeld(int x, int y) {
        base.leftClickHeld(x, y);
        onChange(value);
    }
    public override void receiveKeyPress(Keys key) {
        base.receiveKeyPress(key);
        onChange(value);
    }
}