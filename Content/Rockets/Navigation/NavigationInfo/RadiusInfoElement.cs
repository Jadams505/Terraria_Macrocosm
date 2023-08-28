﻿using ReLogic.Content;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using Terraria.Localization;
using Macrocosm.Common.Config;
using System;

namespace Macrocosm.Content.Rockets.Navigation.NavigationInfo
{
	internal class RadiusInfoElement : ValueUnitSpecialInfoElement
	{
		public RadiusInfoElement(string specialValueKey) : base(specialValueKey) { }

		public RadiusInfoElement(float value, string specialValueKey = "") : base(value, specialValueKey) { }

		protected override LocalizedText GetLocalizedValueUnitText(ref float value)
		{
			var type = MacrocosmConfig.Instance.UnitSystem;

			// convert to miles
			if (type == MacrocosmConfig.UnitSystemType.Imperial)
				value *= 0.621f;

			value = MathF.Round(value, 2);

			return Language.GetText("Mods.Macrocosm.UI.Rocket.Radius.Unit" + type.ToString());
		}
	}
}