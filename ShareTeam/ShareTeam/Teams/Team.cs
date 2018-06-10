using System.Collections.Generic;

using Pipliz;
using Pipliz.JSON;

using ShareTeam.Syncronization;

namespace ShareTeam.Teams
{
    public class Team
    {
        private static uint fake_AcountID = 11;

        private List<NetworkID> playersOnTeam { get; set; }
        public Players.Player fake_player { get; internal set; }
        private uint fake_playerID;

        public Team()
        {
            playersOnTeam = new List<NetworkID>();
            fake_player = Players.GetPlayer(new NetworkID(new Steamworks.CSteamID(new Steamworks.AccountID_t(fake_AcountID), Steamworks.EUniverse.k_EUniversePublic, Steamworks.EAccountType.k_EAccountTypeAnonUser)));

            fake_playerID = fake_AcountID;
            fake_AcountID++;
            TeamManager.GetTeamManager().AddTeam(this);
        }

        public void AddPlayer(Players.Player player)
        {
            Research_Syncronization.SyncronizeResearch(player, fake_player);
            Stockpile_Synchronization.SyncronizeStockpile(player, fake_player);

            foreach(Players.Player plr in GetConnectedPlayersPlayers())
                Pipliz.Chatting.Chat.Send(plr, string.Format("<color=green>{0} now shares his stockpile with you.</color>", player.Name));

            playersOnTeam.Add(player.ID);
        }

        public List<Players.Player> GetPlayers()
        {
            List<Players.Player> players = new List<Players.Player>();

            foreach(NetworkID networkID in playersOnTeam)
            {
                Players.Player player = Players.GetPlayer(networkID);
                if(null != player)
                    players.Add(player);
            }

            return players;
        }

        public List<Players.Player> GetConnectedPlayersPlayers()
        {
            List<Players.Player> players = new List<Players.Player>();

            foreach(NetworkID networkID in playersOnTeam)
            {
                Players.Player player = Players.GetPlayer(networkID);
                if(null != player && player.IsConnected)
                    players.Add(player);
            }

            return players;
        }

        public bool Contains(Players.Player plr)
        {
            return playersOnTeam.Contains(plr.ID);
        }

        public void RemovePlayer(Players.Player player)
        {
            playersOnTeam.Remove(player.ID);
            SplitStockpileOnLeave(player);

            foreach(Players.Player plr in GetConnectedPlayersPlayers())
                Pipliz.Chatting.Chat.Send(plr, string.Format("<color=red>{0} has stopped sharing his stockpile with you and have taken 1/{1} of the stockpile</color>", player.Name, (playersOnTeam.Count + 1) ));

            if(playersOnTeam.Count == 0)
                TeamManager.GetTeamManager().RemoveTeam(this);
        }

        private void SplitStockpileOnLeave(Players.Player player)
        {
            Stockpile player_Stockpile = Stockpile.GetStockPile(player);
            Stockpile fake_player_Stockpile = Stockpile.GetStockPile(fake_player);

            int split = playersOnTeam.Count + 1;

            for(ushort item = 0; item < ItemTypes.IndexLookup.MaxRegistered; item++)
            {
                int amount = fake_player_Stockpile.AmountContained(item);

                amount = amount / split;
                if(amount > 0)
                {
                    player_Stockpile.Add(item, amount);

                    fake_player_Stockpile.TryRemove(item, amount);
                }
            }

            player.SendStockpileInventory();
            fake_player.SendStockpileInventory();
        }

        //Load & saving
        public Team(JSONNode json)
        {
            if(!json.TryGetAs<uint>("fake_playerID", out uint fake_playerID))
                Log.Write("<color=red>Error loading the fake_playerID</color>");

            fake_player = Players.GetPlayer(new NetworkID(new Steamworks.CSteamID(new Steamworks.AccountID_t(fake_playerID), Steamworks.EUniverse.k_EUniversePublic, Steamworks.EAccountType.k_EAccountTypeAnonUser)));
            this.fake_playerID = fake_playerID;

            playersOnTeam = new List<NetworkID>();

            if(!json.TryGetChild("players", out JSONNode players))
                Log.Write("<color=red>Error loading the players</color>");

            foreach(var player in players.LoopArray())
                playersOnTeam.Add(NetworkID.Parse(player.GetAs<string>()));

            if(fake_AcountID <= fake_playerID)
                fake_AcountID = fake_playerID + 1;

            TeamManager.GetTeamManager().AddTeam(this);
        }

        public JSONNode GetSaveJSON()
        {
            JSONNode json = new JSONNode(NodeType.Object);
            json.SetAs("fake_playerID", fake_playerID);

            JSONNode players = new JSONNode(NodeType.Array);
            foreach(NetworkID nID in playersOnTeam)
                players.AddToArray(new JSONNode(nID.ToString()));
            json.SetAs("players", players);

            return json;
        }
    }
}
