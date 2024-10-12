using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using System.Collections.Generic;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using ZombieEvents;
using System.Collections;

namespace ZombiesGameMode
{
    public class bl_Zombies : bl_GameModeBase
    {
        public RoundState gameState
        {
            get => _state;
            set
            {
                if (_state != value)
                {
                    _state = value;
                    if (bl_PhotonNetwork.IsMasterClient)
                    {
                        var data = bl_UtilityHelper.CreatePhotonHashTable();
                        data.Add(ZombieEventManager.GameState, value);
                        bl_PhotonNetwork.CurrentRoom.SetCustomProperties(data);
                    }
                }
            }
        }
        #region Public
        [Header("Main Content")]
        [Space(5)]
        public string ZombieName = "Zombie";
        public GameObject content;
        [Header("Audio")]
        [Space(5)]
        public AudioSource ZombiesAudio;
        public AudioSource ZombiesDeathScreenLaugh;
        [Header("UI")]
        [Space(5)]
        public GameObject GameOverScreen;
        public GameObject KillCameraObject;
        [Header("Camera")]
        [Space(5)]
        public GameObject GameOverCamera;
        public GameObject OriginalDeathCamera;
        [Header("Extra")]
        [Space(5)]
        public EndType EndScreenStyle;

        [Header("Weapon Extras")]
        [Space(5)]
        [LovattoToogle] public bool useStartingGun;
        [LovattoToogle] public bool AddSecondary;
        [LovattoToogle] public bool AddGadgets;
        [LovattoToogle] public bool AddKnife;
        [GunID] public int DefaultGunID;
        [GunID] public int SecondaryGunID;
        [GunID] public int GadgetID;
        [GunID] public int KnifeID;
        [Header("Debug")]
        public bl_RoundManager roundmanager;
        public RoundState _state = RoundState.Starting;
        public List<MFPSPlayer> PlayerSort = new List<MFPSPlayer>();
        #endregion

        #region Hidden

        public static bl_Zombies Instance;
        public bl_PlayerReferences LocalPlayerReferences;
        private Animator animatorforcam; //Add animations to your camera
        private bool gameOver = false;
        private bl_GameManager GameManager;
        private bl_GunManager GunManager;
        [HideInInspector] public List<bl_Gun> AllGuns;
        private PhotonView Photonview;
        private int currentGunID;
        [HideInInspector] public bl_Gun gun;
        private bl_ScenePerkManager manager;
        private bl_FirstPersonController cont;
      

        #endregion

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        public override void Awake()
        {
            Instance = this;
            if (!bl_PhotonNetwork.IsConnected)
                return;

            GameManager = bl_GameManager.Instance;
            if (manager == null)
            {
                manager = FindObjectOfType<bl_ScenePerkManager>();
            }
            else
            {
                Debug.Log("ManagerFound");
            }
            
            Initialize();
        }

        void OnEnable()
        {
            bl_EventHandler.onLocalPlayerSpawn += OnLocalPlayerSpawn;
            bl_PhotonCallbacks.PlayerLeftRoom += OnPlayerLeftRoom;
            bl_PhotonCallbacks.MasterClientSwitched += OnMasterClientSwitched;
            bl_EventHandler.onRemoteActorChange += OnRemotePlayerChange;
            bl_PhotonCallbacks.PlayerPropertiesUpdate += OnPhotonPlayerPropertiesChanged;
            bl_PhotonCallbacks.PlayerPropertiesUpdate += OnPlayerPropertiesUpdate;
            bl_PhotonCallbacks.PlayerEnteredRoom += OnOtherPlayerEnter;

        }
        public override void OnDisable()
        {
            bl_EventHandler.onLocalPlayerSpawn -= OnLocalPlayerSpawn;
            bl_PhotonCallbacks.PlayerLeftRoom -= OnPlayerLeftRoom;
            bl_PhotonCallbacks.MasterClientSwitched -= OnMasterClientSwitched;
            bl_EventHandler.onRemoteActorChange -= OnRemotePlayerChange;
            bl_PhotonCallbacks.PlayerPropertiesUpdate -= OnPhotonPlayerPropertiesChanged;
            bl_PhotonCallbacks.PlayerPropertiesUpdate -= OnPlayerPropertiesUpdate;
            bl_PhotonCallbacks.PlayerEnteredRoom -= OnOtherPlayerEnter;

        }

        public override void OnLocalPlayerSpawn()
        {
            LocalPlayerReferences = bl_GameManager.Instance.LocalPlayerReferences;
            GunManager = LocalPlayerReferences.gunManager;
            Photonview = LocalPlayerReferences.photonView;
            cont = LocalPlayerReferences.firstPersonController.MFPSController;
            cont.OwnsRevive = false;

            StartingGunInfo();
            UpdateTargetList();
            bl_GameManager.Instance.SetLocalPlayerToTeam(Team.Team1);
        }
        #endregion

