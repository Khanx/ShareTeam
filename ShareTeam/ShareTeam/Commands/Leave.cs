using ChatCommands;
using System;

using ShareTeam.Teams;

namespace ShareTeam.Commands
{
    public class Leave : IChatCommand
    {
        public bool IsCommand(string chat)
        {
            if(chat.Equals("/leave_team", StringComparison.OrdinalIgnoreCase))
                return true;


            return false;
        }

        public bool TryDoCommand(Players.Player player, string chat)
        {
            string[] args = CommandManager.SplitCommand(chat);

            Team team = TeamManager.GetTeamManager().GetTeam(player);

            if(null == team)
            {
                Pipliz.Chatting.Chat.Send(player, "<color=orange>You do not belong to any team.</color>");

                return true;
            }

            team.RemovePlayer(player);
            
            Pipliz.Chatting.Chat.Send(player, "<color=lime>You have left the team.</color>");

            return true;
        }
    }
}
