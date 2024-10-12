using MFPS.Internal.Structures;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;


public class InstaKill : MonoBehaviour
{
    [Header("Settings")]
    [Space(5)]
    public float Duration = 30f;
    public float OnSpawnDuration = 30f;
    public AudioClip PickupSound;

    private bl_GunManager GunManager;
    [HideInInspector] public List<bl_Gun> AllGuns;
    [HideInInspector] public bl_Gun gun;
    [HideInInspector] public bool EffectActive;
    [HideInInspector] public bool PickedUp = false;
    private int DamageMultiplyer = 500;
    private float detectionRange = 1.5f;
    private MeshRenderer[] renderers;
    private Canvas canvas;
    private TextMeshProUGUI text;
    [HideInInspector] public int DurationReal;
    [Header("Debug")]
    [Space(5)]
    public InstaKill[] OtherInstances;

    private void Start()
    {
        StopCoroutine(nameof(NewWeaponStats));
        Invoke("OnDestroyed", OnSpawnDuration);
        canvas = GetComponentInChildren<Canvas>();
        text = GetComponentInChildren<TextMeshProUGUI>();
        text.text = "";
        if (canvas != null)
        {
            canvas.enabled = false;
        }
        renderers = GetComponentsInChildren<MeshRenderer>();
        GunManager = bl_MFPS.LocalPlayerReferences.gunManager;
        Duration = DurationReal;
      
    }
    // Update is called once per frame
    void Update()
    {
        // Access the PlayerEquip list from GunManager
        List<bl_Gun> playerEquip = GunManager.PlayerEquip;
        // Get the CurrentGun from GunManager
        gun = GunManager.CurrentGun;
        AllGuns = new List<bl_Gun>(playerEquip);

        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRange);


        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag(bl_MFPS.LOCAL_PLAYER_TAG))
            {
                PickedUp = true;
                EffectActive = true;
                if (PickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(PickupSound, transform.position);
                }
                StartCoroutine(nameof(NewWeaponStats));

                if (canvas != null)
                {
                    canvas.enabled = true;
                }
                text.text = "DURATION: " + Duration.ToString("F0");
                break;
            }
        }

        if (PickedUp)
        {
            foreach (MeshRenderer m in renderers)
            {
                m.enabled = false;
            }
            ShowNotificationSample();
            if (Duration > 0) 
            { 
                Duration -= Time.deltaTime;
            }
            if(Duration <= 0)
            {
                EffectActive = false;
                Invoke(nameof(NewWeaponStats), 0f);
                Destroy(gameObject);
            }
        }
       
    }
    void ShowNotificationSample()
    {
        new MFPSLocalNotification("INSTA KILL ACTIVE");
    }
    private void OnDestroyed()
    {
        if (PickedUp || EffectActive)
            return;
        Destroy(gameObject);
    }
    private void NewWeaponStats()
    {

        foreach (bl_Gun Gun in AllGuns)
        {
            if (Gun != null)
            {
                if (EffectActive && PickedUp)
                {
                    for (int i = 0; i < AllGuns.Count; i++)
                    {
                        Gun.extraDamage = DamageMultiplyer;
                    }
                }
                if (canvas != null)
                {
                    canvas.enabled = false;
                }
                Invoke(nameof(QuitWeaponStats), Duration);
                text.text = "";
            }
        }
    }
    private void QuitWeaponStats()
    {
        foreach (bl_Gun Gun in AllGuns)
        {
            if (Gun != null)
            {
                Gun.extraDamage = 0; //oof
            }
        }
    }
    private void OnApplicationQuit()
    {
        Invoke(nameof(QuitWeaponStats), Duration);

    }
}
