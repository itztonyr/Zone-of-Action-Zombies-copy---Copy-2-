using UnityEngine;

public class BuyableItemsManager : MonoBehaviour
{
    public static BuyableItemsManager instance;

    private int playerScore = 0;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    public bool CanAfford(int cost)
    {
        return playerScore >= cost;
    }

    public void ReduceScore(int amount)
    {
        playerScore -= amount;
    }

    public void IncreaseScore(int amount)
    {
        playerScore += amount;
    }
}
