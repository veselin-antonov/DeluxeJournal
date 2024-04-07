using DeluxeJournal.Framework.Events;
using DeluxeJournal.Patching;
using DeluxeJournal.src.Events;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace DeluxeJournal.src.Patching
{
    internal class ToolPatch : PatchBase<ToolPatch>
    {
        private EventManager EventManager { get; }

        public ToolPatch(EventManager eventManager, IMonitor monitor) : base(monitor)
        {
            EventManager = eventManager;
            Instance = this;
        }

        private static void Postfix_actionWhenClaimed(Tool __instance)
        {
            try
            {   
                Instance.EventManager.ToolClaimed.Raise(__instance, new ToolClaimedEventArgs(Game1.player, __instance));
            }
            catch (Exception ex)
            {
                Instance.LogError(ex, nameof(Postfix_actionWhenClaimed));
            }
        }

        public override void Apply(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(Tool), nameof(Tool.actionWhenClaimed)),
                postfix: new HarmonyMethod(typeof(ToolPatch), nameof(Postfix_actionWhenClaimed))
            );
        }
    }
}
