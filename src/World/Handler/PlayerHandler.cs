﻿using System.Linq;
using System.Threading.Tasks;
using Classic.World.Messages.Client;
using Classic.World.Messages.Server;

namespace Classic.World.Handler
{
    public static class PlayerHandler
    {
        [OpcodeHandler(Opcode.CMSG_PLAYER_LOGIN)]
        public static async Task OnPlayerLogin(HandlerArguments args)
        {
            var request = new CMSG_PLAYER_LOGIN(args.Data);
            var character = args.Client.User.Characters.Single(x => x.ID == request.CharacterID);

            args.Client.Log($"Player logged in with char {character.Name}");

            await args.Client.SendPacket(new SMSG_LOGIN_VERIFY_WORLD(character));
            await args.Client.SendPacket(new SMSG_ACCOUNT_DATA_TIMES());

            await args.Client.SendPacket(new SMSG_SET_REST_START());
            await args.Client.SendPacket(new SMSG_BINDPOINTUPDATE(character));
            await args.Client.SendPacket(new SMSG_TUTORIAL_FLAGS());
            await args.Client.SendPacket(new SMSG_LOGIN_SETTIMESPEED());

            await args.Client.SendPacket(new SMSG_INITIAL_SPELLS(character.Spells));
            await args.Client.SendPacket(new SMSG_ACTION_BUTTONS(character.ActionBar));
            await args.Client.SendPacket(new SMSG_INITIALIZE_FACTIONS());

            // TODO: not working
            // await args.Client.SendPacket(new SMSG_TRIGGER_CINEMATIC(CinematicID.NightElf));

            await args.Client.SendPacket(new SMSG_CORPSE_RECLAIM_DELAY());
            await args.Client.SendPacket(new SMSG_INIT_WORLD_STATES());
            await args.Client.SendPacket(SMSG_UPDATE_OBJECT.CreateOwnPlayerObject(character, out var player));

            // Initially spawn all creatures
            foreach (var unit in args.WorldState.Creatures)
            {
                // TODO: Add range check
                await args.Client.SendPacket(SMSG_UPDATE_OBJECT.CreateUnit(unit));
            }

            args.Client.Player = player;
            await args.WorldState.SpawnPlayer(character);
            
            await args.Client.SendPacket(new SMSG_MESSAGECHAT(character.ID, "Hello World"));
        }

        [OpcodeHandler(Opcode.CMSG_LOGOUT_REQUEST)]
        public static async Task OnPlayerLogoutRequested(HandlerArguments args)
        {
            await args.Client.SendPacket(SMSG_LOGOUT_COMPLETE.Success());
            args.Client.Player = null;

            // TODO: Remove from other clients
        }
    }
}
