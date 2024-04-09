﻿using Macrocosm.Common.DataStructures;
using Macrocosm.Common.Loot;
using Macrocosm.Common.Loot.DropConditions;
using Macrocosm.Common.Loot.DropRules;
using Macrocosm.Common.Storage;
using Macrocosm.Common.Subworlds;
using Macrocosm.Common.Systems.Power;
using Macrocosm.Common.Utils;
using Macrocosm.Content.Items.Blocks.Sands;
using Macrocosm.Content.Items.Blocks.Terrain;
using Macrocosm.Content.Items.Materials.Ores;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Macrocosm.Content.Machines
{
    public class OreExcavatorTE : MachineTE, IInventoryOwner
    {
        public override MachineTile MachineTile => ModContent.GetInstance<OreExcavator>();
        public override bool Operating => MachineTile.IsOperating(Position.X, Position.Y);

        public SimpleLootTable Loot { get; set; }
        protected virtual int OreGenerationRate => 60;

        public Inventory Inventory { get; set; }
        protected virtual int InventorySize => 50;
        public Vector2 InventoryItemDropLocation => Position.ToVector2() * 16 + new Vector2(MachineTile.Width, MachineTile.Height) * 16 / 2;

        protected int checkTimer;
        protected List<int> blacklistedIds = null;

        public override void OnFirstUpdate()
        {
            // Create new inventory if none found on world load
            Inventory ??= new(InventorySize, this);

            // Assign inventory owner if the inventory was found on load
            // IInvetoryOwner does not work well with TileEntities >:(
            if (Inventory.Owner is null)
                Inventory.Owner = this;

            // Generate loot table based on current subworld and biome
            Loot = new();
            PopulateItemLoot();

            // Read blacklist if found on world load
            if (blacklistedIds is not null)
                foreach (var blacklistable in Loot.BlacklistableEntries)
                    if (blacklistedIds.Contains(blacklistable.ItemID))
                        blacklistable.Blacklisted = true;
        }

        public override void MachineUpdate()
        {
            bool foundPower = false;
            for (int i = Position.X - 5; i < Position.X + MachineTile.Width + 5; i++)
            {
                for (int j = Position.Y - 5; j < Position.Y + MachineTile.Height + 5; j++)
                {
                    if (WorldGen.InWorld(i, j))
                    {
                        Tile tile = Main.tile[i, j];
                        if (TileLoader.GetTile(tile.TileType) is Tiles.Furniture.SolarPanelLarge)
                        {
                            foundPower = true;
                        }
                    }
                }
            }

            if (foundPower && !MachineTile.GetPowerState(Position.X, Position.Y))
                MachineTile.TogglePowerState(Position.X, Position.Y);

            if (!foundPower && MachineTile.GetPowerState(Position.X, Position.Y))
                MachineTile.TogglePowerState(Position.X, Position.Y);

            blacklistedIds = Loot.BlacklistableEntries.Where((entry) => entry.Blacklisted).Select((entry) => entry.ItemID).ToList();

            if (Operating)
                checkTimer++;

            if (checkTimer >= OreGenerationRate)
            {
                checkTimer = 0;
                Loot?.Drop(Utility.GetClosestPlayer(Position, MachineTile.Width * 16, MachineTile.Height * 16));
            }
        }

        public virtual void PopulateItemLoot()
        {
            SceneData scene = new(Position);
            scene.Scan();

            switch (MacrocosmSubworld.CurrentID)
            {
                case "Macrocosm/Moon":

                    Loot.Add(new TECommonDrop(this, ModContent.ItemType<Regolith>(), 20, amountDroppedMinimum: 10, amountDroppedMaximum: 100));
                    Loot.Add(new TECommonDrop(this, ModContent.ItemType<Protolith>(), 20, amountDroppedMinimum: 10, amountDroppedMaximum: 100));

                    Loot.Add(new TECommonDrop(this, ModContent.ItemType<ArtemiteOre>(), 10));
                    Loot.Add(new TECommonDrop(this, ModContent.ItemType<SeleniteOre>(), 10));
                    Loot.Add(new TECommonDrop(this, ModContent.ItemType<ChandriumOre>(), 10));
                    Loot.Add(new TECommonDrop(this, ModContent.ItemType<DianiteOre>(), 10));
                    Loot.Add(new TECommonDrop(this, ItemID.LunarOre, 8));

                    break;

                default:

                    // Common loot
                    Loot.Add(new TECommonDrop(this, ModContent.ItemType<Coal>(), 6));
                    Loot.Add(new TECommonDrop(this, ItemID.CopperOre, 6));
                    Loot.Add(new TECommonDrop(this, ItemID.TinOre, 6));
                    Loot.Add(new TECommonDrop(this, ItemID.IronOre, 8));
                    Loot.Add(new TECommonDrop(this, ItemID.LeadOre, 8));
                    Loot.Add(new TECommonDrop(this, ItemID.SilverOre, 10));
                    Loot.Add(new TECommonDrop(this, ItemID.TungstenOre, 10));
                    Loot.Add(new TECommonDrop(this, ItemID.GoldOre, 12));
                    Loot.Add(new TECommonDrop(this, ItemID.PlatinumOre, 12));

                    // Common Hardmode loot
                    Loot.Add(new TEDropWithConditionRule(this, ItemID.CobaltOre, 12, condition: new Conditions.IsHardmode()));
                    Loot.Add(new TEDropWithConditionRule(this, ItemID.PalladiumOre, 12, condition: new Conditions.IsHardmode()));
                    Loot.Add(new TEDropWithConditionRule(this, ItemID.MythrilOre, 16, condition: new Conditions.IsHardmode()));
                    Loot.Add(new TEDropWithConditionRule(this, ItemID.OrichalcumOre, 16, condition: new Conditions.IsHardmode()));
                    Loot.Add(new TEDropWithConditionRule(this, ItemID.AdamantiteOre, 20, condition: new Conditions.IsHardmode()));
                    Loot.Add(new TEDropWithConditionRule(this, ItemID.TitaniumOre, 20, condition: new Conditions.IsHardmode()));

                    // Purity loot
                    Loot.Add(new TEDropWithConditionRule(this, ItemID.DirtBlock, 20, amountDroppedMinimum: 10, amountDroppedMaximum: 100, condition: new SceneDataConditions.IsPurity(scene)));
                    Loot.Add(new TEDropWithConditionRule(this, ItemID.StoneBlock, 20, amountDroppedMinimum: 10, amountDroppedMaximum: 100, condition: new SceneDataConditions.IsPurity(scene)));
                    Loot.Add(new TEDropWithConditionRule(this, ItemID.SiltBlock, 25, amountDroppedMinimum: 10, amountDroppedMaximum: 50, condition: new ConditionsChain.All(new SceneDataConditions.IsSnow(scene).Not(), new SceneDataConditions.IsDesert(scene).Not())));

                    // Corruption loot
                    Loot.Add(new TEDropWithConditionRule(this, ItemID.EbonstoneBlock, 20, amountDroppedMinimum: 10, amountDroppedMaximum: 100, condition: new SceneDataConditions.IsCorruption(scene)));
                    Loot.Add(new TEDropWithConditionRule(this, ItemID.DemoniteOre, 14, condition: new SceneDataConditions.IsCorruption(scene)));

                    // Crimson loot
                    Loot.Add(new TEDropWithConditionRule(this, ItemID.CrimstoneBlock, 20, amountDroppedMinimum: 10, amountDroppedMaximum: 100, condition: new SceneDataConditions.IsCrimson(scene)));
                    Loot.Add(new TEDropWithConditionRule(this, ItemID.CrimtaneOre, 14, condition: new SceneDataConditions.IsCrimson(scene)));

                    // Hallow loot
                    Loot.Add(new TEDropWithConditionRule(this, ItemID.PearlstoneBlock, 20, amountDroppedMinimum: 10, amountDroppedMaximum: 100, condition: new SceneDataConditions.IsHallow(scene)));
                    Loot.Add(new TEDropWithConditionRule(this, ItemID.CrystalShard, 25, amountDroppedMinimum: 2, amountDroppedMaximum: 5, condition: new SceneDataConditions.IsHallow(scene)));
                    Loot.Add(new TEDropWithConditionRule(this, ItemID.QueenSlimeCrystal, 50, condition: new SceneDataConditions.IsHallow(scene)));

                    // Pure desert loot
                    Loot.Add(new TEDropWithConditionRule(this, ItemID.SandBlock, 20, amountDroppedMinimum: 10, amountDroppedMaximum: 100, condition: new ConditionsChain.All(new SceneDataConditions.IsDesert(scene), new SceneDataConditions.IsSpreadableBiome(scene).Not())));
                    Loot.Add(new TEDropWithConditionRule(this, ItemID.HardenedSand, 22, amountDroppedMinimum: 10, amountDroppedMaximum: 100, condition: new ConditionsChain.All(new SceneDataConditions.IsDesert(scene), new SceneDataConditions.IsSpreadableBiome(scene).Not())));
                    Loot.Add(new TEDropWithConditionRule(this, ItemID.Sandstone, 22, amountDroppedMinimum: 10, amountDroppedMaximum: 100, condition: new ConditionsChain.All(new SceneDataConditions.IsDesert(scene), new SceneDataConditions.IsSpreadableBiome(scene).Not())));
                    Loot.Add(new TEDropWithConditionRule(this, ModContent.ItemType<SilicaSand>(), 22, amountDroppedMinimum: 10, amountDroppedMaximum: 100, condition: new ConditionsChain.All(new SceneDataConditions.IsDesert(scene), new SceneDataConditions.IsSpreadableBiome(scene).Not())));

                    // Common desert loot
                    Loot.Add(new TEDropWithConditionRule(this, ItemID.DesertFossil, 25, amountDroppedMinimum: 2, amountDroppedMaximum: 10, condition: new ConditionsChain.All(new SceneDataConditions.IsDesert(scene), new SceneDataConditions.IsSpreadableBiome(scene).Not())));

                    // Corrupt desert loot
                    Loot.Add(new TEDropWithConditionRule(this, ItemID.EbonsandBlock, 20, amountDroppedMinimum: 10, amountDroppedMaximum: 100, condition: new ConditionsChain.All(new SceneDataConditions.IsDesert(scene), new SceneDataConditions.IsCorruption(scene))));
                    Loot.Add(new TEDropWithConditionRule(this, ItemID.CorruptHardenedSand, 22, amountDroppedMinimum: 10, amountDroppedMaximum: 100, condition: new ConditionsChain.All(new SceneDataConditions.IsDesert(scene), new SceneDataConditions.IsCorruption(scene))));
                    Loot.Add(new TEDropWithConditionRule(this, ItemID.CorruptSandstone, 22, amountDroppedMinimum: 10, amountDroppedMaximum: 100, condition: new ConditionsChain.All(new SceneDataConditions.IsDesert(scene), new SceneDataConditions.IsCorruption(scene))));
                    Loot.Add(new TEDropWithConditionRule(this, ModContent.ItemType<SilicaEbonsand>(), 22, amountDroppedMinimum: 10, amountDroppedMaximum: 100, condition: new ConditionsChain.All(new SceneDataConditions.IsDesert(scene), new SceneDataConditions.IsCorruption(scene))));

                    // Crimson desert loot
                    Loot.Add(new TEDropWithConditionRule(this, ItemID.CrimsandBlock, 20, amountDroppedMinimum: 10, amountDroppedMaximum: 100, condition: new ConditionsChain.All(new SceneDataConditions.IsDesert(scene), new SceneDataConditions.IsCrimson(scene))));
                    Loot.Add(new TEDropWithConditionRule(this, ItemID.CrimsonHardenedSand, 22, amountDroppedMinimum: 10, amountDroppedMaximum: 100, condition: new ConditionsChain.All(new SceneDataConditions.IsDesert(scene), new SceneDataConditions.IsCrimson(scene))));
                    Loot.Add(new TEDropWithConditionRule(this, ItemID.CrimsonSandstone, 22, amountDroppedMinimum: 10, amountDroppedMaximum: 100, condition: new ConditionsChain.All(new SceneDataConditions.IsDesert(scene), new SceneDataConditions.IsCrimson(scene))));
                    Loot.Add(new TEDropWithConditionRule(this, ModContent.ItemType<SilicaCrimsand>(), 22, amountDroppedMinimum: 10, amountDroppedMaximum: 100, condition: new ConditionsChain.All(new SceneDataConditions.IsDesert(scene), new SceneDataConditions.IsCrimson(scene))));

                    // Hallowed desert loot
                    Loot.Add(new TEDropWithConditionRule(this, ItemID.PearlsandBlock, 20, amountDroppedMinimum: 10, amountDroppedMaximum: 100, condition: new ConditionsChain.All(new SceneDataConditions.IsDesert(scene), new SceneDataConditions.IsHallow(scene))));
                    Loot.Add(new TEDropWithConditionRule(this, ItemID.HallowHardenedSand, 22, amountDroppedMinimum: 10, amountDroppedMaximum: 100, condition: new ConditionsChain.All(new SceneDataConditions.IsDesert(scene), new SceneDataConditions.IsHallow(scene))));
                    Loot.Add(new TEDropWithConditionRule(this, ItemID.HallowSandstone, 22, amountDroppedMinimum: 10, amountDroppedMaximum: 100, condition: new ConditionsChain.All(new SceneDataConditions.IsDesert(scene), new SceneDataConditions.IsHallow(scene))));
                    Loot.Add(new TEDropWithConditionRule(this, ModContent.ItemType<SilicaPearlsand>(), 22, amountDroppedMinimum: 10, amountDroppedMaximum: 100, condition: new ConditionsChain.All(new SceneDataConditions.IsDesert(scene), new SceneDataConditions.IsHallow(scene))));

                    // Pure snow biome loot
                    Loot.Add(new TEDropWithConditionRule(this, ItemID.IceBlock, 20, amountDroppedMinimum: 10, amountDroppedMaximum: 100, condition: new ConditionsChain.All(new SceneDataConditions.IsSnow(scene), new SceneDataConditions.IsSpreadableBiome(scene).Not())));

                    // Common snow biome loot
                    Loot.Add(new TEDropWithConditionRule(this, ItemID.SnowBlock, 20, amountDroppedMinimum: 10, amountDroppedMaximum: 100, condition: new SceneDataConditions.IsSnow(scene)));
                    Loot.Add(new TEDropWithConditionRule(this, ItemID.SlushBlock, 25, amountDroppedMinimum: 10, amountDroppedMaximum: 50, condition: new SceneDataConditions.IsSnow(scene)));

                    // Corrupt snow biome loot
                    Loot.Add(new TEDropWithConditionRule(this, ItemID.PurpleIceBlock, 20, amountDroppedMinimum: 10, amountDroppedMaximum: 100, condition: new ConditionsChain.All(new SceneDataConditions.IsSnow(scene), new SceneDataConditions.IsCorruption(scene))));

                    // Crimson snow biome loot
                    Loot.Add(new TEDropWithConditionRule(this, ItemID.RedIceBlock, 20, amountDroppedMinimum: 10, amountDroppedMaximum: 100, condition: new ConditionsChain.All(new SceneDataConditions.IsSnow(scene), new SceneDataConditions.IsCrimson(scene))));

                    // Hallowed snow biome loot
                    Loot.Add(new TEDropWithConditionRule(this, ItemID.PinkIceBlock, 20, amountDroppedMinimum: 10, amountDroppedMaximum: 100, condition: new ConditionsChain.All(new SceneDataConditions.IsSnow(scene), new SceneDataConditions.IsHallow(scene))));

                    // Glowing mushroom loot
                    Loot.Add(new TEDropWithConditionRule(this, ItemID.MudBlock, 20, amountDroppedMinimum: 10, amountDroppedMaximum: 100, condition: new SceneDataConditions.IsGlowingMushroom(scene)));
                    Loot.Add(new TEDropWithConditionRule(this, ItemID.MushroomGrassSeeds, 25, amountDroppedMinimum: 5, amountDroppedMaximum: 15, condition: new SceneDataConditions.IsGlowingMushroom(scene)));

                    // Jungle loot
                    Loot.Add(new TEDropWithConditionRule(this, ItemID.MudBlock, 20, amountDroppedMinimum: 10, amountDroppedMaximum: 100, condition: new SceneDataConditions.IsJungle(scene)));
                    Loot.Add(new TEDropWithConditionRule(this, ItemID.ChlorophyteOre, 12, condition: new ConditionsChain.All(new SceneDataConditions.IsJungle(scene), new Conditions.IsHardmode())));

                    // Underworld loot 
                    Loot.Add(new TEDropWithConditionRule(this, ItemID.AshBlock, 20, amountDroppedMinimum: 10, amountDroppedMaximum: 100, condition: new SceneDataConditions.IsUnderworld(scene)));
                    Loot.Add(new TEDropWithConditionRule(this, ItemID.Hellstone, 10, condition: new SceneDataConditions.IsUnderworld(scene)));

                    break;
            }
        }

        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendTileSquare(Main.myPlayer, i, j, MachineTile.Width, MachineTile.Height);
                NetMessage.SendData(MessageID.TileEntityPlacement, number: i, number2: j, number3: Type);
            }

            Point16 tileOrigin = new(0, MachineTile.Height - 1);
            int placedEntity = Place(i - tileOrigin.X, j - tileOrigin.Y);

            return placedEntity;
        }

        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && tile.TileType == ModContent.TileType<OreExcavator>();
        }

        public override void OnNetPlace()
        {
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.TileEntitySharing, number: ID, number2: Position.X, number3: Position.Y);
        }

        // TODO: some way of syncing the blacklist from the client with the server-owned TE
        public override void NetSend(BinaryWriter writer)
        {
            TagIO.Write(Inventory.SerializeData(), writer);

            #region Serialize blacklist
            if (blacklistedIds is not null && blacklistedIds.Count > 0)
            {
                writer.Write(blacklistedIds.Count);
                foreach (int itemId in blacklistedIds)
                    writer.Write(itemId);
            }
            #endregion
        }

        public override void NetReceive(BinaryReader reader)
        {
            TagCompound tag = TagIO.Read(reader);
            Inventory = Inventory.DeserializeData(tag);

            #region Deserialize blacklist
            int blacklistedCount = reader.ReadInt32();
            blacklistedIds = new(blacklistedCount);
            for (int i = 0; i < blacklistedCount; i++)
                blacklistedIds[i] = reader.ReadInt32();
            #endregion
        }

        public override void SaveData(TagCompound tag)
        {
            tag[nameof(Inventory)] = Inventory;

            #region Serialize blacklist
            if (blacklistedIds is not null && blacklistedIds.Count > 0)
                tag[nameof(blacklistedIds)] = blacklistedIds;
            #endregion
        }

        public override void LoadData(TagCompound tag)
        {
            if (tag.ContainsKey(nameof(Inventory)))
                Inventory = tag.Get<Inventory>(nameof(Inventory));

            #region Deserialize blacklist
            if (tag.ContainsKey(nameof(blacklistedIds)))
                blacklistedIds = tag.GetList<int>(nameof(blacklistedIds)) as List<int>;
            #endregion
        }
    }
}
