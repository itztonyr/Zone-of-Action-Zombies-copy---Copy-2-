using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MFPS.Addon.Customizer
{
    public class bl_CanvasAttachmentButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private AudioClip OnEnterSound = null;
        private AudioSource ASource;
        private Vector3 CurrentScale = Vector3.one;
        public bl_AttachType AttachType { get; set; }
        public bl_Customizer CurrentWeapon { get; set; }

        /// <summary>
        /// 
        /// </summary>
        void Awake()
        {
            ASource = GetComponent<AudioSource>();
        }

        /// <summary>
        /// 
        /// </summary>
        void OnEnable()
        {
            StartCoroutine(OnUpdate());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="weapon"></param>
        /// <param name="type"></param>
        public void Setup(bl_Customizer weapon, bl_AttachType type)
        {
            CurrentWeapon = weapon;
            AttachType = type;
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnClick()
        {
            ASource.clip = OnEnterSound;
            ASource.Play();

            var Manager = bl_CustomizerManager.Instance;
            var Attachments = CurrentWeapon.Attachments;

            if (AttachType == bl_AttachType.Suppressers)
            {
                Manager.ChangeAttachWindow(Attachments.Suppressers, bl_AttachType.Suppressers);
            }
            else
                   if (AttachType == bl_AttachType.Sights)
            {
                Manager.ChangeAttachWindow(Attachments.Sights, bl_AttachType.Sights);
            }
            else
                   if (AttachType == bl_AttachType.Foregrips)
            {
                Manager.ChangeAttachWindow(Attachments.Foregrips, bl_AttachType.Foregrips);
            }
            else
                   if (AttachType == bl_AttachType.Magazine)
            {
                Manager.ChangeAttachWindow(Attachments.Magazines, bl_AttachType.Magazine);
            }
            else if (AttachType == bl_AttachType.Camo)
            {
                Manager.ShowCamos(CurrentWeapon.WeaponName);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            ASource.clip = OnEnterSound;
            ASource.Play();
            CurrentScale = Vector3.one * 2;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerExit(PointerEventData eventData)
        {
            CurrentScale = Vector3.one;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator OnUpdate()
        {
            while (true)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, CurrentScale, Time.deltaTime * 15);
                yield return null;
            }
        }
    }
}