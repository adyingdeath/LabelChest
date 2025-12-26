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
        private LabelCacheManager _cacheManager = null!;
        private MenuLabelButton _menuButton = null!;
        private WorldLabelRenderer _worldRenderer = null!;

        public override void Entry(IModHelper helper) {
            // Initialize managers
            _cacheManager = new LabelCacheManager(helper.Translation.LocaleEnum);
            _menuButton = new MenuLabelButton(Helper.Translation, OnLabelButtonClicked);
            _worldRenderer = new WorldLabelRenderer(_cacheManager);

            // Register events
            helper.Events.Display.RenderedWorld += OnRenderedWorld;
            helper.Events.Display.RenderedActiveMenu += OnRenderedActiveMenu;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;

            // Cleanup cache on load/exit to free VRAM
            helper.Events.GameLoop.SaveLoaded += OnCleanupCache;
            helper.Events.GameLoop.ReturnedToTitle += OnCleanupCache;
        }

        private void OnCleanupCache(object? sender, EventArgs e) {
            _cacheManager.ClearCache();
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
                _menuButton.HandleClick(menu, chest, cursorPosition);
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

        private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e) {
            var (chest, menu) = GetCurrentChestMenu();
            if (chest == null || menu == null)
                return;

            // Add button to menu components if not already added
            if (!menu.allClickableComponents.Contains(_menuButton)) {
                menu.allClickableComponents.Add(_menuButton);

                // Setup neighbors for proper controller navigation
                _menuButton.SetupNeighbors(menu);
            }

            _menuButton.Draw(e.SpriteBatch, menu, chest);
        }

        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e) {
            _cacheManager.ProcessPendingLabels(Game1.graphics.GraphicsDevice);
        }

        private void OnRenderedWorld(object? sender, RenderedWorldEventArgs e) {
            if (!Context.IsWorldReady)
                return;

            foreach (var pair in Game1.currentLocation.Objects.Pairs) {
                if (pair.Value is Chest chest && ChestLabelManager.HasLabel(chest)) {
                    string labelText = ChestLabelManager.GetLabel(chest);
                    if (!string.IsNullOrWhiteSpace(labelText)) {
                        _worldRenderer.DrawLabel(e.SpriteBatch, pair.Key, labelText);
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
                _cacheManager?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}