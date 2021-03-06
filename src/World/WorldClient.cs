﻿using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Classic.Common;
using Classic.Cryptography;
using Classic.Data;
using Classic.World.Entities;
using Classic.World.Messages.Server;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Classic.World
{
    public class WorldClient : ClientBase
    {
        private readonly WorldPacketHandler packetHandler;
        private readonly WorldState worldState;

        public WorldClient(WorldPacketHandler packetHandler, ILogger<WorldClient> logger, AuthCrypt crypt, WorldServer world) : base(logger)
        {
            this.packetHandler = packetHandler;
            this.Crypt = crypt;
            this.worldState = world.State;
        }

        public override async Task Initialize(TcpClient client)
        {
            await base.Initialize(client);

            this.logger.LogDebug($"{this.ClientInfo} - connected");

            await Send(new SMSG_AUTH_CHALLENGE().Get());
            await HandleConnection();
        }

        public User User { get; internal set; }
        public PlayerEntity Player { get; internal set; }
        public Character Character => Player?.Character;

        public AuthCrypt Crypt { get; }

        protected override async Task HandlePacket(byte[] data)
        {
            for (var i = 0; i < data.Length; i++)
            {
                // TODO: Spans instead of array.copy!
                var header = new byte[6];
                Array.Copy(data, i, header, 0, 6);

                var (length, opcode) = this.DecodePacket(header);

                this.logger.LogTrace($"{this.ClientInfo} - Recv {opcode} ({length} bytes)");

                var packet = new byte[length];
                Array.Copy(data, i + 6, packet, 0, length - 4);

                var handler = packetHandler.GetHandler(opcode);
                await handler(new HandlerArguments
                {
                    Client = this,
                    Data = packet,
                    Opcode = opcode,
                    WorldState = this.worldState
                });

                i += 2 + (length - 1);
            }
        }

        public async Task SendPacket(ServerMessageBase<Opcode> message)
        {
            var data = this.Crypt.Encode(message);
            await this.Send(data);
            this.logger.LogTrace($"{this.ClientInfo} - Sent {message.GetType().Name}");
            message.Dispose();
        }

        protected override void OnDisconnected()
        {
            this.worldState.Connections.Remove(this);

            // TODO
            var json = JsonConvert.SerializeObject(User.Characters.ToArray());
            File.WriteAllText(User.CharsFile, json);
        }

        private (ushort length, Opcode opcode) DecodePacket(byte[] packet)
        {
            ushort length;
            short opcode;

            if (this.Crypt.IsInitialized)
            {
                this.Crypt.Decrypt(packet, 6);
                length = BitConverter.ToUInt16(new [] { packet[1], packet[0] });
                opcode = BitConverter.ToInt16(new[] { packet[2], packet[3] });
            }
            else
            {
                length = BitConverter.ToUInt16(new[] { packet[1], packet[0] });
                opcode = BitConverter.ToInt16(packet, 2);
            }

            return (length, (Opcode)opcode);
        }
    }
}
