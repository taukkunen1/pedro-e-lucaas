using Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameServer.MadeByDaRkFox
{
    public static class FixMonstersFromMonsterDat
    {
        private static string MonsterTxt = "monster.txt";
        private static string MonsterDat = "monster.dat";
        private enum TqMonsterTypeFields
        {
            ID,
            Name,
            Type,
            Lookface,
            Life,
            Mana,
            AttackMax,
            AttackMin,
            Defense,
            Dexterity,
            Dodge,
            HelmetType,
            ArmorType,
            WeaponRType,
            WeaponLType,
            AttackRange,
            ViewRange,
            EscapeLife,
            AttackSpeed,
            MoveSpeed,
            Level,
            AttackUser,
            DropMoney,
            DropItemtype,
            SizeAdd,
            Action,
            RunSpeed,
            DropArmet,
            DropNecklace,
            DropArmor,
            DropRing,
            DropWeapon,
            DropShield,
            DropShoes,
            DropHP,
            DropMP,
            MagicType,
            MagicDef,
            MagicHitRate,
            AIType,
            Defense2,
            StcType,
            AntiMonster,
            ExtraBattleLev,
            ExtraExp,
            ExtraDamage,
            SpeciesType,
            AttrMetal,
            AttrWood,
            AttrFire,
            AttrEarth,
            VsCallpet,
            TransformFlag,
            TransformCondition,
            TransformMonster,
            AttackNew,
            DefenseNew,
            StableDefense,
            CriticalRate,
            MagicCriticalRate,
            AntiCriticalRate,
            FinalDamageAdd,
            FinalDamageAddMgc,
            FinalDamageReduce,
            FinalDamageReduceMgc,
            ItemDropRule1,
            ItemDropRule2,
            ItemDropRule3,
            ItemDropRule4,
            ExtraMagicInjury,
            Attribute
        }
        public static void FixFromDat()
        {
            MonsterTxt = Path.Combine(ServerConfig.DbLocation, MonsterTxt); // set the path relative to db location
            MonsterDat = Path.Combine(ServerConfig.DbLocation, MonsterDat); // set the path relative to db location
            if (File.Exists(MonsterDat))
            {
                if (Utils.DecryptCommonDat(MonsterDat))
                {
                    File.Delete(MonsterDat);
                }
            }
            if (File.Exists(MonsterTxt))
            {
                // Do the fix process
                Console.WriteLine($"Loading Monsters for check...", ConsoleColor.Red);
                IniFileHelper ini = new();
                //IniFileHelper iniDat = new(MonsterTxt);
                // Load original values from csv for compare
                string TqMonsterTypePath = Path.Combine(ServerConfig.DbLocation, "tqMonstertype.csv");
                if (!File.Exists(TqMonsterTypePath))
                {
                    Console.WriteLine($"[Error] No exists {TqMonsterTypePath} for compare with the original tq values", ConsoleColor.Red);
                    return;
                }
                List<string> TqMonsterTypeLines = File.ReadAllLines(TqMonsterTypePath).ToList();
                bool ModifyAll = false;
                foreach (string fname in Directory.GetFiles(Path.Combine(ServerConfig.DbLocation, "Monsters")))
                {
                    ini.LoadFile(fname);
                    Game.MsgMonster.MonsterFamily Family = new Game.MsgMonster.MonsterFamily();
                    Family.ID = ini.ReadUInt32("cq_monstertype", "id", 0);
                    Family.Name = ini.ReadString("cq_monstertype", "name", "INVALID_MOB");
                    Family.Level = ini.ReadUInt16("cq_monstertype", "level", 0);
                    Family.MaxAttack = ini.ReadInt32("cq_monstertype", "attack_max", 0);
                    Family.MinAttack = ini.ReadInt32("cq_monstertype", "attack_min", 0);
                    if (Family.Name == "INVALID_MOB" || Family.Level == 0 || Family.ID == 0 || Family.MinAttack > Family.MaxAttack)
                    {
                        Console.WriteLine("[Error] Detected invalid Monster data from: " + fname + "", ConsoleColor.Red);
                        continue;
                    }
                    Family.Defense = ini.ReadUInt16("cq_monstertype", "defence", 0);
                    Family.Mesh = (ushort)ini.ReadUInt32("cq_monstertype", "lookface", 0);
                    Family.MaxHealth = ini.ReadInt32("cq_monstertype", "life", 0);
                    Family.ViewRange = 16;
                    Family.AttackRange = ini.ReadSByte("cq_monstertype", "attack_range", 0);
                    Family.Dodge = ini.ReadByte("cq_monstertype", "dodge", 0);
                    Family.DropBoots = ini.ReadByte("cq_monstertype", "drop_shoes", 0);
                    Family.DropNecklace = ini.ReadByte("cq_monstertype", "drop_necklace", 0);
                    Family.DropRing = ini.ReadByte("cq_monstertype", "drop_ring", 0);
                    Family.DropArmet = ini.ReadByte("cq_monstertype", "drop_armet", 0);
                    Family.DropArmor = ini.ReadByte("cq_monstertype", "drop_armor", 0);
                    Family.DropShield = ini.ReadByte("cq_monstertype", "drop_shield", 0);
                    Family.DropWeapon = ini.ReadByte("cq_monstertype", "drop_weapon", 0);
                    Family.DropMoney = (ushort)ini.ReadUInt32("cq_monstertype", "drop_money", 0);
                    Family.DropHPItem = ini.ReadUInt32("cq_monstertype", "drop_hp", 0);
                    Family.DropMPItem = ini.ReadUInt32("cq_monstertype", "drop_mp", 0);
                    Family.Boss = ini.ReadByte("cq_monstertype", "Boss", 0);
                    Family.Defense2 = ini.ReadInt32("cq_monstertype", "defence2", 0);
                    Family.MoveSpeed = ini.ReadInt32("cq_monstertype", "move_speed", 0);
                    Family.AttackSpeed = ini.ReadInt32("cq_monstertype", "attack_speed", 0);
                    Family.SpellId = ini.ReadUInt32("cq_monstertype", "magic_type", 0);
                    Family.ExtraCritical = ini.ReadUInt32("cq_monstertype", "critical", 0);
                    Family.ExtraBreack = ini.ReadUInt32("cq_monstertype", "break", 0);
                    Family.extra_battlelev = ini.ReadInt32("cq_monstertype", "extra_battlelev", 0);
                    Family.extra_exp = ini.ReadInt32("cq_monstertype", "extra_exp", 0);
                    Family.extra_damage = ini.ReadInt32("cq_monstertype", "extra_damage", 0);

                    // Compare from decrypted monster.dat
                    Console.WriteLine($"Checking Monster[{Family.Name}-{Family.ID}]", ConsoleColor.Red);
                    string lineTqMonster = TqMonsterTypeLines.FirstOrDefault(x => x.Split(';', StringSplitOptions.RemoveEmptyEntries)[(uint)TqMonsterTypeFields.ID] == Family.ID.ToString().PadLeft(4, '0'));
                    if (lineTqMonster == null)
                    {
                        Console.WriteLine($"MonsterID not exist in tq original. Skipping [{Family.Name}-{Family.ID}]", ConsoleColor.DarkYellow);
                        continue;
                    }
                    uint MonsterDat_ID = uint.Parse(lineTqMonster.Split(';', StringSplitOptions.RemoveEmptyEntries)[(uint)TqMonsterTypeFields.ID]);
                    uint MonsterDat_MaxLife = uint.Parse(lineTqMonster.Split(';', StringSplitOptions.RemoveEmptyEntries)[(uint)TqMonsterTypeFields.Life]);
                    if (MonsterDat_MaxLife != Family.MaxHealth)
                    {
                        Console.WriteLine("Found a difference with MaxLife Attr, Fix it? [Y/N/A] (A for all)");
                        string confirmFix = "a";
                        if (!ModifyAll)
                        {
                            confirmFix = Console.ReadLine();
                            ModifyAll = confirmFix.ToLower() == "a";
                        }
                        if (confirmFix.ToLower() == "y" || ModifyAll)
                        {
                            ini.Write<int>("cq_monstertype", "life", (int)MonsterDat_MaxLife);
                        }
                    }
                    uint MonsterDat_AttackSpeed = uint.Parse(lineTqMonster.Split(';', StringSplitOptions.RemoveEmptyEntries)[(uint)TqMonsterTypeFields.AttackSpeed]);
                    if (MonsterDat_AttackSpeed != Family.AttackSpeed)
                    {
                        Console.WriteLine("Found a difference with AttackSpeed Attr, Fix it? [Y/N/A] (A for all)");
                        string confirmFix = "a";
                        if (!ModifyAll)
                        {
                            confirmFix = Console.ReadLine();
                            ModifyAll = confirmFix.ToLower() == "a";
                        }
                        if (confirmFix.ToLower() == "y" || ModifyAll)
                        {
                            ini.Write<int>("cq_monstertype", "attack_speed", (int)MonsterDat_AttackSpeed);
                        }
                    }
                    uint MonsterDat_MinAttack = uint.Parse(lineTqMonster.Split(';', StringSplitOptions.RemoveEmptyEntries)[(uint)TqMonsterTypeFields.AttackMin]);
                    if (MonsterDat_MinAttack != Family.MinAttack)
                    {
                        Console.WriteLine("Found a difference with MinAttack Attr, Fix it? [Y/N/A] (A for all)");
                        string confirmFix = "a";
                        if (!ModifyAll)
                        {
                            confirmFix = Console.ReadLine();
                            ModifyAll = confirmFix.ToLower() == "a";
                        }
                        if (confirmFix.ToLower() == "y" || ModifyAll)
                        {
                            ini.Write<int>("cq_monstertype", "attack_min", (int)MonsterDat_MinAttack);
                        }
                    }
                    uint MonsterDat_MaxAttack = uint.Parse(lineTqMonster.Split(';', StringSplitOptions.RemoveEmptyEntries)[(uint)TqMonsterTypeFields.AttackMax]);
                    if (MonsterDat_MaxAttack != Family.MaxAttack)
                    {
                        Console.WriteLine("Found a difference with MaxAttack Attr, Fix it? [Y/N/A] (A for all)");
                        string confirmFix = "a";
                        if (!ModifyAll)
                        {
                            confirmFix = Console.ReadLine();
                            ModifyAll = confirmFix.ToLower() == "a";
                        }
                        if (confirmFix.ToLower() == "y" || ModifyAll)
                        {
                            ini.Write<int>("cq_monstertype", "attack_max", (int)MonsterDat_MaxAttack);
                        }
                    }
                }
            }
        }
    }
}
