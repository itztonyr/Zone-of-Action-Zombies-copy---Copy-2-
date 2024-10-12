using MFPS.Runtime.Settings;
using System.Collections;
using System.Collections.Generic;
using Hashtable = ExitGames.Client.Photon.Hashtable;

/// <summary>
/// This script handle all the room properties, setup the required local player data containers
/// and handle the auto team selection logic.
/// </summary>
public class bl_RoomSettings : bl_MonoBehaviour
{
    [LovattoToogle] public bool canSuicide = true;

    #region Public properties
    public GameMode CurrentGameMode => CurrentRoomInfo.gameMode;
    public int GameGoal => CurrentRoomInfo.goal;
    public bool AutoTeamSelection
    {
        get
        {
            if (CurrentRoomInfo == null) CurrentRoomInfo = bl_PhotonNetwork.CurrentRoom.GetRoomInfo();
            return CurrentRoomInfo.autoTeamSelection;
        }
    }
    public MFPSRoomInfo CurrentRoomInfo { get; set; }
    public bool RoomInfoFetched { get; set; } = false;
    #endregion

    private Dictionary<string, object> matchPersistData = new Dictionary<string, object>();

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        if ((!bl_PhotonNetwork.IsConnected || !bl_PhotonNetwork.InRoom) && !bl_GameData.CoreSettings.offlineMode)
            return;

