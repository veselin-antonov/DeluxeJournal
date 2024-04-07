﻿using StardewModdingAPI;
using StardewValley;
using DeluxeJournal.Events;
using DeluxeJournal.Tasks;
using DeluxeJournal.Util;

using Constraint = DeluxeJournal.Tasks.TaskParameter.Constraint;

namespace DeluxeJournal.Framework.Tasks
{
    internal class CraftTask : TaskBase
    {
        public class Factory : DeluxeJournal.Tasks.TaskFactory
        {
            [TaskParameter("item", Constraints = Constraint.Craftable | Constraint.NotNull)]
            public Item? Item { get; set; }

            [TaskParameter("count", Tag = "count", Constraints = Constraint.GT0)]
            public int Count { get; set; } = 1;

            public override Item? SmartIconItem()
            {
                return Item;
            }

            public override void Initialize(ITask task, ITranslationHelper translation)
            {
                Item = LocalizedObjects.Instance.GetItem(task.TargetName);
                Count = task.MaxCount;
            }

            public override ITask? Create(string name)
            {
                if (Item is SObject item)
                {
                    return new CraftTask(name, item, Count);
                }

                return null;
            }
        }

        /// <summary>Serialization constructor.</summary>
        public CraftTask() : base(TaskTypes.Craft)
        {
        }

        public CraftTask(string name, SObject item, int count) : base(TaskTypes.Craft, name)
        {
            TargetName = item.DisplayName;
            TargetIndex = item.QualifiedItemId;
            Variant = item.bigCraftable.Value ? 1 : 0;
            MaxCount = count;
        }

        public override bool ShouldShowProgress()
        {
            return true;
        }

        public bool IsBigCraftable()
        {
            return Variant == 1;
        }

        public override void EventSubscribe(ITaskEvents events)
        {
            events.ItemCrafted += OnItemCrafted;
        }

        public override void EventUnsubscribe(ITaskEvents events)
        {
            events.ItemCrafted -= OnItemCrafted;
        }

        private void OnItemCrafted(object? sender, ItemReceivedEventArgs e)
        {
            if (CanUpdate() && IsTaskOwner(e.Player) && IsBigCraftable() == e.Item.bigCraftable.Value && e.Item.QualifiedItemId.Equals(TargetIndex))
            {
                IncrementCount(e.Count);
            }
        }
    }
}
