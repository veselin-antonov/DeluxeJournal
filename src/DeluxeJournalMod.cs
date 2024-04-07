using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Graphics;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using DeluxeJournal.Api;
using DeluxeJournal.Framework;
using DeluxeJournal.Framework.Data;
using DeluxeJournal.Framework.Events;
using DeluxeJournal.Framework.Tasks;
using DeluxeJournal.Menus;
using DeluxeJournal.Menus.Components;
using DeluxeJournal.Patching;
using Newtonsoft.Json.Linq;
/*using SpaceCore.Events;*/
using DeluxeJournal.Events;
using DeluxeJournal.Util;
using DeluxeJournal.src.Patching;

namespace DeluxeJournal
{
    /// <summary>The mod entry point.</summary>
    internal class DeluxeJournalMod : Mod
    {
        public const string NOTES_DATA_KEY = "notes-data";
        public const string TASKS_DATA_KEY = "tasks-data";

        private static DeluxeJournalMod? _instance;

        public static DeluxeJournalMod? Instance => _instance;

        public static Texture2D? UiTexture { get; private set; }

        public static Dictionary<string, IconInfo>? CharacterIconInfo;
        public static Dictionary<string, IconInfo>? CharacterIconOverrides;

        public static bool IsMainScreen => !Context.IsSplitScreen || Context.ScreenId == 0;

        private NotesData? _notesData;

        public Config? Config { get; private set; }

        public EventManager? EventManager { get; private set; }

        public TaskManager? TaskManager { get; private set; }

        public PageManager? PageManager { get; private set; }

        public override void Entry(IModHelper helper)
        {
            _instance = this;

            RuntimeHelpers.RunClassConstructor(typeof(TaskTypes).TypeHandle);

            UiTexture = helper.ModContent.Load<Texture2D>("assets/ui.png");
            Config = helper.ReadConfig<Config>();
            _notesData = helper.Data.ReadGlobalData<NotesData>(NOTES_DATA_KEY) ?? new NotesData();

            EventManager = new EventManager(helper.Events, helper.Multiplayer, Monitor);
            TaskManager = new TaskManager(new TaskEvents(EventManager), helper.Data);
            PageManager = new PageManager();

            PageManager.RegisterPage("quests", (bounds) => new QuestLogPage(bounds, UiTexture, helper.Translation), 102);
            PageManager.RegisterPage("tasks", (bounds) => new TasksPage(bounds, UiTexture, helper.Translation), 101);
            PageManager.RegisterPage("notes", (bounds) => new NotesPage(bounds, UiTexture, helper.Translation), 100);

            helper.Events.Display.MenuChanged += OnMenuChanged;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.Saving += OnSaving;

            /*SpaceEvents.BeforeGiftGiven += FarmerPatch.OnBouquetGiven;*/

            Patcher.Apply(new Harmony(ModManifest.UniqueID), Monitor,
                new FarmerPatch(EventManager, Monitor),
                new CarpenterMenuPatch(EventManager, Monitor),
                new QuestLogPatch(Monitor),
                new ToolPatch(EventManager, Monitor)
            );

            Program.enableCheats = true;
        }

