using Harmony;

using ShareTeam.Teams;


namespace ShareTeam.Syncronization
{
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
