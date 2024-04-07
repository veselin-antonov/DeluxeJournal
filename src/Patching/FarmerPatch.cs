using DeluxeJournal.Events;
using DeluxeJournal.Framework.Events;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Quests;

namespace DeluxeJournal.Patching
{
    /// <summary>Patches for <see cref="Farmer"/>.</summary>
    internal class FarmerPatch : PatchBase<FarmerPatch>
    {
        private EventManager EventManager { get; }

        public FarmerPatch(EventManager eventManager, IMonitor monitor) : base(monitor)
        {
            EventManager = eventManager;
            Instance = this;
        }

        private static void Postfix_onGiftGiven(Farmer __instance, NPC npc, SObject item)
        {
            try
            {
                Instance.EventManager.ItemGifted.Raise(__instance, new GiftEventArgs(__instance, npc, item));
            }
            catch (Exception ex)
            {
                Instance.LogError(ex, nameof(Postfix_onGiftGiven));
            }
        }

        private static void Postfix_checkForQuestComplete(Farmer __instance, NPC n, int number1, int number2, Item item, string str, int questType)
        {
            try
            {
                if (questType == Quest.type_crafting && item is SObject obj)
                {
                    Instance.EventManager.ItemCrafted.Raise(__instance, new ItemReceivedEventArgs(__instance, obj, obj.Stack));
                }
            }
            catch (Exception ex)
            {
                Instance.LogError(ex, nameof(Postfix_checkForQuestComplete));
            }
        }

        private static void Prefix_addItemToInventoryBool(Item item)
        {
            try
            {
                if (item is SObject obj && !obj.HasBeenInInventory)
                {
                    Instance.EventManager.ItemCollected.Raise(null, new ItemReceivedEventArgs(Game1.player, obj, obj.Stack));
                }
            }
            catch (Exception ex)
            {
                Instance.LogError(ex, nameof(Prefix_addItemToInventoryBool));
            }
        }

        public override void Apply(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.onGiftGiven)),
                postfix: new HarmonyMethod(typeof(FarmerPatch), nameof(Postfix_onGiftGiven))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.checkForQuestComplete)),
                postfix: new HarmonyMethod(typeof(FarmerPatch), nameof(Postfix_checkForQuestComplete))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.addItemToInventoryBool)),
                prefix: new HarmonyMethod(typeof(FarmerPatch), nameof(Prefix_addItemToInventoryBool))
            );
        }
    }
}
