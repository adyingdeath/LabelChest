using System;
using System.Collections.Generic;
using LabelChest.UI.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace LabelChest.UI {
    public class ConfigMenu : IClickableMenu {
        public List<ClickableComponent> optionSlots = new List<ClickableComponent>();
        private List<OptionsElement> options;
        private int currentItemIndex = 0;
        private int optionsSlotHeld = -1;

        public ConfigMenu() : base(0, 0, Game1.viewport.Width, Game1.viewport.Height, false) {
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
            b.End();
            b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
            for (int i = 0; i < optionSlots.Count; i++) {
                if (currentItemIndex + i < options.Count) {
                    options[currentItemIndex + i].draw(b, optionSlots[i].bounds.X, optionSlots[i].bounds.Y, this);
                }
            }
            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            drawMouse(b);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true) {
            for (int i = 0; i < optionSlots.Count; i++) {
                if (optionSlots[i].bounds.Contains(x, y) && currentItemIndex + i < options.Count) {
                    options[currentItemIndex + i].receiveLeftClick(x - optionSlots[i].bounds.X, y - optionSlots[i].bounds.Y);
                    optionsSlotHeld = i;
                    break;
                }
            }
        }

        public override void leftClickHeld(int x, int y) {
            if (optionsSlotHeld != -1 && currentItemIndex + optionsSlotHeld < options.Count) {
                options[currentItemIndex + optionsSlotHeld].leftClickHeld(x - optionSlots[optionsSlotHeld].bounds.X, y - optionSlots[optionsSlotHeld].bounds.Y);
            }
        }

        public override void releaseLeftClick(int x, int y) {
            if (optionsSlotHeld != -1 && currentItemIndex + optionsSlotHeld < options.Count) {
                options[currentItemIndex + optionsSlotHeld].leftClickReleased(x - optionSlots[optionsSlotHeld].bounds.X, y - optionSlots[optionsSlotHeld].bounds.Y);
            }
            optionsSlotHeld = -1;
        }

        public override void receiveKeyPress(Keys key) {
            if (optionsSlotHeld != -1 && currentItemIndex + optionsSlotHeld < options.Count) {
                options[currentItemIndex + optionsSlotHeld].receiveKeyPress(key);
            }
            base.receiveKeyPress(key);
        }

        public override void performHoverAction(int x, int y) {
            for (int i = 0; i < optionSlots.Count; i++) {
                if (currentItemIndex + i < options.Count && options[currentItemIndex + i].bounds.Contains(x - optionSlots[i].bounds.X, y - optionSlots[i].bounds.Y)) {
                    Game1.SetFreeCursorDrag();
                    break;
                }
            }
        }
    }
}
