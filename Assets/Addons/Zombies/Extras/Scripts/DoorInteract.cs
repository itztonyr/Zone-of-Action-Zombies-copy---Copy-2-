using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using ZombiesGameMode;

public class DoorInteract : MonoBehaviour
{ 
    [Header("Interaction Settings")]
    [Space(5)]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactRange = 3f;

    [Header("Door Settings")]
    [Space(5)]
    [SerializeField] private int costToOpen = 10;
    [SerializeField] private GameObject doorPrefab;
    [Tooltip("How many seconds to display the door afther it has been bought (example if you have an animation and destoy the door afther the animation)")]
    [SerializeField] private float doorDisplayTime = 2f;

    [Header("UI Settings")]
    [Space(5)]
    [SerializeField] private TextMeshProUGUI promptText;



    private bool isInRange = false;
    private PhotonView photonView;
    private Image image;
    private Animator animator;

    private void Start()
    {
        image = GetComponentInChildren<Image>();
        photonView = GetComponent<PhotonView>();
        animator = doorPrefab.GetComponent<Animator>();
        if (promptText != null)
        {
            promptText.enabled = false;
        }
    }

    private void Update()
    {
        if (bl_GameManager.Instance.LocalPlayer == null)
            return;
        CheckPlayerProximity();

        if (isInRange && Input.GetKeyDown(interactKey))
        {
            TryOpenDoor();
        }
    }

    private void CheckPlayerProximity()
    {
        if (bl_GameManager.Instance.LocalPlayer == null)
            return;
        isInRange = Vector3.Distance(transform.position, bl_Zombies.Instance.LocalPlayerReferences.BotAimTarget.position) <= interactRange;
        if (isInRange)
        {
            promptText.text = "COST: " + costToOpen.ToString();
            if (image != null)
            {
                image.gameObject.SetActive(isInRange);
            }
        }
        else
        {
            promptText.text = "" + costToOpen.ToString();
            if (image != null)
            {
                image.gameObject.SetActive(isInRange);
            }
        }
    }

    private void TryOpenDoor()
    {
        bl_RoundManager roundManager = FindObjectOfType<bl_RoundManager>();

        if (roundManager != null && roundManager.CanAfford(costToOpen))
        {
            roundManager.ReduceScore(costToOpen);
            photonView.RPC(nameof(SpawnDoorPrefab), RpcTarget.All);

        }
    }
    [PunRPC]
    private void SpawnDoorPrefab()
    {
        if (doorPrefab != null)
        {
            animator.Play("Open", 0 ,0);
            Destroy(doorPrefab, doorDisplayTime);
            
        }
    }
}
