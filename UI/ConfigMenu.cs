using LabelChest.UI.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace LabelChest.UI;

public class ConfigMenu : OptionsPage {
    private const int MARGIN_X = 100;
    private const int MARGIN_Y = 100;
    private const int PADDING_X = 20;
    private const int PADDING_Y = 20;
    private const int SPACE_Y = 8;
    private const int PREVIEW_WIDTH = 200;
    public ConfigMenu() : base(MARGIN_X, MARGIN_Y, Game1.viewport.Width - MARGIN_X * 2, Game1.viewport.Height - MARGIN_Y * 2) {
        options = new List<OptionsElement> {
            new ConfigSlider("Test", (value) => {
                ModEntry.Log($"{value}");
            }),
            new ConfigCheckbox("Show Portraits", (value) => {
                ModEntry.Log($"{value}");
            }),
            new ConfigDropDown("Gamepad Mode", (value) => {
                ModEntry.Log($"{value}");
            }).AddOption("Test-value", "Test")
            .AddOption("Jungle-value", "Jungle")
        };

        for (int i = 0;i < 30;i++) {
            int current = i;
            options.Add(new ConfigCheckbox($"Checkbox {i}", (value) => {
                ModEntry.Log($"{current}: {value}");
            }));
        }

        optionSlots.Clear();
        for (int i = 0; i < 7; i++) {
            optionSlots.Add(new ClickableComponent(
                new Rectangle(
                    xPositionOnScreen + PADDING_X + PREVIEW_WIDTH,
                    yPositionOnScreen + PADDING_Y + i * ((height - 2 * PADDING_Y - 6 * SPACE_Y) / 7) + SPACE_Y * Math.Max(0, i - 0),
                    width - 2 * MARGIN_X - 2 * PADDING_X - PREVIEW_WIDTH,
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
            MARGIN_X,
            MARGIN_Y,
            width,
            height,
            Color.White,
            4f
        );

        // Draw options
        base.draw(b);

        drawMouse(b);
    }
}