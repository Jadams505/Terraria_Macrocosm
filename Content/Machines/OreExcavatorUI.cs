﻿using Macrocosm.Common.Loot;
using Macrocosm.Common.Netcode;
using Macrocosm.Common.Storage;
using Macrocosm.Common.Systems.Power;
using Macrocosm.Common.UI;
using Macrocosm.Common.UI.Themes;
using Macrocosm.Common.Utils;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.UI.Elements;


namespace Macrocosm.Content.Machines
{
    public class OreExcavatorUI : MachineUI
    {
        public OreExcavatorTE OreExcavator => MachineTE as OreExcavatorTE;

        private UIPanel inventoryPanel;
        private UIListScrollablePanel dropRateList;

        public OreExcavatorUI()
        {
        }

        public override void OnInitialize()
        {
            base.OnInitialize();

            Width.Set(745f, 0f);
            Height.Set(394f, 0f);

            Recalculate();

            if (OreExcavator.Inventory is not null)
            {
                inventoryPanel = OreExcavator.Inventory.ProvideUI(iconsPerRow: 10, rowsWithoutScrollbar: 5, buttonMenuTopPercent: 0.765f);
                inventoryPanel.Width = new(0, 0.69f);
                inventoryPanel.BorderColor = UITheme.Current.ButtonStyle.BorderColor;
                inventoryPanel.BackgroundColor = UITheme.Current.PanelStyle.BackgroundColor;
                inventoryPanel.Activate();
                Append(inventoryPanel);
            }

            dropRateList = CreateDroprateList();
            Append(dropRateList);
        }

        private UIListScrollablePanel CreateDroprateList()
        {
            dropRateList = new("Loot")
            {
                Width = new(0, 0.306f),
                Height = new(0, 1f),
                HAlign = 1f,
                BorderColor = UITheme.Current.PanelStyle.BorderColor,
                BackgroundColor = UITheme.Current.PanelStyle.BackgroundColor,
                PaddingTop = 0f,
                PaddingLeft = 2f,
                PaddingRight = 2f
            };

            List<DropRateInfo> dropRates = new();
            DropRateInfoChainFeed ratesInfo = new(1f);
            foreach (var drop in OreExcavator.Loot.Entries)
                if (drop.CanDrop(SimpleLootTable.CommonDropAttemptInfo) || (drop is IBlacklistable blacklistable && blacklistable.Blacklisted))
                    drop.ReportDroprates(dropRates, ratesInfo);

            List<DropRateInfo> sortedDropRates = dropRates.OrderBy(entry => new Terraria.Item(entry.itemId).value).OrderBy(entry => entry.ComputeDropRarity()).ToList();

            foreach (DropRateInfo dropRateInfo in sortedDropRates)
            {
                UIItemDropInfo itemDropInfo = new(dropRateInfo)
                {
                    Left = new(0, 0),
                    Width = new(0, 1f),
                    BackgroundColor = UITheme.Current.InfoElementStyle.BackgroundColor,
                    BorderColor = UITheme.Current.InfoElementStyle.BorderColor
                };

                foreach (var entry in OreExcavator.Loot.Entries)
                {
                    if (entry is IBlacklistable blacklistable)
                    {
                        if (dropRateInfo.itemId == blacklistable.ItemID && (OreExcavator.BlacklistedItems.Contains(dropRateInfo.itemId) || blacklistable.Blacklisted))
                            itemDropInfo.ToggleBlacklisted();
                    }
                }

                itemDropInfo.OnLeftClick += (_, element) => BlacklistItem(element as UIItemDropInfo, dropRateInfo);
                dropRateList.Add(itemDropInfo);
            }

            return dropRateList;
        }

        private void BlacklistItem(UIItemDropInfo itemDropInfo, DropRateInfo dropRateInfo)
        {
            foreach (var entry in OreExcavator.Loot.Entries)
            {
                if (entry is IBlacklistable blacklistable)
                {
                    if (dropRateInfo.itemId == blacklistable.ItemID)
                    {
                        // For UI
                        blacklistable.Blacklisted = itemDropInfo.ToggleBlacklisted();

                        // For TE data
                        if (blacklistable.Blacklisted)
                            OreExcavator.BlacklistedItems.Add(blacklistable.ItemID);
                        else
                            OreExcavator.BlacklistedItems.Remove(blacklistable.ItemID);
                    }
                }
            }

            TENetHelper.SyncTEFromClient(OreExcavator.ID);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            Inventory.ActiveInventory = OreExcavator.Inventory;
        }
    }
}
