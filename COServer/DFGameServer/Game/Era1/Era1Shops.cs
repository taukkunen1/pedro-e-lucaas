using System.Collections.Generic;

namespace GameServer.Game.Era1
{
    /// <summary>
    /// Economy V3 shop policy for the 5017-inspired Era 1.
    /// The 5695 shop data is kept on disk for compatibility, but only historically
    /// appropriate CP purchases and classic Gold shops are active at runtime.
    /// </summary>
    public static class Era1Shops
    {
        public const uint GreatMerchantShopId = 432;
        public const uint RemoteShoppingMallShopId = 2888;

        // Later currencies/shops remain in the source tree, but do not participate
        // in Era 1. UIDs 6000/6001 are also classic Gold merchants in Shop.dat, so
        // the later Honor/Race interceptors must be disabled instead of blocking
        // those UIDs globally.
        public static bool EnablePost5017SpecialPointShops => false;
        public static bool EnablePost5017BoundConquerPointMall => false;

        static readonly Dictionary<uint, uint> ClassicMallPrices = new Dictionary<uint, uint>
        {
            // Rare items - official Shopping Mall list (2008).
            { Database.ItemType.DragonBall, 215 },
            { Database.ItemType.Meteor, 13 },

            { 700062, 45 }, // Refined Moon Gem
            { 700032, 65 }, // Refined Rainbow Gem
            { 700012, 65 }, // Refined Dragon Gem
            { 700002, 65 }, // Refined Phoenix Gem
            { 700052, 45 }, // Refined Violet Gem
            { 700022, 45 }, // Refined Fury Gem
            { 700042, 35 }, // Refined Kylin Gem

            { 723584, 215 },  // Black Tulip
            { 2100025, 4050 }, // Miraculous Gourd
            { 2100045, 1870 }, // Magical Bottle
            { Database.ItemType.ExperiencePotion, 27 },
            { 721259, 150 }, // Celestial Stone
            { Database.ItemType.NinjaAmulet, 215 },
            { 723087, 75 },  // Lucky Amulet
            { 723701, 1890 }, // Exemption Token
            { Database.ItemType.SuperToroiseGem, 780 },
            { Database.ItemType.ExpBall, 27 },
            { Database.ItemType.ToughDrill, 1890 },

            // The 2008 mall sells +3..+6 stones. +1/+2 and Star Drill are not
            // direct mall purchases in the historical list.
            { 730003, 108 },
            { 730004, 324 },
            { 730005, 972 },
            { 730006, 2916 },

            // Garments present in the legacy 2888 catalogue and named in the
            // official 2008 Shopping Mall list.
            { 181385, 675 }, // RoyalDignity
            { 182325, 675 }, // AngelicalDress
            { 182345, 980 }, // MoonOrchid
            { 181345, 675 }, // ColorfulDress
            { 181365, 675 }, // PrairieWind
            { 181375, 675 }, // SongofTianshan
            { 182305, 675 }, // SouthofCloud
            { 182385, 980 }, // DreaminFlowers
            { 182335, 980 }, // BlueDream
            { 182315, 675 }, // BonfireNight
            { 181335, 675 }, // WeddingGown
            { 191305, 675 }, // GoodLuck

            { 181305, 675 }, // Phoenix variants
            { 181405, 675 },
            { 181505, 675 },
            { 181605, 675 },
            { 181705, 675 },
            { 181805, 675 },
            { 181905, 675 },

            { 181315, 675 }, // Elegance variants
            { 181415, 675 },
            { 181515, 675 },
            { 181615, 675 },
            { 181715, 675 },
            { 181815, 675 },
            { 181915, 675 },

            { 181325, 675 }, // Celestial variants
            { 181425, 675 },
            { 181525, 675 },
            { 181625, 675 },
            { 181725, 675 },
            { 181825, 675 },
            { 181925, 675 },

            { 181355, 675 } // DarkWizard
        };

        public static bool IsClassicMallShop(uint shopUid)
        {
            return shopUid == GreatMerchantShopId || shopUid == RemoteShoppingMallShopId;
        }

        public static bool TryGetClassicMallPrice(uint itemId, out uint price)
        {
            return ClassicMallPrices.TryGetValue(itemId, out price);
        }

