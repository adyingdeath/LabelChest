using LabelChest.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Objects;
using LabelChest.UI.Menus;
using StardewValley.Menus;

namespace LabelChest.UI;

class OptionsManager {
    public List<LComponent> options = new();
    public List<string> optionsTag = new();
    public Dictionary<string, bool> tagVisibility = new();
    private Action<List<LComponent>> onVisibleOptionsChanged;

    public OptionsManager(Action<List<LComponent>> updateAction) {
        onVisibleOptionsChanged = updateAction;
    }

    public void SetUpdateCallback(Action<List<LComponent>> updateAction) {
        onVisibleOptionsChanged = updateAction;
    }

    private void TriggerUpdate() {
        if (onVisibleOptionsChanged != null) {
            onVisibleOptionsChanged(GetVisibleOptions());
        }
    }

    public OptionsManager Add(LComponent option, string tag = "default") {
        options.Add(option);
        optionsTag.Add(tag);
        if (!tagVisibility.ContainsKey(tag)) {
            tagVisibility[tag] = true;
        }
        TriggerUpdate();
        return this;
    }

    public void Display(string tag) {
        if (!tagVisibility.ContainsKey(tag)) return;
        tagVisibility[tag] = true;
        TriggerUpdate();
    }

    public void Display(List<string> tag) {
        bool changed = false;
        tag.ForEach((t) => {
            if (!tagVisibility.ContainsKey(t)) return;
            if (!tagVisibility[t]) {
                tagVisibility[t] = true;
                changed = true;
            }
        });
        if (changed) TriggerUpdate();
    }

    public void Hide(string tag) {
        if (!tagVisibility.ContainsKey(tag)) return;
        tagVisibility[tag] = false;
        TriggerUpdate();
    }

    public void Hide(List<string> tag) {
        bool changed = false;
        tag.ForEach((t) => {
            if (!tagVisibility.ContainsKey(t)) return;
            if (tagVisibility[t]) {
                tagVisibility[t] = false;
                changed = true;
            }
        });
        if (changed) TriggerUpdate();
    }

    public bool IsVisible(string tag) {
        if (!tagVisibility.ContainsKey(tag)) return false;
        return tagVisibility[tag];
    }

    public bool IsVisible(int tagIndex) {
        string tag = optionsTag[tagIndex];
        if (!tagVisibility.ContainsKey(tag)) return false;
        return tagVisibility[tag];
    }

    public List<LComponent> GetVisibleOptions() {
        return options.Where((option, index) => IsVisible(index)).ToList();
    }
}

public record ConfigMenuTranslation(
    string FontSize = "",
    string TextColorType = "",
    string TextColorTypeFixed = "",
    string TextColorTypeInverted = "",
    string TextColorTypeFollowBox = "",
    string TextColorRed = "",
    string TextColorGreen = "",
    string TextColorBlue = "",
    string ButtonConfig = "",
    string Title = ""
);

public class ConfigMenu : IClickableMenu {
    private const int WIDTH = 900;
    private const int HEIGHT = 600;
    private const int PADDING_X = 20;
    private const int PADDING_Y = 20;
    private const int SPACE_Y = 8;
    private const int PREVIEW_WIDTH = 100;
    private Debouncer debouncer = new Debouncer(TimeSpan.FromMilliseconds(50), () => {
        ModEntry.CacheManager.ClearCache();
    });
    private readonly OptionsManager optionsManager;
    private readonly ConfigMenuTranslation translation;
    private readonly LOptionsPage optionsPage;

