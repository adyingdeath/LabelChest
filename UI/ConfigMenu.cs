using LabelChest.UI.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace LabelChest.UI;

public class ConfigMenu : OptionsPage {
    public ConfigMenu() : base(0, 0, Game1.viewport.Width, Game1.viewport.Height) {
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

        optionSlots.Clear();
        for (int i = 0; i < 7; i++) {
            optionSlots.Add(new ClickableComponent(new Rectangle(100, 100 + i * 50, 300, 50), i.ToString() ?? "") {
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
            50,
            50,
            Game1.viewport.Width - 100,
            Game1.viewport.Height - 100,
            Color.White,
            4f
        );

        // Draw options
        base.draw(b);

        drawMouse(b);
    }
}