        public static bool IsAllowedForgingPurchase(uint itemId)
        {
            return itemId == Database.ItemType.DragonBall
                || itemId == Database.ItemType.Meteor
                || itemId == Database.ItemType.ToughDrill
                || itemId == Database.ItemType.SuperToroiseGem
                || itemId == 730003
                || itemId == 730004
                || itemId == 730005
                || itemId == 730006;
        }

        public static bool IsAllowedNpcGoldShopItem(uint itemId)
        {
            return itemId != 0
                && !Era1Items.IsBlockedEquipment(itemId)
                && !global::Core.Features.FeatureRegistry.IsBlockedItem(itemId);
        }

        public static bool CanBuyFromShop(Database.Shops.ShopFile.Shop shop, uint itemId)
        {
            if (shop == null || shop.UID == 0)
                return false;

            switch (shop.MoneyType)
            {
                case Database.Shops.ShopFile.MoneyType.ConquerPoints:
                    return IsClassicMallShop(shop.UID)
                        && TryGetClassicMallPrice(itemId, out _);

                case Database.Shops.ShopFile.MoneyType.Gold:
                    return IsAllowedNpcGoldShopItem(itemId);

                // Honor, Bound CP and other later currencies are not Era 1 NPC-shop currencies.
                default:
                    return false;
            }
        }

        public static void SanitizeEShops(Dictionary<uint, Database.Shops.ShopFile.Shop> shops)
        {
            if (shops == null)
                return;

            foreach (var pair in shops)
            {
                var shop = pair.Value;
                if (shop == null)
                    continue;

                if (shop.MoneyType == Database.Shops.ShopFile.MoneyType.ConquerPoints)
                {
                    if (!IsClassicMallShop(shop.UID))
                    {
                        shop.Items?.Clear();
                        shop.BoundItems?.Clear();
                        continue;
                    }

                    if (shop.Items != null)
                        shop.Items.RemoveAll(itemId => !TryGetClassicMallPrice(itemId, out _));

                    // Bound CP was added after the target era.
                    shop.BoundItems?.Clear();
                }
                else if (shop.MoneyType == Database.Shops.ShopFile.MoneyType.BoundConquerPoints)
                {
                    shop.Items?.Clear();
                    shop.BoundItems?.Clear();
                }
            }
        }

        public static void RunSelfTest()
        {
            uint price;

            if (!IsClassicMallShop(GreatMerchantShopId)
                || !IsClassicMallShop(RemoteShoppingMallShopId)
                || IsClassicMallShop(6000))
                throw new System.InvalidOperationException("Era 1 Shopping Mall shop identity failed.");

            if (!TryGetClassicMallPrice(Database.ItemType.DragonBall, out price) || price != 215
                || !TryGetClassicMallPrice(Database.ItemType.Meteor, out price) || price != 13
                || !TryGetClassicMallPrice(Database.ItemType.ToughDrill, out price) || price != 1890
                || !TryGetClassicMallPrice(730003, out price) || price != 108
                || !TryGetClassicMallPrice(730004, out price) || price != 324
                || !TryGetClassicMallPrice(730005, out price) || price != 972
                || !TryGetClassicMallPrice(730006, out price) || price != 2916)
                throw new System.InvalidOperationException("Era 1 Shopping Mall core prices failed.");

            if (TryGetClassicMallPrice(730001, out _)
                || TryGetClassicMallPrice(730002, out _)
                || TryGetClassicMallPrice(Database.ItemType.StarDrill, out _))
                throw new System.InvalidOperationException("Post/unsupported direct mall resources leaked into Era 1.");

            if (!TryGetClassicMallPrice(182335, out price) || price != 980
                || !TryGetClassicMallPrice(181385, out price) || price != 675)
                throw new System.InvalidOperationException("Era 1 garment mall prices failed.");

            if (EnablePost5017SpecialPointShops || EnablePost5017BoundConquerPointMall)
                throw new System.InvalidOperationException("Post-5017 point-shop systems must stay disabled in Era 1.");

            System.Console.WriteLine("ERA1 SHOPS SELFTEST PASS");
        }
    }
}
