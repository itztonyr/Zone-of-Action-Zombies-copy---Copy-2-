using MFPS.InputManager;
using MFPS.Internal.Structures;
using MFPS.Runtime.UI;
using MFPSEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class bl_UIReferences : bl_PhotonHelper
{
    #region Public members
    [FlagEnum, SerializeField] private RoomUILayers m_uiMask = 0;
    public RoomUILayers UIMask
    {
        get => m_uiMask;
        set => m_uiMask = value;
    }

    [Header("References")]
    public Canvas noInteractableCanvas;
    [SerializeField] private GameObject SuicideButton = null;
    [SerializeField] private GameObject SpectatorButton = null;
    [SerializeField] private GameObject ChangeTeamButton = null;
    [SerializeField] private GameObject globalTimeUI = null;
    public GameObject SpeakerIcon;
    [SerializeField] private TextMeshProUGUI SpawnProtectionText = null;
    public bl_PlayerScoreboardBase playerScoreboards;
    public bl_CanvasGroupFader blackScreen;
    public bl_ConfirmationWindow leaveRoomConfirmation;
    [SerializeField] private bl_MessageBox centerMessageBox = null;
    public GameObject[] addonReferences;
    #endregion

    #region Private members
    private bool inTeam = false;
    private int ChangeTeamTimes = 0;
    public static bool IsAddictiveStart = false;
    private static List<Action> pendingCalls = new List<Action>();
    #endregion

    #region Unity methods
    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        if (IsAddictiveStart)
        {
            InvokePendingCalls();
        }

        bl_EventHandler.DispatchUIMaskChange(UIMask);
        noInteractableCanvas.enabled = false;

        if (!bl_PhotonNetwork.IsConnected || !bl_PhotonNetwork.InRoom)
            return;

        blackScreen.FadeOut(1);
    }

    /// <summary>
    /// 
    /// </summary>
    void OnEnable()
    {
        SetUpUI();

        bl_EventHandler.onLocalPlayerSpawn += OnPlayerSpawn;
        bl_EventHandler.onLocalPlayerDeath += OnLocalPlayerDie;
        bl_EventHandler.onPickUpHealth += OnPicUpMedKit;
        bl_EventHandler.onRoundEnd += OnRoundFinish;
        bl_PhotonCallbacks.LeftRoom += OnLeftRoom;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        StopAllCoroutines();
        bl_EventHandler.onLocalPlayerSpawn -= OnPlayerSpawn;
        bl_EventHandler.onLocalPlayerDeath -= OnLocalPlayerDie;
        bl_EventHandler.onPickUpHealth -= OnPicUpMedKit;
        bl_EventHandler.onRoundEnd -= OnRoundFinish;
        bl_PhotonCallbacks.LeftRoom -= OnLeftRoom;
    }
    #endregion

    /// <summary>
    /// Make sure to invoke a function only when the UI is ready
    /// Since the UI can be loaded additively, we need to wait for it to be ready
    /// If we call a function that uses the UI before it's ready, it will throw an error
    /// Calling this function will make sure that the UI is ready before invoking the function
    /// </summary>
    /// <param name="action"></param>
    public static void SafeUIInvoke(Action action)
    {
        if (Instance != null)
        {
            action?.Invoke();
        }
        else
        {
            pendingCalls.Add(action);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    static void InvokePendingCalls()
    {
        for (int i = 0; i < pendingCalls.Count; i++)
        {
            pendingCalls[i]?.Invoke();
        }
        pendingCalls.Clear();
    }

    /// <summary>
    /// 
    /// </summary>
    void OnRoundFinish()
    {
        playerScoreboards.SetActiveByTeamMode();
        StopAllCoroutines();
        blackScreen.SetAlpha(0);
        bl_GamePadPointer.SetActivePointer(true);
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetUpUI()
    {
        MFPSRoomInfo roomInfo = bl_RoomSettings.GetRoomInfo();
        if (roomInfo == null) return;

        if (!roomInfo.autoTeamSelection)
        {
            bl_PauseMenuBase.Instance.OpenMenu();
        }

        playerScoreboards.gameObject.SetActive(true);
        SetActiveChangeTeamButton(false);
        SpeakerIcon.SetActive(false);
        SetUpJoinButtons();
        UpdateDisplayUI();
        SuicideButton.SetActive(bl_RoomSettings.Instance.canSuicide && bl_GameManager.Instance.IsLocalPlaying);
#if LMS
        if (GetGameMode == GameMode.BR) ShowMenu(false);
#endif
        if (bl_MFPS.GameData.UsingWaitingRoom())
        {
            if (bl_PhotonNetwork.OfflineMode && !roomInfo.autoTeamSelection) return;

            SetUpJoinButtons(true);
            SpectatorButton.SetActive(false);
            ShowMenu(false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetUpJoinButtons(bool forced = false)
    {
        if (!inTeam && (CanSelectTeamToJoin() || forced))
        {
            playerScoreboards.SetActiveByTeamMode(true);
        }
        else { playerScoreboards.SetActiveJoinButtons(false); }
    }

    /// <summary>
    /// 
    /// </summary>
    public void ShowMenu(bool active)
    {
        if (active) bl_PauseMenuBase.Instance.OpenMenu();
        else bl_PauseMenuBase.Instance.CloseMenu();

        SetActiveChangeTeamButton(bl_GameData.CoreSettings.canSwitchTeams && !isOneTeamMode && ChangeTeamTimes <= bl_GameData.CoreSettings.MaxChangeTeamTimes
            && !bl_RoomSettings.Instance.AutoTeamSelection && bl_MatchTimeManagerBase.HaveTimeStarted() && bl_SpectatorModeBase.Instance.CanSelectTeam());

        SuicideButton.SetActive(bl_RoomSettings.Instance.canSuicide && bl_GameManager.Instance.IsLocalPlaying);

        if (bl_Input.IsGamePad) bl_GamePadPointer.Instance?.SetActive(active);
        bl_EventHandler.Match.onMenuActive?.Invoke(active);
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetUIMask(RoomUILayers newMask)
    {
        UIMask = newMask;
        UpdateDisplayUI();
        bl_EventHandler.DispatchUIMaskChange(newMask);
    }

    /// <summary>
    /// 
    /// </summary>
    public void Resume()
    {
        if (!bl_GameManager.Instance.FirstSpawnDone) return;

        bl_UtilityHelper.LockCursor(true);
        ShowMenu(false);
        bl_CrosshairBase.Instance.Show(true);
        bl_EventHandler.DispatchGamePauseEvent(false);
    }

    /// <summary>
    /// 
    /// </summary>
    public void UpdateDisplayUI()
    {
        if (playerScoreboards != null)
        {
            playerScoreboards.SetActive(UIMask.IsEnumFlagPresent(RoomUILayers.Scoreboards));
            playerScoreboards.BlockScoreboards = !UIMask.IsEnumFlagPresent(RoomUILayers.Scoreboards);
            playerScoreboards.SetActive(UIMask.IsEnumFlagPresent(RoomUILayers.Scoreboards), CanSelectTeamToJoin());
        }
        SetActiveGlobalTimeUI(UIMask.IsEnumFlagPresent(RoomUILayers.Time));
    }

    /// <summary>
    /// 
    /// </summary>
    public void AutoTeam(bool v)
    {
        if (!v)
        {
            bl_PauseMenuBase.Instance.CloseMenu();
            bl_SpectatorModeBase.Instance.SetActiveUI(false);
            SpectatorButton.SetActive(false);
            if (bl_PhotonNetwork.OfflineMode)
            {
                HideCenterMessage(bl_GameTexts.StartingOfflineRoom);
            }
            else
            {
                HideCenterMessage(bl_GameTexts.AutoSelectingTeam.Localized(125));
            }

            inTeam = true;
        }
        else
        {
            ShowCenterMessage(bl_GameTexts.AutoSelectingTeam.Localized(125));
            if (bl_PhotonNetwork.OfflineMode)
            {
                ShowCenterMessage(bl_GameTexts.StartingOfflineRoom);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void LeftRoom(bool quit)
    {
        if (!quit)
        {
            bl_UtilityHelper.LockCursor(false);
            leaveRoomConfirmation.AskConfirmation(bl_GameTexts.LeaveMatchWarning.Localized("areusulega"), () => { LeftRoom(true); }, () => { });
        }
        else
        {
            bl_RoomMenu.Instance.LeaveRoom(RoomLeaveCause.UserAction);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void Suicide()
    {
        if (!bl_MFPS.LocalPlayer.Suicide())
        {
            bl_EventHandler.DispatchGamePauseEvent(false);
        }
        bl_UtilityHelper.LockCursor(true);
        ShowMenu(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    public void JoinTeam(int id)
    {
        var team = (Team)id;
        if (bl_GameManager.Joined)
        {
            ChangeTeamTimes++;
            bl_EventHandler.Player.onLocalChangeTeam?.Invoke(team);
            bl_RoomCameraBase.Instance.SetActive(true);
        }

        bl_GameManager.Instance.JoinTeam(team);
        bl_PauseMenuBase.Instance.CloseMenu();
        inTeam = true;
        ShowCenterMessage("");
        bl_SpectatorModeBase.Instance.SetActiveUI(false);
        SpectatorButton.SetActive(false);

        if (bl_GameData.CoreSettings.canSwitchTeams && !isOneTeamMode && ChangeTeamTimes <= bl_GameData.CoreSettings.MaxChangeTeamTimes && bl_MatchTimeManagerBase.HaveTimeStarted())
        {
            SetActiveChangeTeamButton(true);
        }
    }

    /// <summary>
    /// Called from the Change Team button
    /// </summary>
    public void ActiveChangeTeam()
    {
        playerScoreboards.SetActive(false, true);
        SetActiveChangeTeamButton(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public void SetActiveChangeTeamButton(bool active)
    {
        if (ChangeTeamButton == null || !bl_MFPS.RoomGameMode.CurrentGameModeData.AllowSwitchTeam) return;

        ChangeTeamButton.SetActive(active);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public void ShowScoreboard(bool active)
    {
        playerScoreboards.SetActive(active);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="count"></param>
    public void OnSpawnCount(int count)
    {
        SpawnProtectionText.text = string.Format(bl_GameTexts.SpawnProtectionCountdown.Localized(127).ToUpper(), count);
        SpawnProtectionText.gameObject.SetActive(count > 0);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="text"></param>
    /// <param name="autoHideIn"></param>
    public void ShowCenterMessage(string text, float autoHideIn = 0)
    {
        if (centerMessageBox == null) return;

        if (string.IsNullOrEmpty(text)) { centerMessageBox.Hide(); return; }

        centerMessageBox.Show(text, autoHideIn);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ifText"></param>
    public void HideCenterMessage(string ifText = "")
    {
        if (!string.IsNullOrEmpty(ifText)) { centerMessageBox.HideIf(ifText); }
        else centerMessageBox.Hide();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    void OnPlayerSpawn()
    {
        noInteractableCanvas.enabled = true;
        bl_GamePadPointer.SetActivePointer(false);
    }

    /// <summary>
    /// 
    /// </summary>
    void OnLocalPlayerDie()
    {
        noInteractableCanvas.enabled = false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="t_amount"></param>
    void OnPicUpMedKit(int Amount) => new MFPSLocalNotification(string.Format(bl_GameTexts.AddHealth, Amount));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="text"></param>
    /// <param name="show"></param>
    public void SetWaitingPlayersText(string text = "", bool show = false)
    {
        if (show) ShowCenterMessage(text);
        else ShowCenterMessage("");
    }

    /// <summary>
    /// 
    /// </summary>
    public void ResetRound()
    {
        bl_RoundFinishScreenBase.Instance?.Hide();
        inTeam = false;
        blackScreen.FadeOut(1);
        SetUpUI();
        ShowMenu(true);
        if (!bl_RoomSettings.Instance.AutoTeamSelection)
        {
            playerScoreboards.ResetJoinButtons();
            SetUpJoinButtons();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public void SetActiveGlobalTimeUI(bool active)
    {
        if (globalTimeUI == null) return;
        globalTimeUI.SetActive(active);
    }

    public IEnumerator FinalFade(bool fadein, bool goToLobby = true, float delay = 1)
    {
        if (!goToLobby)
        {
            if (bl_GameManager.Instance.GameFinish) { blackScreen.SetAlpha(0); yield break; }
        }

        if (fadein)
        {
            yield return new WaitForSeconds(delay);
            yield return blackScreen.FadeIn(1);
#if ULSP
            if (bl_DataBase.Instance != null && bl_DataBase.Instance.IsRunningTask)
            {
                while (bl_DataBase.Instance.IsRunningTask) { yield return null; }
            }
#endif
            if (goToLobby)
            {
                bl_UtilityHelper.LoadLevel(bl_GameData.CoreSettings.MainMenuScene);
            }
        }
        else
        {
            yield return new WaitForSeconds(delay);
            blackScreen.FadeOut(1);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private bool CanSelectTeamToJoin()
    {
        return !inTeam && (!bl_RoomSettings.Instance.AutoTeamSelection || !bl_MFPS.GameData.UsingWaitingRoom());
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnLeftRoom()
    {
        ShowMenu(false);
    }

    #region Getters
    private static bl_UIReferences _instance;
    public static bl_UIReferences Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_UIReferences>(); }
            return _instance;
        }
    }
    #endregion
}