using UnityEngine;
using UnityEngine.UI;

namespace MFPS.Addon.Customizer
{
    public class bl_CustomizerUI : MonoBehaviour
    {
        [SerializeField] private Button[] topButtons = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        public void OnSelectedTopButton(int index)
        {
            for (int i = 0; i < topButtons.Length; i++)
            {
                topButtons[i].interactable = i != index;
            }
        }
    }
}