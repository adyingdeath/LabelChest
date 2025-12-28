using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace LabelChest.UI.Components;

public class ConfigSlider : OptionsSlider {
    private readonly Action<float> onChange;
    private float minValue = 1;
    private float maxValue = 0;
    public ConfigSlider(string label, Action<float> onChange, int x = -1, int y = -1) : base(label, -1, x, y) {
        this.onChange = onChange;
        labelOffset.X = bounds.Width + 10;
    }
    public ConfigSlider Min(float minValue) {
        this.minValue = minValue;
        return this;
    }
    public ConfigSlider Max(float maxValue) {
        this.maxValue = maxValue;
        return this;
    }
    public ConfigSlider DefaultValue(float value) {
        this.value = (int)((value - minValue) / (maxValue - minValue) * sliderMaxValue);
        return this;
    }
    // Override events so we can listen to them
    public override void leftClickHeld(int x, int y) {
        base.leftClickHeld(x, y);
        onChange(LerpValue(minValue, maxValue, value * 1.0f / sliderMaxValue));
    }
    public override void receiveKeyPress(Keys key) {
        base.receiveKeyPress(key);
        onChange(LerpValue(minValue, maxValue, value * 1.0f / sliderMaxValue));
    }
    public static float LerpValue(float start, float end, float ratio) {
        return start + (end - start) * ratio;
    }
}