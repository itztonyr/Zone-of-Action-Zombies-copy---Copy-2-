using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZombiesGameMode;
using Photon.Pun;
public class bl_ZombiesHealthBar : MonoBehaviour
{
    public enum HealthBarStyle
    {
        AlwaysShow = 0,
        Hidden = 1,
        ShowOnHover = 2,
    }
    [Header("References")]
    [Space(5)]
    public HealthBarStyle style;
    public GameObject Content;
    public Image healthBarFill;
    public TextMeshProUGUI zombieNameDisplay;


    private float targetHealth = 1;
    private float reduceSpeed = 1.25f;
    public static bl_ZombiesHealthBar Instance;
    private float LastSpotedTime;
    private float CoolDownBeforeAllowingShow = 1;
    private float Range = 50;
    // Start is called before the first frame update
    private void Awake()
    {
        Instance = this;
        if (style == HealthBarStyle.Hidden || style == HealthBarStyle.ShowOnHover)
        {
            Content.SetActive(false);
        }
        else
        {
            Content.SetActive(true);
        }
        zombieNameDisplay.text = bl_Zombies.Instance.ZombieName;
    }

    // Update is called once per frame
    private void Update()
    {
        if (bl_GameManager.Instance.LocalPlayerReferences == null || style == HealthBarStyle.Hidden) return;

        Content.transform.rotation = Quaternion.LookRotation(transform.position - bl_GameManager.Instance.LocalPlayerReferences.playerCamera.transform.position);
        healthBarFill.fillAmount = Mathf.MoveTowards(healthBarFill.fillAmount, targetHealth, reduceSpeed * Time.deltaTime);

        if (style == HealthBarStyle.ShowOnHover)
        {
            // Will contain the information of which object the raycast hit
            RaycastHit hit;

            if (Physics.Raycast(bl_GameManager.Instance.LocalPlayerReferences.playerCamera.transform.position, bl_GameManager.Instance.LocalPlayerReferences.playerCamera.transform.forward, out hit, Range) && hit.collider.gameObject.CompareTag("Zombie"))
            {
                Content.SetActive(true);
            }
            else
            {
                Content.SetActive(false);
            }
        }
    }
    [PunRPC]
    public void OnZombieHealthBarHit(float maxHealth, float currentHealth)
    {
        targetHealth = currentHealth / maxHealth;
    }
}