        private void LoadCharacterIcons()
        {
            const string path = "assets/character_icons.json";
            Dictionary<string, IconInfo>? heads = null;

            try
            {
                heads = Helper.Data.ReadJsonFile<Dictionary<string, IconInfo>>(path);
                if (heads == null)
                    Monitor.Log($"The {path} file is missing or invalid.", LogLevel.Error);
            }
            catch (Exception ex)
            {
                Monitor.Log($"The {path} file is invalid. Details: {ex}", LogLevel.Error);
            }

            heads ??= new();

            // Read any extra data files
            foreach (var cp in Helper.ContentPacks.GetOwned())
            {
                if (!cp.HasFile("heads.json"))
                    continue;

                Dictionary<string, IconInfo>? extra = null;
                try
                {
                    extra = cp.ReadJsonFile<Dictionary<string, IconInfo>>("heads.json");
                }
                catch (Exception ex)
                {
                    Monitor.Log($"The heads.json file of {cp.Manifest.Name} is invalid. Details: {ex}", LogLevel.Error);
                }

                if (extra != null)
                    foreach (var entry in extra)
                        if (!string.IsNullOrEmpty(entry.Key))
                            heads[entry.Key] = entry.Value;
            }

            // Now, read the data file used by NPC Map Locations. This is
            // convenient because a lot of mods support it.
            Dictionary<string, JObject>? content = null;

            try
            {
                content = Helper.GameContent.Load<Dictionary<string, JObject>>("Mods/Bouhm.NPCMapLocations/NPCs");

            }
            catch (Exception)
            {
                /* Nothing~ */
            }

            if (content != null)
            {
                int count = 0;

                foreach (var entry in content)
                {
                    if (heads.ContainsKey(entry.Key))
                        continue;

                    int offset;
                    try
                    {
                        offset = entry.Value.Value<int>("MarkerCropOffset");
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    heads[entry.Key] = new()
                    {
                        OffsetY = offset
                    };
                    count++;
                }

                Monitor.Log($"Loaded {count} character icon offsets from NPC Map Location data.", LogLevel.Debug);
            }

            CharacterIconInfo = heads;

            CharacterIconOverrides = Config?.CharacterIconOverrides;
        }

        public override object GetApi()
        {
            return new DeluxeJournalApi(this);
        }

        public string GetNotes()
        {
            if (_notesData != null && Constants.SaveFolderName != null && _notesData.Text.ContainsKey(Constants.SaveFolderName))
            {
                return _notesData.Text[Constants.SaveFolderName];
            }

            return string.Empty;
        }

        public void SaveNotes(string text)
        {
            if (_notesData != null && Constants.SaveFolderName != null)
            {
                _notesData.Text[Constants.SaveFolderName] = text;
                Helper.Data.WriteGlobalData(NOTES_DATA_KEY, _notesData);
            }
        }

        [EventPriority(EventPriority.Low)]
        private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
        {
            // Hijack QuestLog and replace it with DeluxeJournalMenu
            if (PageManager != null && Game1.activeClickableMenu is QuestLog questLog)
            {
                DeluxeJournalMenu deluxeJournalMenu = new DeluxeJournalMenu(PageManager);
                deluxeJournalMenu.SetQuestLog(questLog);
                Game1.activeClickableMenu = deluxeJournalMenu;
            }
        }

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            if (IsMainScreen)
            {
                TaskManager?.Load();
            }

            if (PageManager != null && !Game1.onScreenMenus.OfType<JournalButton>().Any())
            {
                Game1.onScreenMenus.Add(new JournalButton(Helper.Translation));
            }

            if (!LocalizedObjects.IsInitialized())
            {
                LocalizedObjects.OneTimeInit(Helper.Translation);
            }

            LoadCharacterIcons();
        }

        private void OnSaving(object? sender, SavingEventArgs e)
        {
            if (Config != null)
            {
                Helper.WriteConfig(Config);
            }

            if (IsMainScreen)
            {
                TaskManager?.Save();
            }

            if (!LocalizedObjects.IsInitialized())
            {
                LocalizedObjects.OneTimeInit(Helper.Translation);
            }
        }

        private void OnInventoryChanged(object? sender, InventoryChangedEventArgs e)
        {
            foreach (Item item in e.Added)
            {
                if (item is SObject obj && !obj.HasBeenInInventory)
                {
                    EventManager!.ItemCollected.Raise(null, new ItemReceivedEventArgs(e.Player, obj, obj.Stack));
                }
            }
        }
        /*private void OnBouquetGiven(object? sender, EventArgsBeforeReceiveObject args)
        {
            if (args.Gift.ParentSheetIndex == 458 && sender is Farmer farmer)
            {
                EventManager!.ItemGifted.Raise(farmer, new GiftEventArgs(farmer, args.Npc, args.Gift));
            }
        }*/
    }
}
