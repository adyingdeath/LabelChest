using LabelChest.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;
using LabelChest.UI.Menus;
using StardewValley.Menus;

namespace LabelChest.UI;

public class ConfigMenu : LOptionsPage {
    private const int WIDTH = 900;
    private const int HEIGHT = 600;
    private const int PADDING_X = 20;
    private const int PADDING_Y = 20;
    private const int SPACE_Y = 8;
    private const int PREVIEW_WIDTH = 100;
    private readonly OptionsManager optionsManager;

    public ConfigMenu() 
        : base(
            (Game1.viewport.Width - WIDTH) / 2 + PREVIEW_WIDTH + PADDING_X,
            (Game1.viewport.Height - HEIGHT) / 2 + PADDING_Y,
            WIDTH - 2 * PADDING_X - PREVIEW_WIDTH,
            HEIGHT - 2 * PADDING_Y
        ) {
        optionsManager = new((options) => {
            this.options = options;
        });

        optionsManager
        .Add(
            // Font Size
            new LSlider(I18n.ConfigMenu_FontSize(), (value) => {
                if (ModEntry.Config.FontSize == value) return;
                ModEntry.Config.FontSize = value;
            }).Min(0.2f).Max(3.0f).Step(0.1f).DefaultValue(ModEntry.Config.FontSize)
        )
        .Add(
            // Text Color Type
            new LSelect(I18n.ConfigMenu_TextColorType_Label(), (value) => {
                if (Enum.TryParse(value, true, out TextColorType enumValue)) {
                    ModEntry.Config.TextColorType = enumValue;
                    if (enumValue == TextColorType.Fixed) {
                        optionsManager.Display("text-color-fixed");
                    } else {
                        optionsManager.Hide("text-color-fixed");
                    }
                }
            }).AddOption(TextColorType.Fixed.ToString(), I18n.ConfigMenu_TextColorType_Fixed())
            .AddOption(TextColorType.Inverted.ToString(), I18n.ConfigMenu_TextColorType_Inverted())
            .AddOption(TextColorType.FollowBox.ToString(), I18n.ConfigMenu_TextColorType_FollowBox())
            .DefaultValue(ModEntry.Config.TextColorType.ToString())
        )
        .Add(
            // Red
            new LSlider(I18n.ConfigMenu_TextColor_Red(), (value) => {
                Color color = ModEntry.Config.TextColor;
                color.R = (byte)value;
                ModEntry.Config.TextColor = color;
            }).Min(0).Max(255).Step(10).DefaultValue(ModEntry.Config.TextColor.R),
            "text-color-fixed"
        )
        .Add(
            // Green
            new LSlider(I18n.ConfigMenu_TextColor_Green(), (value) => {
                Color color = ModEntry.Config.TextColor;
                color.G = (byte)value;
                ModEntry.Config.TextColor = color;
            }).Min(0).Max(255).Step(10).DefaultValue(ModEntry.Config.TextColor.G),
            "text-color-fixed"
        )
        .Add(
            // Blue
            new LSlider(I18n.ConfigMenu_TextColor_Blue(), (value) => {
                Color color = ModEntry.Config.TextColor;
                color.B = (byte)value;
                ModEntry.Config.TextColor = color;
            }).Min(0).Max(255).Step(10).DefaultValue(ModEntry.Config.TextColor.B),
            "text-color-fixed"
        )
        .Add(
            // Outline Color Type
            new LSelect(I18n.ConfigMenu_OutlineColorType_Label(), (value) => {
                if (Enum.TryParse(value, true, out OutlineColorType enumValue)) {
                    ModEntry.Config.OutlineColorType = enumValue;
                    if (enumValue == OutlineColorType.Fixed) {
                        optionsManager.Display("outline-color-fixed");
                    } else {
                        optionsManager.Hide("outline-color-fixed");
                    }
                }
            }).AddOption(OutlineColorType.Inverted.ToString(), I18n.ConfigMenu_OutlineColorType_Inverted())
            .AddOption(OutlineColorType.Fixed.ToString(), I18n.ConfigMenu_OutlineColorType_Fixed())
            .DefaultValue(ModEntry.Config.OutlineColorType.ToString())
        )
        .Add(
            // Outline Red
            new LSlider(I18n.ConfigMenu_OutlineColor_Red(), (value) => {
                Color color = ModEntry.Config.OutlineColor;
                color.R = (byte)value;
                ModEntry.Config.OutlineColor = color;
            }).Min(0).Max(255).Step(10).DefaultValue(ModEntry.Config.OutlineColor.R),
            "outline-color-fixed"
        )
        .Add(
            // Outline Green
            new LSlider(I18n.ConfigMenu_OutlineColor_Green(), (value) => {
                Color color = ModEntry.Config.OutlineColor;
                color.G = (byte)value;
                ModEntry.Config.OutlineColor = color;
            }).Min(0).Max(255).Step(10).DefaultValue(ModEntry.Config.OutlineColor.G),
            "outline-color-fixed"
        )
        .Add(
            // Outline Blue
            new LSlider(I18n.ConfigMenu_OutlineColor_Blue(), (value) => {
                Color color = ModEntry.Config.OutlineColor;
                color.B = (byte)value;
                ModEntry.Config.OutlineColor = color;
            }).Min(0).Max(255).Step(10).DefaultValue(ModEntry.Config.OutlineColor.B),
            "outline-color-fixed"
        );

        // Apply initial visibility based on default values
        switch(ModEntry.Config.TextColorType) {
            case TextColorType.Fixed:
                optionsManager.Display("text-color-fixed");
                break;
            case TextColorType.Inverted:
            case TextColorType.FollowBox:
                optionsManager.Hide("text-color-fixed");
                break;
        }

        switch(ModEntry.Config.OutlineColorType) {
            case OutlineColorType.Fixed:
                optionsManager.Display("outline-color-fixed");
                break;
            case OutlineColorType.Inverted:
                optionsManager.Hide("outline-color-fixed");
                break;
        }

        options = optionsManager.GetVisibleOptions();
    }

    public override void draw(SpriteBatch b) {
        // Calculate menu position
        int menuX = xPositionOnScreen - PREVIEW_WIDTH - PADDING_X;
        int menuY = yPositionOnScreen - PADDING_Y;
        
        // Draw background dim
        b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);

        // Draw menu frame
        IClickableMenu.drawTextureBox(
            b,
            Game1.mouseCursors,
            new Rectangle(384, 373, 18, 18),
            menuX,
            menuY,
            WIDTH,
            HEIGHT,
            Color.White,
            4f
        );

        // Draw the preview chest
        Chest chest = new Chest(true);
        float chestWidth = 64f;
        float chestScale = chestWidth / 32f;
        Vector2 chestPos = new Vector2(
            menuX + PADDING_X + PREVIEW_WIDTH / 2 - chestWidth / 2,
            menuY + HEIGHT / 2 - chestWidth / 2
        );
        chest.drawInMenu(b, chestPos, chestScale);
        SpriteBatchSwitcher.SwitchAntiAliasing(b);
        ModEntry.WorldLabelRenderer.DrawLabel(b, chestPos + new Vector2(chestWidth / 2), I18n.ConfigMenu_PreviewText(), chest);
        SpriteBatchSwitcher.SwitchDefault(b);

        // Draw options
        base.draw(b);

        // Draw Mouse
        drawMouse(b);
    }
}
