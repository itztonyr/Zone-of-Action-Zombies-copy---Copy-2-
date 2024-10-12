using MFPS.Internal.Structures;
using UnityEngine;
using UnityEngine.UI;
public class bl_GobbleGumMachine : MonoBehaviour
{
    [Header("Settings")]
    [Space(5)]
    public int Price = 500;



    private bl_GobbleGum Manager;
    private int currentPlayerFunds;
    private bl_RoundManager roundmanager;
    private Image UI;
    private void Awake()
    {
        roundmanager = FindObjectOfType<bl_RoundManager>();
        Manager = FindObjectOfType<bl_GobbleGum>();
        UI = GetComponentInChildren<Image>();
        UI.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

        bool isInRange = Vector3.Distance(transform.position, bl_GameManager.Instance.LocalPlayer.transform.position) <= Manager.interactRange;
        if (isInRange && Manager.CurrentGobbleGumAmount < Manager.MaxActiveGobbleGums)
        {
            UI.gameObject.SetActive(true);
        }
        else
        {
            UI.gameObject.SetActive(false);
        }
        if (isInRange && Input.GetKeyDown(Manager.interactionKey))
        {
            AttemptToBuyGobble();
        }
    }

    void AttemptToBuyGobble()
    {
        if (CanAfford(Price))
        {
            if (Manager.MaxActiveGobbleGums == Manager.CurrentGobbleGumAmount) return;
            Manager.CurrentGobbleGumAmount++;
            ReduceScore(Price);
            int maxIDOption = Manager.loadout.gobblegums.Length;
            int RandomGobbleGum = Random.Range(0, maxIDOption);
            Manager.loadout.gobblegums[RandomGobbleGum].ActivateEffect();
            new MFPSLocalNotification("GOBBLEGUM " + Manager.loadout.gobblegums[RandomGobbleGum].Name + " BOUGHT");
        }
    }
    public bool CanAfford(int amount)
    {

        return roundmanager.playerScore >= amount;
    }

    public void ReduceScore(int amount)
    {
        roundmanager.playerScore -= Price;
        roundmanager.IncreaseScore(0);
        UpdateScoreDisplay();
    }

    public void IncreaseScore(int amount)
    {
        roundmanager.playerScore += Price;
        roundmanager.IncreaseScore(0);
        UpdateScoreDisplay();
    }

    void UpdateScoreDisplay()
    {
        currentPlayerFunds = roundmanager.playerScore;
    }
}
