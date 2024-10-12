using UnityEngine;
using ZombiesHealthManagers;
using Photon.Pun;
using Photon.Realtime;
using System.Runtime.InteropServices.WindowsRuntime;


public class Nuke : MonoBehaviour
{
    [Header("Settings")]
    [Space(5)]
    public int NukeMoney = 750;
    public float OnSpawnDuration = 30f;

    [Header("Refremces")]
    [Space(5)]
    public GameObject explosionEffectPrefab;
    public AudioClip explosionSound;
    public AudioClip PickupSound;

    private bl_RoundManager manager;
    private float detectionRange = 0.4f;
    private bool PickedUp = false;

    void Awake()
    {
        manager = FindObjectOfType<bl_RoundManager>(true);
    }

    private void Update()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRange);

        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag(bl_MFPS.LOCAL_PLAYER_TAG))
            {
                PickedUp = true;
                if (PickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(PickupSound, transform.position);
                }
                Invoke(nameof(OnDestroyed), OnSpawnDuration);
                DestroyZombies();
                PlayExplosionEffectAndSound();
                manager.IncreaseScore(NukeMoney);
                break;
            }
        }
    }

    private void DestroyZombies()
    {
        manager.ClearAllZombies();
    }

    private void PlayExplosionEffectAndSound()
    {
        Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        PhotonNetwork.Destroy(gameObject);
    }
    private void OnDestroyed()
    {
        if (PickedUp)
            return;
        PhotonNetwork.Destroy(gameObject);
    }
}
