using System.Collections.Generic;

using Pipliz;
using Server.Science;

using Harmony;
using System.Reflection;

using ShareTeam.Teams;


namespace ShareTeam.Syncronization
{
    [ModLoader.ModManager]
    public static class Research_Syncronization
    {
        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerConnectedLate, "Khanx.ShareTeam.SyncronizeResearchOnPlayerConnected")]
        public static void SyncronizeResearchOnPlayerConnected(Players.Player player)
        {
            if(null == player)
                return;

            //Fake Player
            if(player.ID.type == NetworkID.IDType.Steam && player.ID.steamID.GetEAccountType() == Steamworks.EAccountType.k_EAccountTypeAnonUser)
                return;

            Team team = TeamManager.GetTeamManager().GetTeam(player);

            if(null == team)
                return;

            SyncronizeResearch(player, team.fake_player);
        }

        public static void SyncronizeResearch(Players.Player player, Players.Player fake_plater)
        {
            ScienceManagerPlayer player_ScienceManager = ScienceManager.GetPlayerManager(player);
            ScienceManagerPlayer fake_player_ScienceManager = ScienceManager.GetPlayerManager(fake_plater);

            List<ResearchProgress> plrinProgressResearch = player_ScienceManager.GetType().GetField("inProgressResearch", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player_ScienceManager) as List<ResearchProgress>;
            List<ResearchProgress> fake_plrinProgressResearch = fake_player_ScienceManager.GetType().GetField("inProgressResearch", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(fake_player_ScienceManager) as List<ResearchProgress>;

            if(null == plrinProgressResearch && null == fake_plrinProgressResearch)
                return;

            if(null == fake_plrinProgressResearch)
            {
                Log.Write("<color=green>Research from player to fakeplayer</color>");
                fake_player_ScienceManager.GetType().GetField("inProgressResearch", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(fake_player_ScienceManager, plrinProgressResearch);

                foreach(ResearchProgress rp in plrinProgressResearch)
                {
                    if(rp.research.GetResearchIterationCount() == rp.progress)
                        rp.research.OnResearchComplete(fake_player_ScienceManager, EResearchCompletionReason.Loaded);
                }

                return;
            }

            if(null == plrinProgressResearch)
            {
                Log.Write("<color=green>Research from fake_player to player</color>");
                player_ScienceManager.GetType().GetField("inProgressResearch", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(player_ScienceManager, fake_plrinProgressResearch);

                foreach(ResearchProgress rp in fake_plrinProgressResearch)
                {
                    if(rp.research.GetResearchIterationCount() == rp.progress)
                     rp.research.OnResearchComplete(player_ScienceManager, EResearchCompletionReason.Loaded);
                }

                return;
            }

            foreach(ResearchProgress rpPlr in plrinProgressResearch)
            {
                bool contains = false;
                foreach(ResearchProgress rpFake_Plr in fake_plrinProgressResearch)
                {
                    if(rpFake_Plr.research == rpPlr.research)
                    {
                        contains = true;

                        if(rpPlr.progress > rpFake_Plr.progress)    //Replace for the research with more progress
                        {
                            fake_plrinProgressResearch.Remove(rpFake_Plr);
                            fake_plrinProgressResearch.Add(rpPlr);
                        }

                        break;
                    }
                }

                if(!contains)
                    fake_plrinProgressResearch.Add(rpPlr);
            }

            Log.Write("<color=green>Synchronization</color>");

            //Fake player synchronization
            fake_player_ScienceManager.GetType().GetField("inProgressResearch", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(fake_player_ScienceManager, fake_plrinProgressResearch);

            //Player synchronization
            player_ScienceManager.GetType().GetField("inProgressResearch", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(player_ScienceManager, fake_plrinProgressResearch);

            foreach(ResearchProgress rp in fake_plrinProgressResearch)
            {
                if(rp.research.GetResearchIterationCount() == rp.progress)
                {
                    rp.research.OnResearchComplete(fake_player_ScienceManager, EResearchCompletionReason.Loaded);
                    rp.research.OnResearchComplete(player_ScienceManager, EResearchCompletionReason.Loaded);
                }
            }
        }
    }

    [ModLoader.ModManager]
    [HarmonyPatch(typeof(ScienceManagerPlayer))]
    [HarmonyPatch("Select")]
    [HarmonyPatch(new System.Type[] { typeof(ResolvedResearchable) })]
    public static class HarmonySelect
    {
        //Synchronization of research progess of the selected research
        public static void Postfix(ScienceManagerPlayer __instance)
        {
            //If NOT fake player
            if(__instance.Player.ID.type != NetworkID.IDType.Steam || __instance.Player.ID.steamID.GetEAccountType() != Steamworks.EAccountType.k_EAccountTypeAnonUser)
                return;

            ScienceManagerPlayer fake_player_ScienceManager = ScienceManager.GetPlayerManager(__instance.Player);

            if(!fake_player_ScienceManager.TryGetProgress(__instance.ActiveResearch.research, out ResearchProgress rp))
                return;

            int addProgress = rp.progress - __instance.ActiveResearch.progress;

            if(addProgress > 0)
                __instance.AddActiveResearchProgress(addProgress);
        }
    }

    [ModLoader.ModManager]
    [HarmonyPatch(typeof(ScienceManagerPlayer))]
    [HarmonyPatch("AddActiveResearchProgress")]
    public static class HarmonyAddActiveResearchProgress
    {
        public static bool Prefix(ScienceManagerPlayer __instance, int progress)
        {
            Team team = TeamManager.GetTeamManager().GetTeam(__instance.Player);

            if(null == team)
                return true;

            ScienceManagerPlayer fake_player_ScienceManager = ScienceManager.GetPlayerManager(team.fake_player);

            ResolvedResearchable research = ScienceManager.GetResearchable(__instance.ActiveResearch.research.GetKey());


            if(fake_player_ScienceManager.HasCompleted(research))
                return true;

            if(fake_player_ScienceManager.ActiveResearch.research == __instance.ActiveResearch.research)
                fake_player_ScienceManager.AddActiveResearchProgress(progress);
            else
            {
                fake_player_ScienceManager.Select(research);
                fake_player_ScienceManager.AddActiveResearchProgress(progress);
            }

            return true;
        }

        public static void Postfix(ScienceManagerPlayer __instance)
        {
            //Not is fake_player
            if(__instance.Player.ID.type != NetworkID.IDType.Steam || __instance.Player.ID.steamID.GetEAccountType() != Steamworks.EAccountType.k_EAccountTypeAnonUser)
                return;

            ResolvedResearchable research = ScienceManager.GetResearchable(__instance.ActiveResearch.research.GetKey());

            if(__instance.HasCompleted(research))
            {
                foreach(Players.Player plr in TeamManager.GetTeamManager().GetTeamOfFakePlayer(__instance.Player).GetPlayers())
                {
                    ScienceManagerPlayer SMP = ScienceManager.GetPlayerManager(plr);

                    if(null == SMP || SMP.HasCompleted(research))
                        continue;

                    int addProgress = __instance.ActiveResearch.progress - SMP.ActiveResearch.progress;

                    if(addProgress > 0)
                    {
                        if(__instance.ActiveResearch.research != SMP.ActiveResearch.research)
                            SMP.Select(research);
                        SMP.AddActiveResearchProgress(addProgress);
                    }
                }
            }
        }
    }
}
