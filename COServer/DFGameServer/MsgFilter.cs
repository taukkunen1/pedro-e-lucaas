using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GameServer
{
    public class MsgFilter
    {

        public static List<string> Insults = new List<string>();
        public static void Load()
        {
            string[] readText = File.ReadAllLines(Path.Combine(ServerConfig.DbLocation, "BadMsg.txt"), Encoding.Default);
            foreach (string a in readText)
                Insults.Add(a);
        }
        public static string Filter(string Msg, Client.GameClient client)
        {
            #region AntiSpam
            foreach (var bad in Insults)
            {
                if (Msg.StartsWith(bad))
                {
                    string star = "";

                    for (int i = 0; i < bad.Length; i++)
                    {
                        star += "*";

                    }
                    Msg = Msg.Replace(bad, star);
                    client.Socket.Disconnect();
                    Console.Write("Player " + client.Player.Name + " Esta utilizando un comando : " + bad + "");

                }
                if (Msg.Contains(" " + bad))
                {
                    string star = "";
                    for (int i = 0; i < bad.Length; i++)
                    {
                        star += "*";
                    }
                    Msg = Msg.Replace(" " + bad, star);
                    client.Socket.Disconnect();
                    Console.Write("Player " + client.Player.Name + " Esta utilizando un comando : " + bad + "");
                }
            }
            if (Msg.Contains("http:") || Msg.Contains("www.") || Msg.Contains(".com"))
            {
                var words = Msg.Split(new string[] { " " }, StringSplitOptions.None);
                for (int i = 0; i < words.Length; i++)
                {
                    var word = words[i];
                   
                    Msg = Msg.Replace(word, "*****.com");
                }
            }
            #endregion AntiSpam
            return Msg;
        }
    }
}
