﻿using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using DeluxeJournal.Events;
using StardewModdingAPI.Framework.ModLoading.Rewriters.StardewValley_1_6;

namespace DeluxeJournal.Framework.Events
{
    internal class BuildingConstructedEvent : ManagedNetEvent<BuildingConstructedEventArgs, BuildingConstructedEvent.EventMessage>
    {
        public class EventMessage
        {
            /// <summary>Building tile X position.</summary>
            public int TileX { get; set; }

            /// <summary>Building tile Y position.</summary>
            public int TileY { get; set; }

            /// <summary>Building location name.</summary>
            public string LocationName { get; set; } = string.Empty;

            /// <summary>Value of <see cref="BuildingConstructedEventArgs.IsUpgrade"/></summary>
            public bool IsUpgrade { get; set; }
        }

        public BuildingConstructedEvent(string name, IMultiplayerHelper multiplayer) :
            base(name, multiplayer)
        {
        }

        protected override EventMessage EventArgsToMessage(BuildingConstructedEventArgs args)
        {
            return new EventMessage()
            {
                TileX = args.Building.tileX.Value,
                TileY = args.Building.tileY.Value,
                LocationName = args.Location.NameOrUniqueName,
                IsUpgrade = args.IsUpgrade
            };
        }

        protected override BuildingConstructedEventArgs MessageToEventArgs(EventMessage message)
        {
            Vector2 tile = new (message.TileX, message.TileY);
            GameLocation location = Game1.getLocationFromName(message.LocationName);

            if (!location.IsBuildableLocation())
            {
                throw new ArgumentException(string.Format("You cannot build in location with name '{0}'.", message.LocationName));
            }
            
            if (location.getBuildingAt(tile) is not Building building)
            {
                throw new ArgumentException(string.Format("No building found at location '{0}' on tile ({1},{2}).", message.LocationName, tile.X, tile.Y));
            }

            return new BuildingConstructedEventArgs(location, building, message.IsUpgrade);
        }
    }
}
