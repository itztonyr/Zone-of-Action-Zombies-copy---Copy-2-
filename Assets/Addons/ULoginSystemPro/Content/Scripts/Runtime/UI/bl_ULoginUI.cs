using MFPS.ULogin.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MFPS.ULogin
{
    public class bl_ULoginUI : bl_ULoginUIBase
    {
        [Header("Windows")]
        public List<MenuWindow> windows;

        [Header("References")]
        [SerializeField] private GameObject authUI;
        [SerializeField] private Animator PanelAnim = null;
        [SerializeField] private GameObject loadingUI = null;
        [SerializeField] private GameObject outdateWindow = null;
        [SerializeField] private GameObject noInternetWindow = null;
        [SerializeField] private GameObject GuestButton = null;
        [SerializeField] private CanvasGroup FadeAlpha = null;
        [SerializeField] private TextMeshProUGUI logText = null;
        [SerializeField] private bl_HeaderUI headerUI = null;

        public int CurrentWindow { get; set; } = -1;

        /// <summary>
        /// 
        /// </summary>
        private void Awake()
        {
            SetLoading(false);
            SetPlayAsGuestButtonActive(false);
            SetActiveAuthUI(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callback"></param>
        public override void Init(Action callback)
        {
            StartCoroutine(DoStartFade());
            IEnumerator DoStartFade()
            {
                FadeAlpha.alpha = 1;
                while (FadeAlpha.alpha > 0)
                {
                    FadeAlpha.alpha -= Time.deltaTime;
                    yield return null;
                }
                callback?.Invoke();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="windowName"></param>
        public override void OpenWindow(string windowName)
        {
            var id = windows.FindIndex(x => x.Name == windowName);
            if (id == -1)
            {
                Debug.LogWarning($"Login window {windowName} doesn't exist.");
                return;
            }
            OpenWindow(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public override void SetActiveAuthUI(bool active)
        {
            authUI.SetActive(active);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="windowName"></param>
        public override void OpenWindow(int windowIndex)
        {
            if (windowIndex == CurrentWindow) return;

            StopAllCoroutines();
            StartCoroutine(ChangePanelAnim(() =>
            {
                CurrentWindow = windowIndex;
                windows.ForEach(x => x.SetActive(false));
                var window = windows[CurrentWindow];
                if (headerUI != null) headerUI.SetupFor(windows[windowIndex]);
                window.SetActive(true);
            }));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sceneName"></param>
        public override void LoadScene(string sceneName)
        {
            StartCoroutine(LoadSceneSecuence());
            /// <summary>
            /// 
            /// </summary>
            IEnumerator LoadSceneSecuence()
            {
                while (FadeAlpha.alpha < 1)
                {
                    FadeAlpha.alpha += Time.deltaTime * 2;
                    yield return new WaitForEndOfFrame();
                }
                bl_DataBaseUtils.LoadLevel(sceneName);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="windowName"></param>
        /// <returns></returns>
        public override GameObject GetWindowObject(string windowName)
        {
            var id = windows.FindIndex(x => x.Name == windowName);
            if (id == -1)
            {
                Debug.LogWarning($"Login window {windowName} doesn't exist.");
                return null;
            }
            return windows[id].Window;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loading"></param>
        public override void SetLoading(bool loading)
        {
            if (loadingUI == null) return;

            loadingUI.SetActive(loading);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="log"></param>
        public override void SetLogText(string log)
        {
            if (logText == null) return;

            logText.text = log;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public override void SetPlayAsGuestButtonActive(bool active)
        {
            if (GuestButton == null) return;
            if (!bl_LoginProDataBase.Instance.allowPlayAsGuest)
            {
                GuestButton.gameObject.SetActive(false);
                return;
            }
            GuestButton.gameObject.SetActive(active);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public override void SetActiveMessageWindow(MessageType type, bool active)
        {
            if (type == MessageType.OutdateVersion) outdateWindow.SetActive(active);
            else if (type == MessageType.NoInternet) noInternetWindow.SetActive(active);
        }

        /// <summary>
        /// 
        /// </summary>
        IEnumerator ChangePanelAnim(Action callback)
        {
            PanelAnim.Play("change", 0, 0);
            yield return new WaitForSeconds(PanelAnim.GetCurrentAnimatorStateInfo(0).length / 2);
            callback?.Invoke();
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnLogout()
        {
            SetPlayAsGuestButtonActive(true);
        }
    }
}