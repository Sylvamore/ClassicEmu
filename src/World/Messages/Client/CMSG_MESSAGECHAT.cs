﻿using Classic.Common;
using Classic.Data.Enums.Chat;

namespace Classic.World.Messages.Client
{
    public class CMSG_MESSAGECHAT
    {
        public CMSG_MESSAGECHAT(byte[] data)
        {
            using var reader = new PacketReader(data);
            Type = (MessageType)reader.ReadUInt32();
            Language = (MessageLanguage)reader.ReadUInt32();
            Message = reader.ReadString();
        }

        public MessageType Type { get; }
        public MessageLanguage Language { get; }
        public string Message { get; }
    }
}
