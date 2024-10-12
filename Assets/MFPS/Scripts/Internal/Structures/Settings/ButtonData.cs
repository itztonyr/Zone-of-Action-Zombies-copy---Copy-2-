using System;
using UnityEngine;

namespace MFPS.InputManager
{
    [Serializable]
    public class ButtonData
    {
        public string KeyName;
        [TextArea(1, 2)] public string Description;
        [KeyFinder] public KeyCode PrimaryKey = KeyCode.None;
        [KeyFinder] public KeyCode AlternativeKey = KeyCode.None;

        public string PrimaryAxis = "";
        public string AlternativeAxis = "";

        public bool PrimaryIsAxis = false;
        public bool AlternativeIsAxis = false;

        public float AxisValue = 1;

        private bool wasPressed = false;
        private int lastDownFrame = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsButtonDown()
        {
            bool isPressedThisFrame = !PrimaryIsAxis ? Input.GetKeyDown(PrimaryKey) : IsAxisTrue(PrimaryAxis);
            if (isPressedThisFrame)
            {
                if (wasPressed && lastDownFrame != Time.frameCount)
                {
                    isPressedThisFrame = false;
                }
                else
                {
                    wasPressed = true;
                }
            }
            else if (!isPressedThisFrame) { wasPressed = false; }
            lastDownFrame = Time.frameCount;

            if (isPressedThisFrame) return isPressedThisFrame;
            isPressedThisFrame = !AlternativeIsAxis ? Input.GetKeyDown(AlternativeKey) : IsAxisTrue(AlternativeAxis);

            return isPressedThisFrame;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsButton()
        {
            bool isTrue = !PrimaryIsAxis ? Input.GetKey(PrimaryKey) : IsAxisTrue(PrimaryAxis);
            if (isTrue) return isTrue;
            isTrue = !AlternativeIsAxis ? Input.GetKey(AlternativeKey) : IsAxisTrue(AlternativeAxis);
            return isTrue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsButtonUp()
        {
            bool isTrue = !PrimaryIsAxis ? Input.GetKeyUp(PrimaryKey) : IsAxisTrue(PrimaryAxis);
            if (isTrue) { wasPressed = false; return isTrue; }
            isTrue = !AlternativeIsAxis ? Input.GetKeyUp(AlternativeKey) : IsAxisTrue(AlternativeAxis);
            return isTrue;
        }

        private bool IsAxisTrue(string axisName)
        {
            if (string.IsNullOrEmpty(axisName)) return false;
            return Input.GetAxis(axisName) == AxisValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetInputName()
        {
            if (PrimaryIsAxis)
            {
                if (GamePadButtonsNames.OverrideAxisNames.ContainsKey(PrimaryAxis + AxisValue))
                {
                    return GamePadButtonsNames.OverrideAxisNames[PrimaryAxis + AxisValue];
                }
                return PrimaryAxis;
            }
            else return PrimaryKey.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetInputNameWithValues()
        {
            if (PrimaryIsAxis)
            {
                if (GamePadButtonsNames.OverrideAxisNames.ContainsKey(PrimaryAxis + AxisValue))
                {
                    return GamePadButtonsNames.OverrideAxisNames[PrimaryAxis + AxisValue];
                }
                return PrimaryAxis + AxisValue;
            }
            else return PrimaryKey.ToString();
        }
    }
}