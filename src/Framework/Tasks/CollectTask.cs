﻿using DeluxeJournal.Events;
using DeluxeJournal.Tasks;
using DeluxeJournal.Util;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace DeluxeJournal.Framework.Tasks
{
    internal class CollectTask : TaskBase
    {
        public class Factory : DeluxeJournal.Tasks.TaskFactory
        {
            private Item? _item = null;

            [TaskParameter("item")]
            public Item? Item
            {
                get
                {
                    return _item;
                }

                set
                {
                    if (value is SObject item && item is not Furniture && !item.bigCraftable.Value)
                    {
                        _item = item;
                    }
                    else
                    {
                        _item = null;
                    }
                }
            }

            [TaskParameter("count", Tag = "count", Constraints = TaskParameter.Constraint.GT0)]
            public int Count { get; set; } = 1;

            public override Item? SmartIconItem()
            {
                return Item;
            }

            public override void Initialize(ITask task, ITranslationHelper translation)
            {
                Item = LocalizedObjects.Instance.GetItem(task.TargetDisplayName);
                Count = task.MaxCount;
            }

            public override ITask? Create(string name)
            {
                return Item != null ? new CollectTask(name, Item, Count) : null;
            }
        }

        /// <summary>Serialization constructor.</summary>
        public CollectTask() : base(TaskTypes.Collect)
        {
        }

        public CollectTask(string name, Item item, int count) : base(TaskTypes.Collect, name)
        {
            TargetDisplayName = item.DisplayName;
            TargetIndex = item.QualifiedItemId;
            MaxCount = count;
        }

        public override bool ShouldShowProgress()
        {
            return true;
        }

        public override void EventSubscribe(ITaskEvents events)
        {
            events.ItemCollected += OnItemCollected;
        }

        public override void EventUnsubscribe(ITaskEvents events)
        {
            events.ItemCollected -= OnItemCollected;
        }

        private void OnItemCollected(object? sender, ItemReceivedEventArgs e)
        {
            if (CanUpdate() && IsTaskOwner(e.Player) && e.Item.QualifiedItemId.Equals(TargetIndex))
            {
                IncrementCount(e.Count);
            }
        }
    }
}
