using DeluxeJournal.Events;
using DeluxeJournal.src.Events;
using DeluxeJournal.Tasks;
using DeluxeJournal.Util;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace DeluxeJournal.Framework.Tasks
{
    internal class BlacksmithTask : TaskBase
    {
        public class Factory : DeluxeJournal.Tasks.TaskFactory
        {
            private Item? _item = null;

            [TaskParameter("tool")]
            public Item? Item
            {
                get
                {
                    return _item;
                }

                set
                {
                    if (value is Tool tool)
                    {
                        byte upgradeLevel = (byte)Math.Min(Tool.iridium, ToolHelper.GetToolUpgradeLevelForPlayer(tool.BaseName, Game1.player));
                        _item = ToolHelper.GetToolUpgrade(tool.ItemId, upgradeLevel);
                        
                    }
                    else
                    {
                        _item = null;
                    }
                }
            }

            public override Item? SmartIconItem()
            {
                return Item;
            }

            public override void Initialize(ITask task, ITranslationHelper translation)
            {
                Item = ItemRegistry.Create(task.TargetIndex);
            }

            public override void Initialize(ITask task, ITranslationHelper translation, bool copyTaskState)
            {
                if (copyTaskState)
                {
                    _item = ItemRegistry.Create(task.TargetIndex);
                }
                else
                {
                    Initialize(task, translation);
                }
            }

            public override ITask? Create(string name)
            {
                if (Item is Tool tool)
                {
                    return new BlacksmithTask(name, tool);
                }

                return null;
            }
        }

        /// <summary>Serialization constructor.</summary>
        public BlacksmithTask() : base(TaskTypes.Blacksmith)
        {
        }

        public BlacksmithTask(string name, Tool tool) : base(TaskTypes.Blacksmith, name)
        {
            TargetDisplayName = tool.DisplayName;
            TargetName = tool.BaseName;
            TargetIndex = tool.ItemId;
            Variant = tool.UpgradeLevel;
            MaxCount = 2;

            Validate();
        }

        public override void Validate()
        {
            if (CanUpdate())
            {
                Tool upgraded = Game1.player.toolBeingUpgraded.Value;
                Count = (upgraded != null && upgraded.BaseName == TargetName) ? 1 : 0;
            }
        }

        public override bool ShouldShowCustomStatus()
        {
            return !Complete;
        }

        public override string GetCustomStatusKey()
        {
            if (Count == 0)
            {
                return "ui.tasks.status.deliver";
            }
            else if (Game1.player.daysLeftForToolUpgrade.Value > 0)
            {
                return "ui.tasks.status.upgrading";
            }
            else
            {
                return "ui.tasks.status.ready";
            }
        }

        public override int GetPrice()
        {
            return Count > 0 ? 0 : Game1.toolData[TargetIndex].SalePrice;
        }

        public override void EventSubscribe(ITaskEvents events)
        {
            events.SalablePurchased += OnSalablePurchased;
            events.ModEvents.Player.InventoryChanged += OnInventoryChanged;
            events.ToolClaimed += OnToolClaimed;
        }

        public override void EventUnsubscribe(ITaskEvents events)
        {
            events.SalablePurchased -= OnSalablePurchased;
            events.ModEvents.Player.InventoryChanged -= OnInventoryChanged;
            events.ToolClaimed -= OnToolClaimed;
        }

        private void OnSalablePurchased(object? sender, SalablePurchasedEventArgs e)
        {
            if (CanUpdate() && IsTaskOwner(e.Player) && Count == 0)
            {
                if (e.Salable is Tool tool && tool.BaseName == TargetName)
                {
                    Count = 1;
                }
            }
        }

        private void OnInventoryChanged(object? sender, InventoryChangedEventArgs e)
        {
            if (CanUpdate() && IsTaskOwner(e.Player) && e.Player.toolBeingUpgraded.Value == null && Count == 1)
            {
                foreach (Item item in e.Added)
                {
                    if (item is Tool tool && tool.BaseName == TargetName)
                    {
                        Count = MaxCount;
                        MarkAsCompleted();
                        break;
                    }
                }
            }
        }

        private void OnToolClaimed(object? sender, ToolClaimedEventArgs e)
        {
            if (CanUpdate() && IsTaskOwner(e.Player) && e.Player.toolBeingUpgraded.Value == null && Count == 1)
            {
                if (e.Tool is Tool tool && tool.BaseName == TargetName)
                {
                    Count = MaxCount;
                    MarkAsCompleted();
                }
            }
        }
    }
}
