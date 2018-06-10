using Pipliz;

using Harmony;
using System.Reflection;

using ShareTeam.Commands;
using ShareTeam.Teams;


namespace ShareTeam
{
    [ModLoader.ModManager]
    public class ShareTeam
    {
        public static long _nextUpdate;

        public static string MODPATH { get; internal set; }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnAssemblyLoaded, "Khanx.ShareTeam.GetModPath")]
        public static void GetModPath(string path)
        {
            MODPATH = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, "Khanx.ShareTeam.Harmony")]
        public static void Start()
        {
            var harmony = HarmonyInstance.Create("Khanx.ShareTeam.Harmony");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            //LoadTeams
            TeamManager.GetTeamManager().LoadTeams();
            _nextUpdate = 1000;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnAutoSaveWorld, "Khanx.ShareTeam.AutoSave")]
        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnQuit, "Khanx.ShareTeam.Save")]
        public static void Save()
        {
            Log.Write("Saving teams");
            TeamManager.GetTeamManager().SaveTeams();
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterStartup, "Khanx.ShareTeam.RegisterCommands")]
        public static void RegisterCommands()
        {
            ChatCommands.CommandManager.RegisterCommand(new Join());

            ChatCommands.CommandManager.RegisterCommand(new Accept());
            ChatCommands.CommandManager.RegisterCommand(new Reject());

            ChatCommands.CommandManager.RegisterCommand(new Leave());

            ChatCommands.CommandManager.RegisterCommand(new Members());
            ChatCommands.CommandManager.RegisterCommand(new Chat());
            ChatCommands.CommandManager.RegisterCommand(new AutomaticChat());
        }


        private static bool nextDay = true;

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdateEnd, "Khanx.ShareTeam.Update")]
        public static void Update()
        {
            if(Time.MillisecondsSinceStart > _nextUpdate)
            {
                if(TimeCycle.IsDay && nextDay)
                {
                    foreach(Team team in TeamManager.GetTeamManager().GetTeams())
                    {
                        float total_food_needed = 0f;
                        foreach(Players.Player plr in team.GetPlayers())
                            total_food_needed += ( Colony.Get(plr).FollowerCount * 5 );

                        foreach(Players.Player plr in team.GetConnectedPlayersPlayers())
                            Pipliz.Chatting.Chat.Send(plr, string.Format("<color=green>Total food use/day: {0}</color>", total_food_needed));

                        
                    }

                    nextDay = false;
                }

                if(!TimeCycle.IsDay)
                    nextDay = true;

                foreach(Team team in TeamManager.GetTeamManager().GetTeams())
                {
                    foreach(Players.Player plr in team.GetConnectedPlayersPlayers())
                    {
                        plr.SendStockpileInventory();   //Stockpile Synchronization
                    }
                }
                _nextUpdate = Time.MillisecondsSinceStart + 1000;
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerRecipeSettingChanged, "Khanx.ShareTeam.SyncronizeRecipeSetting")]
        public static void OnPlayerRecipeSettingChanged(RecipeStorage.PlayerRecipeStorage storage, Recipe recipe, Box<RecipeStorage.RecipeSetting> recipeSetting)
        {
            //If the setting changed is from a fake player
            if(storage.Player.ID.type == NetworkID.IDType.Steam && storage.Player.ID.steamID.GetEAccountType() == Steamworks.EAccountType.k_EAccountTypeAnonUser)
            {
                Team team = TeamManager.GetTeamManager().GetTeamOfFakePlayer(storage.Player);

                foreach(Players.Player plr in team.GetConnectedPlayersPlayers())
                {
                    RecipePlayer.SendRecipes(plr);  //Recipes in job (how many create) Synchronization (Depends on the stockpile)
                }
            }
        }
    }
}
