using Harmony;

using Pipliz;

using ShareTeam.Teams;


namespace ShareTeam.Syncronization
{
    [ModLoader.ModManager]
    public class Recipe_Synchronization
    {
        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerRecipeSettingChanged, "Khanx.ShareTeam.SyncronizeRecipeSetting")]
        public static void OnPlayerRecipeSettingChanged(RecipeStorage.PlayerRecipeStorage storage, Recipe recipe, Box<RecipeStorage.RecipeSetting> recipeSetting)
        {
            //If the setting changed is from a fake player
            if(storage.Player.ID.type == NetworkID.IDType.Steam && storage.Player.ID.steamID.GetEAccountType() == Steamworks.EAccountType.k_EAccountTypeAnonUser)
            {
                Team team = TeamManager.GetTeamManager().GetTeamOfFakePlayer(storage.Player);
                if(null == team)
                    return;

                foreach(Players.Player plr in team.GetConnectedPlayersPlayers())
                {
                    RecipePlayer.SendRecipes(plr);  //Recipes in job (how many create) Synchronization (Depends on the stockpile)
                }
            }
        }
    }

    [ModLoader.ModManager]
    [HarmonyPatch(typeof(RecipeStorage))]
    [HarmonyPatch("GetPlayerStorage")]
    public static class HarmonyGetPlayerStorage
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
