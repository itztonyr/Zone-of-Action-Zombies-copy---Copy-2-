using MFPS.Internal;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MFPS.ULogin
{
    public class bl_AdminRoleListUI : MonoBehaviour
    {
        [SerializeField] private GameObject content = null;
        public UIListHandler listHandler;
        private Dictionary<int, Button> cachedButtons = new Dictionary<int, Button>();
        
        /// <summary>
        /// 
        /// </summary>
        public void Init()
        {
            listHandler.Initialize();
            InstanceButtons();
        }

        /// <summary>
        /// 
        /// </summary>
        public void InstanceButtons()
        {
            if (listHandler.Count > 0) return;

            var roles = bl_LoginProDataBase.Instance.roles;
            for (int i = 0; i < roles.Count; i++)
            {
                if (roles[i].RoleKey == "banned") continue;
                
                int rid = i;
                var script = listHandler.InstatiateAndGet<Button>();
                script.GetComponentInChildren<Text>().text = roles[i].RoleName.ToUpper();
                script.onClick.AddListener(() => OnRoleClick(rid));
                cachedButtons.Add(i, script);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public void OnRoleClick(int roleId)
        {
            bl_AdminUserPanel.Instance.ChangeStatus(roleId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleId"></param>
        public void SetInteractable(int roleId)
        {
            foreach (var item in cachedButtons.Values)
            {
                item.interactable = true;
            }
            cachedButtons[roleId].interactable = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public void SetActive(bool active)
        {
            content.SetActive(active);
        }
    }
}