        #region Interface
        private void RoomData()
        {
            bool contains = bl_PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(ZombieEventManager.GameState);
            Debug.Log($"Game State Data Exists: {contains}");
            if (bl_PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(ZombieEventManager.GameState, out object data))
            {
                gameState = (RoundState)data;
                Debug.Log((RoundState)data);
            }
        }
        void StartingGunInfo()
        {
            List<bl_Gun> playerEquip = GunManager.PlayerEquip;
            gun = GunManager.CurrentGun;
            AllGuns = new List<bl_Gun>(playerEquip);

            playerEquip[0] = null;
            playerEquip[1] = null;
            playerEquip[2] = null;
            playerEquip[3] = null;

            if (useStartingGun)
            {
                GunManager.AddWeaponToSlot(0, GunManager.GetGunOnListById(DefaultGunID), true);

                //Change Secondary gun
                if (AddSecondary)
                {
                    GunManager.AddWeaponToSlot(1, GunManager.GetGunOnListById(SecondaryGunID), true);
                }

                if (AddGadgets)
                {
                    GunManager.AddWeaponToSlot(2, GunManager.GetGunOnListById(GadgetID), true);
                }
                if (AddKnife)
                {
                    GunManager.AddWeaponToSlot(3, GunManager.GetGunOnListById(KnifeID), true);
                }
            }
            else
            {
                GunManager.SetupLoadout();
            }

            for (int i = 0; i < playerEquip.Count; i++)
            {
                if (playerEquip[i] == null) continue;
                playerEquip[i].Initialized();
            }
            bl_WeaponLoadoutUIBase.Instance?.SetLoadout(playerEquip);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="changedProps"></param>
        public void OnPhotonPlayerPropertiesChanged(Player target, Hashtable changedProps)
        {
            if (GetGameMode != GameMode.ZOM || GameManager.GameMatchState == MatchState.Finishing)
                return;
        }

        void UnlockMouse()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }


        public override void OnPlayerPropertiesUpdate(Player target, ExitGames.Client.Photon.Hashtable changedProps)
        {

        }
   
