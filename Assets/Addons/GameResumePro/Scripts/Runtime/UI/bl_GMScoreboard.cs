namespace MFPS.Addon.GameResumePro
{
    public class bl_GMScoreboard : bl_PhotonHelper
    {
        public bl_PlayerScoreboard scoreboardManager;
        public bl_PlayerScoreboardTableBase scoreboardTemplate;

        private bool isInit = false;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            if (!isInit) Init();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Init()
        {
            scoreboardManager.playerscoreboardUIBinding.SetActive(false);
            if (isOneTeamMode)
            {
                scoreboardManager.OneTeamScoreboard = scoreboardTemplate;
                var teamUI = scoreboardTemplate.GetComponentInChildren<bl_GraphicTeamColor>();
                if(teamUI != null)
                {
                    teamUI.team = Team.All;
                }
            }
            else
            {
                var teamUI = scoreboardTemplate.GetComponentInChildren<bl_GraphicTeamColor>();
                if (teamUI != null)
                {
                    teamUI.team = Team.Team2;
                }

                var second = Instantiate(scoreboardTemplate.gameObject);
                second.transform.SetParent(scoreboardTemplate.transform.parent, false);
                scoreboardManager.TwoTeamScoreboards = new bl_PlayerScoreboardTableBase[2];
                scoreboardManager.TwoTeamScoreboards[0] = scoreboardTemplate;
                scoreboardManager.TwoTeamScoreboards[1] = second.GetComponent<bl_PlayerScoreboardTableBase>();

                if (teamUI != null)
                {
                    teamUI.team = Team.Team1;
                }
            }

            scoreboardManager.ForceUpdateAll();
            isInit = true;
        }
    }
}