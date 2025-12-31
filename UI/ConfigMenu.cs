using LabelChest.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

    public OptionsManager Add(LComponent option, string tag) {
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

public class ConfigMenu : LOptionsPage {
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
    
    public ConfigMenu(ConfigMenuTranslation translation) : base((Game1.viewport.Width - WIDTH) / 2, (Game1.viewport.Height - HEIGHT) / 2, WIDTH, HEIGHT) {
        this.translation = translation;
        optionsManager = new((options) => {
            this.options = options;
        });

        optionsManager
        .Add(
            // Font Size
            new LSlider(translation.FontSize, (value) => {
                if (ModEntry.Config.FontSize == value) return;
                ModEntry.Config.FontSize = value;
                debouncer.Invoke();
            }).Min(0.2f).Max(3.0f).DefaultValue(ModEntry.Config.FontSize),
            "default"
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
            .AddOption("FollowBox", translation.TextColorTypeFollowBox),
            "default"
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
        options = optionsManager.GetVisibleOptions();

        optionSlots.Clear();
        for (int i = 0; i < 7; i++) {
            optionSlots.Add(new ClickableComponent(
                new Rectangle(
                    xPositionOnScreen + PADDING_X + PREVIEW_WIDTH,
                    yPositionOnScreen + PADDING_Y + i * ((height - 2 * PADDING_Y - 6 * SPACE_Y) / 7) + SPACE_Y * Math.Max(0, i - 0),
                    width - 2 * xPositionOnScreen - 2 * PADDING_X - PREVIEW_WIDTH,
                    (height - 2 * PADDING_Y - 6 * SPACE_Y) / 7
                ),
                i.ToString() ?? ""
            ) {
                myID = i,
                downNeighborID = (i < 6) ? (i + 1) : (-7777),
                upNeighborID = (i > 0) ? (i - 1) : (-7777),
                fullyImmutable = true
            });
        }
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
        base.draw(b);

        drawMouse(b);
    }
}