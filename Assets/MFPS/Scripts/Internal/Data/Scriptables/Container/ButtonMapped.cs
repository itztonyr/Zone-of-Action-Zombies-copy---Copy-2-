using System;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.InputManager
{
    [Serializable, CreateAssetMenu(fileName = "Button Mapped", menuName = "MFPS/Input/Input Mapped")]
    public class ButtonMapped : ScriptableObject
    {
        public MFPSInputSource inputType = MFPSInputSource.Keyboard;
        public Mapped mapped = new Mapped();
        public List<ButtonData> ButtonMap { get { return mapped.ButtonMap; } }

        public Dictionary<string, ButtonData> ButtonMapDictionary;

        /// <summary>
        /// 
        /// </summary>
        public void Init()
        {
            ButtonMapDictionary = new Dictionary<string, ButtonData>();
            foreach (ButtonData item in ButtonMap)
            {
                ButtonMapDictionary.Add(item.KeyName, item);
            }
        }
    }
}