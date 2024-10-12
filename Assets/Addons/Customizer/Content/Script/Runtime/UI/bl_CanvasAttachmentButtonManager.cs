using UnityEngine;

namespace MFPS.Addon.Customizer
{
    public class bl_CanvasAttachmentButtonManager : MonoBehaviour
    {
        [SerializeField] private Camera renderCamera = null;
        [SerializeField] private GameObject buttonPrefab = null;
        [SerializeField] private RectTransform buttonPanel = null;

        private bl_CanvasAttachmentButton[] buttons;
        private bl_Customizer currentWeapon;
        private bl_AttachmentsButtons attachmentsButtons;
        private bool isInit = false;

        /// <summary>
        /// 
        /// </summary>
        public void Init()
        {
            if (isInit) return;
            isInit = true;

            buttons = new bl_CanvasAttachmentButton[5];
            for (int i = 0; i < 5; i++)
            {
                var go = Instantiate(buttonPrefab) as GameObject;
                go.transform.SetParent(buttonPanel, false);
                buttons[i] = go.GetComponent<bl_CanvasAttachmentButton>();
                buttons[i].Setup(currentWeapon, (bl_AttachType)i);
                go.SetActive(false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="weapon"></param>
        public void Setup(bl_Customizer weapon, bl_AttachmentsButtons renderButtons)
        {
            if (weapon == null) return;

            Init();
            currentWeapon = weapon;
            attachmentsButtons = renderButtons;
            foreach (var item in buttons)
            {
                item.CurrentWeapon = weapon;
            }
            buttons[0].SetActive(weapon.Positions.BarrelPosition != null);
            buttons[1].SetActive(weapon.Positions.OpticPosition != null);
            buttons[2].SetActive(weapon.Positions.FeederPosition != null);
            buttons[3].SetActive(weapon.Positions.CylinderPosition != null);
            buttons[4].SetActive(renderButtons.CamoButton != null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public void SetActiveAll(bool active)
        {
            buttonPanel.gameObject.SetActive(active);
        }

        /// <summary>
        /// 
        /// </summary>
        private void Update()
        {
            MoveButtons();
        }

        /// <summary>
        /// 
        /// </summary>
        void MoveButtons()
        {
            if (currentWeapon == null) { return; }

            FollowObject(buttons[0], currentWeapon.Positions.BarrelPosition);
            FollowObject(buttons[1], currentWeapon.Positions.OpticPosition);
            FollowObject(buttons[2], currentWeapon.Positions.FeederPosition);
            FollowObject(buttons[3], currentWeapon.Positions.CylinderPosition);
            if (attachmentsButtons.CamoButton != null) FollowObject(buttons[4], attachmentsButtons.CamoButton.transform);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="button"></param>
        /// <param name="target"></param>
        private void FollowObject(bl_CanvasAttachmentButton button, Transform target)
        {
            if (!button.gameObject.activeSelf || target == null) return;

            button.transform.position = renderCamera.WorldToScreenPoint(target.position);
        }

        private static bl_CanvasAttachmentButtonManager _instance = null;
        public static bl_CanvasAttachmentButtonManager Instance
        {
            get
            {
                if (_instance == null) { _instance = FindObjectOfType<bl_CanvasAttachmentButtonManager>(); }
                return _instance;
            }
        }
    }
}