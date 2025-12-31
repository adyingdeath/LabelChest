using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using LabelChest.Managers;
using LabelChest.UI;
using LabelChest.Rendering;
using LabelChest.Utils;

namespace LabelChest {
    public class ModEntry : Mod {
        public const string MAIN_TEXTURE_PATH = "adyingdeath/LabelChest/Texture";
        public static ModConfig Config { get; private set; } = null!;
        private MenuLabelButton _menuButton = null!;
        private ConfigButton _configButton = null!;
        private ChestMenuButtonManager _chestMenuButtonManager = null!;
        public static WorldLabelRenderer WorldLabelRenderer { get; private set; } = null!;

        public override void Entry(IModHelper helper) {
            I18n.Init(helper.Translation);

            // Load configuration
            try {
                // Try to read configuration
                Config = Helper.ReadConfig<ModConfig>();
            } catch (Exception ex) {
                // Error message
                Monitor.Log($"Error reading configuration file 'config.json'. Written a new one to the Mod folder.\n{ex}", LogLevel.Error);
                // Use default configuration
                Config = new ModConfig();
                // Save
                Helper.WriteConfig(Config);
            }

            // Initialize managers
            _menuButton = new MenuLabelButton(OnLabelButtonClicked);
            _configButton = new ConfigButton(OnConfigButtonClicked);
            _chestMenuButtonManager = new ChestMenuButtonManager(_menuButton, _configButton);
            WorldLabelRenderer = new WorldLabelRenderer();

            // Register events
            helper.Events.Display.RenderedWorld += OnRenderedWorld;
            helper.Events.Display.RenderedActiveMenu += OnRenderedActiveMenu;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.Content.AssetRequested += OnAssetRequested;
        }

        public static void Log(string message) {
            Game1.chatBox.addMessage(message, Color.White);
        }

        private void OnAssetRequested(object? sender, AssetRequestedEventArgs e) {
            // The main texture png asset for this mod.
            if (e.NameWithoutLocale.IsEquivalentTo(MAIN_TEXTURE_PATH)) {
                e.LoadFrom(() => {
                    // The asset will be cached.
                    string path = Path.Combine(Helper.DirectoryPath, "assets", "texture.png");
                    using var stream = File.OpenRead(path);
                    return Texture2D.FromStream(Game1.graphics.GraphicsDevice, stream);
                }, AssetLoadPriority.Exclusive);
            }
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e) {
            var (chest, menu) = GetCurrentChestMenu();
            if (chest == null || menu == null)
                return;

            /* Check if label button was clicked via mouse or controller use 
            tool button and action button.
            These two methods can detect click without knowing how the game binds
            the keys.
            In default, the left-click is "UseToolButton" and right-click is
            "ActionButton" for mouse.
            */
            if (e.Button.IsUseToolButton() || e.Button.IsActionButton()) {
                // Fix for UI Scale: Convert screen pixels to UI coordinates
                Vector2 cursorPosition = Utility.ModifyCoordinatesForUIScale(e.Cursor.ScreenPixels);

                // Handle config button click first (since it's to the right)
                if (_configButton.HandleClick(menu, cursorPosition)) {
                    return;
                }

                // Then handle label button click
                if (_menuButton.HandleClick(menu, chest, cursorPosition)) {
                    return;
                }
            }
        }

        private void OnLabelButtonClicked(string currentLabel) {
            var (chest, _) = GetCurrentChestMenu();
            if (chest == null)
                return;

            // Open ChestNamingMenu with translation dictionary
            Game1.activeClickableMenu = new ChestNamingMenu(
                currentLabel,
                (label) => {
                    ChestLabelManager.SetLabel(chest, label);
                }
            );
        }

        private void OnConfigButtonClicked() {
            Helper.Input.Suppress(SButton.MouseLeft);
            Game1.activeClickableMenu = new ConfigMenu();
        }

        private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e) {
            var (chest, menu) = GetCurrentChestMenu();
            if (chest == null || menu == null)
                return;

            // Add buttons to menu components if not already added
            if (!menu.allClickableComponents.Contains(_menuButton)) {
                menu.allClickableComponents.Add(_menuButton);
            }
            if (!menu.allClickableComponents.Contains(_configButton)) {
                menu.allClickableComponents.Add(_configButton);
            }

            // Update bounds for both buttons
            (_menuButton.bounds, _configButton.bounds) = _chestMenuButtonManager.GetButtonGroupBounds(menu);

            // Setup neighbors for proper controller navigation
            _chestMenuButtonManager.SetupNeighbors();
            _chestMenuButtonManager.UpdateOtherComponentsNeighbors(menu);

            // Draw both buttons
            _menuButton.Draw(e.SpriteBatch, menu, chest);
            _configButton.Draw(e.SpriteBatch, menu);
        }

        private void OnRenderedWorld(object? sender, RenderedWorldEventArgs e) {
            if (!Context.IsWorldReady)
                return;

            // Restart SpriteBatch with our settings for anti-aliasing.
            SpriteBatchSwitcher.SwitchAntiAliasing(e.SpriteBatch);

            foreach (var pair in Game1.currentLocation.Objects.Pairs) {
                if (pair.Value is Chest chest && ChestLabelManager.HasLabel(chest)) {
                    string labelText = ChestLabelManager.GetLabel(chest);
                    if (!string.IsNullOrWhiteSpace(labelText)) {
                        WorldLabelRenderer.DrawLabelWithTile(e.SpriteBatch, pair.Key, labelText, chest);
                    }
                }
            }

            SpriteBatchSwitcher.SwitchDefault(e.SpriteBatch);
        }

        // --- Helpers ---

        private static (Chest?, ItemGrabMenu?) GetCurrentChestMenu() {
            if (Game1.activeClickableMenu is ItemGrabMenu menu && menu.context is Chest chest) {
                return (chest, menu);
            }
            return (null, null);
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                //CacheManager?.Dispose();
                // Save config
                Helper.WriteConfig<ModConfig>(Config);
            }
            base.Dispose(disposing);
        }
    }
}
