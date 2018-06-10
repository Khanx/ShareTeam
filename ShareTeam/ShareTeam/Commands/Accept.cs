using ChatCommands;
using System;

using ShareTeam.Teams;

namespace ShareTeam.Commands
{
    public class Accept : IChatCommand
    {
        public bool IsCommand(string chat)
        {
            if(chat.StartsWith("/accept_team", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        public bool TryDoCommand(Players.Player player, string chat)
        {
            string[] args = CommandManager.SplitCommand(chat);

            if(args.Length == 1)
            {
                Pipliz.Chatting.Chat.Send(player, "<color=red>You need to specify the player to accept.</color>");

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
                Pipliz.Chatting.Chat.Send(player, string.Format("<color=red>Player {0} has not requested to share with you.</color>", name));

                return true;
            }

            Team team1 = TeamManager.GetTeamManager().GetTeam(player);
            Team team2 = TeamManager.GetTeamManager().GetTeam(wantToJoin);

            if(null != team1 && null != team2)
            {
                Pipliz.Chatting.Chat.Send(player, "<color=red> Both of you are sharing with more people, one have to stop sharing.</color>");

                Join.joinTo.Remove(player);
                return true;
            }
            else if(null != team1)
            {
                team1.AddPlayer(wantToJoin);
            }
            else if(null != team2)
            {
                team2.AddPlayer(player);
            }
            else
            {
                Team team = new Team();
                team.AddPlayer(player);
                team.AddPlayer(wantToJoin);
            }

            Pipliz.Chatting.Chat.Send(player, string.Format("<color=green>Now you shares your stockpile with {0}</color>", name));

            Join.joinTo.Remove(player);

            return true;
        }
    }
}
