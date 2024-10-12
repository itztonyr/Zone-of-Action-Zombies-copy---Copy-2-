using TMPro;
using UnityEngine;

namespace MFPS.ULogin
{
    public class bl_AdminUserListRow : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI idText = null;
        [SerializeField] private TextMeshProUGUI usernameText = null;
        [SerializeField] private TextMeshProUGUI killsText = null;
        [SerializeField] private TextMeshProUGUI scoreText = null;
        [SerializeField] private TextMeshProUGUI purchasesText = null;
        [SerializeField] private TextMeshProUGUI statusText = null;
        [SerializeField] private TextMeshProUGUI playtimeText = null;

        private bl_AdminUserList userList = null;
        private ULoginDatabaseUser userData;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        public void Set(ULoginDatabaseUser user, bl_AdminUserList listScript)
        {
            userList = listScript;
            userData = user;
            idText.text = user.id.ToString();
            usernameText.text = user.nick;
            killsText.text = user.kills.ToString();
            scoreText.text = user.score.ToString();
            purchasesText.text = user.GetPurchasesCount().ToString();
            statusText.text = user.GetRole().Role.RoleName;
            playtimeText.text = user.GetPlaytimeString();
        }

        /// <summary>
        /// 
        /// </summary>
        public void View()
        {
            userList.userPanel.CurrentUser = userData.ToLoginUserInfo();
            userList.userPanel.ShowUserInfo();
        }
    }
}