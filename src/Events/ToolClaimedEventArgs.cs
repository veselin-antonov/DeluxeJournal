using StardewValley;

namespace DeluxeJournal.src.Events
{
    public class ToolClaimedEventArgs : EventArgs
    {
        /// <summary>The player who received the item stack.</summary>
        public Farmer Player { get; }

        /// <summary>Item received.</summary>
        public Tool Tool { get; }

        public ToolClaimedEventArgs(Farmer player, Tool tool)
        {
            Player = player;
            Tool = tool;
        }
    }
}
