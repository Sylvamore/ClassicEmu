﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Classic.Auth.Entities;
using Classic.Auth.Extensions;
using Classic.Common;
using static Classic.Auth.Opcode;

namespace Classic.Auth.Challenges
{
    public class ServerRealmlist
    {
        private static readonly List<Realm> Realmlist = new List<Realm>
        {
            new Realm
            {
                Type = 1,
                Lock = 0,
                Flags = 0,
                Name = "TestServer",
                Address = "127.0.0.1:13250",
                Population = 0,
                TimeZone = 1, // 1 seems to be needed for wotlk
            }
        };

        public static async Task Send(LoginClient client)
        {
            using var info = new PacketWriter()
                .WriteUInt32(/* unk */ 0)
                .WriteNumberOfRealms(Realmlist.Count, client.GameVersion);

            foreach (var realm in Realmlist)
            {
                info.WriteRealmType(realm.Type, client.GameVersion);

                if (client.GameVersion == GameVersion.WotLK)
                {
                    info.WriteUInt8(realm.Lock); // 1 is lock
                }

                info
                    .WriteUInt8(realm.Flags)
                    .WriteString(realm.Name)
                    .WriteString(realm.Address)
                    .WriteUInt32(realm.Population)
                    .WriteUInt8( /* num_chars  */ 0)
                    .WriteUInt8(realm.TimeZone);

                if (client.GameVersion == GameVersion.Classic)
                {
                    info.WriteUInt8( /* unk */ 0);
                }
                else if (client.GameVersion == GameVersion.WotLK)
                {
                    // Realm ID for tbc and wotlk
                    info.WriteUInt8(0x2C);
                }
            }

            if (client.GameVersion == GameVersion.Classic)
            {
                info.WriteUInt16(0x0002);
            }
            else if (client.GameVersion == GameVersion.WotLK)
            {
                info.WriteUInt16(0x0010);
            }

            var infoPacket = info.Build();

            using var header = new PacketWriter()
                .WriteUInt8( /* cmd  */ (byte)REALMLIST)
                .WriteUInt16(/* size */ (ushort)(infoPacket.Length));

            await client.Send(header.WriteBytes(infoPacket).Build());
        }
    }
}
