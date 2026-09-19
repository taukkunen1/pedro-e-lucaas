using Core;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GameServer.Game.MsgServer
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct InteractQuery
    {
        public static InteractQuery ShallowCopy(InteractQuery item)
        {
            return (InteractQuery)item.MemberwiseClone();
        }

        public int TimeStamp;
        public uint UID;
        public uint OpponentUID;
        public ushort X;
        public ushort Y;
        public MsgAttackPacket.AttackID AtkType;//24
        public ushort SpellID;//28
        public bool KilledMonster
        {
            get { return (SpellID == 1); }
            set { SpellID = (ushort)(value ? 1 : 0); }
        }
        public ushort SpellLevel;//30
        public int Data
        {
            get { fixed (void* ptr = &X) { return *((int*)ptr); } }
            set { fixed (void* ptr = &X) { *((int*)ptr) = value; } }
        }
        public int dwParam
        {
            get { fixed (void* ptr = &SpellLevel) { return *((int*)ptr); } }
            set { fixed (void* ptr = &SpellLevel) { *((int*)ptr) = value; } }
        }
        public uint KillCounter
        {
            get { return SpellLevel; }
            set { SpellLevel = (ushort)value; }
        }
        public int Damage
        {
            get { fixed (void* ptr = &SpellID) { return *((int*)ptr); } }
            set { fixed (void* ptr = &SpellID) { *((int*)ptr) = value; } }
        }
        public bool OnCounterKill
        {

            get { return Damage != 0; }
            set { Damage = value ? 1 : 0; }
        }
        public uint ResponseDamage;//32
        public MsgAttackPacket.AttackEffect Effect;//36
        public uint Unknow;
    }

    public unsafe static class MsgAttackPacket
    {

        [Flags]
        public enum AttackEffect : uint
        {
            None = 0,

            Block = 1 << 0,
            Penetration = 1 << 1,
            CriticalStrike = 1 << 2,
            Imunity = 1 << 3,
            Break = Imunity | Penetration,
            ResistMetal = 1 << 4,
            ResistWood = 1 << 5,
            ResistWater = 1 << 6,
            ResistFire = 1 << 7,
            ResistEarth = 1 << 8,
            AddStudyPoint = 1 << 9,
            LuckyStrike = 1 << 10
        }
        public enum AttackID : uint
        {
            None = 0x00,
            Physical = 0x02,
            Magic = 0x18,
            Archer = 0x1C,
            RequestMarriage = 0x08,
            AcceptMarriage = 0x09,
            Death = 0x0E,
            Reflect = 26,
            Dash = 27,//28?
            UpdateHunterJar = 36,
            CounterKillSwitch = 44,
            Scapegoat = 43,

            FatalStrike = 45,
            InteractionRequest = 46,
            InteractionAccept = 47,
            InteractionRefuse = 48,
            InteractionEffect = 49,
            InteractionStopEffect = 50,
            InMoveSpell = 53,
            BlueDamage = 55,
            BackFire = 57

        }
        public static unsafe void Interaction(this ServerSockets.Packet stream, InteractQuery* pQuery, bool AutoAttack, ushort AttackKey)
        {

            stream.ReadUnsafe(pQuery, sizeof(InteractQuery));



            if (pQuery->AtkType == AttackID.Magic && AutoAttack == false)
            {
                DecodeMagicAttack(pQuery);
            }
        }
        public static unsafe ServerSockets.Packet InteractionCreate(this ServerSockets.Packet stream, InteractQuery* pQuery)
        {
          

            stream.InitWriter();

            stream.WriteUnsafe(pQuery, sizeof(InteractQuery));
            stream.Finalize(GamePackets.Attack);

            return stream;
        }

        /// <summary>
        /// original  from cosv3
        /// </summary>
        /// <param name="pQuery"></param>
        public static unsafe void EncodeMagicAttack(InteractQuery* pQuery)
        {
            int magicType, magicLevel;
            BitUnfold32(pQuery->Damage, out magicType, out magicLevel);

            magicType = (ushort)(ExchangeShortBits((uint)magicType - 0x14be, 3) ^ pQuery->UID ^ 0x915d);
            magicLevel = (ushort)((magicLevel + 0x100 * (pQuery->TimeStamp % 0x100)) ^ 0x3721);

            pQuery->Damage = BitFold32(magicType, magicLevel);
            pQuery->OpponentUID = (uint)ExchangeLongBits((((uint)pQuery->OpponentUID - 0x8b90b51a) ^ (uint)pQuery->UID ^ 0x5f2d2463u), 32 - 13);
            pQuery->X = (ushort)(ExchangeShortBits((uint)pQuery->X - 0xdd12, 1) ^ pQuery->UID ^ 0x2ed6);
            pQuery->Y = (ushort)(ExchangeShortBits((uint)pQuery->Y - 0x76de, 5) ^ pQuery->UID ^ 0xb99b);
        }
        private static unsafe void DecodeMagicAttack(InteractQuery* pQuery)
        {
            int magicType, magicLevel;
            BitUnfold32(pQuery->Damage, out magicType, out magicLevel);

            magicType = (ushort)(ExchangeShortBits(((ushort)magicType ^ (uint)pQuery->UID ^ 0x915d), 16 - 3) + 0x14be);
            magicLevel = (ushort)(((byte)magicLevel) ^ 0x21);

            pQuery->Damage = BitFold32(magicType, magicLevel);
            pQuery->OpponentUID = (uint)((ExchangeLongBits((uint)pQuery->OpponentUID, 13) ^ (uint)pQuery->UID ^ 0x5f2d2463) + 0x8b90b51a);
            pQuery->X = (ushort)(ExchangeShortBits(((ushort)pQuery->X ^ (uint)pQuery->UID ^ 0x2ed6), 16 - 1) + 0xdd12);
            pQuery->Y = (ushort)(ExchangeShortBits(((ushort)pQuery->Y ^ (uint)pQuery->UID ^ 0xb99b), 16 - 5) + 0x76de);
        }
        public static int BitFold32(int lower16, int higher16)
        {
            return (lower16) | (higher16 << 16);
        }
        public static void BitUnfold32(int bits32, out int lower16, out int upper16)
        {
            lower16 = (int)(bits32 & UInt16.MaxValue);
            upper16 = (int)(bits32 >> 16);
        }
        public static void BitUnfold64(ulong bits64, out int lower32, out int upper32)
        {
            lower32 = (int)(bits64 & UInt32.MaxValue);
            upper32 = (int)(bits64 >> 32);
        }
        private static uint ExchangeShortBits(uint data, int bits)
        {
            data &= 0xffff;
            return ((data >> bits) | (data << (16 - bits))) & 0xffff;
        }

        public static uint ExchangeLongBits(uint data, int bits)
        {
            return (data >> bits) | (data << (32 - bits));
        }
        [PacketAttribute(GamePackets.Attack)]
        public static void HandlerProcess(Client.GameClient user, ServerSockets.Packet stream)
        {
            
            
            user.Player.Protect = DateTime.Now;
            user.OnAutoAttack = false;
            user.Player.RemoveBuffersMovements(stream);
            user.Player.Action = Role.Flags.ConquerAction.None;

            if (user.Player.BlockMovementCo)
            {
                if (DateTime.Now < user.Player.BlockMovement)
                {
                    user.SendSysMesage($"You can`t move for {(user.Player.BlockMovement - DateTime.Now).TotalSeconds} seconds.");
                    user.Pullback();
                    return;
                }
                else
                    user.Player.BlockMovementCo = false;
            }

            var attack_obj = new AttackObj();

            InteractQuery Attack;
            stream.Interaction(&Attack, user.OnAutoAttack, (ushort)user.EncryptTokenSpell);

            if (MsgTournaments.MsgSchedules.CaptureTheFlag != null)
                MsgTournaments.MsgSchedules.CaptureTheFlag.ChechMoveFlag(user);

         
            if (user.Player.ActivePick)
                user.Player.RemovePick(stream);

           
            user.Player.LastAttack = DateTime.Now;
            if (user.Player.WaitingKillCaptcha)
            {
                user.Player.KillCountCaptchaStamp = DateTime.Now;
                user.Player.WaitingKillCaptcha = true;
                user.ActiveNpc = 9999997;
                if (user.Player.KillCountCaptcha == "")
                    user.Player.KillCountCaptcha = Role.Core.Random.Next(10000, 50000).ToString();
                Game.MsgNpc.Dialog dialog = new Game.MsgNpc.Dialog(user, stream);
                dialog.Text("Input the current text: " + user.Player.KillCountCaptcha + " to verify your humanity.")
                    .AddInput("Captcha message:", (byte)user.Player.KillCountCaptcha.Length)
                    .Option("No thank you.", 255)
                    .AddAvatar(39)
                    .FinalizeDialog();
                //  user.Send(dialog.stream);
                return;
            }
            if (user != null && Database.ItemType.IsShield(user.Equipment.LeftWeapon) && !(user.Player.Class >= 21 && user.Player.Class <= 25))
            {
                if (user.Inventory.HaveSpace(1))
                {
                    user.Equipment.Remove(Role.Flags.ConquerItem.LeftWeapon, stream);
                    user.Equipment.QueryEquipment(user.Equipment.Alternante);
                }
                else
                    user.SendSysMesage("Remove the shield or free 1 inventory spot.");
                return;
            }
             Process(user, Attack);





            //  Attack.TimeStamp = Attack.PacketStamp = Environment.TickCount;
            //  EncodeMagicAttack(&Attack);



        }


        public static ProcessAttackQueue ProcessAttack = new ProcessAttackQueue();

        public class AttackObj
        {
            public Client.GameClient User;
            public InteractQuery Attack;
        }

        public class ProcessAttackQueue : ConcurrentSmartThreadQueue<AttackObj>
        {
            public ProcessAttackQueue()
                : base(10)
            {
                Start(5);
            }
            public void TryEnqueue(AttackObj action)
            {
                Enqueue(action);
            }

            protected unsafe override void OnDequeue(AttackObj action, int time)
            {
                //  Process(action.User, action.Attack);
            }
        }

        public static void Process(Client.GameClient user, InteractQuery Attack)
        {

            using (var rec = new ServerSockets.RecycledPacket())
            {
                bool OnTGAutoAttack = false;
                var stream = rec.GetStream();

                if (user.Player.Map == 2068 || (user.IsWatching() && user.Player.Map == 700 && user.InQualifier() == false))
                {
                    user.SendSysMesage("Spells are not allowed on this area.");

                    return;
                }
               
                if (!user.Player.Alive && !user.Fake)
                {

                    return;
                }

                if (user.Player.Map != 1039 && user.Player.Map != 1002)//training
                    AttackHandler.CheckAttack.CheckItems.AttackDurability(user, stream);

                if (user.Player.ContainFlag(MsgUpdate.Flags.Freeze))
                {

                    return;
                }
                switch (Attack.AtkType)
                {
                    case AttackID.UpdateHunterJar:
                        {
                            MsgGameItem Jar;
                            if (user.Inventory.TryGetItem(user.DemonExterminator.ItemUID, out Jar))
                            {
                                Attack.UID = Attack.OpponentUID = user.Player.UID;
                                Attack.X = Jar.MaximDurability;
                                Attack.Y = 0;
                                Attack.dwParam = (int)((user.DemonExterminator.HuntKills << 16) | Jar.MaximDurability);

                                user.Send(stream.InteractionCreate(&Attack));
                            }
                            break;
                        }
                    case AttackID.InteractionRequest:
                        {
                            Role.IMapObj Target;
                            if (user.Player.View.TryGetValue(Attack.OpponentUID, out Target, Role.MapObjectType.Player))
                            {
                                Role.Player Opponent = Target as Role.Player;
                                if (user.Player.ObjInteraction == null && Opponent.ObjInteraction == null)
                                {
                                    Opponent.ActiveDance = user.Player.ActiveDance = (ushort)Attack.ResponseDamage;
                                    Opponent.Owner.Send(stream.InteractionCreate(&Attack));
                                    Opponent.Owner.Send(stream.InteractionCreate(&Attack));
                                }
                            }
                            break;
                        }
                    case AttackID.InteractionRefuse:
                        {
                            user.Player.ActiveDance = 0;
                            Role.IMapObj Target;
                            if (user.Player.View.TryGetValue(Attack.OpponentUID, out Target, Role.MapObjectType.Player))
                            {
                                Role.Player Opponent = Target as Role.Player;
                                Opponent.ActiveDance = 0;
                                Opponent.Owner.Send(stream.InteractionCreate(&Attack));
                            }
                            break;
                        }
                    case AttackID.InteractionAccept:
                        {
                            Role.IMapObj Target;
                            if (user.Player.View.TryGetValue(Attack.OpponentUID, out Target, Role.MapObjectType.Player))
                            {
                                Role.Player Opponent = Target as Role.Player;
                                if (user.Player.ObjInteraction == null && Opponent.ObjInteraction == null)
                                {
                                    Attack.ResponseDamage = user.Player.ActiveDance;
                                    Opponent.Owner.Send(stream.InteractionCreate(&Attack));

                                    Attack.TimeStamp = 0;
                                    user.Send(stream.InteractionCreate(&Attack));

                                    user.Player.Action = (Role.Flags.ConquerAction)Attack.Damage;
                                    Opponent.Action = (Role.Flags.ConquerAction)Attack.Damage;

                                    user.Player.ObjInteraction = Opponent.Owner;
                                    Opponent.ObjInteraction = user;

                                }
                            }
                            break;
                        }
                    case AttackID.InteractionEffect:
                        {
                            if (user.Player.ObjInteraction != null)
                            {
                                if (user.Player.ObjInteraction.Player.ObjInteraction != null)
                                {
                                    Attack.ResponseDamage = user.Player.ActiveDance;
                                    Attack.TimeStamp = 0;
                                    CreateInteractionEffect(Attack, user);

                                    InteractQuery user_effect = user.Player.InteractionEffect;

                                    user.Player.View.SendView(stream.InteractionCreate(&user_effect), true);

                                    Attack.UID = user.Player.ObjInteraction.Player.UID;
                                    Attack.OpponentUID = user.Player.UID;

                                    CreateInteractionEffect(Attack, user.Player.ObjInteraction);

                                    user_effect = user.Player.ObjInteraction.Player.InteractionEffect;
                                    user.Player.ObjInteraction.Player.View.SendView(stream.InteractionCreate(&user_effect), true);
                                }
                            }
                            break;
                        }
                    case AttackID.InteractionStopEffect:
                        {
                            Attack.ResponseDamage = user.Player.ActiveDance;
                            user.Player.View.SendView(stream.InteractionCreate(&Attack), true);

                            Attack.UID = Attack.OpponentUID;
                            Attack.OpponentUID = user.Player.UID;
                            user.Player.View.SendView(stream.InteractionCreate(&Attack), true);

                            if (user.Player.ObjInteraction != null)
                            {
                                user.Player.OnInteractionEffect = false;
                                user.Player.Action = Role.Flags.ConquerAction.None;
                                user.Player.ObjInteraction.Player.OnInteractionEffect = false;
                                user.Player.ObjInteraction.Player.Action = Role.Flags.ConquerAction.None;
                                user.Player.ObjInteraction.Player.ObjInteraction = null;
                                user.Player.ObjInteraction = null;
                            }
                            break;
                        }
                    case AttackID.RequestMarriage:
                        {
                            Role.IMapObj Target;
                            if (user.Player.View.TryGetValue(Attack.OpponentUID, out Target, Role.MapObjectType.Player))
                            {
                                Role.Player Opponent = Target as Role.Player;
                                if (user.Player.Spouse != "None" || Opponent.Spouse != "None")
                                {
                                    user.SendSysMesage("You cannot marry someone that is already married with someone else!");
                                    break;
                                }
                                // if (user.Player.Body % 10 <= 2 && Opponent.Body % 10 >= 3 || user.Player.Body % 10 >= 2 && Opponent.Body % 10 <= 3)
                                {
                                    Opponent.Send(stream.PopupInfoCreate(user.Player.UID, Opponent.UID, user.Player.Level, user.Player.BattlePower));

                                    Attack.X = Opponent.X;
                                    Attack.Y = Opponent.Y;

                                    Opponent.Send(stream.InteractionCreate(&Attack));

                                }
                                //else
                                //{
                                //    user.SendSysMesage("You cannot marry someone of your gender!");
                                //}
                            }
                            break;
                        }
                    case AttackID.AcceptMarriage:
                        {
                            Role.IMapObj Target;
                            if (user.Player.View.TryGetValue(Attack.OpponentUID, out Target, Role.MapObjectType.Player))
                            {
                                Role.Player Opponent = Target as Role.Player;
                                if (user.Player.Spouse != "None" || Opponent.Spouse != "None")
                                {
                                    user.SendSysMesage("You cannot marry someone that is already married with someone else!");
                                    break;
                                }
                                // if (user.Player.Body % 10 <= 2 && Opponent.Body % 10 >= 3 || user.Player.Body % 10 >= 2 && Opponent.Body % 10 <= 3)
                                {
                                    user.Player.Spouse = Opponent.Name;
                                    user.Player.SpouseUID = Opponent.UID;

                                    Opponent.Spouse = user.Player.Name;
                                    Opponent.SpouseUID = user.Player.UID;

#if Arabic
                                     MsgMessage messaj = new MsgMessage("Joy and happiness! " + user.Player.Name + " and " + Opponent.Name + " have joined together in the holy marriage. We wish them a stone house.", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);
#else
                                    MsgMessage messaj = new MsgMessage("Joy and happiness! " + user.Player.Name + " and " + Opponent.Name + " have joined together in the holy marriage. We wish them a stone house.", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);
#endif

                                    Program.SendGlobalPackets.Enqueue(messaj.GetArray(stream));

                                    user.Player.SendString(stream, MsgStringPacket.StringID.Spouse, false, new string[1] { user.Player.Spouse });
                                    Opponent.SendString(stream, MsgStringPacket.StringID.Spouse, false, new string[1] { Opponent.Spouse });
                                    user.Player.SendString(stream, MsgStringPacket.StringID.Fireworks, true, new string[1] { "1122" });
                                    //firework-2love
                                    user.Player.SendString(stream, MsgStringPacket.StringID.Effect, true, new string[1] { "firework-2love" });
                                }
                                
                            }
                            break;
                        }
                    #region Physical
                    case AttackID.Physical:
                        {
                            if (!AttackHandler.CheckAttack.CheckLineSpells.CheckUp(user, Attack.SpellID))
                            {
                                return;
                            }

                            AttackHandler.Updates.GetWeaponSpell.CheckExtraEffects(user, stream);
                            if (user.MySpells.ClientSpells.ContainsKey(Attack.SpellID))
                            {
                                InteractQuery AttackPaket = new InteractQuery();
                                Role.IMapObj _target;
                                if (user.Player.View.TryGetValue(Attack.OpponentUID, out _target, Role.MapObjectType.Player)
                                    || user.Player.View.TryGetValue(Attack.OpponentUID, out _target, Role.MapObjectType.Monster)
                                    || user.Player.View.TryGetValue(Attack.OpponentUID, out _target, Role.MapObjectType.SobNpc))
                                {
                                    if (Role.Core.GetDistance(user.Player.X, user.Player.Y, _target.X, _target.Y) <= 3)
                                    {
                                        AttackPaket.X = _target.X;
                                        AttackPaket.Y = _target.Y;
                                        if (user.OnAutoAttack)
                                        {
                                            user.Player.RandomSpell = AttackPaket.SpellID;
                                            AttackPaket.OpponentUID = Attack.OpponentUID;
                                            AttackPaket.AtkType = MsgAttackPacket.AttackID.Magic;

                                            MsgAttackPacket.ProcessMagic(user, stream, AttackPaket);
                                            if (_target.Alive)
                                            {

                                                CreateAutoAtack(Attack, user);
                                            }
                                            else
                                                user.OnAutoAttack = false;
                                            break;
                                        }

                                        if (!AttackHandler.Updates.GetWeaponSpell.Check(Attack, stream, user, _target))
                                        {



                                            AttackPaket.UID = Attack.UID;
                                            AttackPaket.SpellID = Attack.SpellID;
                                            user.Player.RandomSpell = Attack.SpellID;

                                            AttackPaket.AtkType = MsgAttackPacket.AttackID.Magic;
                                            MsgAttackPacket.ProcessMagic(user, stream, AttackPaket);

                                            if (_target.Alive)
                                            {
                                                //AttackPaket.AtkType = MsgAttackPacket.AttackID.Physical;
                                                CreateAutoAtack(Attack, user);
                                            }
                                            else
                                                user.OnAutoAttack = false;

                                        }
                                    }
                                    else
                                        user.OnAutoAttack = false;
                                    break;
                                }
                            }





                            if (user.MySpells.ClientSpells.ContainsKey(Attack.SpellID))
                            {

                                InteractQuery AttackPaket = new InteractQuery();
                                Role.IMapObj _target;
                                if (user.Player.View.TryGetValue(Attack.OpponentUID, out _target, Role.MapObjectType.Player)
                                    || user.Player.View.TryGetValue(Attack.OpponentUID, out _target, Role.MapObjectType.Monster)
                                    || user.Player.View.TryGetValue(Attack.OpponentUID, out _target, Role.MapObjectType.SobNpc))
                                {
                                    if (Role.Core.GetDistance(user.Player.X, user.Player.Y, _target.X, _target.Y) <= 3)
                                    {
                                        AttackPaket.X = _target.X;
                                        AttackPaket.Y = _target.Y;
                                        if (user.OnAutoAttack)
                                        {
                                            List<ushort> CanUse = new List<ushort>();
                                            AttackPaket.SpellID = (ushort)CanUse[Pool.GetRandom.Next(0, CanUse.Count)];
                                            user.Player.RandomSpell = AttackPaket.SpellID;
                                            AttackPaket.OpponentUID = Attack.OpponentUID;
                                            AttackPaket.AtkType = MsgAttackPacket.AttackID.Magic;
                                            MsgServer.MsgAttackPacket.ProcessMagic(user, stream, AttackPaket);
                                            if (_target.Alive)
                                            {
                                                //AttackPaket.AtkType = MsgAttackPacket.AttackID.Physical;
                                                CreateAutoAtack(Attack, user);
                                            }
                                            else
                                                user.OnAutoAttack = false;
                                            break;
                                        }

                                        if (!AttackHandler.Updates.GetWeaponSpell.Check(Attack, stream, user, _target))
                                        {

                                            AttackPaket.X = _target.X;
                                            AttackPaket.Y = _target.Y;
                                            AttackPaket.OpponentUID = _target.UID;
                                            AttackPaket.UID = Attack.UID;
                                            AttackPaket.SpellID = Attack.SpellID;
                                            user.Player.RandomSpell = Attack.SpellID;

                                            AttackPaket.AtkType = MsgAttackPacket.AttackID.Magic;
                                            MsgServer.MsgAttackPacket.ProcessMagic(user, stream, AttackPaket);

                                            if (_target.Alive)
                                                CreateAutoAtack(Attack, user);
                                            else
                                                user.OnAutoAttack = false;
                                        }
                                    }
                                    else
                                        user.OnAutoAttack = false;
                                    break;
                                }
                            }
                            if (user.MySpells.ClientSpells.ContainsKey(Attack.SpellID))
                            {

                                InteractQuery AttackPaket = new InteractQuery();
                                Role.IMapObj _target;
                                if (user.Player.View.TryGetValue(Attack.OpponentUID, out _target, Role.MapObjectType.Player)
                                    || user.Player.View.TryGetValue(Attack.OpponentUID, out _target, Role.MapObjectType.Monster)
                                    || user.Player.View.TryGetValue(Attack.OpponentUID, out _target, Role.MapObjectType.SobNpc))
                                {
                                    if (Role.Core.GetDistance(user.Player.X, user.Player.Y, _target.X, _target.Y) <= 3)
                                    {
                                        AttackPaket.X = _target.X;
                                        AttackPaket.Y = _target.Y;
                                        if (user.OnAutoAttack)
                                        {
                                            List<ushort> CanUse = new List<ushort>();
                                            AttackPaket.SpellID = (ushort)CanUse[Pool.GetRandom.Next(0, CanUse.Count)];
                                            user.Player.RandomSpell = AttackPaket.SpellID;
                                            AttackPaket.OpponentUID = Attack.OpponentUID;
                                            AttackPaket.AtkType = MsgAttackPacket.AttackID.Magic;

                                            MsgServer.MsgAttackPacket.ProcessMagic(user, stream, AttackPaket);
                                            if (_target.Alive)
                                            {
                                                //AttackPaket.AtkType = MsgAttackPacket.AttackID.Physical;
                                                CreateAutoAtack(Attack, user);
                                            }
                                            else
                                                user.OnAutoAttack = false;
                                            break;
                                        }


                                        AttackPaket.X = _target.X;
                                        AttackPaket.Y = _target.Y;
                                        AttackPaket.OpponentUID = _target.UID;
                                        AttackPaket.UID = Attack.UID;
                                        AttackPaket.SpellID = Attack.SpellID;
                                        user.Player.RandomSpell = Attack.SpellID;

                                        AttackPaket.AtkType = MsgAttackPacket.AttackID.Magic;
                                        MsgServer.MsgAttackPacket.ProcessMagic(user, stream, AttackPaket);

                                        if (_target.Alive)
                                            CreateAutoAtack(Attack, user);
                                        else
                                            user.OnAutoAttack = false;
                                        break;
                                    }
                                    else
                                        user.OnAutoAttack = false;

                                    break;
                                }
                            }
                            if (user.MySpells.ClientSpells.ContainsKey(Attack.SpellID))
                            {
                                InteractQuery AttackPaket = new InteractQuery();
                                Role.IMapObj _target;
                                if (user.Player.View.TryGetValue(Attack.OpponentUID, out _target, Role.MapObjectType.Player)
                                    || user.Player.View.TryGetValue(Attack.OpponentUID, out _target, Role.MapObjectType.Monster)
                                    || user.Player.View.TryGetValue(Attack.OpponentUID, out _target, Role.MapObjectType.SobNpc))
                                {
                                    if (Role.Core.GetDistance(user.Player.X, user.Player.Y, _target.X, _target.Y) <= 3)
                                    {
                                        AttackPaket.X = _target.X;
                                        AttackPaket.Y = _target.Y;
                                        if (user.OnAutoAttack)
                                        {
                                            List<ushort> CanUse = new List<ushort>();
                                            if (user.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.TripleAttack))
                                                CanUse.Add((ushort)Role.Flags.SpellID.TripleAttack);
                                            AttackPaket.SpellID = (ushort)CanUse[Pool.GetRandom.Next(0, CanUse.Count)];
                                            user.Player.RandomSpell = AttackPaket.SpellID;
                                            AttackPaket.OpponentUID = Attack.OpponentUID;
                                            AttackPaket.AtkType = MsgAttackPacket.AttackID.Magic;

                                            MsgServer.MsgAttackPacket.ProcessMagic(user, stream, AttackPaket);
                                            if (_target.Alive)
                                            {
                                                //AttackPaket.AtkType = MsgAttackPacket.AttackID.Physical;
                                                CreateAutoAtack(Attack, user);
                                            }
                                            else
                                                user.OnAutoAttack = false;
                                            break;
                                        }
                                        AttackPaket.X = _target.X;
                                        AttackPaket.Y = _target.Y;
                                        AttackPaket.OpponentUID = _target.UID;

                                        AttackPaket.UID = Attack.UID;

                                        if (AttackHandler.Calculate.Base.Success(30) && user.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.TripleAttack))
                                        {
                                            AttackPaket.SpellID = (ushort)Role.Flags.SpellID.TripleAttack;
                                            user.Player.RandomSpell = AttackPaket.SpellID;
                                        }
                                        AttackPaket.AtkType = MsgAttackPacket.AttackID.Magic;
                                        MsgServer.MsgAttackPacket.ProcessMagic(user, stream, AttackPaket);


                                        if (_target.Alive)
                                            CreateAutoAtack(Attack, user);
                                        else
                                            user.OnAutoAttack = false;
                                        break;
                                    }
                                    else
                                        user.OnAutoAttack = false;

                                    break;
                                }
                            }



                            DateTime Timer = DateTime.Now;
                            Role.IMapObj target;
                            bool TCBoss = false;
                            if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Monster))
                            {
                                MsgMonster.MonsterRole attacked = target as MsgMonster.MonsterRole;

                            }
                            if (TCBoss)
                            {
                                if (Timer < user.Player.AttackStamp.AddMilliseconds(500))
                                    return;
                            }
                            else
                            {
                                if (Timer < user.Player.AttackStamp.AddMilliseconds(user.Equipment.AttackSpeed(true)))
                                {
                                    return;
                                }
                            }
                            user.Player.AttackStamp = Timer;



                            MsgServer.AttackHandler.CheckAttack.CheckGemEffects.TryngEffect(user);
                            if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Player))
                            {
                                if (!AttackHandler.Updates.GetWeaponSpell.Check(Attack, stream, user, target))
                                {
                                    if (AttackHandler.Calculate.Base.GetDistance(user.Player.X, user.Player.Y, target.X, target.Y) <= 3)
                                    {
                                        Role.Player attacked = target as Role.Player;
                                        if (AttackHandler.CheckAttack.CanAttackPlayer.Verified(user, attacked, null))
                                        {
                                            Attack.TimeStamp = 0;

                                            MsgSpellAnimation.SpellObj AnimationObj;
                                            AttackHandler.Calculate.Physical.OnPlayer(user.Player, attacked, null, out AnimationObj);



                                            Attack.Damage = (int)AnimationObj.Damage;
                                            Attack.Effect = AnimationObj.Effect;


                                            user.Player.View.SendView(stream.InteractionCreate(&Attack), true);

                                            Attack.AtkType = AttackID.Physical;



                                            AttackHandler.ReceiveAttack.Player.Execute(AnimationObj, user, attacked);

                                            if (attacked.Alive)
                                                CreateAutoAtack(Attack, user);
                                        }
                                        else
                                            user.OnAutoAttack = false;
                                    }
                                    else
                                        user.OnAutoAttack = false;
                                }
                            }
                            else if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Monster))
                            {
                                if (!AttackHandler.Updates.GetWeaponSpell.Check(Attack, stream, user, target))
                                {
                                    MsgMonster.MonsterRole attacked = target as MsgMonster.MonsterRole;
                                    if (AttackHandler.Calculate.Base.GetDistance(user.Player.X, user.Player.Y, target.X, target.Y) <= 3 || user.Player.ContainFlag(MsgUpdate.Flags.FatalStrike))
                                    {

                                        if (AttackHandler.CheckAttack.CanAttackMonster.Verified(user, attacked, null))
                                        {
                                            MsgSpellAnimation.SpellObj AnimationObj;
                                            Attack.TimeStamp = 0;

                                            if (user.Player.ContainFlag(MsgUpdate.Flags.FatalStrike))
                                            {
                                                Attack.AtkType = AttackID.FatalStrike;
                                                user.Shift(target.X, target.Y, stream);
                                                AttackHandler.Calculate.Physical.OnMonster(user.Player, attacked, Pool.Magic[(ushort)Role.Flags.SpellID.FatalStrike][0], out AnimationObj);
                                            }
                                            else
                                                AttackHandler.Calculate.Physical.OnMonster(user.Player, attacked, null, out AnimationObj);

                                            Attack.Damage = (int)AnimationObj.Damage;
                                            Attack.Effect = AnimationObj.Effect;

                                            user.Player.View.SendView(stream.InteractionCreate(&Attack), true);

                                            Attack.AtkType = AttackID.Physical;
                                            AttackHandler.Updates.IncreaseExperience.Up(stream, user, AttackHandler.ReceiveAttack.Monster.Execute(stream, AnimationObj, user, attacked));
                                            AttackHandler.Updates.UpdateSpell.CheckUpdate(stream, user, Attack, AnimationObj.Damage, null);
                                            CreateAutoAtack(Attack, user);
                                        }
                                        else
                                            user.OnAutoAttack = false;
                                    }
                                    else
                                        user.OnAutoAttack = false;
                                }
                            }
                            else if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.SobNpc))
                            {

                                if (!AttackHandler.Updates.GetWeaponSpell.Check(Attack, stream, user, target))
                                {
                                    if (AttackHandler.Calculate.Base.GetDistance(user.Player.X, user.Player.Y, target.X, target.Y) <= 3)
                                    {
                                        var attacked = target as Role.SobNpc;
                                        if (AttackHandler.CheckAttack.CanAttackNpc.Verified(user, attacked, null))
                                        {
                                            Attack.TimeStamp = 0;

                                            MsgSpellAnimation.SpellObj AnimationObj;
                                            AttackHandler.Calculate.Physical.OnNpcs(user.Player, attacked, null, out AnimationObj);

                                            Attack.Damage = (int)AnimationObj.Damage;
                                            Attack.Effect = AnimationObj.Effect;
                                            user.Player.View.SendView(stream.InteractionCreate(&Attack), true);


                                            Attack.AtkType = AttackID.Physical;

                                            AttackHandler.Updates.IncreaseExperience.Up(stream, user, AttackHandler.ReceiveAttack.Npc.Execute(stream, AnimationObj, user, attacked));
                                            AttackHandler.Updates.UpdateSpell.CheckUpdate(stream, user, Attack, AnimationObj.Damage, null);
                                            if (attacked.Alive)
                                                CreateAutoAtack(Attack, user);
                                        }
                                        else
                                            user.OnAutoAttack = false;
                                    }
                                    else
                                        user.OnAutoAttack = false;
                                }

                            }
                            else
                                user.OnAutoAttack = false;
                            break;
                        }
                    #endregion
                    case AttackID.Archer:
                        {
                            OnTGAutoAttack = true;
                            if (!AttackHandler.CheckAttack.CheckLineSpells.CheckUp(user, Attack.SpellID))
                            {
                                return;
                            }
                            AttackHandler.Updates.GetWeaponSpell.CheckExtraEffects(user, stream);

                            DateTime Timer = DateTime.Now;
                            Role.IMapObj target;
                            if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Monster))
                            {

                                if (AttackHandler.Calculate.Base.GetDistance(user.Player.X, user.Player.Y, target.X, target.Y) <= 18)
                                {
                                    MsgMonster.MonsterRole attacked = target as MsgMonster.MonsterRole;
                                }
                            }
                            if (Timer < user.Player.AttackStamp.AddMilliseconds(700))
                                return;
                            user.Player.AttackStamp = Timer;
                            MsgServer.AttackHandler.CheckAttack.CheckGemEffects.TryngEffect(user);

                            if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Player))
                            {

                                if (AttackHandler.Calculate.Base.GetDistance(user.Player.X, user.Player.Y, target.X, target.Y) <= 18)
                                {
                                    Role.Player attacked = target as Role.Player;
                                    if (AttackHandler.CheckAttack.CanAttackPlayer.Verified(user, attacked, null, true))
                                    {
                                        MsgSpellAnimation.SpellObj AnimationObj;
                                        AttackHandler.Calculate.Range.OnPlayer(user.Player, attacked, null, out AnimationObj);
                                        Attack.Damage = (int)AnimationObj.Damage;
                                        Attack.Effect = AnimationObj.Effect;
                                        user.Player.View.SendView(stream.InteractionCreate(&Attack), true);
                                        Attack.AtkType = AttackID.Archer;
                                        CreateAutoAtack(Attack, user);
                                        AttackHandler.ReceiveAttack.Player.Execute(AnimationObj, user, attacked);
                                    }
                                    else
                                        user.OnAutoAttack = false;
                                }
                                else
                                    user.OnAutoAttack = false;

                            }
                            else if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Monster))
                            {

                                if (AttackHandler.Calculate.Base.GetDistance(user.Player.X, user.Player.Y, target.X, target.Y) <= 18)
                                {
                                    MsgMonster.MonsterRole attacked = target as MsgMonster.MonsterRole;
                                    if (AttackHandler.CheckAttack.CanAttackMonster.Verified(user, attacked, null))
                                    {
                                        MsgSpellAnimation.SpellObj AnimationObj;
                                        AttackHandler.Calculate.Range.OnMonster(user.Player, attacked, null, out AnimationObj);
                                        Attack.Damage = (int)AnimationObj.Damage;
                                        Attack.Effect = AnimationObj.Effect;
                                        user.Player.View.SendView(stream.InteractionCreate(&Attack), true);

                                        Attack.AtkType = AttackID.Archer;
                                        AttackHandler.Updates.IncreaseExperience.Up(stream, user, AttackHandler.ReceiveAttack.Monster.Execute(stream, AnimationObj, user, attacked));
                                        CreateAutoAtack(Attack, user);
                                        AttackHandler.Updates.UpdateSpell.CheckUpdate(stream, user, Attack, AnimationObj.Damage, null);
                                    }
                                    else
                                        user.OnAutoAttack = false;
                                }
                                else
                                    user.OnAutoAttack = false;

                            }
                            else if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.SobNpc))
                            {

                                if (AttackHandler.Calculate.Base.GetDistance(user.Player.X, user.Player.Y, target.X, target.Y) <= 18)
                                {
                                    var attacked = target as Role.SobNpc;
                                    if (AttackHandler.CheckAttack.CanAttackNpc.Verified(user, attacked, null))
                                    {
                                        MsgSpellAnimation.SpellObj AnimationObj;
                                        AttackHandler.Calculate.Range.OnNpcs(user.Player, attacked, null, out AnimationObj);

                                        Attack.Damage = (int)AnimationObj.Damage;
                                        Attack.Effect = AnimationObj.Effect;

                                        user.Player.View.SendView(stream.InteractionCreate(&Attack), true);

                                        Attack.AtkType = AttackID.Archer;
                                        CreateAutoAtack(Attack, user);
                                        AttackHandler.Updates.IncreaseExperience.Up(stream, user, AttackHandler.ReceiveAttack.Npc.Execute(stream, AnimationObj, user, attacked));
                                        AttackHandler.Updates.UpdateSpell.CheckUpdate(stream, user, Attack, AnimationObj.Damage, null);
                                    }
                                    else
                                        user.OnAutoAttack = false;
                                }
                                else
                                    user.OnAutoAttack = false;

                            }
                            else
                                user.OnAutoAttack = false;

                            break;
                        }
                    case AttackID.CounterKillSwitch:
                        {
                            if (!AttackHandler.CheckAttack.CheckLineSpells.CheckUp(user, Attack.SpellID))
                            {
                                return;
                            }
                            Dictionary<ushort, Database.MagicType.Magic> Spells;
                            if (Pool.Magic.TryGetValue((ushort)Role.Flags.SpellID.CounterKill, out Spells))
                            {
                                MsgSpell ClientSpell;
                                if (user.MySpells.ClientSpells.TryGetValue((ushort)Role.Flags.SpellID.CounterKill, out ClientSpell))
                                {
                                    Database.MagicType.Magic spell;
                                    if (Spells.TryGetValue(ClientSpell.Level, out spell))
                                    {
                                        switch (spell.Type)
                                        {
                                            case Database.MagicType.MagicSort.CounterKill:
                                                {
                                                    Database.MagicType.Magic DBSpell;

                                                    Attack.SpellID = (ushort)Role.Flags.SpellID.CounterKill;
                                                    Attack.SpellLevel = ClientSpell.Level;

                                                    if (AttackHandler.CheckAttack.CanUseSpell.Verified(Attack, user, Spells, out ClientSpell, out DBSpell))
                                                    {
                                                        user.Player.ActivateCounterKill = !user.Player.ActivateCounterKill;
                                                        Attack.OnCounterKill = user.Player.ActivateCounterKill;
                                                        user.Send(stream.InteractionCreate(&Attack));
                                                    }
                                                    break;
                                                }
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    case AttackID.Magic:
                        {

                            ProcessMagic(user, stream, Attack);
                            break;
                        }
                }

                if (user.Player.Map == 1039 && OnTGAutoAttack)
                {
                    CreateAutoAtack(Attack, user);
                }
            }
        }
        public static void ProcessMagic(Client.GameClient user, ServerSockets.Packet stream, InteractQuery Attack, bool ignoreStamp = false)
        {
            if (!AttackHandler.CheckAttack.CheckLineSpells.CheckUp(user, Attack.SpellID))
            {
                return;
            }
            MsgServer.AttackHandler.CheckAttack.CheckGemEffects.TryngEffect(user);
            if (!user.AllowUseSpellOnSteed(Attack.SpellID))
            {
                user.Player.RemoveFlag(MsgUpdate.Flags.Ride);

                return;
            }
           
            bool OnTGAutoAttack = true;
            Dictionary<ushort, Database.MagicType.Magic> Spells;
            if (Pool.Magic.TryGetValue(Attack.SpellID, out Spells))
            {

                Database.MagicType.Magic spell;
                if (Spells.TryGetValue(Attack.SpellLevel, out spell))
                {
                    DateTime Timer = DateTime.Now;
                    if (spell.CoolDown == 0)
                        spell.CoolDown = 500;
                    if (ignoreStamp == false)
                    {

                        if (Timer < user.Player.AttackStamp.AddMilliseconds(spell.CustomCoolDown))//spell.CustomCoolDown))

                        {

                            return;
                        }
                       
                        user.Player.AttackStamp = Timer;
                    }
                    //if (user.OnAutoAttack == false)
                    {
                        if (user.Equipment.RightWeaponEffect == Role.Flags.ItemEffect.MP)
                        {
                            if (spell != null && spell.UseMana > 0)
                            {
                                if (Attack.SpellID == (ushort)Role.Flags.SpellID.EffectMP || AttackHandler.Calculate.Base.Success(30))
                                {
                                    user.Player.RandomSpell = 1175;
                                    AttackHandler.EffectMP.Execute(Attack, user, stream, Spells);

                                }
                            }

                        }
                        AttackHandler.Updates.GetWeaponSpell.CheckExtraEffects(user, stream);


                    }

                    if (user.ProjectManager)
                    {
                        user.SendSysMesage($"[MsgAttackPacket] use skill {spell.ID} [Type: {spell.Type}]");
                    }
                    switch (spell.Type)
                    {
                        case Database.MagicType.MagicSort.AddMana: AttackHandler.AddMana.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.MortalDrag: AttackHandler.MortalDrag.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.ChargingVortex: AttackHandler.ChargingVortex.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.PirateXpSkill: AttackHandler.PirateXpSkill.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.AddBlackSpot: AttackHandler.AddBlackSpot.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.MoveLine: AttackHandler.MoveLine.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.BombLine: AttackHandler.BombLine.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.BlackSpot: AttackHandler.BlackSpot.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.CannonBarrage: AttackHandler.CannonBarrage.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.ScurvyBomb: AttackHandler.ScurvyBomb.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.PhysicalSpells: AttackHandler.PhysicalSpells.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.WhirlwindKick: AttackHandler.WhirlwindKick.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.Oblivion: AttackHandler.Oblivion.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.DispatchXp: AttackHandler.DispatchXp.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.ShieldBlock: AttackHandler.ShieldBlock.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.Compasion:
                        case Database.MagicType.MagicSort.Tranquility:
                        case Database.MagicType.MagicSort.RemoveBuffers:
                            {

                                AttackHandler.RemoveBuffers.Execute(user, Attack, stream, Spells);
                                break;
                            }
                        case Database.MagicType.MagicSort.Perimeter: AttackHandler.Perimeter.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.Auras: AttackHandler.Auras.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.DirectAttack: AttackHandler.DirectAttack.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.DragonWhirl: AttackHandler.DragonWhirl.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.StarArrow: AttackHandler.StarArrow.ExecuteExecute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.ChainBolt: AttackHandler.ChainBolt.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.Spook: AttackHandler.Spook.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.WarCry: AttackHandler.WarCry.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.Riding: AttackHandler.Riding.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.ShurikenVortex: AttackHandler.ShurikenVortex.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.Toxic: AttackHandler.Toxic.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.DecLife: AttackHandler.DecLife.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.Transform: AttackHandler.Transform.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.AttackStatus: AttackHandler.AttackStatus.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.Collide: AttackHandler.Collide.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.Sector: AttackHandler.Sector.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.Line: AttackHandler.Line.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.Attack: AttackHandler.Attack.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.AttachStatus:
                            {
                                OnTGAutoAttack = false;
                                AttackHandler.AttachStatus.Execute(user, Attack, stream, Spells); break;
                            }
                        case Database.MagicType.MagicSort.DetachStatus: AttackHandler.DetachStatus.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.Recruit: AttackHandler.Recruit.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.Bomb: AttackHandler.Bomb.Execute(user, Attack, stream, Spells); break;
                        case Database.MagicType.MagicSort.Pounce: AttackHandler.Pounce.Execute(user, Attack, stream, Spells); break;
						case Database.MagicType.MagicSort.SectorBack: AttackHandler.Assassin.Execute(user, Attack, stream, Spells); break;
						case Database.MagicType.MagicSort.BladeFlurry: AttackHandler.Assassin.Execute(user, Attack, stream, Spells); break;
						case Database.MagicType.MagicSort.LayTrap: AttackHandler.Assassin.Execute(user, Attack, stream, Spells); break;
						case Database.MagicType.MagicSort.KineticSpark: AttackHandler.Assassin.Execute(user, Attack, stream, Spells); break;
					}
                }
            }
            if (user.Player.Map == 1039 && OnTGAutoAttack)
            {
                CreateAutoAtack(Attack, user);
            }
        }

        public static void CreateAutoAtack(InteractQuery pQuery, Client.GameClient client)
        {
            client.OnAutoAttack = true;
            client.AutoAttack = new InteractQuery();
            client.AutoAttack.AtkType = pQuery.AtkType;
            client.AutoAttack.Damage = pQuery.Damage;
            client.AutoAttack.Data = pQuery.Data;
            client.AutoAttack.dwParam = pQuery.dwParam;
            client.AutoAttack.Effect = pQuery.Effect;
            client.AutoAttack.OpponentUID = pQuery.OpponentUID;
            client.AutoAttack.ResponseDamage = pQuery.ResponseDamage;
            client.AutoAttack.SpellID = pQuery.SpellID;
            client.AutoAttack.SpellLevel = pQuery.SpellLevel;
            client.AutoAttack.UID = pQuery.UID;
            client.AutoAttack.X = pQuery.X;
            client.AutoAttack.Y = pQuery.Y;
        }

        public static void CreateInteractionEffect(InteractQuery pQuery, Client.GameClient client)
        {
            client.Player.OnInteractionEffect = true;

            client.Player.InteractionEffect = new InteractQuery();
            client.Player.InteractionEffect.AtkType = pQuery.AtkType;
            client.Player.InteractionEffect.Damage = pQuery.Damage;
            client.Player.InteractionEffect.Data = pQuery.Data;
            client.Player.InteractionEffect.dwParam = pQuery.dwParam;
            client.Player.InteractionEffect.Effect = pQuery.Effect;
            client.Player.InteractionEffect.OpponentUID = pQuery.OpponentUID;
            client.Player.InteractionEffect.ResponseDamage = pQuery.ResponseDamage;
            client.Player.InteractionEffect.SpellID = pQuery.SpellID;
            client.Player.InteractionEffect.SpellLevel = pQuery.SpellLevel;
            client.Player.InteractionEffect.UID = pQuery.UID;
            client.Player.InteractionEffect.X = pQuery.X;
            client.Player.InteractionEffect.Y = pQuery.Y;
        }

    }
}
