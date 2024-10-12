using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Photon.Pun;
using ExitGames.Client.Photon;
using ZombiesGameMode;
using MFPS.Internal.Structures;

[System.Serializable]
public class RewardOption
{
    public byte Id { get; set; }

    public static object Deserialize(byte[] data)
    {
        var result = new RewardOption();
        result.Id = data[0];
        return result;
    }

    public static byte[] Serialize(object customType)
    {
        var c = (RewardOption)customType;
        return new byte[] { c.Id };
    }

    public GameObject rewardPrefab;
    [Range(0f, 100f)] public float spawnPercentage = 25f;
    public float rewardLifetime = 10f;
}

public class MysteryBox : MonoBehaviour, IPunObservable
{
    [Header("Main Setings")]
    [Space(5)]
    public float animationduration;
    public float waituntillopenagain;
    public float interactionRange = 5f;
    public int cost = 950;
    public float rewardSpawnDelay = 2f;

    [Header("Refrence Setings")]
    [Space(5)]
    public AudioSource boxopen;
    public Animator animator;
    public Transform spawnPoint;
    [Header("Text Setings")]
    [Space(5)]
    public Image backround;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI promptText;
    [Header("Key Setings")]
    [Space(5)]
    public KeyCode interactionKey = KeyCode.F;

    public RewardOption[] possibleRewards;

    #region Private
    private bl_RoundManager roundManager;
    private float animationdurartionold;
    private float waituntillopenagainold;
    private bool IsWaiting = false;
    private bool isInRange = false;
    private bool isAnimating = false;
    private bool canInteract = false;
    private bool canOpen = true;
    private PhotonView photonView;
    private Dictionary<GameObject, float> spawnedRewardsTimers = new Dictionary<GameObject, float>();
    private bool isInteractingWithBuyableWeapon = false;
    private List<RewardOption> recentRewards = new List<RewardOption>(); // List to store the last 5 rewards
    #endregion
    private void Start()
    {
        waituntillopenagainold = waituntillopenagain;
        animationdurartionold = animationduration;
        costText.gameObject.SetActive(false);
        promptText.gameObject.SetActive(false);

        // Initialize recentRewards list with null values
        for (int i = 0; i < 5; i++)
        {
            recentRewards.Add(null);
        }

    }
    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        PhotonPeer.RegisterType(typeof(RewardOption), 207, RewardOption.Serialize, RewardOption.Deserialize);
        roundManager = FindObjectOfType<bl_RoundManager>();
    }
    private void Update()
    {
        if (isAnimating)
        {
            if (animationduration > 0)
            {
                animationduration -= Time.deltaTime;
            }
            if (animationduration <= 0)
            {
                animator.Play("close");
                isAnimating = false;
                animationduration = animationdurartionold;
            }

        }
        if (IsWaiting)
        {
            if(waituntillopenagain > 0)
            {
                waituntillopenagain -= Time.deltaTime;
            }
            if(waituntillopenagain <= 0)
            {
                IsWaiting = false;
                waituntillopenagain = waituntillopenagainold;
            }
        }
        if (bl_GameManager.Instance.LocalPlayer == null) return;
        isInRange = Vector3.Distance(transform.position, bl_Zombies.Instance.LocalPlayerReferences.BotAimTarget.position) <= interactionRange;
        if (backround != null)
        {
            backround.gameObject.SetActive(isInRange && !isAnimating);
        }
            costText.gameObject.SetActive(isInRange && !isAnimating);
        promptText.gameObject.SetActive(isInRange && !isAnimating);

        canInteract = isInRange && roundManager.CanAfford(cost) && !isInteractingWithBuyableWeapon;

        if (canInteract && Input.GetKeyDown(interactionKey) && IsWaiting == false)
        {
            UseMysteryBox();
        }

        CheckAndRemoveExpiredRewards();
    }

    public void SetInteractingWithBuyableWeapon(bool value)
    {
        isInteractingWithBuyableWeapon = value;
    }
    private void UseMysteryBox()
    {
        if (!isAnimating && roundManager.CanAfford(cost))
        {
            costText.text = "COST: " + cost.ToString();
            costText.gameObject.SetActive(true);
            roundManager.ReduceScore(cost);
            roundManager.IncreaseScore(0);
            photonView.RPC(nameof(SpawnRandomReward), RpcTarget.All);
            photonView.RPC(nameof(PlayAnimation), RpcTarget.All);
        }
        else
        {
            new MFPSLocalNotification("YOU DONT HAVE ENOUGH MONEY FOR MYSTERYBOX!");
        }
    }

    private RewardOption GetRandomReward()
    {
        float totalPercentage = 0f;
        foreach (var rewardOption in possibleRewards)
        {
            totalPercentage += rewardOption.spawnPercentage;
        }

        float randomValue = Random.Range(0f, totalPercentage);
        float cumulativePercentage = 0f;

        foreach (var rewardOption in possibleRewards)
        {
            cumulativePercentage += rewardOption.spawnPercentage;
            if (randomValue <= cumulativePercentage)
            {
                return rewardOption;
            }
        }

        return null; // No reward found (shouldn't happen if percentages add up to 100)
    }
  
    [PunRPC]
    void PlayAnimation()
    {
        isAnimating = true;
        animator.Play("open");
        boxopen.Play();
        IsWaiting = true;
    }
    private void AddToRecentRewards(RewardOption reward)
    {
        recentRewards.RemoveAt(0); // Remove the oldest reward
        recentRewards.Add(reward); // Add the new reward to the end of the list
    }

    private RewardOption GetUniqueRandomReward()
    {
        RewardOption selectedReward = null;

        while (selectedReward == null || recentRewards.Contains(selectedReward))
        {
            selectedReward = GetRandomReward();
        }

        AddToRecentRewards(selectedReward);

        return selectedReward;
    }
    [PunRPC]
    private void SpawnRandomReward()
    {
        Invoke(nameof(SpawnRewardPrivate), 2);
    }
    private void SpawnRewardPrivate()
    {
        RewardOption selectedReward = GetUniqueRandomReward();
        if (selectedReward != null)
        {
            GameObject spawnedReward = Instantiate(selectedReward.rewardPrefab, spawnPoint.position, Quaternion.identity);

            spawnedRewardsTimers.Add(spawnedReward, selectedReward.rewardLifetime);

            StartCoroutine(DestroyRewardAfterLifetime(spawnedReward, selectedReward.rewardLifetime));
        }
    }
    private IEnumerator DestroyRewardAfterLifetime(GameObject reward, float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        spawnedRewardsTimers.Remove(reward);
        Destroy(reward);
    }


    private void CheckAndRemoveExpiredRewards()
    {
        List<GameObject> rewardsToRemove = new List<GameObject>();

        foreach (var spawnedReward in spawnedRewardsTimers.Keys)
        {
            spawnedRewardsTimers[spawnedReward] -= Time.deltaTime;
            if (spawnedRewardsTimers[spawnedReward] <= 0f)
            {
                rewardsToRemove.Add(spawnedReward);
            }
        }

        foreach (var rewardToRemove in rewardsToRemove)
        {
            spawnedRewardsTimers.Remove(rewardToRemove);
            Destroy(rewardToRemove);
        }
    }
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(waituntillopenagain);
            stream.SendNext(animationduration);
            stream.SendNext(rewardSpawnDelay);
        }
        else
        {
            rewardSpawnDelay = (float)stream.ReceiveNext();
            animationduration = (float)stream.ReceiveNext();
            waituntillopenagain = (float)stream.ReceiveNext();
        }
    }
}
