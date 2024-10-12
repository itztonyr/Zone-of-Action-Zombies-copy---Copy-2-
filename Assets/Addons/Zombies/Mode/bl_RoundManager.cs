using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ZombiesHealthManagers;
using TMPro;
using Photon.Pun;
using ZombiesGameMode;
using ExitGames.Client.Photon;
using Random = UnityEngine.Random;
using ZombieEvents;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.AI;
using static bl_GameModeBase;

//my code is cursed

#if ACTK_IS_HERE
using CodeStage.AntiCheat.ObscuredTypes;
#endif

[System.Serializable]
public class SpawnPoint
{
    public Transform transform;
    public int maxZombies;
}

[System.Serializable]
public class RoundDetails
{
    public byte Id { get; set; }

    public static object Deserialize(byte[] data)
    {
        var result = new RoundDetails();
        result.Id = data[0];
        return result;
    }

    public static byte[] Serialize(object customType)
    {
        var c = (RoundDetails)customType;
        return new byte[] { c.Id };
    }

    public int numberOfZombies;
    public float delayBetweenZombies;
    public GameObject zombiePrefab;
}

[System.Serializable]
public class ZombieRound
{

#if UNITY_EDITOR
[HideInInspector] public string Name = "Round";
#endif
    public byte Id { get; set; }

    public static object Deserialize(byte[] data)
    {
        var result = new ZombieRound();
        result.Id = data[0];
        return result;
    }

    public static byte[] Serialize(object customType)
    {
        var c = (ZombieRound)customType;
        return new byte[] { c.Id };
    }
    public RoundDetails[] roundDetails;
}

public class bl_RoundManager : bl_MonoBehaviour, IPunObservable
{
    #region Public
    [Header("Settings")]
    [Space(5)]
    public int OnRoundFinishScore = 500;
    [Tooltip("When you enable this you still need 1 round to let the script know from where to add")]
    [LovattoToogle] [SerializeField] private bool InfiniteRounds;
    [Tooltip("Each Round All Zombies will gain slightly better stats like move speed, better tracking, more health etc")]
    [LovattoToogle][SerializeField] private bool DifficultyIncrease;
    [SerializeField] private int ExtraZombiesPerRound;
    [SerializeField] private SpawnPoint[] spawnPoints;
    public ZombieRound[] rounds;

    [Header("UI Refrences")]
    [Space(5)]
    [SerializeField] private TextMeshProUGUI roundDisplayText;
    [SerializeField] private TextMeshProUGUI scoreDisplayText;
    [SerializeField] private TextMeshProUGUI endResultroundDisplayer;
    [SerializeField] private TextMeshProUGUI zombiesCountText;
    [SerializeField] private TextMeshProUGUI betweenRoundText;
    [SerializeField] private TextMeshProUGUI CountDownText;
    [SerializeField] private TextMeshProUGUI PlayerText = null;

    [Header("UI Settings")]
    [Space(5)]
    [LovattoToogle][SerializeField] private bool useScoreEffects;
    [LovattoToogle][SerializeField] private bool AllowInBetweenRoundDisplay;
    [LovattoToogle][SerializeField] private bool AllowZombieCount;
    [LovattoToogle][SerializeField] private bool AllowZombieRoundDisplay;
    [LovattoToogle][SerializeField] private bool AllowZombieScore;

    [Header("Zombies Game Mode")]
    public bl_Zombies main;
    [Space(5)]
    [LovattoToogle][SerializeField] private bool UseMFPSCountDown;
    [LovattoToogle][SerializeField] private bool AllowUserNameDisplay;
    [LovattoToogle][SerializeField] private bool RespawnPlayerAftherTheRoundFinished;
    [Space(10)]
    [Header("Event")]
    [Space(5)]
    [SerializeField] private AudioSource roundCompletedSound;
    public int PreperationTime;
    #endregion

