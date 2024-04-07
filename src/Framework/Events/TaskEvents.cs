﻿using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;
using DeluxeJournal.Events;
using DeluxeJournal.Util;
using DeluxeJournal.src.Events;

namespace DeluxeJournal.Framework.Events
{
    internal class TaskEvents : ITaskEvents
    {
        public event EventHandler<ItemReceivedEventArgs> ItemCollected
        {
            add => EventManager.ItemCollected.Add(value);
            remove => EventManager.ItemCollected.Remove(value);
        }

        public event EventHandler<ItemReceivedEventArgs> ItemCrafted
        {
            add => EventManager.ItemCrafted.Add(value);
            remove => EventManager.ItemCrafted.Remove(value);
        }

        public event EventHandler<GiftEventArgs> ItemGifted
        {
            add => EventManager.ItemGifted.Add(value);
            remove => EventManager.ItemGifted.Remove(value);
        }

        public event EventHandler<SalablePurchasedEventArgs> SalablePurchased
        {
            add => EventManager.SalablePurchased.Add(value);
            remove => EventManager.SalablePurchased.Remove(value);
        }

        public event EventHandler<SalableSoldEventArgs> SalableSold
        {
            add => EventManager.SalableSold.Add(value);
            remove => EventManager.SalableSold.Remove(value);
        }

        public event EventHandler<ToolClaimedEventArgs> ToolClaimed
        {
            add => EventManager.ToolClaimed.Add(value);
            remove => EventManager.ToolClaimed.Remove(value);
        }

        public event EventHandler<BuildingConstructedEventArgs> BuildingConstructed
        {
            add => EventManager.BuildingConstructed.Add(value);
            remove => EventManager.BuildingConstructed.Remove(value);
        }

        public IModEvents ModEvents => EventManager.ModEvents;

        private EventManager EventManager { get; }

        public TaskEvents(EventManager eventManager)
        {
            EventManager = eventManager;

            ModEvents.Display.MenuChanged += OnMenuChanged;
            ModEvents.GameLoop.DayEnding += OnDayEnding;
            ModEvents.World.BuildingListChanged += OnBuildingListChanged;
        }

        // Run with low priority to ensure completed sell tasks remain for the next day
        [EventPriority(EventPriority.Low)]
        private void OnDayEnding(object? sender, DayEndingEventArgs e)
        {
            foreach (Item item in Game1.getFarm().getShippingBin(Game1.player))
            {
                if (item != null)
                {
                    OnSell(item);
                }
            }
        }

        [EventPriority(EventPriority.Low)]
        private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
        {
            if (Game1.activeClickableMenu is ShopMenu shopMenu)
            {
                ShopHelper.AttachPurchaseCallback(shopMenu, OnPurchase);
                ShopHelper.AttachQuietSellCallback(shopMenu, OnSell);
            }
        }

        private bool OnPurchase(ISalable salable, Farmer player, int amount)
        {
            EventManager.SalablePurchased.Raise(player, new SalablePurchasedEventArgs(player, salable, amount));
            return false;
        }

        private bool OnSell(ISalable salable)
        {
            Farmer player = Game1.player;
            EventManager.SalableSold.Raise(player, new SalableSoldEventArgs(player, salable, salable.Stack));
            return false;
        }

        private void OnBuildingListChanged(object? sender, BuildingListChangedEventArgs e)
        {
            if (DeluxeJournalMod.IsMainScreen)
            {
                foreach (Building building in e.Added)
                {
                    EventManager.BuildingConstructed.Raise(sender, new BuildingConstructedEventArgs(e.Location, building, false));
                }
            }
        }
    }
}
