using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace DeluxeJournal.Menus.Components
{
    public class CharacterIcon

    {

        public static void DrawIcon(SpriteBatch b, NPC npc, Rectangle destination)
        {
            DrawIcon(b, npc, destination, Color.White);
        }

        public static void DrawIcon(SpriteBatch b, NPC npc, Rectangle destination, Color color)
        {
            Texture2D texture;
            try
            {
                texture = Game1.content.Load<Texture2D>(@"Characters\" + npc.getTextureName());
            }
            catch (Exception)
            {
                texture = npc.Sprite.Texture;
            }

            IconInfo? info = null;

            if (DeluxeJournalMod.CharacterIconOverrides != null && DeluxeJournalMod.CharacterIconOverrides.TryGetValue(npc.displayName, out var ovr))
                info = ovr;

            if (info == null)
            {
                DeluxeJournalMod.CharacterIconInfo?.TryGetValue(npc.Name, out info);
            }

            b.Draw(
                texture,
                destination, new Rectangle(
                    info?.OffsetX ?? 0,
                    info?.OffsetY ?? 0,
                    info?.Width ?? 16,
                    info?.Height ?? 16
                ),
                color
            );
        }
    }
}
