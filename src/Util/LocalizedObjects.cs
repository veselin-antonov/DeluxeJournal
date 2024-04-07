using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.Characters;
using StardewValley.TokenizableStrings;
using StardewValley.Tools;

namespace DeluxeJournal.Util
{
    /// <summary>Provides a means of querying game objects/data by their corresponding localized display names.</summary>
    public class LocalizedObjects
    {
        private static LocalizedObjects? _instance;
        private readonly IDictionary<string, string> _items;
        private readonly IDictionary<string, string> _npcs;
        private readonly IDictionary<string, string> _tools;
        private readonly IDictionary<string, BlueprintInfo> _blueprints;

        private LocalizedObjects(ITranslationHelper translation)
        {
            _items = CreateItemMap();
            _npcs = CreateNPCMap();
            _tools = CreateToolMap(translation);
            _blueprints = CreateBlueprintMap();
        }

        public static LocalizedObjects Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new InvalidOperationException("LocalizedObjects class is not initialized with ITranslationHelper object. You must first call the LocalizedObjects.OneTimeInit() method.");
                }
                return _instance;
            }
        }

        public static bool IsInitialized()
        {
            return _instance != null;
        }

        public static void OneTimeInit(ITranslationHelper translation)
        {
            if (_instance == null)
            {
                _instance = new LocalizedObjects(translation);
            }
            else
            {
                throw new InvalidOperationException("LocalizedObjects is already initialized.");
            }
        }

        /// <summary>Get an Item by display name.</summary>
        /// <param name="localizedName">Localized display name (the one that appears in-game).</param>
        /// <param name="fuzzy">Perform a fuzzy search if true, otherwise only return an Item with the exact name.</param>
        public Item? GetItem(string localizedName, bool fuzzy = false)
        {
            if (GetValue(_tools, localizedName, fuzzy) is string tool)
            {
                return ItemRegistry.Create(tool);
            }
            else if (GetValue(_items, localizedName, fuzzy) is string item)
            {
                return ItemRegistry.Create(item);
            }

            return null;
        }

        /// <summary>Get an NPC by display name.</summary>
        /// <param name="localizedName">Localized display name (the one that appears in-game).</param>
        /// <param name="fuzzy">Perform a fuzzy search if true, otherwise only return the NPC with the exact name.</param>
        public NPC? GetNPC(string localizedName, bool fuzzy = false)
        {
            if (GetValue(_npcs, localizedName, fuzzy) is string npc)
            {
                return Game1.getCharacterFromName(npc);
            }

            return null;
        }

        /// <summary>Get BlueprintInfo given the display name.</summary>
        /// <param name="localizedName">Localized display name (the one that appears in-game).</param>
        /// <param name="fuzzy">Perform a fuzzy search if true, otherwise only match a blueprint with the exact name.</param>
        public BlueprintInfo? GetBlueprintInfo(string localizedName, bool fuzzy = false)
        {
            return GetValue(_blueprints, localizedName, fuzzy);
        }

        private static T? GetValue<T>(IDictionary<string, T> map, string key, bool fuzzy) where T : class
        {
            key = key.Trim().ToUpperInvariant();
            key = fuzzy ? Utility.fuzzySearch(key, map.Keys.ToList()) : key;

            if (key != null && map.ContainsKey(key))
            {
                return map[key];
            }

            return null;
        }

        private static IDictionary<string, string> CreateItemMap()
        {
            IDictionary<string, string> furnitureData = DataLoader.Furniture(Game1.content);
            IDictionary<string, string> bootsData = DataLoader.Boots(Game1.content);
            IDictionary<string, string> hatsData = DataLoader.Hats(Game1.content);
            IDictionary<string, string> map = new Dictionary<string, string>();

            foreach (var (key, value) in Game1.objectData)
            {
                if (value != null)
                    map[TokenParser.ParseText(text: value.DisplayName).ToUpperInvariant()] = key;
            }

            foreach (var (key, value) in Game1.bigCraftableData)
            {
                if (value != null && CraftingRecipe.craftingRecipes.ContainsKey(key))
                {
                    map[TokenParser.ParseText(value.DisplayName).ToUpperInvariant()] = key;
                }
            }

            foreach (var (key, value) in Game1.shirtData)
            {
                if (value != null)
                {
                    map[TokenParser.ParseText(value.DisplayName).ToUpperInvariant()] = key;
                }
            }

            foreach (var (key, value) in Game1.pantsData)
            {
                if (value != null)
                {
                    map[TokenParser.ParseText(value.DisplayName).ToUpperInvariant()] = key;
                }
            }

            foreach (var (key, value) in furnitureData)
            {
                if (value != null)
                {
                    string[] values = value.Split('/');
                    map[values[0]] = "(F)" + key + " 1";
                }
            }

            foreach (var (key, value) in Game1.weaponData)
            {
                if (value != null)
                {
                    map[TokenParser.ParseText(value.DisplayName).ToUpperInvariant()] = key;
                }
            }

            foreach (var (key, value) in bootsData)
            {
                if (value != null)
                {
                    string[] values = value.Split('/');
                    map[values[0]] = "(B)" + key + " 1";
                }
            }

            foreach (var (key, value) in hatsData)
            {
                if (value != null)
                {
                    string[] values = value.Split('/');
                    map[values[0]] = "(H)" + key + " 1";
                }
            }

            return map;
        }

        private static IDictionary<string, string> CreateToolMap(ITranslationHelper translation)
        {
            Dictionary<string, string> toolMap = new();

            foreach (var (toolID, toolData) in Game1.toolData)
            {
                Tool tool = ItemRegistry.Create<Tool>(toolID);
                string toolName = ("Pan").Equals(toolID) ? 
                    translation.Get("tool.pan").ToString().ToUpperInvariant()
                    : tool.DisplayName.ToUpperInvariant();
                string originalToolName = TokenParser.ParseText(toolData.DisplayName).ToUpperInvariant();
                toolMap[toolName] = toolID;
                if (!toolMap.ContainsKey(originalToolName) || Game1.toolData[toolMap[originalToolName]].UpgradeLevel > toolData.UpgradeLevel)
                {
                    toolMap[originalToolName] = toolID;
                }
            }

            return toolMap;
        }

        private static IDictionary<string, string> CreateNPCMap()
        {
            IDictionary<string, CharacterData> npcData = Game1.characterData;
            IDictionary<string, string> map = new Dictionary<string, string>();

            foreach (var (name, data) in npcData)
            {
                map[TokenParser.ParseText(data.DisplayName).ToUpperInvariant()] = name;
            }

            return map;
        }

        private static IDictionary<string, BlueprintInfo> CreateBlueprintMap()
        {
            IDictionary<string, BuildingData> buildingData = Game1.buildingData;
            Dictionary<string, BlueprintInfo> map = new();

            foreach (var (key, data) in buildingData)
            {
                map[TokenParser.ParseText(data.Name).ToUpperInvariant()] = new BlueprintInfo(key, TokenParser.ParseText(data.Name), data.BuildingType, data.BuildCost);
            }

            return map;
        }
    }
}
