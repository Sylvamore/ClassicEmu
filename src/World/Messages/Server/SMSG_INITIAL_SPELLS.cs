﻿using System.Collections.Generic;
using Classic.Common;
using Classic.Data;

namespace Classic.World.Messages.Server
{
    public class SMSG_INITIAL_SPELLS : ServerMessageBase<Opcode>
    {
        private readonly List<Spell> spells;

        public SMSG_INITIAL_SPELLS(List<Spell> spells) : base(Opcode.SMSG_INITIAL_SPELLS)
        {
            this.spells = spells;
        }

        public override byte[] Get()
        {
            this.Writer
                .WriteUInt8(0) // ??
                .WriteUInt16((ushort)this.spells.Count);

            ushort slot = 1;
            foreach (var spell in this.spells)
            {
                this.Writer
                    .WriteUInt16((ushort)spell.Id)
                    .WriteUInt16(slot++);
            }

            this.Writer.WriteUInt16(0); // ??
            return this.Writer.Build();
        }
    }
}
