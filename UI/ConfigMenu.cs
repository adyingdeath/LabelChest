using LabelChest.UI.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

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
            new ConfigSlider("Font Size", (value) => {
                ModEntry.Config.FontSize = value;
            }).Min(0.2f).Max(3.0f).DefaultValue(1.0f),
        };

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

        // Draw the preview chest
        Chest chest = new Chest(true);
        float chestWidth = PREVIEW_WIDTH / 3;
        float chestScale = chestWidth / 32f;
        Vector2 chestPos = new Vector2(
            MARGIN_X + PADDING_X + PREVIEW_WIDTH / 2 - chestWidth / 2,
            MARGIN_Y + height / 2 - chestWidth / 2
        );
        chest.drawInMenu(b, chestPos, chestScale);
        ModEntry.WorldLabelRenderer.DrawLabel(b, chestPos + new Vector2(chestWidth / 2), "example");
        
        // Draw options
        base.draw(b);

        drawMouse(b);
    }
}