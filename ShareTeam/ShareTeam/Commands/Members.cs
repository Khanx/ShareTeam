using ChatCommands;
using System;

using ShareTeam.Teams;

namespace ShareTeam.Commands
{
    public class Members : IChatCommand
    {
        public bool IsCommand(string chat)
        {
            if(chat.Equals("/members_team", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        public bool TryDoCommand(Players.Player player, string chat)
        {
            Team team = TeamManager.GetTeamManager().GetTeam(player);

            if(null == team)
            {
                Pipliz.Chatting.Chat.Send(player, "<color=red>You do not belong to any team.</color>");

                return true;
            }

            Pipliz.Chatting.Chat.Send(player, "<color=olive>Players on your team:</color>");
            foreach(Players.Player plr in team.GetPlayers())
                Pipliz.Chatting.Chat.Send(player, string.Format("<color=green>{0}</color>", plr.Name));

            return true;
        }
    }
}
