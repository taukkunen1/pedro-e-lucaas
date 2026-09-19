using Core;
using System;

namespace GameServer.Role.Instance
{
    public class RoleStatus
    {
        public enum StatusType : byte
        {
            IncreaseMStrike = 60,
            IncreasePStrike = 59,
            IncreaseImunity = 61,
            IncreaseBreack = 62,
            IncreaseAntiBreack = 63,
            IncreaseMaxHp = 64,
            IncreasePAttack = 65,
            IncreaseMAttack = 66,
            IncreaseFinalPDamage = 67,
            IncreaseFinalMDamage = 68,
            IncreaseFinalPAttack = 69,
            IncreaseFinalMAttack = 70,

        }

        private uint Power = 0;

        public DateTime Stamp = new DateTime();
        public RoleStatus(uint _Power, int Secounds)
        {
            Power = _Power;
            Stamp = DateTime.Now.AddSeconds(Secounds);
        }

        public uint GetPower
        {
            get { return Power; }
        }
        public static implicit operator bool(RoleStatus big)
        {
            if (DateTime.Now > big.Stamp)
                return true;
            return false;
        }
        public static implicit operator uint(RoleStatus big)
        {
            return big.GetPower;
        }
    }
}
