using MFPS.Internal.Structures;
using MFPS.Runtime.UI;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MFPS.Addon.Customizer
{
    public class bl_AttachmentInfoButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI m_Text = null;
        [SerializeField] private RawImage CamoImg = null;
        public GameObject blockUI;
        public TextMeshProUGUI PriceText;
        private CanvasGroup Alpha;
        [SerializeField] private AudioClip InitAudio = null;
        [SerializeField] private bl_MFPSCoinPriceUI coinPricesUI = null;

        private int ID;
        private bl_AttachType m_Type;

        /// <summary>
        /// 
        /// </summary>
        public void Init(CustomizerModelInfo info, bl_AttachType typ, float d, bool selected)
        {
            Alpha = GetComponent<CanvasGroup>();
            Alpha.alpha = 0;
            m_Text.text = info.Name;
            ID = info.ID;
            m_Type = typ;
            StartCoroutine(Fade(d));
            GetComponent<Button>().interactable = !selected;
        }

        /// <summary>
        /// 
        /// </summary>
        public void InitCamo(CamoInfo info, float d, bool selected)
        {
            Alpha = GetComponent<CanvasGroup>();
            Alpha.alpha = 0;
            CamoImg.texture = info.Preview;
            ID = info.ID;
            StartCoroutine(Fade(d));

            GlobalCamo gc = bl_CustomizerData.Instance.GlobalCamos[info.GlobalID];
            if (!gc.Unlockability.IsUnlocked(info.GlobalID))
            {
                var reason = gc.Unlockability.GetLockReason(info.GlobalID);
                if (reason == MFPSItemUnlockability.LockReason.NoPurchased || reason == MFPSItemUnlockability.LockReason.NoPurchasedAndLevel)
                {
                    coinPricesUI.SetPrice(gc.Unlockability).SetActive(true);
                }
                else coinPricesUI.SetActive(false);

                if (reason == MFPSItemUnlockability.LockReason.Level || reason == MFPSItemUnlockability.LockReason.NoPurchasedAndLevel)
                {
                    PriceText.text = $"LEVEL {gc.Unlockability.UnlockAtLevel}";
                }

                GetComponentInChildren<Button>().interactable = false;
                blockUI.SetActive(true);
            }
            else
            {
                blockUI.SetActive(false);
                GetComponentInChildren<Button>().interactable = !selected;
            }
        }

        public void OnSelect()
        {
            bl_CustomizerManager c = FindObjectOfType<bl_CustomizerManager>();
            c.OnSelectAttachment(m_Type, ID);
            Button[] bt = transform.parent.GetComponentsInChildren<Button>();
            for (int i = 0; i < bt.Length; i++)
            {
                bt[i].interactable = true;
                bt[i].OnDeselect(null);
            }
            GetComponent<Button>().interactable = false;
        }

        public void OnSelectCamo()
        {
            bl_CustomizerManager c = FindObjectOfType<bl_CustomizerManager>();
            c.OnSelectCamo(ID);
            Button[] bt = transform.parent.GetComponentsInChildren<Button>();
            for (int i = 0; i < bt.Length; i++)
            {
                bt[i].interactable = true;
                bt[i].OnDeselect(null);
            }
            GetComponentInChildren<Button>().interactable = false;
        }

        IEnumerator Fade(float delay)
        {
            yield return new WaitForSeconds(delay);
            float d = 0;
            AudioSource.PlayClipAtPoint(InitAudio, Camera.main.transform.position);
            while (d < 1)
            {
                d += Time.deltaTime * 2;
                Alpha.alpha = d;
                yield return null;
            }
        }
    }
}