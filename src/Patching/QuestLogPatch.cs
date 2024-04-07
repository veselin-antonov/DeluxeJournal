﻿using Microsoft.Xna.Framework.Graphics;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace DeluxeJournal.Patching
{
    /// <summary>Patches for <see cref="QuestLog"/>.</summary>
    internal class QuestLogPatch : PatchBase<QuestLogPatch>
    {
        public QuestLogPatch(IMonitor monitor) : base(monitor)
        {
            Instance = this;
        }

        private static bool Prefix_draw(QuestLog __instance, SpriteBatch b)
        {
            try
            {
                // !!! DO NOT DRAW THIS QUESTLOG AS THE ACTIVE MENU !!!
                // ----------------------------------------------------
                // 1) Prevents handling jittery frames being drawn while giving other mods a chance to replace the QuestLog.
                // 2) No logic should be done within draw(), so this SHOULD NOT impact a modded QuestLog.
                // 3) We only want to draw the QuestLog from within the QuestLogPage anyway.
                if (Game1.activeClickableMenu == __instance)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Instance.LogError(ex, nameof(Prefix_draw));
            }

            return true;
        }

        public override void Apply(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(QuestLog), nameof(QuestLog.draw), new[] { typeof(SpriteBatch) }),
                prefix: new HarmonyMethod(typeof(QuestLogPatch), nameof(Prefix_draw))
            );
        }
    }
}
