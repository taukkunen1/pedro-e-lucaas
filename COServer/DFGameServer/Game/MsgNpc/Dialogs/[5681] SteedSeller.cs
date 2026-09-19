namespace GameServer.Game.MsgNpc.Dialogs
{
    class DialogSteedSeller
    {

        [NpcAttribute(NpcID.SteedSeller)]
        public static void SteedSeller(Client.GameClient client, ServerSockets.Packet stream, byte Option, string Input, uint id)
        {
            Dialog dialog = new Dialog(client, stream);
            switch (Option)
            {
                case 0:
                    {
                        dialog.Text("Hello, I can sell you horses, but I do not accept gold or cps, I only accept saddles")
                            .Option("Deliver saddles", 1)
                            .Option("I'll be back", byte.MaxValue)
                            .AddAvatar(245).FinalizeDialog();
                        break;
                    }
                case 1:
                    {
                        dialog.Text("choose the color of the horse")
                            .Option("Black", 2)
                            .Option("White", 3)
                            .Option("Brown", 4)
                            .AddAvatar(245).FinalizeDialog();
                        break;
                    }
                case 2://negro
                    {
                        if (client.Inventory.Contain(723903, 1))
                        {
                            client.Inventory.RemoveStackItem(723903, 1, stream);
                            client.Inventory.AddSteed(stream, 300000, 1, 0, false, 255, 0, 150);
                            dialog.FinalizeDialog(true);
                        }
                        break;
                    }
                case 3://Blanco
                    {
                        if (client.Inventory.Contain(723903, 1))
                        {
                            client.Inventory.RemoveStackItem(723903, 1, stream);
                            client.Inventory.AddSteed(stream, 300000, 1, 0, false, 150, 255, 0);
                            dialog.FinalizeDialog(true);
                        }
                        break;
                    }
                case 4://Brown/marron
                    {
                        if (client.Inventory.Contain(723903, 1))
                        {
                            client.Inventory.RemoveStackItem(723903, 1, stream);
                            client.Inventory.AddSteed(stream, 300000, 1, 0, false, 0, 150, 255);
                            dialog.FinalizeDialog(true);
                        }
                        break;
                    }
            }
        }
    }
}
