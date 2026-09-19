using GameServer.Client;
using GameServer.ServerSockets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace GameServer.Telemetry
{
    public static class ServerLog
    {
        private static readonly object Sync = new object();
        private static string _logDirectory = Path.Combine(AppContext.BaseDirectory, "Logs");

        public static bool DevMode { get; private set; }

        public static void Init(string databasePath, string mode)
        {
            DevMode = string.Equals(mode, "Dev", StringComparison.OrdinalIgnoreCase)
                || string.Equals(mode, "Development", StringComparison.OrdinalIgnoreCase);

            if (!string.IsNullOrWhiteSpace(databasePath))
                _logDirectory = Path.Combine(databasePath, "Logs");

            Directory.CreateDirectory(_logDirectory);
            System("startup", "GameServer log initialized.", new Dictionary<string, object>
            {
                ["mode"] = DevMode ? "DEV" : "PRODUCTION",
                ["logDirectory"] = _logDirectory
            });
        }

        public static void System(string eventName, string message, IDictionary<string, object> data = null)
        {
            Write("system", eventName, message, data);
        }

        public static void Error(string eventName, Exception exception, IDictionary<string, object> data = null)
        {
            var payload = data != null
                ? new Dictionary<string, object>(data)
                : new Dictionary<string, object>();

            payload["exceptionType"] = exception.GetType().FullName;
            payload["exceptionMessage"] = exception.Message;
            payload["stackTrace"] = exception.ToString();

            Write("error", eventName, exception.Message, payload);
        }

        public unsafe static void Packet(string eventName, GameClient client, ushort packetId, Packet packet, Exception exception = null)
        {
            var payload = BuildClientData(client);
            payload["packetId"] = packetId;
            payload["packetSize"] = packet != null ? packet.Size : 0;

            if (DevMode && packet != null)
                payload["packetHex"] = SnapshotPacket(packet, 96);

            if (exception != null)
            {
                payload["exceptionType"] = exception.GetType().FullName;
                payload["exceptionMessage"] = exception.Message;
                payload["stackTrace"] = exception.ToString();
            }

            Write("packet", eventName, $"Packet {packetId}", payload);
        }

        private static Dictionary<string, object> BuildClientData(GameClient client)
        {
            var data = new Dictionary<string, object>();
            try
            {
                data["connectionUid"] = client != null ? client.ConnectionUID : 0;
                data["playerUid"] = client != null && client.Player != null ? client.Player.UID : 0;
                data["playerName"] = client != null && client.Player != null ? client.Player.Name : "";
                data["map"] = client != null && client.Player != null ? client.Player.Map : 0;
                data["remoteIp"] = client != null && client.Socket != null ? client.Socket.RemoteIp : "";
            }
            catch
            {
                data["clientContextError"] = true;
            }
            return data;
        }

        private unsafe static string SnapshotPacket(Packet packet, int maxBytes)
        {
            int size = Math.Min(packet.Size, maxBytes);
            if (size <= 0 || packet.Memory == null)
                return "";

            byte[] bytes = new byte[size];
            Marshal.Copy((IntPtr)packet.Memory, bytes, 0, size);
            return BitConverter.ToString(bytes).Replace("-", " ");
        }

        private static void Write(string category, string eventName, string message, IDictionary<string, object> data)
        {
            try
            {
                Directory.CreateDirectory(_logDirectory);
                string path = Path.Combine(_logDirectory, $"server-{DateTime.Now:yyyy-MM-dd}.ndjson");
                var entry = new Dictionary<string, object>
                {
                    ["time"] = DateTime.Now.ToString("o"),
                    ["category"] = category,
                    ["event"] = eventName,
                    ["message"] = message,
                    ["data"] = data ?? new Dictionary<string, object>()
                };

                string line = JsonSerializer.Serialize(entry);
                lock (Sync)
                    File.AppendAllText(path, line + Environment.NewLine);
            }
            catch (Exception ex)
            {
                global::System.Console.Error.WriteLine("ServerLog failed: " + ex.Message);
            }
        }
    }
}
