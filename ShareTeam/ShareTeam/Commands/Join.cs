using System;
using System.Collections.Generic;

using ChatCommands;

using ShareTeam.Teams;


namespace ShareTeam.Commands
{
    public class Join : IChatCommand
    {
        public static Dictionary<Players.Player, Players.Player> joinTo = new Dictionary<Players.Player, Players.Player>();

        public bool IsCommand(string chat)
        {
            if(chat.StartsWith("/join_team", StringComparison.OrdinalIgnoreCase))
                return true;


            return false;
        }

        public bool TryDoCommand(Players.Player player, string chat)
        {
            string[] args = CommandManager.SplitCommand(chat);

            if(args.Length == 1)
            {
                Pipliz.Chatting.Chat.Send(player, "<color=orange>You have to specify the player with whom you are going to make/join a team the stockpile.</color>");

                return true;
            }

            string name = args[1];

            if(!Players.TryMatchName(name, out Players.Player join_to_player))
            {
                Pipliz.Chatting.Chat.Send(player, string.Format("<color=orange>Player {0} not found.</color>", name));

                return true;
            }

            
            if(player == join_to_player)
            {
                Pipliz.Chatting.Chat.Send(player, "<color=orange>You can not share with yourself.</color>");

                return true;
            }
            

            TeamManager TM = TeamManager.GetTeamManager();

            if(null != TM.GetTeam(player) && null != TM.GetTeam(join_to_player))
            {
                Pipliz.Chatting.Chat.Send(player, "<color=orange> Both of you are in teams, and teams can not be merged, one should leave his team.</color>");

                return true;
            }


            if(joinTo.ContainsKey(join_to_player))
            {
                Pipliz.Chatting.Chat.Send(player, string.Format("<color=orange>{0} already has a request to accept, try latter.</color>", name));

                return true;
            }

            joinTo.Add(join_to_player, player);

            Pipliz.Chatting.Chat.Send(join_to_player, string.Format("<color=lime>{0} has requested to join your team, write /accept_team {0} or /reject_team {0}</color>", player.Name));
            Pipliz.Chatting.Chat.Send(join_to_player, string.Format("<color=yellow>{0} will be rejected in a minute if you do not answer</color>", player.Name));

            Pipliz.Chatting.Chat.Send(player, string.Format("<color=lime>You have requested to join {0} team</color>", name));

            Pipliz.Threading.ThreadManager.InvokeOnMainThread(delegate () //Automatically reject after 30s
            {
                if(joinTo.ContainsKey(join_to_player))
                {
                    Pipliz.Chatting.Chat.Send(join_to_player, string.Format("<color=lime>You have rejected {0} to join your team.</color>", player.Name));
                    Pipliz.Chatting.Chat.Send(player, string.Format("<color=lime>{0} has rejected your request to join his team.</color>", join_to_player.Name));

                    joinTo.Remove(join_to_player);
                }

            }, 60);

            return true;
        }
    }
}
