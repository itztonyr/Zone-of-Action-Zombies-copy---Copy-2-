using UnityEngine;

namespace MFPS.Addon.GameResumePro
{
    public class bl_GameResumeProTest : MonoBehaviour
    {
        /* public int startScore = 200;
         public int kills = 10;
         public int deaths = 3;
         [SerializeField] private bl_GameResumePro gameResumePro = null;


         public void Test()
         {
             SetFakeStats();
             CalculateStats();
         }

         private void SetFakeStats()
         {
             gameResumePro.SetStat("start-time", 0);
             gameResumePro.SetStat("start-time", 0);
             gameResumePro.SetStat($"kw-Rifle", 10);
             gameResumePro.SetStat($"kp-TesPlayer", 5);
 #if LM
             gameResumePro.SetStat("start-score", startScore);
 #endif
         }

         /// <summary>
         /// 
         /// </summary>
         private void CalculateStats()
         {

             int score = kills * bl_GameData.Instance.ScoreReward.ScorePerKill;
             float kd = bl_MathUtility.GetKDRatio(kills, deaths);

             int secondsPlayed = 100;
             int minutesPlayed = secondsPlayed / 60;
             int scorePerMinute = minutesPlayed > 0 ? score / minutesPlayed : score;
             int shotsHits = 35;
             int headShots = 4;

             int winScore = 0;
             int scorePerPlayedTime = 0;
             int totalScore = score + winScore + scorePerPlayedTime;
             gameResumePro.SetStat("total-score-gained", totalScore);

             float hsPercentage = 0;
             if (shotsHits > 0 && headShots > 0)
             {
                 hsPercentage = ((float)headShots / (float)shotsHits) * 100;
             }

             int totalCoinsGained = bl_GameData.Instance.VirtualCoins.GetCoinsPerScore(totalScore);
             gameResumePro.SetStat("total-coins-gained", totalCoinsGained);

             var ui = gameResumePro.resumeUI;

             ui.InstanceStat("Kills", kills);
             ui.InstanceStat("Deaths", deaths);
             ui.InstanceStat("Score", score);
             ui.InstanceStat("Shots Fired", 70);
             ui.InstanceStat("Shots Hit", shotsHits);
             ui.InstanceStat("Head Shots", headShots);
             ui.InstanceStat("Time Played", secondsPlayed);
             ui.InstanceStat("Score Per Minute", scorePerMinute);
             ui.InstanceStat("Total XP Gained", totalScore);
 #if LM
             ui.InstanceStat("Total Score", startScore + totalScore);
 #endif

             ui.CalculateBestWeapon();
             ui.CalculatedAdoption();
             ui.SetReturnToLobbyTime(200);
             ui.Show();
         }

         private void Update()
         {
             if (Input.GetKeyDown(KeyCode.Space))
             {
                 Test();
             }
         }*/
    }
}