using MFPS.Internal.Scriptables;
using UnityEngine;

public abstract class bl_MatchFinishResumeBase : bl_MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    public abstract void CollectMatchData();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public abstract void SetActive(bool active);

    /// <summary>
    /// 
    /// </summary>
    public virtual void SaveMatchInDataBase(int coinsEarned, int scoreEarned, bool overridePlayerScore = false)
    {
        if (overridePlayerScore) bl_MFPSDatabase.User.StorePlayerMatchStats(scoreEarned);
        else bl_MFPSDatabase.User.StorePlayerMatchStats();

        bl_MFPSDatabase.PlayTime.StopRecordingPlayTime();

        if (coinsEarned > 0)
        {
            bl_MFPSDatabase.Coins.Add(coinsEarned, (MFPSCoin)bl_GameData.ScoreSettings.XPCoin);
        }

#if CLANS
        if (bl_DataBase.IsUserLogged) bl_DataBase.Instance.SetClanScore(scoreEarned);
#endif

        bl_EventHandler.Match.onMatchOver?.Invoke();
    }

    /// <summary>
    /// 
    /// </summary>
    public virtual void GoToLobby()
    {
        CancelInvoke();
        if (bl_PhotonNetwork.IsConnected && bl_PhotonNetwork.InRoom)
        {
            if (bl_MFPS.RoomGameMode.CurrentGameModeData.AfterFinishMatch == GameModeSettings.AfterFinishMatchLogic.ReturnToWaitingRoom && bl_MFPS.GameData.UsingWaitingRoom())
            {
                bl_GameManager.Instance.OnLeftRoom();
            }
            else
            {
                bl_PhotonNetwork.LeaveRoom();
            }
        }
        else
        {
            bl_UtilityHelper.LoadLevel(bl_GameData.CoreSettings.MainMenuScene);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public int GetLocalPlayerGainScore()
    {
        int score = bl_PhotonNetwork.LocalPlayer.GetPlayerScore();
        int timePlayed = Mathf.RoundToInt(bl_GameManager.Instance.PlayedTime);
        int scorePerTime = timePlayed * bl_GameData.ScoreSettings.ScorePerTimePlayed;
        int hsscore = bl_GameManager.Instance.Headshots * bl_GameData.ScoreSettings.ScorePerHeadShot;
        bool winner = bl_GameManager.Instance.IsLocalPlayerWinner();
        int winScore = (winner) ? bl_GameData.ScoreSettings.ScoreForWinMatch : 0;
        int total = score + winScore + scorePerTime + hsscore;

        return total;
    }

    private static bl_MatchFinishResumeBase instance = null;
    public static bl_MatchFinishResumeBase Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<bl_MatchFinishResumeBase>();
            }
            return instance;
        }
    }
}