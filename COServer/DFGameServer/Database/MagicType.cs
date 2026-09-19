using System;
using System.Collections.Generic;
using System.IO;

namespace GameServer.Database
{
    public class MagicType : Dictionary<ushort, Dictionary<ushort, MagicType.Magic>>
    {
        public enum WeaponsType : ushort
        {
            Boxing = 0,
            Blade = 410,
            Sword = 420,
            Backsword = 421,
            Hook = 430,
            Whip = 440,
            Mace = 441,
            Axe = 450,
            Hammer = 460,
            Crutch = 470,
            Club = 480,
            Scepter = 481,
            Dagger = 490,
            Prod = 491,
            Fan = 492,
            Flute = 493,
            Glaive = 510,
            Scythe = 511,
            Epee = 520,
            Zither = 521,
            Lute = 522,
            Poleaxe = 530,
            LongHammer = 540,
            //Scythe = 550,
            Spear = 560,
            Pickaxe = 562,
            Spade = 570,
            Halbert = 580,
            Wand = 561,
            Bow = 500,
            NinjaSword = 601,
            ThrowingKnife = 613,
            //Boxing = 700,
            //Blade = 710,
            //Sword = 720,
            MagicSword = 721,
            //Hook = 730,
            //Whip = 740,
            //Mace = 741,
            //Axe = 750,
            //Hammer = 760,
            //Crutch = 770,
            //Club = 780,
            //Scepter = 781,
            //Dagger = 790,
            //Prod = 791,
            //Fan = 792,
            //Flute = 793,
            Shield = 900,
            Other = 422,
            PrayerBeads = 610,
            Rapier = 611,
            Pistol = 612,
            CrossSaber = 614,
            TwistedClaw = 616,
            Nunchaku = 617,
            Fist = 624,
            WindFan = 626
        }
        public enum MagicSort
        {

            Attack = 1,
            Recruit = 2,
            Cross = 3,
            Sector = 4,
            Bomb = 5,
            AttachStatus = 6,
            DetachStatus = 7,
            Square = 8,
            JumpAttack = 9,
            RandomTransport = 10,
            DispatchXp = 11,
            Collide = 12,
            SerialCut = 13,
            Line = 14,
            AtkRange = 15,
            AttackStatus = 16,
            CallTeamMember = 17,
            RecordTransportSpell = 18,
            Transform = 19,
            AddMana = 20,
            LayTrap = 21,
            Dance = 22,
            CallPet = 23,
            Vampire = 24,
            Instead = 25,
            DecLife = 26,
            Toxic = 27,
            ShurikenVortex = 28,
            CounterKill = 29,

            Spook = 30,
            WarCry = 31,
            Riding = 32,

            ChainBolt = 37,
            StarArrow = 38,
            DragonWhirl = 40,
            RemoveBuffers = 46,
            Tranquility = 47,
            DirectAttack = 48,
            Compasion = 50,
            Auras = 51,
            ShieldBlock = 52,
            Oblivion = 53,
            WhirlwindKick = 54,
            PhysicalSpells = 55,
            ScurvyBomb = 56,
            CannonBarrage = 57,
            BlackSpot = 58,
            Summon = 72,
            BombLine = 60,
            MoveLine = 61,
            AddBlackSpot = 62,
            PirateXpSkill = 63,
            ChargingVortex = 64,
            MortalDrag = 65,
            KineticSpark = 67,
            BladeFlurry = 68,
            SectorBack = 69,
            BreathFocus = 70,
            FatalCross = 71,
            Summons = 72,
            FatalSpin = 73,
            DragonCyclone = 75,
            StraightFist = 76,
            Perimeter = 78,
            AirKick = 79,
            Strike = 81,
            SectorPasive = 82,
            ManiacDance = 83,
            Omnipotence = 85,
            Pounce = 84,
            BurntFrost = 86,

            Rectangle = 87,
            RemoveStamin = 88,
            PetAttachStatus = 89,
            SwirlingStorm = 91
        }

        public static List<Role.Flags.SpellID> RandomSpells = new List<Role.Flags.SpellID>()
        {
           Role.Flags.SpellID.Phoenix, Role.Flags.SpellID.Rage, Role.Flags.SpellID.Boreas, Role.Flags.SpellID.Earthquake
           , Role.Flags.SpellID.Halt,Role.Flags.SpellID.Phoenix
           , Role.Flags.SpellID.Rage, Role.Flags.SpellID.Roamer, Role.Flags.SpellID.Seizer, Role.Flags.SpellID.Snow
           , Role.Flags.SpellID.TripleAttack, Role.Flags.SpellID.WideStrike, Role.Flags.SpellID.Windstorm, Role.Flags.SpellID.Poison
        };

