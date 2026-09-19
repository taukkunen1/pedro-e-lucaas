namespace GameServer
{
    using PacketInvoker = CachedAttributeInvocation<System.Action<
    Client.GameClient,
    ServerSockets.Packet>,
    PacketAttribute,
    ushort>;
    public static class MsgInvoker
    {
        public static PacketInvoker PacketInvoker;
        public static void Initialize()
        {
            PacketInvoker = new PacketInvoker(PacketAttribute.Translator);
        }
    }
}
