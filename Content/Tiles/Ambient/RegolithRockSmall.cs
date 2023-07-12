using Macrocosm.Content.Dusts;
using Macrocosm.Content.Items.Placeable.Blocks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Macrocosm.Content.Tiles.Ambient
{
	public abstract class RegolithRockSmallBase : ModTile
	{
		// We want both tiles to use the same texture
		public override string Texture => "Macrocosm/Content/Tiles/Ambient/RegolithRockSmall";

		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = false;
			Main.tileSolidTop[Type] = false;
			Main.tileTable[Type] = false;
			Main.tileNoAttach[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = true;

			DustType = ModContent.DustType<RegolithDust>();

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);

			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.addTile(Type);


			TileID.Sets.DisableSmartCursor[Type] = true;
			AddMapEntry(new Color(220, 220, 220));
		}
	}

	// This is the fake tile that will be placed by the Rubblemaker.
	public class RegolithRockSmallFake : RegolithRockSmallBase
	{
		public override void SetStaticDefaults() {
			// Call to base SetStaticDefaults. Must inherit static defaults from base type 
			base.SetStaticDefaults();

			// Add rubble variant, all existing styles, to Rubblemaker, allowing to place this tile by consuming ExampleBlock
			FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<Regolith>(), Type, 0, 1, 2, 3, 4, 5);

			// Tiles placed by Rubblemaker drop the item used to place them.
			RegisterItemDrop(ModContent.ItemType<Regolith>());
		}
	}

	// This is the natural tile, this version is placed during world generation
	public class RegolithRockSmallNatural : RegolithRockSmallBase
	{
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
		}
	}
}