        public void Load()
        {
            string magictypeDatPath = Path.Combine(ServerConfig.DbLocation, "magictype.dat");
            if (File.Exists(magictypeDatPath))
            {
                Console.WriteLine("[Detected magictype.dat] You need apply the changes from magictype.dat to magictype.txt? [Y/N]", ConsoleColor.DarkYellow);
                string keyYN = Console.ReadLine();
                if (keyYN.ToLower() == "y")
                {
                    Core.Utils.DecryptCommonDat(magictypeDatPath);
                    System.Threading.Thread.Sleep(1000);
                    File.Delete(magictypeDatPath);
                }
            }
            if (File.Exists(Path.Combine(ServerConfig.DbLocation, "magictype.txt")))
            {
                using (StreamReader read = File.OpenText(Path.Combine(ServerConfig.DbLocation, "magictype.txt")))
                {
                    string Linebas;
                    while ((Linebas = read.ReadLine()) != null)
                    {
                        string[] line = Linebas.Split(new string[] { "@@" }, StringSplitOptions.RemoveEmptyEntries);

                        Magic spell = new Magic();

                        spell.ID = ushort.Parse(line[1]);
                        spell.AttackInFly = AttackInFlay(spell);
                        spell.Type = (MagicSort)byte.Parse(line[2]);
                        spell.Name = line[3];
                        spell.Level = byte.Parse(line[8]);
                        spell.UseMana = ushort.Parse(line[9]);
                        if (spell.ID == 1190 || spell.ID == 1195 || spell.ID == 1005 || spell.ID == 1175 || spell.ID == 1055 || spell.ID == 1170)
                            spell.Damage = uint.Parse(line[10]);
                        else if (spell.ID == (ushort)Role.Flags.SpellID.ToxicFog)
                            spell.Damage = (byte)((double.Parse(line[10]) % 100));
                        else
                            spell.Damage = (byte)((double.Parse(line[10]) % 1000));
                        spell.CoolDown = ushort.Parse(line[33]);//(ushort)Math.Max(500, uint.Parse(line[11]));
                        spell.Rate = int.Parse(line[12]);
                        spell.MaxTargets = byte.Parse(line[14]);
                        spell.Experience = uint.Parse(line[18]);
                        spell.NeedLevel = ushort.Parse(line[20]);
                        spell.WeaponType = uint.Parse(line[22]);
                        spell.UseStamina = byte.Parse(line[29]);
                        if (spell.ID == (ushort)Role.Flags.SpellID.ToxicFog)
                            spell.Duration = 20;
                        else
                            spell.Duration = uint.Parse(line[13]);
                        spell.AutoLearn = byte.Parse(line[30]);
                        spell.UseArrows = uint.Parse(line[34]);
                        spell.Damage2 = int.Parse(line[36]);
                        spell.Damage3 = int.Parse(line[37]);

                        spell.DamageOnMonster = int.Parse(line[44]);
                        spell.ColdTime = int.Parse(line[47]);
                        spell.CpsCostRatio = uint.Parse(line[48]);
                        if (spell.ID == 8001)
                            spell.UseArrows = 1;
                      
                        spell.Range = byte.Parse(line[15]);
                        if (spell.ID == 1055)
                        {
                            Console.WriteLine("Range for Nectar Skill:" + spell.Range);
                        }
                        spell.Status = int.Parse(line[16]);
                        spell.Sector = (ushort)(spell.Range * 20);
                        if (spell.ID == 10415)
                            spell.UseStamina = 30;
                        if (spell.ID == 10381 || spell.ID == 1115)
                            spell.CustomCoolDown = 800;
                        if (spell.ID == 1260)
                            spell.CustomCoolDown = 1000;
                        if (spell.ID == 6002)
                            spell.CustomCoolDown = 5000;
                        if (spell.ID == 30000)
                        {
                            spell.CustomCoolDown = 1500;
                            spell.UseStamina = 50;
                        }
                        if (spell.ID == 10415 || spell.ID == 1180)
                            spell.CustomCoolDown = 500;
                        if (/*spell.ID >= 1000 && spell.ID <= 1002 */spell.ID == 1165 || spell.ID == 10309 || spell.ID == 1010)
                            spell.CustomCoolDown = 620;
                        if(spell.ID == 11110)
                        {
                            spell.CustomCoolDown = 1200;
                        }
                        if (spell.ID >= 1002)
                            spell.CustomCoolDown = 620;
                        if (spell.ID >= 1120 && spell.ID <= 1180)
                            spell.CustomCoolDown = 750;
                        if (spell.ID == 8001)
                            spell.CustomCoolDown = 50;
                        if (spell.ID == 1045 || spell.ID == 1046 || spell.ID == 11000 || spell.ID == 11005 || spell.ID == 11110)
                            spell.CustomCoolDown = 500;
                        if (spell.ID == 10381 || spell.ID == 6000)// Twofoldblades & Radiant Palm.
                            spell.CustomCoolDown = 650;
                        if (spell.ID == 10415)
                            spell.CustomCoolDown = 800;
                        if (spell.WeaponType != 0)
                        {
                            if (spell.WeaponType > 100000)
                            {
                                ushort profone, proftwo = 0;
                                profone = ushort.Parse(spell.WeaponType.ToString()[0].ToString() + spell.WeaponType.ToString()[1].ToString() + spell.WeaponType.ToString()[2].ToString());
                                proftwo = ushort.Parse(spell.WeaponType.ToString()[3].ToString() + spell.WeaponType.ToString()[4].ToString() + spell.WeaponType.ToString()[5].ToString());
                                if (!Pool.WeaponSpells.ContainsKey((ushort)profone))
                                    Pool.WeaponSpells.Add((ushort)profone, spell.ID);
                                if (!Pool.WeaponSpells.ContainsKey((ushort)proftwo))
                                    Pool.WeaponSpells.Add((ushort)proftwo, spell.ID);
                            }
                            else
                                if (!Pool.WeaponSpells.ContainsKey((ushort)spell.WeaponType))
                                Pool.WeaponSpells.Add((ushort)spell.WeaponType, spell.ID);

                        }
                        if (spell.ID == 12070)
                            spell.Damage = 104;


                       
                        if (this.ContainsKey(spell.ID))
                        {
                            if (!this[spell.ID].ContainsKey(spell.Level))
                                this[spell.ID].Add(spell.Level, spell);
                        }
                        else
                        {
                            this.Add(spell.ID, new Dictionary<ushort, Magic>());
                            this[spell.ID].Add(spell.Level, spell);
                        }

                    }
                }
            }
            Console.WriteLine("Loading " + this.Count + " DBSkills");


            if (Pool.WeaponSpells.ContainsKey(420))//sword
                Pool.WeaponSpells[420] = 5030;
            if (Pool.WeaponSpells.ContainsKey(410))//blade
                Pool.WeaponSpells.Remove(410);
            if (Pool.WeaponSpells.ContainsKey(900))//shield460
                Pool.WeaponSpells.Remove(900);
            if (!Pool.WeaponSpells.ContainsKey(460))
                Pool.WeaponSpells.Add(460, 5040);
            if (Pool.WeaponSpells.ContainsKey(601))//ninja sword
                Pool.WeaponSpells[601] = 11230;
            if (Pool.WeaponSpells.ContainsKey(610))//MonkSpell
                Pool.WeaponSpells[610] = 10490;
            if (Pool.WeaponSpells.ContainsKey(611))//Rapier
                Pool.WeaponSpells[611] = 11140;

            if (Pool.WeaponSpells.ContainsKey(612))//pistol
                Pool.WeaponSpells[612] = 11120;
            else
            {
                Pool.WeaponSpells.Add(612, 11120);
            }
            if (Pool.WeaponSpells.ContainsKey(616))//TwistedClaw
                Pool.WeaponSpells[616] = 12110;
            if (Pool.WeaponSpells.ContainsKey(617))//Nunchaku
                Pool.WeaponSpells[617] = 12240;

            if (Pool.WeaponSpells.ContainsKey(511))//scytle
                Pool.WeaponSpells[511] = 0;
            if (!Pool.WeaponSpells.ContainsKey(624))//ScarofEarth
                Pool.WeaponSpells.Add(624, 12670);


            Add((ushort)Role.Flags.SpellID.ShurikenEffect, this[(ushort)Role.Flags.SpellID.ShurikenVortex]);
        }
        public bool AttackInFlay(Magic DBSpell)
        {
            if (DBSpell.ID == 1045 || DBSpell.ID == 1046 || DBSpell.ID == 1115
    || DBSpell.ID == 1250 || DBSpell.ID == 1260 || DBSpell.ID == 1290
    || DBSpell.ID >= 5010 && DBSpell.ID <= 5050 || DBSpell.ID == 6000
    || DBSpell.ID == 6001 || DBSpell.ID >= 7000 && DBSpell.ID <= 7040
    || DBSpell.ID == 10315 || DBSpell.ID == 10381
    || DBSpell.ID == 10490 || DBSpell.ID == 11000 || DBSpell.ID == 11005
    || DBSpell.ID == 11040 || DBSpell.ID == 11070 || DBSpell.ID == 11110
    || DBSpell.ID == 11140 || DBSpell.ID == 11170 || DBSpell.ID == 11180
    || DBSpell.ID == 11190 || DBSpell.ID == 11230)
                return false;
            else
                return true;
        }
        public uint GetSpell(string name)
        {
            uint spellid = 0;
            foreach (var spells in Values)
            {
                foreach (var spell in spells.Values)
                {
                    if (spell.Name != null)
                        if (spell.Name.Contains(name))
                            spellid = spell.ID;
                }
            }
            return spellid;
        }
        public class Magic
        {
            public ushort ID;
            public string Name;
            public MagicSort Type;
            public byte Level;
            public ushort UseMana;
            public uint UseArrows;
            public float Damage;
            public int Rate = 0;
            public uint Experience;
            public byte Range;
            public ushort Sector;
            public uint Duration;
            public uint WeaponType;
            public byte UseStamina;
            public uint GiveHitPoints;
            public bool AttackInFly;
            public int Status;
            public ushort NeedLevel = 0;
            public byte MaxTargets = 0;
            public byte IncreaseStamin = 0;
            public ushort CoolDown = 100;
            public byte AutoLearn = 0;
            public uint CpsCostRatio;
            public int ColdTime;
            public bool IsSpellWithColdTime
            {
                get { return ColdTime > 0; }
            }
            public int Damage2;
            public int Damage3;
            public int DamageOnMonster;
            public int CustomCoolDown = 200;
        }
    }

}
