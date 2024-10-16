﻿using MFPS.Runtime.AI;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class bl_PlayerScoreboardTable : bl_PlayerScoreboardTableBase
{
    public Team team = Team.All;
    public RectTransform panel;
    public GameObject joinButton;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    /// <param name="uiPrefab"></param>
    /// <returns></returns>
    public override bl_PlayerScoreboardUIBase Instance(Player player, GameObject uiPrefab)
    {
        GameObject instance = Instantiate(uiPrefab) as GameObject;
        instance.transform.SetParent(panel, false);
        var script = instance.GetComponent<bl_PlayerScoreboardUIBase>();
        script.Init(player);
        instance.SetActive(true);
        return script;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    /// <param name="uiPrefab"></param>
    /// <returns></returns>
    public override bl_PlayerScoreboardUIBase InstanceBot(MFPSBotProperties player, GameObject uiPrefab)
    {
        GameObject instance = Instantiate(uiPrefab) as GameObject;
        instance.transform.SetParent(panel, false);
        var script = instance.GetComponent<bl_PlayerScoreboardUIBase>();
        script.Init(null, player);
        instance.SetActive(true);
        return script;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void SetActiveJoinButton(bool active)
    {
        joinButton.SetActive(active);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void ResetJoinButton()
    {
        joinButton.GetComponent<Button>().interactable = true;
    }
}