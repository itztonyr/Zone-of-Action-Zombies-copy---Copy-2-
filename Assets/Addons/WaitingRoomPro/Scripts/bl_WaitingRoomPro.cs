using MFPS.Runtime.Settings;
using Photon.Realtime;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using HashTable = ExitGames.Client.Photon.Hashtable;

public class bl_WaitingRoomPro : bl_PhotonHelper
{
    public GameObject RoomSettingsButton;
    public bl_SingleSettingsBinding mapSelector;
    public bl_SingleSettingsBinding maxPlayersSelector;
    public bl_SingleSettingsBinding gameModeSelector;
    public bl_SingleSettingsBinding timeLimitSelector;
    public bl_SingleSettingsBinding goalSelector;
    public bl_SingleSettingsBinding pingSelector;
    public bl_SingleSettingsBinding botsActiveSelector;
    public bl_SingleSettingsBinding friendlyFireSelector;
    public bl_SingleSettingsBinding allowedWeaponsSelector;

    public Button[] changeTeamButtons;
    private string lastRoom = "";
    private bl_WaitingRoomUIBase waitingRoomUI;

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        TryGetComponent(out waitingRoomUI);
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        bl_PhotonCallbacks.PlayerEnteredRoom += OnPlayerEnter;
        bl_PhotonCallbacks.PlayerPropertiesUpdate += OnPlayerPropertiesUpdate;
        bl_PhotonCallbacks.JoinRoom += OnJoinRoom;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        bl_PhotonCallbacks.PlayerEnteredRoom -= OnPlayerEnter;
        bl_PhotonCallbacks.PlayerPropertiesUpdate -= OnPlayerPropertiesUpdate;
        bl_PhotonCallbacks.JoinRoom -= OnJoinRoom;
    }

    /// <summary>
    /// 
    /// </summary>
    public void ChangeTeam(int teamID)
    {
        Team newTeam = (Team)teamID;
        bl_WaitingRoomBase.Instance.JoinToTeam(newTeam);
    }

    /// <summary>
    /// 
    /// </summary>
    public void OpenRoomSettings()
    {
        RoomSettingsButton.SetActive(false);
        if (bl_PhotonNetwork.CurrentRoom.Name == lastRoom) return;

        CopySelector(bl_LobbyRoomCreatorUI.Instance.MapSettingsSelector, mapSelector);
        CopySelector(bl_LobbyRoomCreatorUI.Instance.GameModeSelector, gameModeSelector);
        CopySelector(bl_LobbyRoomCreatorUI.Instance.MaxPlayersSelector, maxPlayersSelector);
        CopySelector(bl_LobbyRoomCreatorUI.Instance.MaxPingSelector, pingSelector);
        CopySelector(bl_LobbyRoomCreatorUI.Instance.TimeLimitSelector, timeLimitSelector);
        CopySelector(bl_LobbyRoomCreatorUI.Instance.GameGoalSelector, goalSelector);
        CopySelector(bl_LobbyRoomCreatorUI.Instance.FriendlyFireSelector, friendlyFireSelector, true);
        CopySelector(bl_LobbyRoomCreatorUI.Instance.BotsActiveSelector, botsActiveSelector);
        CopySelector(bl_LobbyRoomCreatorUI.Instance.WeaponOptionsSelector, allowedWeaponsSelector, true);

        lastRoom = bl_PhotonNetwork.CurrentRoom.Name;
        var currentInfo = bl_PhotonNetwork.CurrentRoom.GetRoomInfo();
        mapSelector.SetCurrentFromOptionName(currentInfo.GetMapInfo().ShowName);
        gameModeSelector.SetCurrentFromOptionName(currentInfo.gameMode.GetModeInfo().ModeName.Localized(currentInfo.gameMode.ToString().ToLower()));
    }

    /// <summary>
    /// 
    /// </summary>
    void UpdateChangeButtons()
    {
        if (isOneTeamMode) return;
        if (bl_PhotonNetwork.PlayerList.Length >= bl_PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            changeTeamButtons[0].gameObject.SetActive(false);
            changeTeamButtons[1].gameObject.SetActive(false);
            return;
        }

        Team t = bl_PhotonNetwork.LocalPlayer.GetPlayerTeam();
        changeTeamButtons[0].gameObject.SetActive(t == Team.Team2);
        changeTeamButtons[1].gameObject.SetActive(t == Team.Team1);

        int oct1 = bl_PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team1).Length;
        int oct2 = bl_PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team2).Length;

        changeTeamButtons[0].interactable = (oct2 - oct1) >= 0;
        changeTeamButtons[1].interactable = (oct1 - oct2) >= 0;
    }

    /// <summary>
    /// 
    /// </summary>
    public void ConfirmRoomSettings()
    {
        if (!bl_PhotonNetwork.IsMasterClient) return;

        var allModes = GetModesForCurrentMap();
        var gameMode = allModes[gameModeSelector.currentOption];

        bl_PhotonNetwork.CurrentRoom.MaxPlayers = (byte)gameMode.maxPlayers[maxPlayersSelector.currentOption];
        HashTable t = new HashTable
        {
            { PropertiesKeys.RoomSceneID, mapSelector.currentOption }
        };
        if (GetGameModeUpdated != allModes[gameModeSelector.currentOption].gameMode)
        {
            t.Add(PropertiesKeys.GameModeKey, allModes[gameModeSelector.currentOption].gameMode);
        }
        t.Add(PropertiesKeys.TimeRoomKey, gameMode.timeLimits[timeLimitSelector.currentOption]);
        t.Add(PropertiesKeys.MaxPing, bl_Lobby.Instance.MaxPing[pingSelector.currentOption]);
        t.Add(PropertiesKeys.RoomFriendlyFire, friendlyFireSelector.currentOption == 1 ? true : false);
        t.Add(PropertiesKeys.WithBotsKey, gameMode.supportBots ? (botsActiveSelector.currentOption == 1 ? true : false) : false);
        t.Add(PropertiesKeys.RoomGoal, gameMode.GetGoalValue(goalSelector.currentOption));

        bl_PhotonNetwork.CurrentRoom.SetCustomProperties(t);
        RoomSettingsButton.SetActive(true);
        waitingRoomUI?.UpdateRoomInfoUI();
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnGameModeChange(int id)
    {
        goalSelector.currentOption = 0;
        maxPlayersSelector.currentOption = 0;
        timeLimitSelector.currentOption = 0;

        var allModes = GetModesForCurrentMap();
        var gameMode = allModes[gameModeSelector.currentOption];

        maxPlayersSelector.SetOptions(gameMode.maxPlayers.AsStringArray($" Players"));
        timeLimitSelector.SetOptions(gameMode.timeLimits.Select(x => $"{(x / 60)} Minutes").ToArray());

        if (gameMode.GameGoalsOptions.Length > 0)
            goalSelector.SetOptions(gameMode.GameGoalsOptions.AsStringArray($" {gameMode.GoalName}"));
        else goalSelector.SetOptions(new string[1] { gameMode.GoalName });

        botsActiveSelector.gameObject.SetActive(gameMode.supportBots);

        if (allowedWeaponsSelector != null)
        {
            allowedWeaponsSelector.currentOption = 0;
            allowedWeaponsSelector.SetActive(gameMode.AllowSingleWeaponTypeOnly);
        }

        friendlyFireSelector.SetActive(!gameMode.gameMode.IsOneTeamMode());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mapID"></param>
    public void OnMapChange(int mapID)
    {
        gameModeSelector.currentOption = 0;
        var allModes = GetModesForCurrentMap();
        gameModeSelector.SetOptions(allModes.Select(x => x.ModeName.Localized(x.gameMode.ToString().ToLower())).ToArray());
        OnGameModeChange(0);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private GameModeSettings[] GetModesForCurrentMap()
    {
        var currentMap = bl_LobbyRoomCreator.Instance.MapList[mapSelector.currentOption];
        return currentMap.GetAllowedGameModes(bl_Lobby.Instance.GameModes);
    }

    void OnPlayerEnter(Player newPlayer)
    {

    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps.ContainsKey(PropertiesKeys.TeamKey))
        {
            UpdateChangeButtons();
        }
    }

    public void OnJoinRoom()
    {
        RoomSettingsButton.SetActive(bl_PhotonNetwork.IsMasterClient);
    }

    /// <summary>
    /// 
    /// </summary>
    void CopySelector(bl_SingleSettingsBinding source, bl_SingleSettingsBinding target, bool copyActive = false)
    {
        target.optionsNames = source.optionsNames;
        target.currentOption = source.currentOption;
        target.ApplyCurrentValue();
        if (copyActive)
        {
            target.gameObject.SetActive(source.gameObject.activeSelf);
        }
    }
}