using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace LabelChest.UI.Components;

public class ConfigDropDown : OptionsDropDown {
    private readonly Action<string> onChange;
    public ConfigDropDown(string label, Action<string> onChange, int x = -1, int y = -1) : base(label, -1, x, y) {
        this.onChange = onChange;
        labelOffset.X = bounds.Width + 10;
    }
    public ConfigDropDown AddOption(string value, string displayName) {
        dropDownOptions.Add(value);
        dropDownDisplayOptions.Add(displayName);
        return this;
    }
    // Override events so we can listen to them
    public override void leftClickReleased(int x, int y) {
        base.leftClickReleased(x, y);
        onChange(dropDownOptions[selectedOption]);
    }
    public override void receiveKeyPress(Keys key) {
        base.receiveKeyPress(key);
        onChange(dropDownOptions[selectedOption]);
    }
}