        ResetRoom();
        FetchRoomInfo();
    }

    /// <summary>
    /// 
    /// </summary>
    IEnumerator Start()
    {
        while (!bl_GameData.isDataCached) yield return null;
        if (bl_MFPS.Settings != null) bl_MFPS.Settings.ApplySettings(bl_RuntimeSettingsProfile.ResolutionApplication.NoApply, true);
    }

    /// <summary>
    /// Reset all the room properties to it's default values
    /// </summary>
    public void ResetRoom()
    {
        var table = new Hashtable();
        //Initialize new properties where the information will stay Room
        if (bl_PhotonNetwork.IsMasterClient)
        {
            table.Add(PropertiesKeys.Team1Score, 0);
            table.Add(PropertiesKeys.Team2Score, 0);
            bl_PhotonNetwork.CurrentRoom.SetCustomProperties(table);
        }
        table.Clear();
        //Initialize new properties where the information will stay Players
        if (!bl_MFPS.GameData.UsingWaitingRoom() || bl_PhotonNetwork.OfflineMode)
        {
            table.Add(PropertiesKeys.TeamKey, Team.None);
        }
        table.Add(PropertiesKeys.KillsKey, 0);
        table.Add(PropertiesKeys.DeathsKey, 0);
        table.Add(PropertiesKeys.ScoreKey, 0);
        table.Add(PropertiesKeys.AssistsKey, 0);
        table.Add(PropertiesKeys.UserRole, bl_GameData.Instance.RolePrefix);
        table.Add(PropertiesKeys.PlayerTotalScore, bl_MFPSDatabase.GetInt("score"));

        LocalPlayer.SetCustomProperties(table);

        matchPersistData = new Dictionary<string, object>();

        bl_EventHandler.DispatchRoomPropertiesReset();
    }

    /// <summary>
    /// Get the custom room properties of this room
    /// These properties was set by the room creator (Host)
    /// </summary>
    void FetchRoomInfo()
    {
        CurrentRoomInfo = bl_PhotonNetwork.CurrentRoom.GetRoomInfo();
        CheckAutoSpawn();
        RoomInfoFetched = true;
    }

    /// <summary>
    /// Check if the local player should spawn automatically
    /// or should let him decide when spawn.
    /// </summary>
    public void CheckAutoSpawn()
    {
        if (CurrentRoomInfo.autoTeamSelection && !bl_MFPS.GameData.UsingWaitingRoom())
        {
            bl_UIReferences.SafeUIInvoke(() =>
            {
                bl_UIReferences.Instance.AutoTeam(true);
                bl_UIReferences.Instance.ShowMenu(false);
            });
            Invoke(nameof(SelectTeamAutomatically), 1);
        }
        else if (bl_MFPS.GameData.UsingWaitingRoom() && LocalPlayer.GetPlayerTeam() == Team.None)
        {
            if (bl_PhotonNetwork.OfflineMode && !CurrentRoomInfo.autoTeamSelection)
            {

                bl_UIReferences.SafeUIInvoke(() =>
                {
                    bl_UIReferences.Instance.SetUpUI();
                    bl_UIReferences.Instance.SetUpJoinButtons(true);
                });
                return;
            }

            if (!bl_SpectatorModeBase.EnterAsSpectator && bl_RoomCameraBase.Instance.GetCameraMode() != bl_RoomCameraBase.CameraMode.Spectator)
            {
                bl_UIReferences.SafeUIInvoke(() =>
                {
                    bl_UIReferences.Instance.AutoTeam(true);
                    bl_UIReferences.Instance.ShowMenu(false);
                });

                Invoke(nameof(SelectTeamAutomatically), 1);
            }
            else
            {
                // if the player joined as spectator from the waiting room
            }
        }
    }

    /// <summary>
    /// Set the player that just join to the room to the team with less players on it automatically
    /// </summary>
    void SelectTeamAutomatically()
    {
        string joinText = isOneTeamMode ? bl_GameTexts.JoinedInMatch.Localized(17) : bl_GameTexts.JoinIn.Localized(23);
        int teamDelta = bl_PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team1).Length;
        int teamRecon = bl_PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team2).Length;
        Team team = Team.All;

        if (!isOneTeamMode)
        {
            if (teamDelta > teamRecon)
            {
                team = Team.Team2;
            }
            else if (teamDelta < teamRecon)
            {
                team = Team.Team1;
            }
            else if (teamDelta == teamRecon)
            {
                team = Team.Team1;
            }

            string jt = string.Format("{0} {1}", joinText, team.GetTeamName());
            bl_KillFeedBase.Instance.SendTeamHighlightMessage(bl_PhotonNetwork.NickName, jt, team);
        }
        else
        {
            bl_KillFeedBase.Instance.SendMessageEvent(string.Format("{0} {1}", bl_PhotonNetwork.NickName, joinText));
        }

        bl_UIReferences.SafeUIInvoke(() =>
        {
            bl_UIReferences.Instance.AutoTeam(false);
        });

        if (GetGameMode.GetGameModeInfo().onRoundStartedSpawn == GameModeSettings.OnRoundStartedSpawn.WaitUntilRoundFinish && bl_GameManager.Instance.GameMatchState == MatchState.Playing)
        {
            bl_GameManager.Instance.onWaitUntilRoundFinish?.Invoke(team);
            bl_GameManager.Instance.SetLocalPlayerToTeam(team);
            return;
        }

        //spawn player
        bl_GameManager.Instance.SpawnPlayer(team);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static MFPSRoomInfo GetRoomInfo()
    {
        if (Instance == null || bl_PhotonNetwork.CurrentRoom == null) return null;

        if (Instance.CurrentRoomInfo == null)
        {
            Instance.CurrentRoomInfo = bl_PhotonNetwork.CurrentRoom.GetRoomInfo();
            Instance.RoomInfoFetched = true;
        }
        return Instance.CurrentRoomInfo;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static void AddMatchPersistData(string key, object value)
    {
        if (Instance == null) return;

        if (!Instance.matchPersistData.ContainsKey(key))
        {
            Instance.matchPersistData.Add(key, value);
            return;
        }

        Instance.matchPersistData[key] = value;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static float IncreaseMatchPersistData(string key, float value)
    {
        if (Instance == null) return 0;

        if (!Instance.matchPersistData.ContainsKey(key))
        {
            Instance.matchPersistData.Add(key, value);
            return value;
        }

        float current = (float)Instance.matchPersistData[key];
        current += value;
        Instance.matchPersistData[key] = current;
        return current;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool TryGetMatchPersistData(string key, out object value)
    {
        value = null;
        if (Instance == null || !Instance.matchPersistData.ContainsKey(key)) return false;

        value = Instance.matchPersistData[key];
        return true;
    }

    private static bl_RoomSettings _instance;
    public static bl_RoomSettings Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_RoomSettings>(); }
            return _instance;
        }
    }
}