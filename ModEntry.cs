using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace LabelChest
{
    public class ModEntry : Mod
    {
        private const string LabelKey = "LabelChest.label";

        // UI Configuration
        private const int ButtonHeight = 48;
        private const int MinButtonWidth = 150;
        
        // World Draw Configuration
        private const float WorldFontScale = 0.8f;
        private const int WorldTileSize = 64;
        private const int WorldMaxWidth = 60;

        public override void Entry(IModHelper helper)
        {
            helper.Events.Display.RenderedWorld += OnRenderedWorld;
            helper.Events.Display.RenderedActiveMenu += OnRenderedActiveMenu;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (e.Button != SButton.MouseLeft) return;

            // Get current chest menu
            var (chest, menu) = GetCurrentChestMenu();
            if (chest == null || menu == null) return;

            Rectangle buttonRect = GetLabelButtonBounds(menu);

            // Check if label button was clicked
            if (buttonRect.Contains(e.Cursor.ScreenPixels.X, e.Cursor.ScreenPixels.Y))
            {
                Game1.playSound("drumkit6");

                string currentLabel = chest.modData.TryGetValue(LabelKey, out string val) ? val : "";

                string title = Helper.Translation.Get("set-label-title");

                // Open ChestNamingMenu
                Game1.activeClickableMenu = new ChestNamingMenu(title, currentLabel, delegate (string input)
                {
                    // 1. Update Data
                    if (string.IsNullOrWhiteSpace(input))
                    {
                        if (chest.modData.ContainsKey(LabelKey))
                            chest.modData.Remove(LabelKey);
                    }
                    else
                    {
                        chest.modData[LabelKey] = input;
                    }
                });
            }
        }

        private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
        {
            var (chest, menu) = GetCurrentChestMenu();
            if (chest == null || menu == null) return;

            Rectangle buttonRect = GetLabelButtonBounds(menu);
            SpriteBatch b = e.SpriteBatch;

            // Draw button background
            IClickableMenu.drawTextureBox(
                b,
                Game1.menuTexture,
                new Rectangle(0, 256, 60, 60),
                buttonRect.X,
                buttonRect.Y,
                buttonRect.Width,
                buttonRect.Height,
                Color.White,
                1f,
                false
            );

            // Determine text
            string labelText = chest.modData.TryGetValue(LabelKey, out string val) && !string.IsNullOrWhiteSpace(val)
                ? val 
                : Helper.Translation.Get("set-label-button");; 

            // Center text
            Vector2 textSize = Game1.smallFont.MeasureString(labelText);
            Vector2 textPos = new Vector2(
                buttonRect.X + (buttonRect.Width - textSize.X) / 2,
                buttonRect.Y + (buttonRect.Height - textSize.Y) / 2
            );

            b.DrawString(Game1.smallFont, labelText, textPos, Game1.textColor);

            // Draw hover effect
            if (buttonRect.Contains(Game1.getMouseX(), Game1.getMouseY()))
            {
                IClickableMenu.drawTextureBox(
                    b, Game1.menuTexture, new Rectangle(0, 256, 60, 60),
                    buttonRect.X, buttonRect.Y, buttonRect.Width, buttonRect.Height,
                    Color.White * 0.25f, 1f, false);
            }

            // Redraw mouse cursor
            menu.drawMouse(b);
        }

        private void OnRenderedWorld(object? sender, RenderedWorldEventArgs e)
        {
            if (!Context.IsWorldReady) return;

            // Iterate over objects in current location
            foreach (var pair in Game1.currentLocation.Objects.Pairs)
            {
                StardewValley.Object obj = pair.Value;
                Vector2 tileLocation = pair.Key;

                if (obj is Chest chest && obj.modData.TryGetValue(LabelKey, out string labelText))
                {
                    if (!string.IsNullOrWhiteSpace(labelText))
                    {
                        DrawChestLabelInWorld(e.SpriteBatch, tileLocation, labelText);
                    }
                }
            }
        }

        // --- Helpers ---

        private static (Chest?, ItemGrabMenu?) GetCurrentChestMenu()
        {
            if (Game1.activeClickableMenu is ItemGrabMenu menu && menu.context is Chest chest)
            {
                return (chest, menu);
            }
            return (null, null);
        }

        private Rectangle GetLabelButtonBounds(ItemGrabMenu menu)
        {
            int width = Math.Max(MinButtonWidth, menu.width / 3);
            int x = menu.xPositionOnScreen + (menu.width - width) / 2;
            // Position above the menu content
            int y = menu.ItemsToGrabMenu.yPositionOnScreen - ButtonHeight - (
                menu.ItemsToGrabMenu.capacity switch
                {
                    36 => 24,
                    70 => 8,
                    _ => 24,
                }
            );

            return new Rectangle(x, y, width, ButtonHeight);
        }

        private void DrawChestLabelInWorld(SpriteBatch b, Vector2 tileLocation, string text)
        {
            Vector2 screenPos = Game1.GlobalToLocal(Game1.viewport, tileLocation * 64f);
            
            // Bounds check
            if (screenPos.X < -64 || screenPos.X > Game1.viewport.Width || 
                screenPos.Y < -64 || screenPos.Y > Game1.viewport.Height)
                return;

            SpriteFont font = Game1.smallFont;
            List<string> lines = WrapText(font, text, WorldMaxWidth / WorldFontScale);
            
            float lineHeight = font.MeasureString("A").Y * WorldFontScale;
            float totalHeight = lines.Count * lineHeight;
            
            // Draw slightly above center to avoid overlap
            float startY = screenPos.Y - (WorldTileSize / 4f) - (totalHeight / 2f) + 2f; 

            foreach (string line in lines)
            {
                Vector2 size = font.MeasureString(line) * WorldFontScale;
                float startX = screenPos.X + (WorldTileSize / 2f) - (size.X / 2f);
                Vector2 position = new Vector2(startX, startY);

                // Draw outline (Black)
                b.DrawString(font, line, position + new Vector2(-1, -1), Color.Black, 0f, Vector2.Zero, WorldFontScale, SpriteEffects.None, 1f);
                b.DrawString(font, line, position + new Vector2(1, 1), Color.Black, 0f, Vector2.Zero, WorldFontScale, SpriteEffects.None, 1f);
                b.DrawString(font, line, position + new Vector2(1, -1), Color.Black, 0f, Vector2.Zero, WorldFontScale, SpriteEffects.None, 1f);
                b.DrawString(font, line, position + new Vector2(-1, 1), Color.Black, 0f, Vector2.Zero, WorldFontScale, SpriteEffects.None, 1f);

                // Draw text (White)
                b.DrawString(font, line, position, Color.White, 0f, Vector2.Zero, WorldFontScale, SpriteEffects.None, 1f);

                startY += lineHeight;
            }
        }

        // Text wrapping logic
        private List<string> WrapText(SpriteFont font, string text, float maxLineWidth)
        {
            List<string> lines = new List<string>();
            string[] words = text.Split(' ');
            StringBuilder currentLine = new StringBuilder();
            float currentWidth = 0f;
            float spaceWidth = font.MeasureString(" ").X;

            foreach (string word in words)
            {
                Vector2 wordSize = font.MeasureString(word);
                
                // Handle extremely long single words
                if (wordSize.X > maxLineWidth)
                {
                    if (currentLine.Length > 0)
                    {
                        lines.Add(currentLine.ToString());
                        currentLine.Clear();
                        currentWidth = 0f;
                    }
                    string remaining = word;
                    while (remaining.Length > 0)
                    {
                        int extractCount = 1;
                        for (int i = 1; i <= remaining.Length; i++)
                        {
                            if (font.MeasureString(remaining.Substring(0, i)).X > maxLineWidth)
                            {
                                extractCount = i - 1;
                                break;
                            }
                            extractCount = i;
                        }
                        if (extractCount < 1) extractCount = 1;
                        lines.Add(remaining.Substring(0, extractCount));
                        remaining = remaining.Substring(extractCount);
                    }
                    continue;
                }

                // Normal wrapping
                if (currentWidth + wordSize.X < maxLineWidth)
                {
                    if (currentLine.Length > 0)
                    {
                        currentLine.Append(" ");
                        currentWidth += spaceWidth;
                    }
                    currentLine.Append(word);
                    currentWidth += wordSize.X;
                }
                else
                {
                    lines.Add(currentLine.ToString());
                    currentLine.Clear();
                    currentLine.Append(word);
                    currentWidth = wordSize.X;
                }
            }
            if (currentLine.Length > 0)
                lines.Add(currentLine.ToString());
            return lines;
        }
    }
}