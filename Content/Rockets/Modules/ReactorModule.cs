﻿using Microsoft.Xna.Framework;

namespace Macrocosm.Content.Rockets.Modules
{
	public class ReactorModule : RocketModule
    {
		public ReactorModule(Rocket rocket) : base(rocket)
		{
		}

		public override int DrawPriority => 2;

		public override int Width => 84;
		public override int Height => 80;

		public override Rectangle Hitbox => base.Hitbox with { Y = base.Hitbox.Y + 4 };
	}
}
