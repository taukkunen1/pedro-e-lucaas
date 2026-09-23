namespace GameServer.Game.Era1
{
    /// <summary>
    /// Central allow-list for profession skills available in the 5017-inspired Era 1.
    /// It intentionally excludes Ninja/Monk/Pirate/Windwalker/Dragon-Warrior skills
    /// and later additions to the classic professions.
    ///
    /// Reborn-only classic skills remain valid Era 1 content, but they are not
    /// promotion rewards and must be awarded by the rebirth system.
    /// </summary>
    public static class Era1Skills
    {
        public static bool IsPostClassic(Role.Flags.SpellID spell)
        {
            switch (spell)
            {
                // Ninja
                case Role.Flags.SpellID.TwofoldBlades:
                case Role.Flags.SpellID.ToxicFog:
                case Role.Flags.SpellID.PoisonStar:
                case Role.Flags.SpellID.CounterKill:
                case Role.Flags.SpellID.ArcherBane:
                case Role.Flags.SpellID.ShurikenEffect:
                case Role.Flags.SpellID.ShurikenVortex:
                case Role.Flags.SpellID.FatalStrike:
                case Role.Flags.SpellID.BloodyScythe:
                case Role.Flags.SpellID.MortalDrag:

                // Monk / auras
                case Role.Flags.SpellID.RadiantPalm:
                case Role.Flags.SpellID.Oblivion:
                case Role.Flags.SpellID.TyrantAura:
                case Role.Flags.SpellID.Serenity:
                case Role.Flags.SpellID.SoulShackle:
                case Role.Flags.SpellID.FendAura:
                case Role.Flags.SpellID.WhirlwindKick:
                case Role.Flags.SpellID.MetalAura:
                case Role.Flags.SpellID.WoodAura:
                case Role.Flags.SpellID.WatherAura:
                case Role.Flags.SpellID.FireAura:
                case Role.Flags.SpellID.EarthAura:
                case Role.Flags.SpellID.Tranquility:
                case Role.Flags.SpellID.Compassion:

                // Pirate / later physical professions
                case Role.Flags.SpellID.DragonTail:
                case Role.Flags.SpellID.ViperFang:
                case Role.Flags.SpellID.EagleEye:
                case Role.Flags.SpellID.ScurvyBomb:
                case Role.Flags.SpellID.CannonBarrage:
                case Role.Flags.SpellID.BlackbeardsRage:
                case Role.Flags.SpellID.GaleBomb:
                case Role.Flags.SpellID.KrakensRevenge:
                case Role.Flags.SpellID.BladeTempest:
                case Role.Flags.SpellID.AdrenalineRush:
                case Role.Flags.SpellID.Windstorm:
                case Role.Flags.SpellID.ChargingVortex:
                case Role.Flags.SpellID.GapingWounds:
                case Role.Flags.SpellID.PathOfshadow:
                case Role.Flags.SpellID.BlisteringWave:
                case Role.Flags.SpellID.BladeFlurry:
                case Role.Flags.SpellID.DaggerStorm:
                case Role.Flags.SpellID.KineticSpark:
                case Role.Flags.SpellID.SpiritFocus:
                case Role.Flags.SpellID.MortalWound:

                // Later additions to classic professions.
                case Role.Flags.SpellID.ShieldBlock:
                case Role.Flags.SpellID.TripleAttack:
                case Role.Flags.SpellID.DefensiveStance:
                case Role.Flags.SpellID.ChainBolt:
                case Role.Flags.SpellID.HeavenBlade:
                case Role.Flags.SpellID.StarArrow:
                case Role.Flags.SpellID.DragonWhirl:
                case Role.Flags.SpellID.Perseverance:
                case Role.Flags.SpellID.ArrowRain:
                case Role.Flags.SpellID.Intensify:
                    return true;

                default:
                    return false;
            }
        }
    }
}
