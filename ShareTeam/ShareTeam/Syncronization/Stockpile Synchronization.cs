using Harmony;

using ShareTeam.Teams;

namespace ShareTeam.Syncronization
{
    [ModLoader.ModManager]
    public class Stockpile_Synchronization
    {
        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerConnectedLate, "Khanx.ShareTeam.SyncronizeStockpileOnPlayerConnected")]
        public static void OnPlayerConnectedLate(Players.Player player)
        {
            if(null == player)
                return;

            //Fake Player
            if(player.ID.type == NetworkID.IDType.Steam && player.ID.steamID.GetEAccountType() == Steamworks.EAccountType.k_EAccountTypeAnonUser)
                return;

            Team team = TeamManager.GetTeamManager().GetTeam(player);

            if(null == team)
                return;

            SyncronizeStockpile(player, team.fake_player);
        }

        public static void SyncronizeStockpile(Players.Player player, Players.Player fake_player)
        {
            Stockpile player_Stockpile = Stockpile.GetStockPile(player);
            Stockpile fake_player_Stockpile = Stockpile.GetStockPile(fake_player);

            for(ushort item = 0; item < ItemTypes.IndexLookup.MaxRegistered; item++)
            {
                int amount = player_Stockpile.AmountContained(item);

                if(amount > 0)
                {
                    fake_player_Stockpile.Add(item, amount);

                    player_Stockpile.TryRemove(item, amount);
                }
            }
        }
    }

    [ModLoader.ModManager]
    [HarmonyPatch(typeof(Stockpile))]
    [HarmonyPatch("GetStockPile")]
    public static class HarmonyGetStockpile
    {
        public static bool Prefix(ref Players.Player player)
        {
            Team team = TeamManager.GetTeamManager().GetTeam(player);
            if(null != team)
                player = team.fake_player;

            return true;
        }
    }
}
