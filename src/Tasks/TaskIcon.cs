﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace DeluxeJournal.Tasks
{
    public class TaskIcon(Texture2D? texture, Rectangle sourceRect)
    {
        /// <summary>A custom icon texture, or null if using the default.</summary>
        public Texture2D? CustomIconTexture { get; set; } = texture;

        /// <summary>The icon source rect for the given texture.</summary>
        public Rectangle SourceRect { get; set; } = sourceRect;

        public void DrawIcon(SpriteBatch b, Rectangle bounds, Color color, float scale = 4f, bool drawShadow = false)
        {
            if ((CustomIconTexture ?? DeluxeJournalMod.UiTexture) is Texture2D texture)
            {
                Vector2 position = new Vector2(bounds.X + bounds.Height / 2, bounds.Y + bounds.Width / 2);
                Vector2 origin = new Vector2(SourceRect.Width / 2, SourceRect.Height / 2);

                if (drawShadow)
                {
                    Utility.drawWithShadow(b, texture, position, SourceRect, color, 0, origin, scale);
                }
                else
                {
                    b.Draw(texture, position, SourceRect, color, 0, origin, scale, SpriteEffects.None, position.Y / 10000f);
                }
            }
        }
    }
}
