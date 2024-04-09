using Macrocosm.Content.Items.Materials.Bars;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Macrocosm.Content.Items.Materials.Tech
{
    public class ReactorComponent : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 5;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = 100;
            Item.rare = ItemRarityID.Purple;
            Item.material = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SteelBar>(20)
                .AddIngredient(ItemID.LunarBar, 20)
                .AddIngredient(ItemID.Glass, 50)
                .AddIngredient(ItemID.IronBar, 50)
                .AddTile<Tiles.Crafting.Fabricator>()
                .Register();

            CreateRecipe()
                .AddIngredient<SteelBar>(20)
                .AddIngredient(ItemID.LunarBar, 20)
                .AddIngredient(ItemID.Glass, 50)
                .AddIngredient(ItemID.LeadBar, 50)
                .AddTile<Tiles.Crafting.Fabricator>()
                .Register();
        }
    }
}