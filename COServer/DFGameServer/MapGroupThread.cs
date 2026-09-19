using Core;
using System;

namespace GameServer
{
    public class MapGroupThread
    {
        public const int AI_Buffers = 500,
           AI_Guards = 700,
           JUMP = 800,
                       User_ItemTIme = 1000,

           AI_Monsters = 400,
           User_Buffers = 500,
           User_Stamina = 500,
           User_StampXPCount = 3000,
           User_AutoAttack = 100,
           User_CheckSeconds = 1000,
           User_FloorSpell = 300,
                        User_Auto = 2000,

       User_CheckItems = 1000;
        public ThreadItem Thread;
        public ThreadItem Thread2;
        public MapGroupThread(int interval, string name)
        {
            Thread = new ThreadItem(interval, name, OnProcess);
        }
        public void Start()
        {
            Thread.Open();
        }
        public void OnProcess()
        {
            DateTime clock = DateTime.Now;           
            foreach (var user in Pool.GamePoll.Values)
            {
                if (!user.Fake)
                {                  
                    if (clock > user.StaminStamp)
                    {
                        Client.PoolProcesses.StaminaCallback(user);
                        user.StaminStamp.AddMilliseconds(User_Stamina);
                    }
                   
                  
                }
            }
        }
        
    }
}
