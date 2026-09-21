using GameServer.Client;

namespace GameServer.Game.MsgServer
{
    public static class PacketGuards
    {
        public static bool ValidateClientPacket(GameClient client, ServerSockets.Packet stream, ushort packetId, out string reason)
        {
            reason = null;

            if (!ValidateHeader(stream, packetId, out reason))
                return false;

            switch (packetId)
            {
                case GamePackets.Usage:
                    return ValidateUsage(stream, out reason);
                case GamePackets.Attack:
                    return ValidateAttack(stream, out reason);
                case GamePackets.DataMap:
                    return ValidateDataMap(client, stream, out reason);
                case GamePackets.Movement:
                    return ValidateMovement(stream, out reason);
                case GamePackets.Update:
                    reason = "client attempted to send server-only update packet";
                    return false;
                default:
                    return true;
            }
        }

        private static bool ValidateHeader(ServerSockets.Packet stream, ushort packetId, out string reason)
        {
            reason = null;
            if (stream.Size < 4)
            {
                reason = "packet shorter than header";
                return false;
            }

            ushort declaredLength = ReadUInt16At(stream, 0);
            ushort declaredType = ReadUInt16At(stream, 2);
            if (declaredType != packetId)
            {
                reason = $"header type mismatch declared={declaredType} actual={packetId}";
                return false;
            }

            if (declaredLength > stream.Size)
            {
                reason = $"declared length {declaredLength} exceeds buffer size {stream.Size}";
                return false;
            }

            int expectedWithSeal = declaredLength + ServerSockets.Packet.SealSize;
            if (ServerSockets.Packet.SealSize > 0 && expectedWithSeal != stream.Size)
            {
                reason = $"declared length {declaredLength} does not match sealed buffer size {stream.Size}";
                return false;
            }

            return true;
        }

        private static bool ValidateUsage(ServerSockets.Packet stream, out string reason)
        {
            reason = null;
            if (!RequireMinPayload(stream, 0x58, out reason))
                return false;

            uint action = ReadUInt32At(stream, 0x0C);
            if (!System.Enum.IsDefined(typeof(MsgItemUsuagePacket.ItemUsageID), action))
            {
                reason = $"unknown item usage action {action}";
                return false;
            }

            uint argCount = ReadUInt32At(stream, 0x14);
            if (argCount >= 50)
            {
                reason = $"item usage arg count too large {argCount}";
                return false;
            }

            uint requiredLength = 0x58u + (argCount * sizeof(uint));
            ushort declaredLength = ReadUInt16At(stream, 0);
            if (declaredLength < requiredLength)
            {
                reason = $"item usage length {declaredLength} too short for {argCount} args";
                return false;
            }

            return true;
        }

        private static bool ValidateAttack(ServerSockets.Packet stream, out string reason)
        {
            reason = null;
            if (!RequireExactPayload(stream, 0x28, out reason))
                return false;

            uint attackType = ReadUInt32At(stream, 0x14);
            if (!System.Enum.IsDefined(typeof(MsgAttackPacket.AttackID), attackType))
            {
                reason = $"unknown attack type {attackType}";
                return false;
            }

            return true;
        }

        private static bool ValidateDataMap(GameClient client, ServerSockets.Packet stream, out string reason)
        {
            reason = null;
            if (!RequireMinPayload(stream, 0x26, out reason))
                return false;

            ushort actionType = ReadUInt16At(stream, 0x14);
            if (!System.Enum.IsDefined(typeof(ActionType), actionType))
            {
                reason = $"unknown data action {actionType}";
                return false;
            }

            if (client?.Player == null && actionType != (ushort)ActionType.CompleteLogin)
            {
                reason = $"data action {actionType} before player is loaded";
                return false;
            }

            return true;
        }

        private static bool ValidateMovement(ServerSockets.Packet stream, out string reason)
        {
            reason = null;
            if (!RequireExactPayload(stream, 0x18, out reason))
                return false;

            uint direction = ReadUInt32At(stream, 0x04);
            uint running = ReadUInt32At(stream, 0x0C);
            if (running != MsgMovement.Walk && running != MsgMovement.Run && running != MsgMovement.Steed)
            {
                reason = $"unknown movement mode {running}";
                return false;
            }

            uint maxDirection = running == MsgMovement.Steed ? 24u : 8u;
            if (direction >= maxDirection)
            {
                reason = $"movement direction {direction} outside mode {running}";
                return false;
            }

            return true;
        }

        private static bool RequireExactPayload(ServerSockets.Packet stream, ushort payloadLength, out string reason)
        {
            ushort declaredLength = ReadUInt16At(stream, 0);
            if (declaredLength != payloadLength)
            {
                reason = $"declared length {declaredLength} expected {payloadLength}";
                return false;
            }

            reason = null;
            return true;
        }

        private static bool RequireMinPayload(ServerSockets.Packet stream, ushort payloadLength, out string reason)
        {
            ushort declaredLength = ReadUInt16At(stream, 0);
            if (declaredLength < payloadLength)
            {
                reason = $"declared length {declaredLength} below minimum {payloadLength}";
                return false;
            }

            reason = null;
            return true;
        }

        private static ushort ReadUInt16At(ServerSockets.Packet stream, int offset)
        {
            int position = stream.Position;
            stream.Seek(offset);
            ushort value = stream.ReadUInt16();
            stream.Seek(position);
            return value;
        }

        private static uint ReadUInt32At(ServerSockets.Packet stream, int offset)
        {
            int position = stream.Position;
            stream.Seek(offset);
            uint value = stream.ReadUInt32();
            stream.Seek(position);
            return value;
        }
    }
}
