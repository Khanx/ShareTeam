using System.Collections.Generic;

using ChatCommands;
using System;

using ShareTeam.Teams;

namespace ShareTeam.Commands
{
    class AutomaticChat : IChatCommand
    {
        public bool IsCommand(string chat)
        {
            if(chat.StartsWith("") || chat.Equals("/chat_team", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        public List<Players.Player> activeTeamChat { get; } = new List<Players.Player>();

        public bool TryDoCommand(Players.Player player, string chat)
        {
            Team team = TeamManager.GetTeamManager().GetTeam(player);

            if(chat.Equals("/chat_team", StringComparison.OrdinalIgnoreCase))
            {
                if(team == null)
                {
                    Pipliz.Chatting.Chat.Send(player, "<color=orange>You do not belong to any team.</color>");

                    return true;
                }

                if(!activeTeamChat.Contains(player))
                {
                    Pipliz.Chatting.Chat.Send(player, "<color=lime>Activated the automatic chat team.</color>");
                    activeTeamChat.Add(player);

                    return true;
                }
                else
                {
                    Pipliz.Chatting.Chat.Send(player, "<color=lime>Desactivated the automatic chat team.</color>");
                    activeTeamChat.Remove(player);

                    return true;
                }
             }

            if(activeTeamChat.Contains(player) && team == null)
            {
                activeTeamChat.Remove(player);

                return false;
            }

            if(chat.StartsWith("/") || !activeTeamChat.Contains(player))
                return false;

            string name = player.Name;

            foreach(Players.Player plr in team.GetConnectedPlayersPlayers())
                Pipliz.Chatting.Chat.Send(plr, string.Format("<color=lightblue>[{0}]: {1}</color>", name, chat));

            return true;
        }
    }
}
