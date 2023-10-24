﻿using System.Collections.Generic;
using System.Linq;
using Macrocosm.Common.Subworlds;
using Macrocosm.Common.UI;
using Macrocosm.Common.Utils;
using Macrocosm.Content.Rockets.Navigation.NavigationPanel;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace Macrocosm.Content.Rockets.Navigation.Checklist
{
    public class UIFlightChecklist : UIListScrollablePanel, IRocketDataConsumer
	{
		public Rocket Rocket { get; set; }
		public UIMapTarget MapTarget { get; set; }

		private ChecklistConditionCollection commonLaunchConditions = new();
		private ChecklistCondition selectedLaunchCondition;

		public UIFlightChecklist() : base(new LocalizedColorScaleText(Language.GetText("Mods.Macrocosm.UI.Rocket.Common.Checklist"), scale: 1.2f))
		{
			selectedLaunchCondition = new ChecklistCondition("Selected", "Symbols/QuestionMarkGold", () => MapTarget is not null);

			commonLaunchConditions.Add(new ChecklistCondition("Fuel", () => Rocket.Fuel >= Rocket.GetFuelCost(MapTarget.Name)));

			// NOTE: This must be kept as an explicit lambda expression!
			#pragma warning disable IDE0200
			commonLaunchConditions.Add(new ChecklistCondition("Obstruction", () => Rocket.CheckFlightPathObstruction(), checkPeriod: 10));
			#pragma warning restore IDE0200

			commonLaunchConditions.Add(new ChecklistCondition("Boss", () => !Utility.BossActive && !Utility.MoonLordIncoming, hideIfMet: true));
			commonLaunchConditions.Add(new ChecklistCondition("Invasion", () => !Utility.InvastionActive && !Utility.PillarsActive, hideIfMet: true));
			commonLaunchConditions.Add(new ChecklistCondition("BloodMoon", () => !Utility.BloodMoonActive, hideIfMet: true));
		}

		public override void OnInitialize()
		{
			base.OnInitialize();
			BackgroundColor = new(53, 72, 135);
			BorderColor = new(89, 116, 213, 255);
		}

		public bool CheckLaunchConditions()
		{
			bool met = selectedLaunchCondition.IsMet() && commonLaunchConditions.MetAll();

			if (MapTarget is not null)
			{
				met &= MapTarget.CheckLaunchConditions();
				MapTarget.IsReachable = met;
			}

			return met;
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			Deactivate();
			ClearList();
			AddRange(GetUpdatedChecklist());
			Activate();
		}

		private List<UIElement> GetUpdatedChecklist()
		{
			List<UIElement> uIChecklist = new();
			ChecklistConditionCollection checklistConditions = new();

			if (!selectedLaunchCondition.IsMet())
			{
				checklistConditions.Add(selectedLaunchCondition);
			}
			else
			{
				if (MapTarget.LaunchConditions is not null)
					checklistConditions.AddRange(MapTarget.LaunchConditions);

				checklistConditions.AddRange(commonLaunchConditions);
			}

			var sortedConditions = checklistConditions
				.Where(condition => !(condition.IsMet() && condition.HideIfMet))
				.OrderBy(condition => condition.IsMet()).ToList();

			foreach (var condition in sortedConditions)
				uIChecklist.Add(condition.ProvideUIInfoElement());
 
			return uIChecklist;
		}
	}
}