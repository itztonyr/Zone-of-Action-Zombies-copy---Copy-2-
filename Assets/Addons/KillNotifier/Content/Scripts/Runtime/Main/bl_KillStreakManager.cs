using MFPS.Addon.KillStreak;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class bl_KillStreakManager : MonoBehaviour
{
    public int currentStreak { get; set; }
    public Queue<KillStreakInfo> queueNotifiers = new();
    private string lastLocalKiller;
    private int deathsInStreak = 0;

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        bl_EventHandler.onLocalKill += OnLocalKill;
        bl_EventHandler.onLocalPlayerDeath += OnLocalPlayerDeath;
        bl_EventHandler.onPlayerDeath += OnPlayerDie;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        bl_EventHandler.onLocalKill -= OnLocalKill;
        bl_EventHandler.onLocalPlayerDeath -= OnLocalPlayerDeath;
        bl_EventHandler.onPlayerDeath -= OnPlayerDie;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnLocalKill(KillInfo info)
    {
        currentStreak++;

        if (currentStreak % 5 == 0 && currentStreak < 50) // more than 50 is too much
        {
            // fire an kill streak event each 5 kills until 50
            bl_EventHandler.DispatchGameplayPlayerEvent($"kill {currentStreak}");
        }

        KillStreakInfo notifierInfo = bl_KillNotifierData.Instance.GetKillStreakInfo(currentStreak);
        if (notifierInfo.Skip) return;

        notifierInfo.killID = currentStreak;
        notifierInfo.info = info;
        CheckSpecialEvents(ref notifierInfo, info);

        if (notifierInfo.ExtraScore > 0)
        {
            bool isBot = IsBot(info.Killed);
            if (isBot && bl_GameData.CoreSettings.howConsiderBotsEliminations == MFPS.Runtime.AI.BotKillConsideration.SameAsRealPlayers)
            {
                bl_PhotonNetwork.LocalPlayer.PostScore(notifierInfo.ExtraScore);
            }
            else if (!isBot)
            {
                bl_PhotonNetwork.LocalPlayer.PostScore(notifierInfo.ExtraScore);
            }
        }

        queueNotifiers.Enqueue(notifierInfo);
        if (queueNotifiers.Count <= 1)
        {
            //start showing streaks UI
            bl_KillNotifier.Instance.Show();
        }

        deathsInStreak = 0;
#if KSA
        if (bl_KillStreakHandler.Instance != null) bl_KillStreakHandler.Instance.OnNewKill(currentStreak);
#endif
    }

    /// <summary>
    /// Verify if the kill has some special events
    /// </summary>
    /// <param name="killInfo"></param>
    private void CheckSpecialEvents(ref KillStreakInfo killNotifierInfo, KillInfo killInfo)
    {
        killNotifierInfo.specials.Clear();
        if (killInfo.byHeadShot) killNotifierInfo.AddSpecial("headshot");

        // if the killed player is the last player who killed the local player
        if (killInfo.Killed == lastLocalKiller && lastLocalKiller != bl_PhotonNetwork.LocalPlayer.NickName)
        {
            killNotifierInfo.AddSpecial("revenge");
            lastLocalKiller = string.Empty;
        }

        if (killInfo.Distance > bl_KillNotifierData.Instance.longShotDistanceThreshold)
        {
            killNotifierInfo.AddSpecial("long-shot");
        }

        var weaponInfo = bl_MFPS.Weapon.GetWeaponInfo(killInfo.GunID);
        if (weaponInfo != null)
        {
            if (weaponInfo.Type == GunType.Melee)
            {
                killNotifierInfo.AddSpecial("melee-kill");
            }
        }

        if (deathsInStreak >= 3)
        {
            killNotifierInfo.AddSpecial("comeback");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="playerName"></param>
    /// <returns></returns>
    private bool IsBot(string playerName)
    {
        return bl_AIMananger.Instance.GetBotStatistics(playerName) != null;
    }

    /// <summary>
    /// 
    /// </summary>
    public KillStreakInfo GetQueueNotifier()
    {
        return queueNotifiers.Count > 0 ? queueNotifiers.Dequeue() : null;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnLocalPlayerDeath()
    {
        ResetStreak();
        deathsInStreak++;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    void OnPlayerDie(bl_EventHandler.PlayerDeathData e)
    {
        // if the dead player is the local player
        if (e.Player.IsLocal)
        {
            lastLocalKiller = e.KillerName;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void ResetStreak()
    {
        currentStreak = 0;
        bl_KillNotifier.Instance.Hide();
    }

    private static bl_KillStreakManager _instance;
    public static bl_KillStreakManager Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_KillStreakManager>(); }
            return _instance;
        }
    }
}