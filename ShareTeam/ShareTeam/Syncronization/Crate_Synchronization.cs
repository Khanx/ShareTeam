using Pipliz;
using ShareTeam.Teams;

namespace ShareTeam.Syncronization
{
    [ModLoader.ModManager]
    public class Crate_Synchronization
    {
        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerConnectedLate, "Khanx.ShareTeam.SyncronizeCratesOnPlayerConnected")]
        public static void SyncronizeCratesOnPlayerConnected(Players.Player player)
        {
            if(null == player)
                return;

            //Fake Player
            if(player.ID.type == NetworkID.IDType.Steam && player.ID.steamID.GetEAccountType() == Steamworks.EAccountType.k_EAccountTypeAnonUser)
                return;

            Team team = TeamManager.GetTeamManager().GetTeam(player);

            if(null == team)
                return;

            SyncronizeCrates(player, team.fake_player);
        }

        public static void SyncronizeCrates(Players.Player player, Players.Player fake_player)
        {
            //Stockpile player_Stockpile = Stockpile.GetStockPile(player);
            Stockpile.TryGetStockpile(player, out Stockpile player_Stockpile);
            Stockpile fake_player_Stockpile = Stockpile.GetStockPile(fake_player);

            foreach(Vector3Int crate in player_Stockpile._crates)
            {
                if(!fake_player_Stockpile._crates.Contains(crate))
                    fake_player_Stockpile._crates.Add(crate);
            }
        }
    }
}
