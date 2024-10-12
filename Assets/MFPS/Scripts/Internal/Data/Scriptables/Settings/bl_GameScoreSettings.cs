using MFPSEditor;
using UnityEngine;

namespace MFPS.Internal.Scriptables
{
    [CreateAssetMenu(fileName = "Score Settings", menuName = "MFPS/Settings/Game Score")]
    public class bl_GameScoreSettings : ScriptableObject
    {
        [Tooltip("Coin to apply the rewards")]
        [MFPSCoinID] public int XPCoin;
        [Tooltip("how much score/xp worth one coin")]
        public int CoinScoreValue = 1000;//how much score/xp worth one coin
        [Space]
        public int ScorePerKill = 50;
        public int ScorePerHeadShot = 25;
        public int ScoreForWinMatch = 100;
        [Tooltip("Per minute played")]
        public int ScorePerTimePlayed = 3;
        public int ScorePerKillAssist = 20;
        public int ScorePerVehicleDestroy = 200;

        /// <summary>
        /// 
        /// </summary>
        public int InitialCoins
        {
            get
            {
                var coin = bl_MFPS.Coins.GetCoinData(XPCoin);
                return coin == null ? 0 : coin.InitialCoins;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newCoins"></param>
        public void AddCoins(int newCoins, string endPoint = "") => bl_MFPS.Coins.GetCoinData(XPCoin)?.Add(newCoins, endPoint);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="score"></param>
        /// <returns></returns>
        public int GetCoinsPerScore(int score)
        {
            return score <= 0 || score < CoinScoreValue || CoinScoreValue <= 0 ? 0 : score / CoinScoreValue;
        }

        public int GetScorePerTimePlayed(int time)
        {
            return ScorePerTimePlayed <= 0 ? 0 : time * ScorePerTimePlayed;
        }
    }
}