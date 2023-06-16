﻿using Macrocosm.Common.Drawing.Particles;
using Macrocosm.Common.Netcode;
using Macrocosm.Common.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace Macrocosm.Content.Particles
{
    public class ChandriumCrescentMoon : Particle
    {
        bool rotateClockwise = false;
		byte alpha;
        
		public override void OnSpawn()
        {
            rotateClockwise = Main.rand.NextBool();
        }

        public override void AI()
        {
            Rotation += 0.16f * (rotateClockwise ? 1f : -1f);

			Scale -= 0.003f;
            alpha++;

            if (Scale < 0.03f)
                Kill();

            Lighting.AddLight(Position, new Vector3(0.607f, 0.258f, 0.847f) * Scale);
        }


		public override void Draw(SpriteBatch spriteBatch, Vector2 screenPosition, Color lightColor)
		{
            var state = spriteBatch.SaveState();

            spriteBatch.End();
            spriteBatch.Begin(BlendState.Additive, state);
			spriteBatch.Draw(Texture, Position - screenPosition, null, new Color(112, 69, 214).NewAlpha(1f), Rotation, Texture.Size() / 2f, ScaleV, SpriteEffects.None, 0f);
			spriteBatch.End();
			spriteBatch.Begin(state);
		}
	}
}
