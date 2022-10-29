using Macrocosm.Content.Rarities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Macrocosm.Content.Items.Dev
{
	public class DevPick : ModItem
	{
		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("Dev Pickaxe");
		}

		public override void SetDefaults()
		{
			Item.damage = 10000;
			Item.DamageType = DamageClass.Melee;
			Item.width = 40;
			Item.height = 40;
			Item.useTime = 1;
			Item.useAnimation = 12;
			Item.pick = 10000;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 6;
			Item.value = 10000;
			Item.rare = ModContent.RarityType<MoonRarityT3>();
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
			Item.tileBoost = 50;
		}
	}
}