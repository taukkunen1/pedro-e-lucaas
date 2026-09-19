using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Poker
{
    internal class Ranks
    {
        public static List<string> SuitRank = new List<string>()
        {
            "2c".ToLower(), "3c".ToLower(), "4c".ToLower(), "5c".ToLower(), "6c".ToLower(), "7c".ToLower(), "8c".ToLower(), "9c".ToLower(), "Tc".ToLower(), "Jc".ToLower(), "Qc".ToLower(), "Kc".ToLower(), "Ac".ToLower(),
            "2d".ToLower(), "3d".ToLower(), "4d".ToLower(), "5d".ToLower(), "6d".ToLower(), "7d".ToLower(), "8d".ToLower(), "9d".ToLower(), "Td".ToLower(), "Jd".ToLower(), "Qd".ToLower(), "Kd".ToLower(), "Ad".ToLower(),
            "2h".ToLower(), "3h".ToLower(), "4h".ToLower(), "5h".ToLower(), "6h".ToLower(), "7h".ToLower(), "8h".ToLower(), "9h".ToLower(), "Th".ToLower(), "Jh".ToLower(), "Qh".ToLower(), "Kh".ToLower(), "Ah".ToLower(),
            "2s".ToLower(), "3s".ToLower(), "4s".ToLower(), "5s".ToLower(), "6s".ToLower(), "7s".ToLower(), "8s".ToLower(), "9s".ToLower(), "Ts".ToLower(), "Js".ToLower(), "Qs".ToLower(), "Ks".ToLower(), "As".ToLower()
        };
        public static List<string> HighCard = new List<string>()
        {
              "2".ToLower(),
              "3".ToLower(), 
              "4".ToLower(),
              "5".ToLower(),
              "6".ToLower(), 
              "7".ToLower(), 
              "8".ToLower(),
              "9".ToLower(), 
              "T".ToLower(), 
              "J".ToLower(), 
              "Q".ToLower(), 
              "K".ToLower(), 
              "A".ToLower(),
        };

    }   
}
