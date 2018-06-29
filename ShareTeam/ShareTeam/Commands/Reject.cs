using ChatCommands;
using System;
using System.Text.RegularExpressions;

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
            Match m = Regex.Match(chat, @"/reject_team (?<playername>['].+?[']|[^ ]+)");
            if(!m.Success)
            {
                Pipliz.Chatting.Chat.Send(player, "<color=orange>Command didn't match, use /reject_team [playername]</color>");
                return true;
            }

            string name = m.Groups["playername"].Value;

            if(name.StartsWith("'"))
            {
                if(name.EndsWith("'"))
                {
                    name = name.Substring(1, name.Length - 2);
                }
                else
                {
                    Pipliz.Chatting.Chat.Send(player, "<color=orange>Command didn't match, missing ' after playername</color>");
                    return true;
                }
            }

            if(!Players.TryMatchName(name, out Players.Player wantToJoin_byNAME))
            {
                Pipliz.Chatting.Chat.Send(player, string.Format("<color=orange>Player {0} not found.</color>", name));

                return true;
            }

            if(!Join.joinTo.TryGetValue(player, out Players.Player wantToJoin) || wantToJoin != wantToJoin_byNAME)
            {
                Pipliz.Chatting.Chat.Send(player, string.Format("<color=orange>Player {0} has not requested to join your team.</color>", name));

                return true;
            }


            Pipliz.Chatting.Chat.Send(player, string.Format("<color=lime>You have rejected {0} to join your team.</color>", name));
            Pipliz.Chatting.Chat.Send(wantToJoin, string.Format("<color=lime>{0} has rejected your request to join his team.</color>", player.Name));


            Join.joinTo.Remove(player);

            return true;
        }
    }
}
