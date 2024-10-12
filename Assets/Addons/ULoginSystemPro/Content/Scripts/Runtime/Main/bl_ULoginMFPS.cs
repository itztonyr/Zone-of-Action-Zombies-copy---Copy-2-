using MFPS.ULogin;
using UnityEngine;

public static class bl_ULoginMFPS
{
    /// <summary>
    /// 
    /// </summary>
    public static bool SendNewData(this LoginUserInfo userInfo, int kills, int deaths, int score, int assist)
    {
        if (kills == 0 && deaths == 0 && score == 0 && assist == 0) return false;

        int finalScore = Mathf.Max(1, score);
        userInfo.Score += finalScore;
        userInfo.Kills += kills;
        userInfo.Deaths += deaths;
        userInfo.Assist += assist;

        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static string GetLocalUserInitialCoins()
    {
        string line = "";
        var all = bl_MFPS.Coins.GetAllCoins();
        for (int i = 0; i < all.Count; i++)
        {
            var coin = all[i];
            line += $"{coin.InitialCoins}";
            if (i != all.Count - 1) { line += "&"; }
        }
        return line;
    }
}