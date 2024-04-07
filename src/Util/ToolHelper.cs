using StardewValley;
using StardewValley.GameData.Tools;
using StardewValley.Tools;
using System.Numerics;

namespace DeluxeJournal.Util
{
    public static class ToolHelper
    {
        private static readonly IDictionary<string, IDictionary<int, string>> toolUpgrades = InitToolUpgrades();
        private static IDictionary<string, IDictionary<int, string>> InitToolUpgrades()
        {
            Dictionary<string, IDictionary<int, string>> map = [];
            foreach (var (toolID, toolData) in Game1.toolData)
            {
                string? initialTool = GetToolFromUpgraded(toolID);
                if (initialTool == null)
                {
                    continue;
                }
                if (!map.ContainsKey(initialTool))
                {
                    map[initialTool] = new Dictionary<int, string>();
                }
                map[initialTool][toolData.UpgradeLevel] = toolID;
            }
            return map;
        }

        public static string? GetToolFromUpgraded(string toolID)
        {
            if (ItemRegistry.IsQualifiedItemId(toolID))
            {
                toolID = toolID[3..];
            }
            if (!Game1.toolData.ContainsKey(toolID))
            {
                return ItemRegistry.GetDataOrErrorItem(null).ItemId;
            }
            if (toolID.Contains("Trash"))
            {
                return "CopperTrashCan";
            }
            if (Game1.toolData[toolID].ConventionalUpgradeFrom == null)
            {
                return toolID;
            }
            return GetToolFromUpgraded(Game1.toolData[toolID].ConventionalUpgradeFrom);
        }

        public static Tool? GetToolUpgrade(string toolID, int upgradeLevel)
        {
            toolID = GetToolFromUpgraded(toolID);
            if (toolID != null && toolUpgrades.ContainsKey(toolID) && toolUpgrades[toolID].ContainsKey(upgradeLevel))
            {
                return ItemRegistry.Create<Tool>(toolUpgrades[toolID][upgradeLevel]);
            }
            return null;
        }

        /// <summary>Extract a ToolDescription from a Tool.</summary>
        /*public static Vector<byte> GetToolDescription(Tool tool)
        {
            return tool.GetType().Name switch
            {
                nameof(Axe) => new Vector<byte>(new byte[2] { 0, (byte)tool.UpgradeLevel }),
                nameof(Hoe) => new Vector<byte>(new byte[2] { 1, (byte)tool.UpgradeLevel }),
                nameof(Pickaxe) => new Vector<byte>(new byte[2] { 2, (byte)tool.UpgradeLevel }),
                nameof(WateringCan) => new Vector<byte>(new byte[2] { 3, (byte)tool.UpgradeLevel }),
                nameof(FishingRod) => new Vector<byte>(new byte[2] { 4, (byte)tool.UpgradeLevel }),
                nameof(Pan) => new Vector<byte>(new byte[2] { 5, (byte)tool.UpgradeLevel }),
                nameof(Shears) => new Vector<byte>(new byte[2] { 6, (byte)tool.UpgradeLevel }),
                nameof(MilkPail) => new Vector<byte>(new byte[2] { 7, (byte)tool.UpgradeLevel }),
                nameof(Wand) => new Vector<byte>(new byte[2] { 8, (byte)tool.UpgradeLevel }),
                _ => new Vector<byte>(new byte[2] { 0, 0 }),
            };
        }*/

        /// <summary>Create a Tool from a ToolDescription.</summary>
        public static Tool? GetToolFromDescription(string index, int upgradeLevel)
    {
        string upgradeId = Game1.toolData.FirstOrDefault(pair => pair.Value.Name.Contains(index) && pair.Value.UpgradeLevel == upgradeLevel).Key;
        return ItemRegistry.Create<Tool>(upgradeId);
    }

    /// <summary>Get the upgrade level for a tool owned by a given player.</summary>
    /// <remarks>
    /// "Ownership" here is defined as the last player to use a tool or  the tool in a
    /// player's inventory. Enabling the guess flag prioritizes unused tools to fallback on
    /// and always grabs the tool of the lowest level when breaking ties.
    /// </remarks>
    /// <param name="name">The base name of the tool.</param>
    /// <param name="player">The player that owns the tool.</param>
    /// <param name="guess">Make a guess if a definitive owner could not be found.</param>
    /// <returns>The upgrade level for the tool, or the base level if not found.</returns>
    public static int GetToolUpgradeLevelForPlayer(string name, Farmer player, bool guess = true)
        {
            int level = -1;
            int fallback = 0;
            bool foundUnownedFallback = false;

            if (player.toolBeingUpgraded.Value is Tool upgraded && upgraded.BaseName == name)
            {
                return upgraded.UpgradeLevel;
            }
            else if (player.getToolFromName(name) is Tool held)
            {
                return held.UpgradeLevel + 1;
            }
            else if (name.Contains("Trash"))
            {
                return player.trashCanLevel + 1;
            }


            Utility.ForEachItem(searchForTool);

            bool searchForTool(Item item)
            {
                if (item is Tool tool && tool.BaseName == name)
                {
                    Farmer lastPlayer = tool.getLastFarmerToUse();

                    if (lastPlayer != null && lastPlayer.UniqueMultiplayerID == player.UniqueMultiplayerID)
                    {
                        level = tool.UpgradeLevel;
                        return false;
                    }
                    else if (!guess)
                    {
                        return true;
                    }
                    else if (lastPlayer == null && (!foundUnownedFallback || (tool.UpgradeLevel < fallback)))
                    {
                        fallback = tool.UpgradeLevel;
                        foundUnownedFallback = true;
                    }
                    else if (!foundUnownedFallback && (tool.UpgradeLevel < fallback))
                    {
                        fallback = tool.UpgradeLevel;
                    }
                }

                return true;
            }

            return fallback + 1;
        }

        /// <summary>Get the price for a Tool upgrade (at the blacksmith shop) for the given upgrade level.</summary>
        public static int PriceForToolUpgradeLevel(int level)
        {
            return level switch
            {
                1 => 2000,
                2 => 5000,
                3 => 10000,
                4 => 25000,
                _ => 2000,
            };
        }
    }
}