    #region Private
    private int startingRoundIndex = 1;
    private AudioSource audioSource;
    [HideInInspector] public int currentRoundIndex = 0;
    private bool roundInProgress = false;
    private List<GameObject> spawnedZombies = new List<GameObject>();
    private int zombiesRemainingInRound;
#if ACTK_IS_HERE
    [HideInInspector] public ObscuredInt playerScore = 0;
#else
    [HideInInspector] public int playerScore = 0;
#endif
    private int oldwaitfor;

    public bl_RoundManager bl_roundmanager { get { return roundmanager; } }
    private bl_RoundManager roundmanager;
    private GameObject zombie;
    private bool PrepStarted = false;
    private GameObject[] lastEffects;

    private float startingBoostAmount = 0;
    private int startinghealthIncrease = 0;
    private float startingUpdateRate = 0;
    /// <summary>
    /// Modify these values based on your preferences
    /// </summary>
    private float moveSpeedIncrease = 0.2f;
    private int healthIncrease = 10;
    private float UpdaterateIncrease = -0.1f;
    /// <summary>
    /// 
    /// </summary>
    #endregion

    #region Serialize
    protected override void Awake()
    {
        base.Awake();
        PhotonPeer.RegisterType(typeof(ZombieRound), (byte)'Z', ZombieRound.Serialize, ZombieRound.Deserialize);
        PhotonPeer.RegisterType(typeof(RoundDetails), (byte)'D', RoundDetails.Serialize, RoundDetails.Deserialize);
        startingBoostAmount = 0;
        startinghealthIncrease = 0;
        startingUpdateRate = 0;
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(FirstTimeSetup), RpcTarget.All);
        }
    }
    #endregion

    #region Classes

    [PunRPC]
    public int ClearAllZombies()
    {
        int totalScore = 0;

        foreach (var zombie in spawnedZombies)
        {
            if (zombie != null)
            {
                bl_ZombieHealth zombieHealth = zombie.GetComponent<bl_ZombieHealth>();
                if (zombieHealth != null)
                {
                    totalScore += zombieHealth.ZombieScore;
                    Destroy(zombie);
                }
            }
        }

        spawnedZombies.Clear();
        zombiesRemainingInRound = 0;
        StartPrep();
        return totalScore;
    }

    public bool CanAfford(int amount)
    {
        return playerScore >= amount;
    }

    public void ReduceScore(int amount)
    {
        playerScore -= amount;
        UpdateScoreDisplay();
    }
    [PunRPC]
    private void FirstTimeSetup()
    {
        oldwaitfor = PreperationTime;
        audioSource = GetComponent<AudioSource>();
        SetStartingRound(startingRoundIndex);
        UpdateRoundDisplay();
        
        UpdateScoreDisplay();
        Timer();

        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(TimeManagement), RpcTarget.All);
        }

        if (AllowUserNameDisplay)
        {
            PlayerText.text = bl_MFPS.LocalPlayer.FullNickName();
        }
        else
        {
            PlayerText.text = "";
        }
      
        if (UseMFPSCountDown)
        {
            CountDownText.enabled = false;
            CountDownText.transform.parent.gameObject.SetActive(false);
            bl_CountDownBase.Instance.SetCount(PreperationTime);
        }
        else
        {
            Timer();
            bl_GameData.Instance.coreSettings.useCountDownOnStart = false;
        }
        if (!PrepStarted)
        {
            StartPrep();
        }
    }
    void Timer()
    {
        CountDownText.enabled = true;
        CountDownText.transform.parent.gameObject.SetActive(true);
        CountDownText.text = PreperationTime.ToString("F0");
    }
    [PunRPC]
    public void TimeManagement()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        CancelInvoke(nameof(SetCountDown));
        InvokeRepeating(nameof(SetCountDown), 1, 1);
    }

    /// <summary>
    /// 
    /// </summary>
    void SetCountDown()
    {
        PreperationTime--;
        photonView.RPC(nameof(CountDownManager), RpcTarget.All, PreperationTime);
    }
    [PunRPC]
    void CountDownManager(int c)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            PreperationTime = c;
        }
        if (PreperationTime <= 0)
        {
            CancelInvoke(nameof(SetCountDown));
            StartRoundMain();
        }
    }
    [PunRPC]
    public void NetworkStartPreparation(Hashtable data)
    {
        main.gameState = RoundState.Waiting;
        currentRoundIndex = (int)data[ZombieEventManager.RoundIndex];

        roundInProgress = false;
        PrepStarted = true;
        UpdateRoundsCountDisplay();
        UpdateRoundDisplay();
        bl_GameManager.Instance.SetGameState(MatchState.Waiting);
        main.gameState = RoundState.Starting;
        StopCoroutine(nameof(SpawnZombiesInRound));
        if (UseMFPSCountDown)
        {
            bl_CountDownBase.Instance.StartCountDown();
        }
        else
        {
            Timer();
        }
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(TimeManagement), RpcTarget.All);
        }
        if (RespawnPlayerAftherTheRoundFinished)
        {
            if (bl_GameManager.Instance.LocalPlayer == null)
            {
                bl_GameManager.Instance.SpawnPlayer(PhotonNetwork.LocalPlayer.GetPlayerTeam());
            }
        }
        bl_GameManager.Instance.SetGameState(MatchState.Playing);
    }
    /// <summary>
    /// This is called when is waiting for player and the last needed enter
    /// </summary>
    public void StartPrep()
    {
        //master set the call to all other to start the game
        if (bl_PhotonNetwork.IsMasterClient)
        {
            var data = bl_UtilityHelper.CreatePhotonHashTable();
            data.Add(ZombieEventManager.EventType, (byte)RoundEventTypes.Preparation);
            data.Add(ZombieEventManager.RoundIndex, currentRoundIndex);
            main.SendDataOverNetwork(data);
        }
    }
   
    [PunRPC]
    public void StartRoundMain()
    {
        //master set the call to all other to start the game
        if (bl_PhotonNetwork.IsMasterClient)
        {
            var data = bl_UtilityHelper.CreatePhotonHashTable();
            data.Add(ZombieEventManager.EventType, (byte)RoundEventTypes.WaveStart);
            data.Add(ZombieEventManager.RoundIndex, currentRoundIndex);
            main.SendDataOverNetwork(data);
        }
    }
    [PunRPC]
    public void StartRound()
    {
        if (currentRoundIndex < rounds.Length)
        {
            ResetTimer();
            roundCompletedSound.Play();
            bl_GameManager.Instance.SetGameState(MatchState.Playing);
            roundInProgress = true;
            ZombieRound currentRound = rounds[currentRoundIndex];
           
            if (!InfiniteRounds)
            {
                currentRoundIndex++;
                StartCoroutine(SpawnZombiesInRound(currentRound));
                zombiesRemainingInRound = GetTotalZombiesInRound(currentRound);
            }
            else
            {
                ZombieRound currentRound0 = rounds[0];
                if (zombiesRemainingInRound <= 0)
                {
                    StartCoroutine(SpawnZombiesInRound(currentRound0));
                    currentRound0.roundDetails[0].numberOfZombies += ExtraZombiesPerRound;
                }
            }
            if (DifficultyIncrease)
            {
                startingBoostAmount += moveSpeedIncrease;
                startingUpdateRate += UpdaterateIncrease;
                startinghealthIncrease += healthIncrease;
            }
            PreperationTime = oldwaitfor;
            PrepStarted = false;
            main.gameState = RoundState.Playing;
            betweenRoundText.text = " ";
            betweenRoundText.transform.parent.gameObject.SetActive(false);
            CountDownText.text = "";
            UpdateRoundDisplay();
            AftherRoundRemainUpdate(currentRound);
            IncreaseScore(OnRoundFinishScore);
        }
        else
        {
            //master set the call to all other to start the game
            if (bl_PhotonNetwork.IsMasterClient)
            {
                var data = bl_UtilityHelper.CreatePhotonHashTable();
                data.Add(ZombieEventManager.EventType, (byte)RoundEventTypes.GameOver);
                main.SendDataOverNetwork(data);
                return;
            }
        }
    }

    private void ResetTimer()
    {
        PreperationTime = oldwaitfor;
    }
    private bool AreZombiesRemaining()
    {
        return zombiesRemainingInRound > 0;
    }

    private int GetTotalZombiesInRound(ZombieRound round)
    {
    
        int totalZombies = 0;
        foreach (RoundDetails roundDetail in round.roundDetails)
        {
            if (InfiniteRounds)
            {
                totalZombies += roundDetail.numberOfZombies + ExtraZombiesPerRound;
            }
            else
            {
                totalZombies += roundDetail.numberOfZombies;
            }
         
        }

        return totalZombies;
    }
    [PunRPC]
    public IEnumerator SpawnZombiesInRound(ZombieRound round)
    {
        if(!roundInProgress) yield return null;
        if (!PhotonNetwork.IsMasterClient) yield return null;

        List<SpawnPoint> availableSpawnPoints = new List<SpawnPoint>(spawnPoints);
        if (roundInProgress == true)
        {

            foreach (RoundDetails roundDetail in round.roundDetails)
            {
                GameObject zombiePrefab = roundDetail.zombiePrefab;

                for (int i = 0; i < roundDetail.numberOfZombies; i++)
                {
                    if (availableSpawnPoints.Count == 0)
                    {
                        break;
                    }

                    int randomIndex = Random.Range(0, availableSpawnPoints.Count);
                    SpawnPoint selectedSpawnPoint = availableSpawnPoints[randomIndex];

                    GameObject zombie = SpawnZombie(zombiePrefab.name, selectedSpawnPoint);
                    if (DifficultyIncrease)
                    {
                        if (zombie.GetComponent<AIController>().ZombieUpdateRate >= 1)
                        {
                            zombie.GetComponent<AIController>().ZombieUpdateRate += startingUpdateRate;
                        }
                        if (zombie.GetComponent<NavMeshAgent>().speed <= 10)
                        {
                            zombie.GetComponent<NavMeshAgent>().speed += startingBoostAmount;
                        }
                        if (zombie.GetComponent<bl_ZombieHealth>().maxHealth <= 750)
                        {
                            zombie.GetComponent<bl_ZombieHealth>().maxHealth += startinghealthIncrease;
                        }
                    }
                    spawnedZombies.Add(zombie);

                    selectedSpawnPoint.maxZombies--;


                    if (selectedSpawnPoint.maxZombies <= 0)
                    {
                        availableSpawnPoints.RemoveAt(randomIndex);
                    }

                    yield return new WaitForSeconds(roundDetail.delayBetweenZombies);
                }
            }
        }

    }

    GameObject SpawnZombie(string zombiePrefab, SpawnPoint spawnPoint)
    {
        zombie = PhotonNetwork.Instantiate(zombiePrefab, spawnPoint.transform.position, spawnPoint.transform.rotation);
        return zombie;
    }
   
    [PunRPC]
    public void ZombieDied()
    {
        OnZombieDied();
    }

    private void OnZombieDied()
    {
        zombiesRemainingInRound--;
        photonView.RPC(nameof(UpdateZombiesCountDisplay), RpcTarget.All);
        if (!AreZombiesRemaining())
        {
            bool zombiesWithTagsLeft = CheckForZombiesWithTags();

            if (!zombiesWithTagsLeft)
            {
                roundInProgress = false;

                if (currentRoundIndex >= rounds.Length)
                {
                    currentRoundIndex = 0;
                }
                StartPrep();
                
            }
        }

       
    }
    private bool CheckForZombiesWithTags()
    {
        foreach (var zombie in spawnedZombies)
        {
            if (zombie != null)
            {
                if (zombie.CompareTag("Zombie"))
                {
                    return true;
                }
            }
        }
        return false;
    }
    [PunRPC]
    public void UpdateRoundDisplay()
    {
        int displayedRound = (currentRoundIndex - startingRoundIndex + 0);
        if (displayedRound < 1)
        {
            displayedRound = 1;
        }
        DisplayLastRound();
        if (AllowZombieRoundDisplay)
        {
            if (roundDisplayText != null)
            {
                if (!InfiniteRounds)
                {
                    roundDisplayText.transform.parent.gameObject.SetActive(true);
                    roundDisplayText.text = "ROUND: " + displayedRound.ToString();
                }
                else
                {
                    roundDisplayText.transform.parent.gameObject.SetActive(true);
                    roundDisplayText.text = "NEXT HORDE INCOMING";
                }
            }
        }
        else
        {
            roundDisplayText.text = "";
            roundDisplayText.transform.parent.gameObject.SetActive(false);
        }
    }
    public void AftherRoundRemainUpdate(ZombieRound round)
    {
        if (AllowZombieCount)
        {
            if (zombiesCountText != null)
            {
                foreach (RoundDetails roundDetail in round.roundDetails)
                {
                    zombiesCountText.text = "REMAINING: " + roundDetail.numberOfZombies;
                }
            }
        }
        else
        {
            zombiesCountText.text = "";
            zombiesCountText.transform.parent.gameObject.SetActive(false);
        }
    }
    public void SetStartingRound(int roundIndex)
    {
        startingRoundIndex = Mathf.Clamp(roundIndex, 0, rounds.Length - 1);
        currentRoundIndex = startingRoundIndex;
    }

    public void IncreaseScore(int amount)
    {
        playerScore += amount;
        UpdateScoreDisplay();
    }
   
    void UpdateScoreDisplay()
    {
        if (AllowZombieScore)
        {
            if (scoreDisplayText != null)
            {
                scoreDisplayText.text = "SCORE: " + playerScore.ToString();
            }
        }
        else
        {
            scoreDisplayText.text = " ";
            scoreDisplayText.transform.parent.gameObject.SetActive(false);
        }
    }
    void DisplayLastRound()
    {
        int displayedRound = (currentRoundIndex - startingRoundIndex + 0);
        if (endResultroundDisplayer != null)
        {
            if (displayedRound <= 1)
            {
                endResultroundDisplayer.text = "YOU SURVIVED: " + displayedRound.ToString() + " ROUND";
            }
            else
            {
                endResultroundDisplayer.text = "YOU SURVIVED: " + displayedRound.ToString() + " ROUNDS";
            }
        }
    }
    [PunRPC]
    void UpdateZombiesCountDisplay()
    {
        if (AllowZombieCount)
        {
            if (zombiesCountText != null)
            {
                zombiesCountText.text = "REMAINING: " + zombiesRemainingInRound.ToString();
            }
            
        }
        else
        {
            zombiesCountText.text = "";
            zombiesCountText.transform.parent.gameObject.SetActive(false);
        }
    }
    [PunRPC]
    void UpdateRoundsCountDisplay()
    {
        if (AllowInBetweenRoundDisplay)
        {
            int displayedRound = (currentRoundIndex - startingRoundIndex + 1);
            if (betweenRoundText != null)
            {
                betweenRoundText.text = "ROUND: " + displayedRound.ToString();
                betweenRoundText.transform.parent.gameObject.SetActive(true);
            }
        }
        else
        {
            betweenRoundText.text = "";
            betweenRoundText.transform.parent.gameObject.SetActive(false);
        }
    }
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(zombiesRemainingInRound);
            stream.SendNext(currentRoundIndex);
            stream.SendNext(roundInProgress);
            stream.SendNext(PrepStarted);
            stream.SendNext(PreperationTime);
        }
        else
        {
            PreperationTime = (int)stream.ReceiveNext();
            PrepStarted = (bool)stream.ReceiveNext();
            zombiesRemainingInRound = (int)stream.ReceiveNext();
            roundInProgress = (bool)stream.ReceiveNext();
            currentRoundIndex = (int)stream.ReceiveNext();
        }
    }


    #endregion
}

