using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using ZombiesGameMode;

public class bl_AttackPlayer : MonoBehaviour
{
    #region Public
    [Header("Attack Setings")]
    [Space(5)]
    public int DamageAmount;
    public float AttackSpeed;
    public float EscapeTime;
    [Header("Audio Setings")]
    [Space(5)]
    public AudioSource Source;
    public List<AudioClip> Scream;
    [Header("Animation Setings")]
    [Space(5)]
    public Animator animator;
    #endregion

    #region Private
    [HideInInspector] public AudioClip scream;
    private float timeToWaitBetween;
    private float waitTimer;
    private MFPSPlayer targetObject;
    private List<MFPSPlayer> PlayerList = new List<MFPSPlayer>();
    private List<MFPSPlayer> AlivePlayerList = new List<MFPSPlayer>();
    #endregion

    private void Update()
    {
        PlayerList = bl_Zombies.Instance.PlayerSort;
        for (int i = 0; i < PlayerList.Count; i++)
        {
            if (PlayerList[i].isAlive)
            {
                AlivePlayerList.Add(PlayerList[i]);
            }
            else
            {
                AlivePlayerList.Remove(PlayerList[i]);
            }
        }
        if (PlayerList.Count <= 0)
            return;
        targetObject = GetClosestEnemy(AlivePlayerList);
    }
    void OnEnable()
    {
        bl_EventHandler.onLocalPlayerDeath += OnLocalPlayerDeath;
        bl_EventHandler.onLocalPlayerSpawn += OnLocalPlayerSpawn;
    }

    private void OnDisable()
    {
        bl_EventHandler.onLocalPlayerDeath -= OnLocalPlayerDeath;
        bl_EventHandler.onLocalPlayerSpawn -= OnLocalPlayerSpawn;
    }

    void OnLocalPlayerDeath()
    {
        CancelInvoke(nameof(DealDamageFunction));
        bl_Zombies.Instance.UpdateTargetList();
    }
    void OnLocalPlayerSpawn()
    {
        bl_Zombies.Instance.UpdateTargetList();
    }
    private void DealDamageFunction()
    {
        if (targetObject != null)
        {
            bl_PlayerHealthManager playerHealth = targetObject.Actor.gameObject.GetComponent<bl_PlayerHealthManager>();
            if (playerHealth != null)
            {
                DamageData damageData = new DamageData();
                damageData.Damage = DamageAmount;
                damageData.From = bl_Zombies.Instance.ZombieName;
                damageData.Cause = DamageCause.Bot;
                damageData.IsHeadShot = false;
                damageData.SpecialWeaponName = "Scratch";
                damageData.OriginPosition = transform.position + targetObject.Actor.position;
                playerHealth.DoDamage(damageData);
            }
        }

    }
    MFPSPlayer GetClosestEnemy(List<MFPSPlayer> enemies)
    {
        MFPSPlayer bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;
        foreach (MFPSPlayer potentialTarget in enemies)
        {
            Vector3 directionToTarget = potentialTarget.Actor.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = potentialTarget;
            }
        }
        return bestTarget;
    }
    void PlayAttackAnimation()
    {
        animator.Play("Punching", 1, 0);
        scream = Scream[Random.Range(0, Scream.Count)];
        Source.clip = scream;
        Source.Play();
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.tag == bl_MFPS.LOCAL_PLAYER_TAG)
        {
            animator.Play("Idle");
            InvokeRepeating(nameof(PlayAttackAnimation), 0f, AttackSpeed);
            InvokeRepeating(nameof(DealDamageFunction), EscapeTime, AttackSpeed);
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == bl_MFPS.LOCAL_PLAYER_TAG)
        {
            animator.Play("Run");
            CancelInvoke("DealDamageFunction");
            CancelInvoke("PlayAttackAnimation");
        }

    }


}
