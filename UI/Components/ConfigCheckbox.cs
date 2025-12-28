using Microsoft.Xna.Framework.Input;
using StardewValley.Menus;

namespace LabelChest.UI.Components;

public class ConfigCheckbox : OptionsCheckbox {
    private readonly Action<bool> onChange;
    public ConfigCheckbox(string label, Action<bool> onChange, int x = -1, int y = -1) : base(label, -1, x, y) {
        this.onChange = onChange;
        labelOffset.X = bounds.Width + 10;
    }
    // Override events so we can listen to them
    public override void receiveLeftClick(int x, int y) {
        base.receiveLeftClick(x, y);
        onChange(isChecked);
    }
}