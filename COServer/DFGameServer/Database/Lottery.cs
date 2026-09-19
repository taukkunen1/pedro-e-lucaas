using System.Collections.Generic;

namespace GameServer.Database
{
    public class Lottery
    {
        public static List<uint> RateH = new List<uint>()
        {
            #region base lottary item
            723859
,723855
,723856
,723712
,723727
,723724
,723713
,723714
,181425
,181325
,181315
,181305
,181415
,181405
,181715
,181705
,182355
,752099
,1200000
,721258
,723834
,723017
,1088001
,723711
,1088000
#endregion
        };
        public static List<uint> RateH2 = new List<uint>()
        {
            #region Items
            1200001
,720611
,723862
,720609
,723860
,181565
,181925
,181805
,181625
,181525
,181905
,723715
,723716
,721259
,720610
,723861
,723584
,723725
,753999
,752999
,751999
,754999
,700103
,700123
,730003
            #endregion
        };
        public static List<uint> RateH3 = new List<uint>()
        {
            #region Items
            181395
,725016
,723717
,1088000
,2100045
,723725
,1200005
,723718
,753009
,752009
,751009
,182445
,191405
,183325
,183305
,183325
,183375
,184325
,181955
,754999
,751999
,753999
,754999
,754099
,753099
,751099
,751009
            #endregion
        };
        public static List<uint> RateH4 = new List<uint>()
        {
            #region new gramets accsory
            723865
,723863
,723864
,723719
,723695
,753003
,752003
,751003
,723584
,183305
,360001
,360014
,360018
,360016
,360022
,360024
,360028
,360041
,183335
,183345
,183365
,183375
,183385
,183395
,183405
,183225
,183425
,183465
,183475
,183485
,183635
,184305
,184315
,184325
            #endregion
        };
       
        public static List<uint> Super1SocItems = new List<uint>()
        {
            #region Super1Soc
            121129
,133049
,152129
,131059
,151119
,130059
,123059
,141059
,112059
,118059
,117069
,201009
,150119
,113039
,160099
,601199
,111059
,120129
,134059
,142059
,114069
,202009
,135059
            #endregion
        };

        public static List<uint> Super2SocItems = new List<uint>()
        {
            #region Super 2 sokcet
            142049
,121099
,151099
,131049
,421199
,114049
,561199
,160119
,430199
,130049
,601099
,450199
,152089
,120099
,530199
,201009
,150099
,113029
,118049
,560199
,440199
,601199
,420199
,135029
,141049
,540199
,112029
,134049
,410199
,490199
,500189
,123029
,460199
,580199
,480199
,202009
,111049
,117049
,900089
,481199
,510199
,133029
            #endregion
        };

        public static List<uint> SuperNoSocItems = new List<uint>()
        {
            #region super ni socket
            113069
,131089
,123069
,112069
,152189
,134099
,150199
,120189
,121189
,201009
,117089
,151179
,160199
,141069
,118089
,135089
,133079
,114099
,202009
,142069
,111089
,130089
            #endregion
        };
        public static List<uint> ElitePlus8Items = new List<uint>()
        {
            #region +6
            150078
,121088
,113018
,120088
,410078
,123038
,490088
,160078
,142018
,421078
,540088
,130038
,580088
,201008
,500078
,151078
,133038
,481088
,112038
,117038
,560088
,135048
,420088
,430088
,510088
,141038
,450088
,440088
,111038
,900008
,134038
,202008
,131038
,118038
,114038
,530088
,601088
,561088
,480088
,152108
,460088
            #endregion
        };

        //public static void GetRandomPrize(GameClient Client, Packet stream)
        //{
        //    if (Role.Core.Rate(0.0400))
        //    {
        //        // Lucky +8 elite item.
        //        uint Id = ElitePlus8Items[Role.Core.Random.Next(0, ElitePlus8Items.Count)];
        //        Client.Inventory.Add(stream, Id, 1, 6);
        //        Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("Congratulations! " + Client.Player.Name + " won a +6" + Pool.ItemsBase[Id].Name + " in Lottery.", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
        //        Program.Plus8++;
        //    }
        //    else if (Role.Core.Rate(0.0500))
        //    {
        //        // Lucky Super2Soc
        //        uint Id = Super2SocItems[Role.Core.Random.Next(0, Super2SocItems.Count)];
        //        Client.Inventory.Add(stream, Id, 1, 0, 0, 0, Role.Flags.Gem.EmptySocket, Role.Flags.Gem.EmptySocket);
        //        Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("Congratulations! " + Client.Player.Name + " won a Super-2soc." + Pool.ItemsBase[Id].Name + " in lottery.", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
        //        Program.Super2Soc++;

        //    }
        //    else if (Role.Core.Rate(0.0600))
        //    {
        //        // Lucky Super1Soc
        //        uint Id = Super1SocItems[Role.Core.Random.Next(0, Super1SocItems.Count)];
        //        Client.Inventory.Add(stream, Id, 1, 0, 0, 0, Role.Flags.Gem.EmptySocket);
        //        Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("Congratulations! " + Client.Player.Name + " won a Super-1soc." + Pool.ItemsBase[Id].Name + " in lottery.", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
        //        Program.Super1Soc++;

        //    }
        //    else if (Role.Core.Rate(0.0700))
        //    {
        //        // Lucky Super
        //        uint Id = SuperNoSocItems[Role.Core.Random.Next(0, SuperNoSocItems.Count)];
        //        Client.Inventory.Add(stream, Id, 1);
        //        Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("Congratulations! " + Client.Player.Name + " won a Super" + Pool.ItemsBase[Id].Name + " in lottery.", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
        //        Program.SuperNoSoc++;

        //    }
            
        //    else if (Role.Core.Rate(0.0800))
        //    {
        //        if (Client.ProjectManager)
        //            Client.SendSysMesage("Group4", Game.MsgServer.MsgMessage.ChatMode.TopLeft);
        //        uint Id = RateH4[Role.Core.Random.Next(0, RateH4.Count)];
        //        Client.Inventory.Add(stream, Id, 1);
        //        Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("Congratulations! " + Client.Player.Name + " won " + Pool.ItemsBase[Id].Name + " in lottery.", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
        //    }
        //    else if (Role.Core.Rate(0.0900))
        //    {
        //        if (Client.ProjectManager)
        //            Client.SendSysMesage("Group3", Game.MsgServer.MsgMessage.ChatMode.TopLeft);
        //        uint Id = RateH3[Role.Core.Random.Next(0, RateH3.Count)];
        //        Client.Inventory.Add(stream, Id, 1);
        //    }
        //    else if (Role.Core.Rate(5))
        //    {
        //        if (Client.ProjectManager)
        //            Client.SendSysMesage("Group2", Game.MsgServer.MsgMessage.ChatMode.TopLeft);
        //        uint Id = RateH2[Role.Core.Random.Next(0, RateH2.Count)];
        //        Client.Inventory.Add(stream, Id, 1);
        //    }
        //    else
        //    {
        //        if (Client.ProjectManager)
        //            Client.SendSysMesage("Group1", Game.MsgServer.MsgMessage.ChatMode.TopLeft);
        //        uint Id = RateH[Role.Core.Random.Next(0, RateH.Count)];
        //        Client.Inventory.Add(stream, Id, 1);
        //    }
        //}
    }
}