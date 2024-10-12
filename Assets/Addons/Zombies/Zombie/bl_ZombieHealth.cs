using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using MFPS.Audio;
using System;
using Random = UnityEngine.Random;
using ZombiesGameMode;
using ZombieEvents;
using Photon.Realtime;

namespace ZombiesHealthManagers
{


    public class bl_ZombieHealth : bl_PlayerHealthManagerBase
    {
        [Header("General")]
        [Space(5)]
        [Range(10, 500)] public int maxHealth = 100;
        [HideInInspector] public int Health = 100;
        public int OnDeathScoreIncrease = 100;
        public int ZombieScore { get; private set; }
        public int OnHitScoreIncrease = 50;

        [Header("Drops")]
        [Space(5)]
        public DropPrefabData[] possibleRewards;


        [Header("Extras")]
        [Space(5)]
        [LovattoToogle] public bool InfluenceScore;
        [LovattoToogle] public bool LocalNotifcation;
        [LovattoToogle] public bool DisplayOnFeed;
#if FT
public bool FloatingText;
#endif


        [Serializable]
        public class DropPrefabData
        {
            public GameObject prefab;
            [Range(0f, 100f)]
            public float dropChancePercentage;
            public Transform spawnPoint;
            public float rewardLifetime = 10f;

        }
        private Animator animator;
        private RepetingDamageInfo repetingDamageInfo;
        private List<DropPrefabData> recentRewards = new List<DropPrefabData>(); // List to store the last 5 rewards
        private Dictionary<GameObject, float> spawnedRewardsTimers = new Dictionary<GameObject, float>();
        private bl_AttackPlayer attack;
        private bl_ZombiesHealthBar healthBar;
        private bool isdeath = false;
        private string LastDamageGiver;
        private bl_RoundManager roundManager;
        private GameObject ragdollPrefab;
        private Rigidbody[] ragdollRigidbodies;
        private bl_ZombieRagdoll ragdoll;
        public void ReceiveDamage(DamageData damageData)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="damageData"></param>
        public override void DoDamage(DamageData damageData)
        {
            if (isdeath)
                return;
            zombieDamage(damageData);
        }

        public void zombieDamage(DamageData e)
        {
            if (isdeath)
                return;
            bool canDamage = true;
            if (!DamageEnabled)
            {
                if (e.MFPSActor != null) { canDamage = true; }
                else canDamage = false;
            }

            if (!canDamage || bl_GameManager.Instance.GameMatchState == MatchState.Waiting)
            {
                if (!IsMine)
                {
                    animator.Play("Hurt");
                }
                return;
            }
            photonView.RPC(nameof(ZombieDamage), RpcTarget.AllBuffered, e.Damage, e.From, e.Cause, e.OriginPosition, e.IsHeadShot, e.GunID);
        }
        protected override void Awake()
        {
            roundManager = FindObjectOfType<bl_RoundManager>();
            ragdoll = GetComponent<bl_ZombieRagdoll>();
            animator = GetComponent<Animator>();
            ragdollPrefab = this.gameObject;
            attack = GetComponent<bl_AttackPlayer>();
            healthBar = GetComponent<bl_ZombiesHealthBar>();
            Health = maxHealth;

        }
        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            
        }

