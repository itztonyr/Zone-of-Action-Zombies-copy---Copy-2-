using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MFPS.Addon.KillStreak
{
    [RequireComponent(typeof(AudioSource))]
    public class bl_KillNotifier : MonoBehaviour
    {
        public GameObject Content;
        public Image KillLogo;
        public TextMeshProUGUI KillText;
        public CanvasGroup GlobalAlpha;
        public AnimationClip HideAnimation;
        [SerializeField] private bl_KillNotifierSpecialUI specialBadgeUI = null;
        [SerializeField] private RectTransform specialBadgePanel = null;

        private bool isShowing = false;
        private KillStreakInfo currentInfo;
        private AudioSource ASource;
        private List<bl_KillNotifierSpecialUI> specialBadges = new();

        [Serializable]
        public class SecundaryNotifier
        {
            public string Key;
            public GameObject Root;

            public void SetActive(bool active)
            {
                if (Root == null) return;
                Root.SetActive(active);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            Content.SetActive(false);
            specialBadgeUI.SetActive(false);
            ASource = GetComponent<AudioSource>();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Show()
        {
            if (isShowing && !bl_KillNotifierData.Instance.OverrideOnNewStreak) return;

            isShowing = true;
            currentInfo = bl_KillStreakManager.Instance.GetQueueNotifier();
            if (currentInfo == null) return;

            ASource.Stop();
            if (currentInfo.info.byHeadShot && bl_KillNotifierData.Instance.prioretizeHeadShotNotification)
            {
                if (bl_KillNotifierData.TryGetNotification("headshot", out var info))
                {
                    SetupNotification(info);
                }
                else
                {
                    SetupNotification(currentInfo);
                }
            }
            else
            {
                SetupNotification(currentInfo);
            }

            ASource.volume = bl_KillNotifierData.Instance.volumeMultiplier;
            ASource.Play();

            StopAllCoroutines();
            StartCoroutine(DoDisplay());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        public void SetupNotification(KillStreakInfo info)
        {
            KillLogo.sprite = info.KillIcon;
            ASource.clip = info.KillClip;

            if (bl_KillNotifierData.Instance.killNotifierTextType == KillNotifierTextType.KillCount)
            {
                KillText.text = string.Format(bl_KillNotifierData.Instance.killCountFormat, bl_KillNotifierUtils.AddOrdinal(info.killID)).ToUpper();
            }
            else
            {
                KillText.text = info.KillName.ToUpper();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void InstanceSpecialBadges(KillStreakInfo killStreakInfo)
        {
            foreach (var g in specialBadges) { g.SetActive(false); }

            if (killStreakInfo.specials.Count <= 0) return;

            var showList = new List<bl_KillNotifierSpecialUI>();
            for (int i = 0; i < killStreakInfo.specials.Count; i++)
            {
                string key = killStreakInfo.specials[i];
                if (bl_KillNotifierData.TryGetNotification(key, out var info))
                {
                    if (info.Skip) continue;

                    int index = specialBadges.FindIndex(x => x.SpecialKey == key);
                    if (index == -1)
                    {
                        var badge = Instantiate(specialBadgeUI.gameObject, specialBadgePanel, false);
                        var script = badge.GetComponent<bl_KillNotifierSpecialUI>();
                        specialBadges.Add(script);
                        script.Setup(key, info);
                        showList.Add(script);
                    }
                    else
                    {
                        showList.Add(specialBadges[index]);
                    }
                }
            }

            StartCoroutine(ShowSpecials());
            IEnumerator ShowSpecials()
            {
                for (int i = 0; i < showList.Count; i++)
                {
                    showList[i].SetAtSiblingPosition(i);
                    showList[i].SetActive(true);
                    yield return new WaitForSeconds(bl_KillNotifierData.Instance.timeBetweenSpecialBadge);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Hide()
        {
            StopAllCoroutines();
            GlobalAlpha.alpha = 0;
            Content.SetActive(false);
            isShowing = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator DoDisplay()
        {
            GlobalAlpha.alpha = 1;
            Content.SetActive(false);
            Content.SetActive(true);

            InstanceSpecialBadges(currentInfo);

            yield return new WaitForSeconds(bl_KillNotifierData.Instance.TimeToShow);
            if (HideAnimation == null)
            {
                float d = 1;
                while (d > 0)
                {
                    d -= Time.deltaTime * 3;
                    GlobalAlpha.alpha = d;
                    yield return null;
                }
            }
            else
            {
                Content.GetComponent<Animator>().Play("hide", 0, 0);
                yield return new WaitForSeconds(HideAnimation.length);
            }
            isShowing = false;
            Show();
        }

        private static bl_KillNotifier _instance;
        public static bl_KillNotifier Instance
        {
            get
            {
                if (_instance == null) { _instance = FindObjectOfType<bl_KillNotifier>(); }
                return _instance;
            }
        }
    }
}