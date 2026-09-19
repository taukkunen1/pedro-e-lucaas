using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer.Game.MsgServer
{
    public unsafe static partial class MsgBuilder
    {

        public static void GetHeroInfo(this ServerSockets.Packet stream, Client.GameClient Owner, out Role.Player user)
        {
            user = new Role.Player(Owner);
            user.InitTransfer = stream.ReadUInt32();

            user.RealUID = stream.ReadUInt32();
            user.AparenceType = stream.ReadUInt16();
            uint mesh = stream.ReadUInt32();
            user.Body = (ushort)(mesh % 10000);
            user.Face = (ushort)((mesh - user.Body) / 10000);
            user.Hair = stream.ReadUInt16();
            user.Money = stream.ReadUInt32();
            user.ConquerPoints = stream.ReadUInt32();
            user.Experience = stream.ReadUInt64();
            user.ServerID = (ushort)stream.ReadUInt16();
            user.SetLocationType = (ushort)stream.ReadUInt16();
            user.VirtutePoints = stream.ReadUInt32();
            user.HeavenBlessing = stream.ReadInt32();
            user.Strength = stream.ReadUInt16();
            user.Agility = stream.ReadUInt16();
            user.Vitality = stream.ReadUInt16();
            user.Spirit = stream.ReadUInt16();
            user.Atributes = stream.ReadUInt16();
            user.HitPoints = stream.ReadInt32();
            user.Mana = stream.ReadUInt16();
            user.PKPoints = stream.ReadUInt16();
            user.Level = stream.ReadUInt8();
            user.Class = stream.ReadUInt8();
            user.FirstClass = stream.ReadUInt8();
            user.SecondClass = stream.ReadUInt8();
            user.NobilityRank = (Role.Instance.Nobility.NobilityRank)stream.ReadUInt8();
            user.Reborn = stream.ReadUInt8();
            stream.SeekForward(sizeof(byte));
            user.QuizPoints = stream.ReadUInt32();
            stream.SeekForward(sizeof(uint));
            user.Enilghten = stream.ReadUInt16();
            user.EnlightenReceive = (ushort)(stream.ReadUInt16() / 100);
            stream.SeekForward(sizeof(uint));
            user.VipLevel = (byte)stream.ReadUInt32();
            user.MyTitle = (byte)stream.ReadUInt16();
            user.BoundConquerPoints = stream.ReadInt32();
            stream.SeekForward(sizeof(byte));
            stream.SeekForward(sizeof(uint));
            stream.SeekForward(2 * sizeof(uint));
            stream.SeekForward(sizeof(ushort));
            stream.SeekForward(sizeof(uint));
            stream.SeekForward(sizeof(uint));
            stream.SeekForward(sizeof(uint));
            string[] strs = stream.ReadStringList();
            user.Name = strs[0];
            user.Spouse = strs[1];
        }

        public static unsafe ServerSockets.Packet HeroInfo(this ServerSockets.Packet stream, Role.Player client, int inittransfer = 0)
        {
            stream.InitWriter();
            stream.InitWriter();
            stream.Write(client.UID);//4
            stream.Write((ushort)client.AparenceType);//8
            stream.Write(client.Mesh);//10
            stream.Write(client.Hair);//14
            stream.Write((uint)client.Money);//16
            stream.Write(client.ConquerPoints);//20
            stream.Write((uint)client.Experience);//24
            stream.Write((ushort)0);
            stream.Write(client.SetLocationType);
            stream.Write((uint)0);
            stream.Write((uint)0);
            stream.Write((uint)0);
            stream.Write(client.VirtutePoints);//44
            stream.Write(client.HeavenBlessing);//forinterserver//48
            stream.Write(client.Strength);//52
            stream.Write(client.Agility);//54
            stream.Write(client.Vitality);//56
            stream.Write(client.Spirit);//58
            stream.Write(client.Atributes);//60
            stream.Write((ushort)client.HitPoints);//62
            stream.Write(client.Mana);//64
            stream.Write(client.PKPoints);//66
            stream.Write((byte)client.Level);//68
            stream.Write(client.Class);//69
            stream.Write(client.FirstClass);//70
            stream.Write(client.SecondClass);//71
            stream.Write((byte)client.NobilityRank);//72
            stream.Write(client.Reborn);//73
            stream.Write((byte)0);//74
            stream.Write(client.QuizPoints);//75
            stream.Write((ushort)(client.Enilghten));//79
            stream.Write((ushort)(client.EnlightenReceive * 100));//81
            stream.Write((uint)0);//62
            stream.Write((uint)client.VipLevel);//88
            stream.Write((ushort)client.MyTitle);//92
            stream.Write(client.BoundConquerPoints);//93
            if (client.SubClass != null)
            {
                stream.Write((byte)client.ActiveSublass);
                stream.Write(client.SubClass.GetHashPoint());
                stream.Write((uint)0);
            }
            else
                stream.ZeroFill(5);
            stream.Write((uint)0);//100
            stream.Write((ushort)client.CountryID);//110
            stream.Write(client.Name, "", client.Spouse);//112
            stream.Finalize(GamePackets.HeroInfo);
            return stream;
        }

    }

}