        [PunRPC]
        private void ZombieDamage(int damage, string killer, DamageCause cause, Vector3 hitDirection, bool isHeatShot, int weaponID, Player m_sender)
        {
            if (isdeath)
                return;
            if (Health > 0)
            {
                Health -= damage;
                LastDamageGiver = killer;
                healthBar.OnZombieHealthBarHit(maxHealth, Health);

                // When the damage is given by the local player
                if (m_sender != null && m_sender.NickName == LocalName || killer == LocalName && killer != null)
                {
#if FT
                       if (!FloatingText) //was only FloatingText before
                       {
                          GameObject floatingText = FindObjectOfType<bl_FloatingTextManager>().gameObject;
                          Destroy(floatingText);
                          /*
                          Vector3 offset = Vector3.zero;
                          var fts = bl_FloatingTextManagerSettings.Instance;
                          offset = bl_MFPS.LocalPlayerReferences.transform.TransformDirection(fts.damagePositionOffset);
                          var TargetTransform = this.transform;
                          new FloatingText(string.Format(fts.damageTextFormat, damageData.Damage)) // display as damage ft
                         .SetPositionOffset(offset) //offset
                         .SetSettings(fts.damageTextSetting) //add any custom one you want
                         .SetTextColor(Color.red) //set any color you want
                         .SetOutlineColor(Color.black) // set outline
                         .SetTarget(TargetTransform) //this is just zombie
                         .Show();
                          */ //Only duplicates the floating text sence last update fixing health :)
                       }
#endif
                    if (LastDamageGiver == LocalName)
                    {
                        bl_CrosshairBase.Instance.OnHit();
                        bl_AudioController.Instance.audioController.audioBank.PlayAudioInSource(bl_GameManager.Instance.gameObject.GetComponent<AudioSource>(), "body-hit");
                        bl_EventHandler.DispatchLocalPlayerHitEnemy(new MFPSHitData()
                        {
                            HitTransform = transform,
                            HitPosition = transform.position,
                            Damage = damage,
                            HitName = gameObject.name
                        });
                        if (roundManager != null)
                        {
                            roundManager.IncreaseScore(OnHitScoreIncrease);
                        }
                    }
                }
            }
            if (Health <= 0)
            {
                if (m_sender != null && m_sender.NickName == LocalName || killer == LocalName && killer != null)
                {
                    string weaponName = bl_GameData.Instance.GetWeapon(weaponID).Name;
                    AddKill(isHeatShot, weaponName, weaponID);
                }
                photonView.RPC(nameof(ZombieDie), RpcTarget.All);
            }
        }
        public void AddKill(bool isHeadshot, string m_weapon, int gunID)
        {
       
            if (DisplayOnFeed)
            {
                //send kill feed kill message
                var feed = new bl_KillFeedBase.FeedData()
                {
                    LeftText = LocalName,
                    RightText = bl_Zombies.Instance.ZombieName,
                    Team = bl_PhotonNetwork.LocalPlayer.GetPlayerTeam()
                };
                feed.AddData("gunid", gunID);
                feed.AddData("headshot", isHeadshot);

                bl_KillFeedBase.Instance.SendKillMessageEvent(feed);
            }
            if (LastDamageGiver == LocalName && LastDamageGiver != null)
            {
                if (roundManager != null)
                {
                    roundManager.IncreaseScore(OnDeathScoreIncrease);
                }
                if (InfluenceScore)
                {
                    //Add a new kill and update the player information
                    bl_PhotonNetwork.LocalPlayer.PostKill(1);
                    // calculate the score gained for this kill
                    int score = isHeadshot ? bl_GameData.Instance.scoreSettings.ScorePerKill + bl_GameData.Instance.scoreSettings.ScorePerHeadShot : bl_GameData.Instance.scoreSettings.ScorePerKill;
                    bl_PhotonNetwork.LocalPlayer.PostScore(score);
                }
                //show an local notification for the kill
                if (LocalNotifcation)
                {
                    var localKillInfo = new KillInfo();
                    localKillInfo.Killer = LastDamageGiver;
                    localKillInfo.Killed = bl_Zombies.Instance.ZombieName;
                    localKillInfo.byHeadShot = isHeadshot;
                    localKillInfo.KillMethod = m_weapon;
                    bl_EventHandler.DispatchLocalKillEvent(localKillInfo);
                }

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// 
        [PunRPC]
        private void ZombieDie()
        {
            isdeath = true;
            ragdoll.ActivateRagdoll();
            if (roundManager != null)
            {
                var data = bl_UtilityHelper.CreatePhotonHashTable();
                data.Add(ZombieEventManager.EventType, (byte)RoundEventTypes.ZombieDeath);
                bl_Zombies.Instance.SendDataOverNetwork(data);
                SpawnRandomReward();
            }
        }

        private DropPrefabData GetUniqueRandomReward()
        {
            DropPrefabData selectedReward = null;

            selectedReward = GetRandomReward();

            if (selectedReward == null)
                return null;
            else
            {
                return selectedReward;
            }
        }
        private void SpawnRandomReward()
        {
            DropPrefabData selectedReward = GetUniqueRandomReward();
            if (selectedReward != null)
            {
                GameObject spawnedReward = PhotonNetwork.Instantiate(selectedReward.prefab.name, selectedReward.spawnPoint.position, Quaternion.identity);

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

        private DropPrefabData GetRandomReward()
        {
            float totalPercentage = 0f;
            foreach (var rewardOption in possibleRewards)
            {
                totalPercentage += rewardOption.dropChancePercentage;
            }

            float randomValue = Random.Range(0f, totalPercentage);
            float cumulativePercentage = 0f;

            foreach (var rewardOption in possibleRewards)
            {
                cumulativePercentage += rewardOption.dropChancePercentage;
                if (randomValue <= cumulativePercentage)
                {
                    return rewardOption;
                }
            }

            return null; // No reward found (shouldn't happen if percentages add up to 100)
        }
        /// <summary>
        /// 
        /// </summary>
        public override void DestroyEntity()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        public override void DoRepetingDamage(RepetingDamageInfo info)
        {
            repetingDamageInfo = info;
            InvokeRepeating(nameof(MakeDamageRepeting), 0, info.Rate);
        }

        /// <summary>
        /// 
        /// </summary>
        void MakeDamageRepeting()
        {
            if (repetingDamageInfo == null)
            {
                CancelRepetingDamage();
                return;
            }

            var damageinfo = repetingDamageInfo.DamageData;
            if (damageinfo == null)
            {
                damageinfo = new DamageData();
                damageinfo.OriginPosition = Vector3.zero;
                damageinfo.Cause = DamageCause.Map;
            }
            damageinfo.Damage = repetingDamageInfo.Damage;

        }

        /// <summary>
        /// 
        /// </summary>
        public override void CancelRepetingDamage()
        {
            CancelInvoke(nameof(MakeDamageRepeting));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="healthToAdd"></param>
        public override void SetHealth(int healthToAdd, bool overrideHealth)
        {
            if (overrideHealth) Health = healthToAdd;
            else Health += healthToAdd;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool Suicide()
        {
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHealth() => Health;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetMaxHealth() => 100;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool IsDeath()
        {
            return isdeath;
        }


        [PunRPC]
        void RpcSyncHealth(int _health)
        {
            Health = _health;
        }
    }
}

