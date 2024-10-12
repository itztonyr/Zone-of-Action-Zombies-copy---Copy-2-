using MFPSEditor;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.InputManager
{
    [CreateAssetMenu(fileName = "InputManager", menuName = "MFPS/Input/Manager")]
    public class bl_InputData : ScriptableObject
    {

        [SerializeField, ScriptableDrawer] private ButtonMapped Mapped = null;
        public string inputVersion = "1.0.0";
        [LovattoToogle] public bool useGamePadNavigation = false;
        [LovattoToogle] public bool runWithButton = false;

        [Header("References")]
        public GameObject GamePadInputModule;
        public GameObject GamePadPointerPrefab;

        [Header("Mapped Options")]
        public ButtonMapped[] mappedOptions;

        public ButtonMapped mappedInstance { get; set; }
        private readonly Dictionary<string, ButtonData> cachedKeys = new();
        public const string KEYS = "mfps.input.bindings";
        private const string NONE = "None";

        public MFPSInputSource InputType
        {
            get
            {
                return Mapped != null ? Mapped.inputType : MFPSInputSource.Keyboard;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Initialize()
        {
            cachedKeys.Clear();
            mappedInstance = null;
            LoadMapped();
        }

        /// <summary>
        /// 
        /// </summary>
        void LoadMapped()
        {
            if (PlayerPrefs.HasKey(MappedBindingKey))
            {
                string json = PlayerPrefs.GetString(MappedBindingKey);
                mappedInstance = Instantiate(Mapped);
                mappedInstance.mapped = JsonUtility.FromJson<Mapped>(json);
            }
            else
            {
                mappedInstance = Instantiate(Mapped);
            }
            mappedInstance.Init();
        }

        /// <summary>
        /// 
        /// </summary>
        public void ChangeMapped(int mappedID)
        {
            Mapped = mappedOptions[mappedID];
            Initialize();
            if (Mapped.inputType != MFPSInputSource.Keyboard)
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool GetButton(string key)
        {
            return IsCached(key, out ButtonData button) && button.IsButton();
        }

        /// <summary>
        /// 
        /// </summary>
        public bool GetButtonDown(string key)
        {
            return IsCached(key, out ButtonData button) && button.IsButtonDown();
        }

        /// <summary>
        /// 
        /// </summary>
        public bool GetButtonUp(string key)
        {
            return IsCached(key, out ButtonData button) && button.IsButtonUp();
        }

        /// <summary>
        /// Get the name of the primary button name
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetButtonName(string key)
        {
            return !IsCached(key, out ButtonData button) ? NONE : button.GetInputName();
        }

        /// <summary>
        /// 
        /// </summary>
        private bool IsCached(string key, out ButtonData button)
        {
            if (mappedInstance == null) { Initialize(); }

            if (!cachedKeys.TryGetValue(key, out var buttonData))
            {
                if (mappedInstance.ButtonMapDictionary.TryGetValue(key, out buttonData))
                {
                    cachedKeys.Add(key, buttonData);
                }
                else
                {
                    Debug.Log($"Key <color=yellow>{key}</color> has not been mapped in the InputManager.");
                    button = null;
                    return false;
                }
            }
            button = buttonData;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void SaveMappedInstance()
        {
            string json = JsonUtility.ToJson(mappedInstance.mapped);
            PlayerPrefs.SetString(MappedBindingKey, json);
        }

        /// <summary>
        /// 
        /// </summary>
        public void RevertMappedInstance()
        {
            LoadMapped();
        }

        public string MappedBindingKey => Mapped == null ? KEYS : $"{KEYS}.{(short)Mapped.inputType}.{inputVersion}";
        public ButtonMapped DefaultMapped => Mapped;

        private static bl_InputData m_Data;
        public static bl_InputData Instance
        {
            get
            {
                if (m_Data == null)
                {
                    m_Data = Resources.Load("InputManager", typeof(bl_InputData)) as bl_InputData;
                }
                return m_Data;
            }
        }
    }
}