﻿using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Macrocosm.Common.Enums;

namespace Macrocosm.Content.Items.Furniture.Mercury
{
    public class MercuryWorkbench : ModItem
    {
        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Furniture.Luminite.LuminiteWorkbench>(),(int)LuminiteStyle.Mercury);
            Item.width = 28;
            Item.height = 16;
            Item.value = 150;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.MercuryBrick, 10)
                .Register();
        }
    }
}