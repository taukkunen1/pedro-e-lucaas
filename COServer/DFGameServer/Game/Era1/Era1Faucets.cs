namespace GameServer.Game.Era1
{
    /// <summary>
    /// Economy V5 policy for non-hunting/non-shop faucets and deferred transfers.
    /// Keeps transfer semantics explicit so telemetry does not mistake escrow,
    /// guild treasury or PK redemption for currency creation/destruction.
    /// </summary>
    public static class Era1Faucets
    {
        public const uint SmallLotteryTicketItem = 711504;
        public const uint LotteryTicketsPerRoll = 3;
        public const uint LotteryTicketPerJade = 1;

        // Weekly PK already advertises a fixed 2,500 CP prize in the legacy code.
        // Do not scale it by online population and do not route it through a
        // fallback configurable reward.
        public const uint WeeklyPkWarRewardConquerPoints = 2500;

        // Missing reward definitions must never become an implicit currency faucet.
        public const uint UnknownEventRewardValue = 0;

        public static bool CanAddCurrency(uint current, uint amount)
        {
            return current <= uint.MaxValue - amount;
        }

        public static bool IsDeferredCurrencyTransfer(Game.MsgServer.MsgItemUsuagePacket.ItemUsageID action)
        {
            return action == Game.MsgServer.MsgItemUsuagePacket.ItemUsageID.RedeemGear
                || action == Game.MsgServer.MsgItemUsuagePacket.ItemUsageID.ClaimGear;
        }

        public static void RunSelfTest()
        {
            if (SmallLotteryTicketItem != 711504
                || LotteryTicketsPerRoll != 3
                || LotteryTicketPerJade != 1
                || WeeklyPkWarRewardConquerPoints != 2500
                || UnknownEventRewardValue != 0)
                throw new System.InvalidOperationException("Era 1 faucet constants failed.");

            if (!CanAddCurrency(uint.MaxValue - 1, 1)
                || CanAddCurrency(uint.MaxValue, 1)
                || !IsDeferredCurrencyTransfer(Game.MsgServer.MsgItemUsuagePacket.ItemUsageID.RedeemGear)
                || !IsDeferredCurrencyTransfer(Game.MsgServer.MsgItemUsuagePacket.ItemUsageID.ClaimGear)
                || IsDeferredCurrencyTransfer(Game.MsgServer.MsgItemUsuagePacket.ItemUsageID.BuyItem))
                throw new System.InvalidOperationException("Era 1 faucet transfer/overflow policy failed.");

            System.Console.WriteLine("ERA1 FAUCETS SELFTEST PASS");
        }
    }
}
