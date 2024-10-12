using UnityEngine;

namespace MFPS.Addon.LevelManager
{
    public class bl_LobbyLevelNotification : MonoBehaviour
    {

        [SerializeField] private bl_LevelRender levelRender = null;
        [SerializeField] private GameObject content = null;

        /// <summary>
        /// 
        /// </summary>
        private void Start()
        {
            content.SetActive(false);
            CheckLevelProgress();
        }

        /// <summary>
        /// 
        /// </summary>
        void CheckLevelProgress()
        {
            bl_LevelManager.Instance.Initialize();
            if (bl_LevelManager.Instance.isNewLevel)
            {
                var info = bl_LevelManager.Instance.GetLevel();
                levelRender.Render(info);
                bl_LevelManager.Instance.Refresh();
            }
            bl_LevelManager.Instance.GetInfo();
        }
    }
}