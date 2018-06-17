using Pipliz;
using ShareTeam.Teams;

namespace ShareTeam.Syncronization
{
    [ModLoader.ModManager]
    public class Crate_Synchronization
    {
        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, "Khanx.ShareTeam.SyncronizeCratesAfterWorldLoad")]
        [ModLoader.ModCallbackDependsOn("Khanx.ShareTeam.Start")]
        public static void SyncronizeCratesAfterWorldLoad()
        {
            foreach(Team team in TeamManager.GetTeamManager().GetTeams())
                foreach(Players.Player player in team.GetPlayers())
                    SyncronizeCrates(player, team.fake_player);
        }

        public static void SyncronizeCrates(Players.Player player, Players.Player fake_player)
        {
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