    public ConfigMenu(ConfigMenuTranslation translation) : base((Game1.viewport.Width - WIDTH) / 2, (Game1.viewport.Height - HEIGHT) / 2, WIDTH, HEIGHT, true) {
        this.translation = translation;
        optionsPage = new LOptionsPage(
            xPositionOnScreen + PADDING_X + PREVIEW_WIDTH,
            yPositionOnScreen + PADDING_Y,
            width - 2 * PADDING_X - PREVIEW_WIDTH,
            height - 2 * PADDING_Y
        );
        optionsManager = new((options) => {
            optionsPage.options = options;
        });

        optionsManager
        .Add(
            // Font Size
            new LSlider(translation.FontSize, (value) => {
                if (ModEntry.Config.FontSize == value) return;
                ModEntry.Config.FontSize = value;
                debouncer.Invoke();
            }).Min(0.2f).Max(3.0f).DefaultValue(ModEntry.Config.FontSize)
        )
        .Add(
            // Text Color Type
            new LSelect(translation.TextColorType, (value) => {
                if (value == "Fixed") {
                    optionsManager.Display("text-color-fixed");
                } else {
                    optionsManager.Hide("text-color-fixed");
                }
            }).AddOption("Fixed", translation.TextColorTypeFixed)
            .AddOption("Inverted", translation.TextColorTypeInverted)
            .AddOption("FollowBox", translation.TextColorTypeFollowBox)
        )
        .Add(
            // Red
            new LSlider(translation.TextColorRed, (value) => {
                Color color = ModEntry.Config.TextColor;
                color.R = (byte)value;
                ModEntry.Config.TextColor = color;
                debouncer.Invoke();
            }).Min(0).Max(255).DefaultValue(ModEntry.Config.TextColor.R),
            "text-color-fixed"
        )
        .Add(
            // Green
            new LSlider(translation.TextColorGreen, (value) => {
                Color color = ModEntry.Config.TextColor;
                color.G = (byte)value;
                ModEntry.Config.TextColor = color;
                debouncer.Invoke();
            }).Min(0).Max(255).DefaultValue(ModEntry.Config.TextColor.G),
            "text-color-fixed"
        )
        .Add(
            // Blue
            new LSlider(translation.TextColorBlue, (value) => {
                Color color = ModEntry.Config.TextColor;
                color.B = (byte)value;
                ModEntry.Config.TextColor = color;
                debouncer.Invoke();
            }).Min(0).Max(255).DefaultValue(ModEntry.Config.TextColor.B),
            "text-color-fixed"
        )
        ;
        optionsPage.options = optionsManager.GetVisibleOptions();
    }

    public override void draw(SpriteBatch b) {
        // Draw background dim
        b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);

        // Draw menu frame
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

        // Draw the preview chest
        Chest chest = new Chest(true);
        float chestWidth = 64f;
        float chestScale = chestWidth / 32f;
        Vector2 chestPos = new Vector2(
            xPositionOnScreen + PADDING_X + PREVIEW_WIDTH / 2 - chestWidth / 2,
            yPositionOnScreen + height / 2 - chestWidth / 2
        );
        chest.drawInMenu(b, chestPos, chestScale);
        ModEntry.WorldLabelRenderer.DrawLabel(b, chestPos + new Vector2(chestWidth / 2), "example");

        // Draw options
        optionsPage.draw(b);

        // Draw close button
        base.draw(b);

        drawMouse(b);
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true) {
        base.receiveLeftClick(x, y, playSound);
        optionsPage.receiveLeftClick(x, y, playSound);
    }

    public override void receiveKeyPress(Keys key) {
        optionsPage.receiveKeyPress(key);
        base.receiveKeyPress(key);
    }

    public override void performHoverAction(int x, int y) {
        optionsPage.performHoverAction(x, y);
        base.performHoverAction(x, y);
    }

    public override void receiveScrollWheelAction(int direction) {
        optionsPage.receiveScrollWheelAction(direction);
        base.receiveScrollWheelAction(direction);
    }

    public override void leftClickHeld(int x, int y) {
        optionsPage.leftClickHeld(x, y);
        base.leftClickHeld(x, y);
    }

    public override void releaseLeftClick(int x, int y) {
        optionsPage.releaseLeftClick(x, y);
        base.releaseLeftClick(x, y);
    }
}
