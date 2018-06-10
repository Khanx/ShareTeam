using System.Collections.Generic;

using Pipliz;
using Pipliz.JSON;

namespace ShareTeam.Teams
{
    public class TeamManager
    {
        private static TeamManager instance;
        private List<Team> teams;

        private static string jsonFile = "/teams.json";

        private TeamManager()
        {
            teams = new List<Team>();
        }

        public static TeamManager GetTeamManager()
        {
            if(null == instance)
                instance = new TeamManager();

            return instance;
        }

        public void AddTeam(Team team)
        {
            teams.Add(team);
        }

        public void RemoveTeam(Team team)
        {
            teams.Remove(team);
        }

        public Team GetTeam(Players.Player plr)
        {
            foreach(Team team in teams)
                if(team.Contains(plr))
                    return team;

            return null;
        }

        public List<Team> GetTeams()
        {
            return teams;
        }

        public Team GetTeamOfFakePlayer(Players.Player plr)
        {
            foreach(Team team in teams)
                if(team.fake_player == plr)
                    return team;

            return null;
        }

        public void LoadTeams()
        {
            Log.Write("<color=green>Loading teams</color>");
            try
            {
                JSONNode teams = JSON.Deserialize(ShareTeam.MODPATH + jsonFile);

                foreach(JSONNode team in teams.LoopArray())
                    new Team(team);
            }
            catch(System.Exception e)
            {
                Log.Write(string.Format("<color=red>Error loading teams:</color>\n{0}", e.Message));
            }
            Log.Write(string.Format("<color=green>{0} teams loaded</color>", this.teams.Count));
        }

        public void SaveTeams()
        {
            try
            {
                if(teams.Count == 0)
                    Log.Write("<color=blue>No teams to save</color>");

                JSONNode json = new JSONNode(NodeType.Array);

                foreach(Team team in teams)
                    json.AddToArray(team.GetSaveJSON());

                JSON.Serialize(ShareTeam.MODPATH + jsonFile, json, 2);
            }
            catch(System.Exception e)
            {
                Log.Write(string.Format("<color=red>Error Saving teams:</color>\n{0}", e.Message));
            }
        }
    }
}
