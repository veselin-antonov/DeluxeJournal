﻿using DeluxeJournal.Menus.Components;

namespace DeluxeJournal.Framework
{
    internal class Config
    {
        public bool EnableVisualTaskCompleteIndicator { get; set; } = false;

        public bool ShowSmartAddTip { get; set; } = true;

        public bool MoneyViewNetWealth { get; set; } = false;

        public Dictionary<string, IconInfo>? CharacterIconOverrides { get; set; }
    }
}