        public override void Initialize()
        {

            //check if this is the game mode of this room
            //proveri da li je ovaj mode u ovoj prostoriji
            if (bl_GameManager.Instance.IsGameMode(GameMode.ZOM, this))
            {
                bl_GameManager.Instance.SetGameState(MatchState.Starting);
  
                content.SetActive(true);
                ZombiesAudio.Play();
                if (GameOverScreen != null)
                {
                    GameOverScreen.SetActive(false);
                }
                if (GameOverCamera != null)
                {
                    GameOverCamera.SetActive(false);
                }
                RoomData();
            }

            else
            {
                content.SetActive(false);
                if (GameOverScreen != null)
                {
                    GameOverScreen.SetActive(false);
                }
                if (GameOverCamera != null)
                {
                    GameOverCamera.SetActive(false);
                }
                if (manager != null)
                {
                    manager.manager.isBuyH = false;
                    manager.manager.isBuyF = false;
                    manager.manager.isBuyS = false;
                    manager.manager.isBuyM = false;
                    manager.manager.isBuyR = false;
                }
            }
        }
        private void Update()
        {
            RaycastHit hit;
            if (Physics.Raycast(bl_GameManager.Instance.LocalPlayerReferences.playerCamera.transform.position, bl_GameManager.Instance.LocalPlayerReferences.playerCamera.transform.forward, out hit, 1000) && hit.collider.gameObject.CompareTag(bl_MFPS.REMOTE_PLAYER_TAG))
            {
                bl_GameManager.Instance.LocalPlayerReferences.gunManager.CurrentGun.CanFire = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// vereme je isteklo
        public void OnFinishTime(bool gameOver)
        {
            gameOver = false;
            return;
        }

        public override void OnLocalPlayerKill()
        {
            //bl_KillFeedBase.Instance.SendMessageEvent($"Killed Zombie");// Send a KillFeed message on kill
        }

        public override void OnLocalPoint(int points, Team team)
        {

        }

        public override void OnLocalPlayerDeath()
        {
            //lose all attachments
            manager.manager.isBuyH = false;
            manager.manager.isBuyF = false;
            manager.manager.isBuyS = false;
            manager.manager.isBuyM = false;
            manager.manager.isBuyR = false;
            manager.manager.CurrentPerkAmount = 0;
            bl_PerkIconManager icon = FindObjectOfType<bl_PerkIconManager>();

            if (cont.OwnsRevive)
            {
                cont.OwnsRevive = false;
                manager.manager.canBuyR = true;
                icon.RPerkIcon.gameObject.SetActive(false);
                icon.HPerkIcon.gameObject.SetActive(false);
                icon.MPerkIcon.gameObject.SetActive(false);
                icon.FPerkIcon.gameObject.SetActive(false);
                icon.SPerkIcon.gameObject.SetActive(false);
                UpdateTargetList();
                bl_GameManager.Instance.RespawnLocalPlayerAfter();
                return;
            }

            var data = bl_UtilityHelper.CreatePhotonHashTable();
            data.Add(ZombieEventManager.EventType, (byte)RoundEventTypes.PlayerDeath);
            data.Add(ZombieEventManager.Actor, bl_PhotonNetwork.LocalPlayer);

            HandleDeath(data);
            UpdateTargetList();
            SpectateOnLocalPlayerDeath();
        }
        public void SpectateOnLocalPlayerDeath()
        {
            var killCam = bl_KillCamBase.Instance;
            MFPSPlayer friend = null;
            //after local die, and round have not finish, find a team player to spectating
            var allActors = bl_GameManager.Instance.OthersActorsInScene;
            RoomData();

            foreach (var actor in allActors)
            {
                if (actor.isAlive && actor.Actor != null && actor.isRealPlayer && actor.ActorNumber != bl_PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    friend = actor;
                    break;
                }
            }

            if (friend != null && friend.Actor == null)
            {
                friend = bl_GameManager.Instance.GetMFPSPlayer(friend.Name);
            }

            if (friend != null && friend.Actor != null)
            {
                this.InvokeAfter(1, () => bl_KillCamBase.Instance.SetTarget(new bl_KillCamBase.KillCamInfo() { Target = friend.Actor, RealPlayer = friend.ActorView.Owner, }));
            }
            else
            {
                Debug.Log("No teammate available to spectate.");
            }
        }
        [PunRPC]
        private void HandleDeath(Hashtable data)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            PlayerSort.Clear();
            PlayerSort.AddRange(bl_GameManager.Instance.OthersActorsInScene);
            PlayerSort.Add(bl_GameManager.Instance.LocalActor);

            var actor = (Player)data[ZombieEventManager.Actor];

            if (actor == bl_PhotonNetwork.LocalPlayer && bl_MFPS.LocalPlayer.IsAlive)
            {
                bl_MFPS.LocalPlayer.IsAlive = false;
            }
            for (int i = 0; i < PlayerSort.Count; i++)
            {
                if (PlayerSort.Count <= 0)//everyone dies, games finished
                {
                    if (bl_PhotonNetwork.IsMasterClient)
                    {
                        var data1 = bl_UtilityHelper.CreatePhotonHashTable();
                        data1.Add(ZombieEventManager.EventType, (byte)RoundEventTypes.GameOver);
                        SendDataOverNetwork(data1);
                    }
                }
                else if(PlayerSort.Count == 1)
                {
                    if (bl_PhotonNetwork.IsMasterClient)
                    {
                        var data2 = bl_UtilityHelper.CreatePhotonHashTable();
                        data2.Add(ZombieEventManager.EventType, (byte)RoundEventTypes.GameOver);
                        SendDataOverNetwork(data2);
                    }
                }
            }
        }
 
        public bool isLocalPlayerWinner
        {
            get
            {
                return GetWinnerTeam() == bl_PhotonNetwork.LocalPlayer.GetPlayerTeam();
            }
        }

        public Team GetWinnerTeam()
        {
            Team winner = Team.None;
            return winner;
        }
        [PunRPC]
        public void EndGame()
        {
            StartCoroutine(GameEnd());

        }
        IEnumerator GameEnd()
        {

            gameState = RoundState.Finish;
            bl_EventHandler.DispatchRoundEndEvent();
            bl_GameManager.Instance.OnGameTimeFinish(true);

            if (bl_PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;
                PhotonNetwork.CurrentRoom.IsVisible = false;
            }
            if (EndScreenStyle == EndType.ShowRoundScreen)
            {
                animatorforcam = GameOverCamera.gameObject.GetComponent<Animator>();
                GameOverScreen.SetActive(true);
                gameOver = true;
                KillCameraObject.SetActive(false);
                UnlockMouse();
                GameOverCamera.SetActive(true);
                ZombiesDeathScreenLaugh.Play();
                OriginalDeathCamera.SetActive(false);
                if (animatorforcam != null)
                {
                    animatorforcam.Play("Camera");
                }

            }
            if (EndScreenStyle == EndType.ShowResume)
            {
                UnlockMouse();
                gameOver = true;
                var MatchInfo = new MatchOverInformation();
                MatchInfo.FinishReason = FinishRoundCause.ForceFinish.ToString();
                MatchInfo.WinnerName = ZombieName;
                MatchInfo.IsLocalWinner = false;
                FinishRound(FinishRoundCause.ForceFinish);
            }
            yield return new WaitForSeconds(5);
            roundmanager.photonView.RPC("ClearAllZombies", RpcTarget.All);
        }
        #endregion

        #region Network
        [PunRPC]
        void OnNetworkMessage(Hashtable data)
        {
            var events = (byte)data[ZombieEventManager.EventType];
            switch ((RoundEventTypes)events)
            {
                case RoundEventTypes.Preparation:
                    roundmanager.NetworkStartPreparation(data);
                    break;
                case RoundEventTypes.WaveStart:
                    roundmanager.StartRound();
                    break;
                case RoundEventTypes.GameOver:
                    EndGame();
                    break;
                case RoundEventTypes.PlayerDeath:
                    HandleDeath(data);
                    break;
                case RoundEventTypes.SyncData:
                    SyncState(data);
                    break;
                case RoundEventTypes.ZombieDeath:
                    roundmanager.ZombieDied();
                    break;
            }
        }
        public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
        {

        }


        public void OnPlayerLeftRoom(Player otherPlayer)
        {
            if (bl_GameManager.Instance.GameMatchState == MatchState.Playing)
            {
                var data = bl_UtilityHelper.CreatePhotonHashTable();
                data.Add(ZombieEventManager.EventType, (byte)RoundEventTypes.PlayerDeath);
                data.Add(ZombieEventManager.Actor, otherPlayer);
                HandleDeath(data);
            }
        }
        public void OnMasterClientSwitched(Player newMaster)
        {
            if (newMaster == PhotonNetwork.LocalPlayer)
            {
                if (gameState == RoundState.Waiting)
                {
                    roundmanager.TimeManagement();
                }
                else if (gameState == RoundState.Playing)
                {
                    bl_GameManager.Instance.SetGameState(MatchState.Playing);
                    roundmanager.SpawnZombiesInRound(roundmanager.rounds[roundmanager.currentRoundIndex]);
                }
            }
        }
        private void SyncGameState(Player newPlayer)
        {
            for (int i = 0; PlayerSort.Count < i; i++)
            {
                var data = bl_UtilityHelper.CreatePhotonHashTable();
                data.Add(ZombieEventManager.EventType, (byte)RoundEventTypes.SyncData);
                data.Add(ZombieEventManager.GameState, gameState);
                data.Add(ZombieEventManager.AlivePlayers, PlayerSort[i].isAlive);
                data.Add(ZombieEventManager.RoundIndex, roundmanager.currentRoundIndex);
                SendDataToPlayer(data, newPlayer);
            }
        }
        [PunRPC]
        void SyncState(Hashtable data)
        {
            gameState = (RoundState)data[ZombieEventManager.GameState];
            roundmanager.currentRoundIndex = (int)data[ZombieEventManager.RoundIndex];
            int[] players = (int[])data[ZombieEventManager.AlivePlayers];
            List<Player> playerList = bl_PhotonNetwork.PlayerList.ToList();
        }
     
        public void UpdateTargetList()
        {
            if (!bl_PhotonNetwork.IsMasterClient || bl_GameManager.Instance == null) return;

            PlayerSort.Clear();
            var all = bl_GameManager.Instance.OthersActorsInScene;

            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].Actor == null) continue;
                PlayerSort.Add(all[i]);
            }

