using ChatCommands;
using System;

using ShareTeam.Teams;


namespace ShareTeam.Commands
{
    public class Chat : IChatCommand
    {
        public bool IsCommand(string chat)
        {
            if(chat.StartsWith("/ct", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        public bool TryDoCommand(Players.Player player, string chat)
        {
            Team team = TeamManager.GetTeamManager().GetTeam(player);

            if(null == team)
            {
                Pipliz.Chatting.Chat.Send(player, "<color=orange>You do not belong to any team.</color>");

                return true;
            }

            string name = player.Name;

            foreach(Players.Player plr in team.GetConnectedPlayersPlayers())
                Pipliz.Chatting.Chat.Send(plr, string.Format("<color=lightblue>[{0}]: {1}</color>", name, chat.Substring(4)));

            return true;
        }
    }
}
