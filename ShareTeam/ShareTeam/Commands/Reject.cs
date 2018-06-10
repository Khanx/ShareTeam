using ChatCommands;
using System;


namespace ShareTeam.Commands
{
    public class Reject : IChatCommand
    {
        public bool IsCommand(string chat)
        {
            if(chat.StartsWith("/reject_team", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        public bool TryDoCommand(Players.Player player, string chat)
        {
            string[] args = CommandManager.SplitCommand(chat);

            if(args.Length == 1)
            {
                Pipliz.Chatting.Chat.Send(player, "<color=red>You need to specify the player to reject.</color>");

                return true;
            }

            string name = args[1];

            if(!Players.TryMatchName(name, out Players.Player wantToJoin_byNAME))
            {
                Pipliz.Chatting.Chat.Send(player, string.Format("<color=red>Player {0} not found.</color>", name));

                return true;
            }

            if(!Join.joinTo.TryGetValue(player, out Players.Player wantToJoin) || wantToJoin != wantToJoin_byNAME)
            {
                Pipliz.Chatting.Chat.Send(player, string.Format("<color=red>Player {0} has not requested to join your team.</color>", name));

                return true;
            }


            Pipliz.Chatting.Chat.Send(player, string.Format("<color=green>You have rejected {0} to join your team.</color>", name));
            Pipliz.Chatting.Chat.Send(wantToJoin, string.Format("<color=green>{0} has rejected your request to join his team.</color>", player.Name));


            Join.joinTo.Remove(player);

            return true;
        }
    }
}
