using System;
using UnityEngine;

namespace MFPS.ULogin
{
    public abstract class bl_ULoginUIBase : MonoBehaviour
    {
        public enum MessageType
        {
            OutdateVersion,
            NoInternet,
        }

        [Serializable]
        public class MenuWindow
        {
            public string Name;
            public string DisplayName;
            public HeaderParts HeaderToShow = HeaderParts.None;
            public GameObject Window;
            public bl_EventHandler.UEvent onOpen;
            public bl_EventHandler.UEvent onBack;

            [Flags]
            public enum HeaderParts
            {
                None = 0,
                Everything = 1,
                Title = 2,
                GameTitle = 4,
                Empty = 8,
            }

            public void SetActive(bool active)
            {
                if (Window != null) Window.SetActive(active);
                if (active) onOpen?.Invoke();
            }
        }

        public GameObject[] addonObjects;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callback"></param>
        public virtual void Init(Action callback)
        {
            if (callback != null) { callback.Invoke(); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="windowName"></param>
        public abstract void OpenWindow(string windowName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="windowIndex"></param>
        public abstract void OpenWindow(int windowIndex);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public abstract void SetActiveAuthUI(bool active);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loading"></param>
        public abstract void SetLoading(bool loading);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public abstract void SetActiveMessageWindow(MessageType type, bool active);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="log"></param>
        public virtual void SetLogText(string log) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public virtual void SetPlayAsGuestButtonActive(bool active) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sceneName"></param>
        public virtual void LoadScene(string sceneName)
        {
            bl_DataBaseUtils.LoadLevel(sceneName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="log"></param>
        public static void Log(string log)
        {
            if (Instance == null) return;
            Instance.SetLogText(log);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="windowName"></param>
        /// <returns></returns>
        public abstract GameObject GetWindowObject(string windowName);

        private static bl_ULoginUIBase _instance;
        public static bl_ULoginUIBase Instance
        {
            get
            {
                if (_instance == null) { _instance = FindObjectOfType<bl_ULoginUIBase>(); }
                return _instance;
            }
        }
    }
}