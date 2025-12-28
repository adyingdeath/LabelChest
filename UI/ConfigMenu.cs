using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace LabelChest.UI {
    public class ConfigMenu : IClickableMenu {
        public ConfigMenu() {
            
        }

        public override void draw(SpriteBatch b) {
            // Draw background dim
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);

            drawMouse(b);
        }
    }
}