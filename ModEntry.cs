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

namespace LabelChest {
    public class ModEntry : Mod {
        public const string MAIN_TEXTURE_PATH = "adyingdeath/LabelChest/Texture";
        public static ModConfig Config { get; private set; } = null!;
        public static LabelCacheManager CacheManager { get; private set; } = null!;
        private MenuLabelButton _menuButton = null!;
        private ConfigButton _configButton = null!;
        private ChestMenuButtonManager _chestMenuButtonManager = null!;
        public static WorldLabelRenderer WorldLabelRenderer { get; private set; } = null!;

        public override void Entry(IModHelper helper) {
            // Load configuration
            Config = Helper.ReadConfig<ModConfig>();

            // Initialize managers
            CacheManager = new LabelCacheManager(helper.Translation.LocaleEnum);
            _menuButton = new MenuLabelButton(Helper.Translation, OnLabelButtonClicked);
            _configButton = new ConfigButton(Helper.Translation, OnConfigButtonClicked);
            _chestMenuButtonManager = new ChestMenuButtonManager(_menuButton, _configButton);
            WorldLabelRenderer = new WorldLabelRenderer(CacheManager);

            // Register events
            helper.Events.Display.RenderedWorld += OnRenderedWorld;
            helper.Events.Display.RenderedActiveMenu += OnRenderedActiveMenu;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.Content.AssetRequested += OnAssetRequested;

            // Cleanup cache on load/exit to free VRAM
            helper.Events.GameLoop.SaveLoaded += OnCleanupCache;
            helper.Events.GameLoop.ReturnedToTitle += OnCleanupCache;
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

        private void OnCleanupCache(object? sender, EventArgs e) {
            CacheManager.ClearCache();
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

            // Create translation dictionary for the naming menu
            ChestNamingMenuTranslation namingTranslations = new(
                Helper.Translation.Get("set-label-title"),
                Helper.Translation.Get("cancel-button"),
                Helper.Translation.Get("ok-button")
            );

            // Open ChestNamingMenu with translation dictionary
            Game1.activeClickableMenu = new ChestNamingMenu(
                namingTranslations,
                currentLabel,
                (label) => {
                    ChestLabelManager.SetLabel(chest, label);
                }
            );
        }

        private void OnConfigButtonClicked() {
            Helper.Input.Suppress(SButton.MouseLeft);
            ConfigMenuTranslation translation = new(
                Helper.Translation.Get("config-menu.font-size"),
                Helper.Translation.Get("config-menu.text-color-type"),
                Helper.Translation.Get("config-menu.text-color-type.fixed"),
                Helper.Translation.Get("config-menu.text-color-type.inverted"),
                Helper.Translation.Get("config-menu.text-color-type.follow-box"),
                Helper.Translation.Get("config-menu.text-color.red"),
                Helper.Translation.Get("config-menu.text-color.green"),
                Helper.Translation.Get("config-menu.text-color.blue"),
                Helper.Translation.Get("config-menu.button.config"),
                Helper.Translation.Get("config-menu.title")
            );
            Game1.activeClickableMenu = new ConfigMenu(translation);
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

        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e) {
            CacheManager.ProcessPendingLabels(Game1.graphics.GraphicsDevice);
        }

        private void OnRenderedWorld(object? sender, RenderedWorldEventArgs e) {
            if (!Context.IsWorldReady)
                return;

            foreach (var pair in Game1.currentLocation.Objects.Pairs) {
                if (pair.Value is Chest chest && ChestLabelManager.HasLabel(chest)) {
                    string labelText = ChestLabelManager.GetLabel(chest);
                    if (!string.IsNullOrWhiteSpace(labelText)) {
                        WorldLabelRenderer.DrawLabelWithTile(e.SpriteBatch, pair.Key, labelText);
                    }
                }
            }
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
                CacheManager?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