            if (bl_GameManager.Instance.LocalActor.Actor != null)
            {
                PlayerSort.Add(bl_GameManager.Instance.LocalActor);
            }
        }
        private void OnRemotePlayerChange(bl_EventHandler.PlayerChangeData changeData)
        {
            UpdateTargetList();
        }

        public override void OnOtherPlayerEnter(Player newPlayer)
        {
            if (!bl_PhotonNetwork.IsMasterClient)
                return;
            if (gameState != RoundState.Waiting)
            {
                SpectateOnLocalPlayerDeath();
            }
            SyncGameState(newPlayer);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void OnOtherPlayerLeave(Player otherPlayer)
        {
            //if match already start
            if (bl_GameManager.Instance.GameMatchState == MatchState.Playing)
            {
                UpdateTargetList();
                if (PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team1).Length <= 0)
                {
                    //finish match
                    EndGame();
                }
            }
        }

        public void SendDataToPlayer(Hashtable dataTable, Player targetPlayer)
        {
            if (!bl_PhotonNetwork.IsMasterClient) return;
            photonView.RPC(nameof(OnNetworkMessage), targetPlayer, dataTable);
        }

        public void SendDataOverNetwork(Hashtable dataTable)
        {
            if (!bl_PhotonNetwork.IsMasterClient) return;
            photonView.RPC(nameof(OnNetworkMessage), RpcTarget.All, dataTable);
        }
        public override bool IsLocalPlayerWinner()
        {
            return false;
        }
        #endregion

        #region Enums
        public enum EndType
        {
            ShowResume = 0,
            ShowRoundScreen = 1,
        }
        #endregion
    }
}