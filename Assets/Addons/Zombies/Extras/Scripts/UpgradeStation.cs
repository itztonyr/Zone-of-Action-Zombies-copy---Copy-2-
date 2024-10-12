using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;

public class UpgradeStation : MonoBehaviour
{
    [Header("Main Settings")]
    [Space(5)]
    public float interactionRange = 2f;
    public GameObject upgradeUICanvas;
    public TextMeshProUGUI upgradeText;

    public float upgradeDelay = 5f; // Delay in seconds before automatic upgrade
    private float upgradeTimer;

    private bool isPlayerNearby;
    private bl_GunManager GunManager;
    [HideInInspector] public List<bl_Gun> AllGuns;
    private PhotonView Photonview;
    private int currentGunID;
    private bl_FirstPersonController cont;
    [HideInInspector] public bl_Gun gun;

    private void Start()
    {
        upgradeUICanvas.SetActive(false);
        upgradeTimer = upgradeDelay;
    }
    void OnEnable()
    {
        bl_EventHandler.onLocalPlayerSpawn += OnLocalPlayerSpawn;
    }
    private void OnDisable()
    {
        bl_EventHandler.onLocalPlayerSpawn -= OnLocalPlayerSpawn;
    }
    void OnLocalPlayerSpawn()
    {
        GunManager = bl_GameManager.Instance.LocalPlayerReferences.gunManager;
        cont = bl_GameManager.Instance.LocalPlayerReferences.gameObject.GetComponent<bl_FirstPersonController>();
    }
    private void Update()
    {

        if (isPlayerNearby)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                ToggleUpgradeUI();
            }

            // Automatically upgrade the weapon when the timer reaches 0
            if (upgradeTimer <= 0)
            {
                UpgradeWeaponAutomatically();
            }
            else
            {
                upgradeTimer -= Time.deltaTime;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(bl_MFPS.LOCAL_PLAYER_TAG))
        {
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(bl_MFPS.LOCAL_PLAYER_TAG))
        {
            isPlayerNearby = false;
            gun = null;
            upgradeUICanvas.SetActive(false);
        }
    }
    void UnlockMouse()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void ToggleUpgradeUI()
    {
        if (upgradeUICanvas.activeSelf)
        {
            upgradeUICanvas.SetActive(false);
        }
        else
        {
            // Display available upgrades (customize this as needed)
            upgradeText.text = "Choose an upgrade:\n1. Weapon Damage\n2. Weapon Reload Speed\n3. Weapon Speed\n4. Fire Rate";
            UnlockMouse();
            upgradeUICanvas.SetActive(true);
        }
    }

    // Called when the player selects an upgrade button
    public void UpgradeWeapon(int upgradeType)
    {
        if (gun != null)
        {
            switch (upgradeType)
            {
                case 1:
                    UpgradeDamage();
                    break;
                case 2:
                    UpgradeReloadSpeed();
                    break;
                case 3:
                    UpgradeWeaponSpeed();
                    break;
                case 4:
                    UpgradeWeaponFireRate();
                    break;
                    // Add more cases for other upgrades
            }
            upgradeUICanvas.SetActive(false);
        }
    }

    private void UpgradeWeaponAutomatically()
    {
        // Apply the desired upgrade automatically
        if (gun != null)
        {
            // For example, let's upgrade damage automatically

            // Reset the upgrade timer for the next automatic upgrade
            upgradeTimer = upgradeDelay;
        }
    }
    private void UpgradeDamage()
    {

    }
    private void UpgradeReloadSpeed()
    {

    }
    private void UpgradeWeaponFireRate()
    {

    }
    private void UpgradeWeaponSpeed()
    {

    }
}
