using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace LabelChest.UI.Menus;

public class LOptionsPage : IClickableMenu
{
    public const int itemsPerPage = 7;

    private string hoverText = "";

    public List<ClickableComponent> optionSlots = new List<ClickableComponent>();

    public int currentItemIndex;

    private ClickableTextureComponent upArrow;

    private ClickableTextureComponent downArrow;

    private ClickableTextureComponent scrollBar;

    private bool scrolling;

    public List<LComponent> options = new List<LComponent>();

    private Rectangle scrollBarRunner;

    internal static int _lastSelectedIndex;

    internal static int _lastCurrentItemIndex;

    public int lastRebindTick = -1;

    private int optionsSlotHeld = -1;

    public LOptionsPage(int x, int y, int width, int height)
        : base(x, y, width, height)
    {
        upArrow = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 16, yPositionOnScreen + 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f);
        downArrow = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 16, yPositionOnScreen + height - 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f);
        scrollBar = new ClickableTextureComponent(new Rectangle(upArrow.bounds.X + 12, upArrow.bounds.Y + upArrow.bounds.Height + 4, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f);
        scrollBarRunner = new Rectangle(scrollBar.bounds.X, upArrow.bounds.Y + upArrow.bounds.Height + 4, scrollBar.bounds.Width, height - 128 - upArrow.bounds.Height - 8);
        for (int i = 0; i < 7; i++)
        {
            optionSlots.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + 16, yPositionOnScreen + 80 + 4 + i * ((height - 128) / 7) + 16, width - 32, (height - 128) / 7 + 4), i.ToString() ?? "")
            {
                myID = i,
                downNeighborID = ((i < 6) ? (i + 1) : (-7777)),
                upNeighborID = ((i > 0) ? (i - 1) : (-7777)),
                fullyImmutable = true
            });
        }
    }

    public override bool readyToClose()
    {
        if (lastRebindTick == Game1.ticks)
        {
            return false;
        }

        return base.readyToClose();
    }

    private void waitForServerConnection(Action onConnection)
    {
        IClickableMenu thisMenu;
        if (Game1.server != null)
        {
            if (Game1.server.connected())
            {
                onConnection();
                return;
            }

            thisMenu = Game1.activeClickableMenu;
            Game1.activeClickableMenu = new ServerConnectionDialog(OnConfirm, OnClose);
        }

        void OnClose(Farmer who)
        {
            Game1.activeClickableMenu = thisMenu;
            thisMenu.snapCursorToCurrentSnappedComponent();
        }

        void OnConfirm(Farmer who)
        {
            OnClose(who);
            onConnection();
        }
    }

    private void offerInvite()
    {
        waitForServerConnection(Game1.server.offerInvite);
    }

    private void showInviteCode()
    {
        IClickableMenu thisMenu = Game1.activeClickableMenu;
        waitForServerConnection(delegate
        {
            Game1.activeClickableMenu = new InviteCodeDialog(Game1.server.getInviteCode(), OnClose);
        });
        void OnClose(Farmer who)
        {
            Game1.activeClickableMenu = thisMenu;
            thisMenu.snapCursorToCurrentSnappedComponent();
        }
    }

    public override void snapToDefaultClickableComponent()
    {
        base.snapToDefaultClickableComponent();
        currentlySnappedComponent = getComponentWithID(1);
        snapCursorToCurrentSnappedComponent();
    }

    public override void applyMovementKey(int direction)
    {
        if (!IsDropdownActive())
        {
            base.applyMovementKey(direction);
        }
    }

    protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
    {
        base.customSnapBehavior(direction, oldRegion, oldID);
        if (oldID == 6 && direction == 2 && currentItemIndex < Math.Max(0, options.Count - 7))
        {
            downArrowPressed();
            Game1.playSound("shiny4");
        }
        else
        {
            if (oldID != 0 || direction != 0)
            {
                return;
            }

            if (currentItemIndex > 0)
            {
                upArrowPressed();
                Game1.playSound("shiny4");
                return;
            }

            currentlySnappedComponent = getComponentWithID(12348);
            if (currentlySnappedComponent != null)
            {
                currentlySnappedComponent.downNeighborID = 0;
            }

            snapCursorToCurrentSnappedComponent();
        }
    }

    private void setScrollBarToCurrentIndex()
    {
        if (options.Count > 0)
        {
            scrollBar.bounds.Y = scrollBarRunner.Height / Math.Max(1, options.Count - 7 + 1) * currentItemIndex + upArrow.bounds.Bottom + 4;
            if (scrollBar.bounds.Y > downArrow.bounds.Y - scrollBar.bounds.Height - 4)
            {
                scrollBar.bounds.Y = downArrow.bounds.Y - scrollBar.bounds.Height - 4;
            }
        }
    }

    public override void snapCursorToCurrentSnappedComponent()
    {
        if (currentlySnappedComponent != null && currentlySnappedComponent.myID < options.Count)
        {
            LComponent optionsElement = options[currentlySnappedComponent.myID + currentItemIndex];
            if (!(optionsElement is LSelect optionsDropDown))
            {
                if (!(optionsElement is OptionsPlusMinusButton))
                {
                    if (optionsElement is OptionsInputListener)
                    {
                        Game1.setMousePosition(currentlySnappedComponent.bounds.Right - 48, currentlySnappedComponent.bounds.Center.Y - 12);
                    }
                    else
                    {
                        Game1.setMousePosition(currentlySnappedComponent.bounds.Left + 48, currentlySnappedComponent.bounds.Center.Y - 12);
                    }
                }
                else
                {
                    Game1.setMousePosition(currentlySnappedComponent.bounds.Left + 64, currentlySnappedComponent.bounds.Center.Y + 4);
                }
            }
            else
            {
                Game1.setMousePosition(currentlySnappedComponent.bounds.Left + optionsDropDown.bounds.Right - 32, currentlySnappedComponent.bounds.Center.Y - 4);
            }
        }
        else if (currentlySnappedComponent != null)
        {
            base.snapCursorToCurrentSnappedComponent();
        }
    }

    public override void leftClickHeld(int x, int y)
    {
        if (GameMenu.forcePreventClose)
        {
            return;
        }

        base.leftClickHeld(x, y);
        if (scrolling)
        {
            int y2 = scrollBar.bounds.Y;
            scrollBar.bounds.Y = Math.Min(yPositionOnScreen + height - 64 - 12 - scrollBar.bounds.Height, Math.Max(y, yPositionOnScreen + upArrow.bounds.Height + 20));
            float num = (float)(y - scrollBarRunner.Y) / (float)scrollBarRunner.Height;
            currentItemIndex = Math.Min(options.Count - 7, Math.Max(0, (int)((float)options.Count * num)));
            setScrollBarToCurrentIndex();
            if (y2 != scrollBar.bounds.Y)
            {
                Game1.playSound("shiny4");
            }
        }
        else if (optionsSlotHeld != -1 && optionsSlotHeld + currentItemIndex < options.Count)
        {
            options[currentItemIndex + optionsSlotHeld].leftClickHeld(x - optionSlots[optionsSlotHeld].bounds.X, y - optionSlots[optionsSlotHeld].bounds.Y);
        }
    }

    public override void setCurrentlySnappedComponentTo(int id)
    {
        currentlySnappedComponent = getComponentWithID(id);
        snapCursorToCurrentSnappedComponent();
    }

    public override void receiveKeyPress(Keys key)
    {
        if ((optionsSlotHeld != -1 && optionsSlotHeld + currentItemIndex < options.Count) || (Game1.options.snappyMenus && Game1.options.gamepadControls))
        {
            if (currentlySnappedComponent != null && Game1.options.snappyMenus && Game1.options.gamepadControls && options.Count > currentItemIndex + currentlySnappedComponent.myID && currentItemIndex + currentlySnappedComponent.myID >= 0)
            {
                options[currentItemIndex + currentlySnappedComponent.myID].receiveKeyPress(key);
            }
            else if (options.Count > currentItemIndex + optionsSlotHeld && currentItemIndex + optionsSlotHeld >= 0)
            {
                options[currentItemIndex + optionsSlotHeld].receiveKeyPress(key);
            }
        }

        base.receiveKeyPress(key);
    }

    public override void receiveScrollWheelAction(int direction)
    {
        if (!GameMenu.forcePreventClose && !IsDropdownActive())
        {
            base.receiveScrollWheelAction(direction);
            if (direction > 0 && currentItemIndex > 0)
            {
                upArrowPressed();
                Game1.playSound("shiny4");
            }
            else if (direction < 0 && currentItemIndex < Math.Max(0, options.Count - 7))
            {
                downArrowPressed();
                Game1.playSound("shiny4");
            }

            if (Game1.options.SnappyMenus)
            {
                snapCursorToCurrentSnappedComponent();
            }
        }
    }

    public override void releaseLeftClick(int x, int y)
    {
        if (!GameMenu.forcePreventClose)
        {
            base.releaseLeftClick(x, y);
            if (optionsSlotHeld != -1 && optionsSlotHeld + currentItemIndex < options.Count)
            {
                options[currentItemIndex + optionsSlotHeld].leftClickReleased(x - optionSlots[optionsSlotHeld].bounds.X, y - optionSlots[optionsSlotHeld].bounds.Y);
            }

            optionsSlotHeld = -1;
            scrolling = false;
        }
    }

    public bool IsDropdownActive()
    {
        if (optionsSlotHeld != -1 && optionsSlotHeld + currentItemIndex < options.Count && options[currentItemIndex + optionsSlotHeld] is LSelect)
        {
            return true;
        }

        return false;
    }

    private void downArrowPressed()
    {
        if (!IsDropdownActive())
        {
            UnsubscribeFromSelectedTextbox();
            downArrow.scale = downArrow.baseScale;
            currentItemIndex++;
            setScrollBarToCurrentIndex();
        }
    }

    public virtual void UnsubscribeFromSelectedTextbox()
    {
        if (Game1.keyboardDispatcher.Subscriber == null)
        {
            return;
        }

        foreach (LComponent option in options)
        {
            if (option is OptionsTextEntry optionsTextEntry && Game1.keyboardDispatcher.Subscriber == optionsTextEntry.textBox)
            {
                Game1.keyboardDispatcher.Subscriber = null;
                break;
            }
        }
    }

    //
    // Summary:
    //     Restore the page state when it's recreated for a window resize.
    //
    // Parameters:
    //   oldPage:
    //     The previous page instance before it was recreated.
    public void postWindowSizeChange(IClickableMenu oldPage)
    {
        if (oldPage is LOptionsPage optionsPage)
        {
            int currentlySnappedComponentTo = optionsPage.getCurrentlySnappedComponent()?.myID ?? (-1);
            int num = optionsPage.currentItemIndex;
            if (Game1.options.SnappyMenus)
            {
                Game1.activeClickableMenu.setCurrentlySnappedComponentTo(currentlySnappedComponentTo);
            }

            currentItemIndex = num;
            setScrollBarToCurrentIndex();
        }
    }

    private void upArrowPressed()
    {
        if (!IsDropdownActive())
        {
            UnsubscribeFromSelectedTextbox();
            upArrow.scale = upArrow.baseScale;
            currentItemIndex--;
            setScrollBarToCurrentIndex();
        }
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        if (GameMenu.forcePreventClose)
        {
            return;
        }

        if (downArrow.containsPoint(x, y) && currentItemIndex < Math.Max(0, options.Count - 7))
        {
            downArrowPressed();
            Game1.playSound("shwip");
        }
        else if (upArrow.containsPoint(x, y) && currentItemIndex > 0)
        {
            upArrowPressed();
            Game1.playSound("shwip");
        }
        else if (scrollBar.containsPoint(x, y))
        {
            scrolling = true;
        }
        else if (!downArrow.containsPoint(x, y) && x > xPositionOnScreen + width && x < xPositionOnScreen + width + 128 && y > yPositionOnScreen && y < yPositionOnScreen + height)
        {
            scrolling = true;
            leftClickHeld(x, y);
            releaseLeftClick(x, y);
        }

        currentItemIndex = Math.Max(0, Math.Min(options.Count - 7, currentItemIndex));
        UnsubscribeFromSelectedTextbox();
        for (int i = 0; i < optionSlots.Count; i++)
        {
            if (optionSlots[i].bounds.Contains(x, y) && currentItemIndex + i < options.Count && options[currentItemIndex + i].bounds.Contains(x - optionSlots[i].bounds.X, y - optionSlots[i].bounds.Y))
            {
                options[currentItemIndex + i].receiveLeftClick(x - optionSlots[i].bounds.X, y - optionSlots[i].bounds.Y);
                optionsSlotHeld = i;
                break;
            }
        }
    }

    public override void performHoverAction(int x, int y)
    {
        for (int i = 0; i < optionSlots.Count; i++)
        {
            if (currentItemIndex >= 0 && currentItemIndex + i < options.Count && options[currentItemIndex + i].bounds.Contains(x - optionSlots[i].bounds.X, y - optionSlots[i].bounds.Y))
            {
                Game1.SetFreeCursorDrag();
                break;
            }
        }

        if (scrollBarRunner.Contains(x, y))
        {
            Game1.SetFreeCursorDrag();
        }

        if (!GameMenu.forcePreventClose)
        {
            hoverText = "";
            upArrow.tryHover(x, y);
            downArrow.tryHover(x, y);
            scrollBar.tryHover(x, y);
        }
    }

    public override void draw(SpriteBatch b)
    {
        b.End();
        b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
        for (int i = 0; i < optionSlots.Count; i++)
        {
            if (currentItemIndex >= 0 && currentItemIndex + i < options.Count)
            {
                options[currentItemIndex + i].draw(b, optionSlots[i].bounds.X, optionSlots[i].bounds.Y, this);
            }
        }

        b.End();
        b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        if (!GameMenu.forcePreventClose)
        {
            upArrow.draw(b);
            downArrow.draw(b);
            if (options.Count > 7)
            {
                IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), scrollBarRunner.X, scrollBarRunner.Y, scrollBarRunner.Width, scrollBarRunner.Height, Color.White, 4f, drawShadow: false);
                scrollBar.draw(b);
            }
        }

        if (!hoverText.Equals(""))
        {
            IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont);
        }
    }
}