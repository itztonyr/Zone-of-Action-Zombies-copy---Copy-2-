using TMPro;
using UnityEngine;

namespace MFPS.ULogin
{
    public class bl_RankingUIPro : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Color AdminColor;
        [SerializeField] private Color ModColor;
        [Header("References")]
        [SerializeField] private TextMeshProUGUI RankText = null;
        [SerializeField] private TextMeshProUGUI PlayerNameText = null;
        [SerializeField] private TextMeshProUGUI ScoreText = null;
        [SerializeField] private TextMeshProUGUI KillsText = null;
        [SerializeField] private TextMeshProUGUI DeathsText = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="rank"></param>
        public void SetInfo(LoginUserInfo info, int rank)
        {
            RankText.text = rank.ToString();
            PlayerNameText.text = info.NickName;
            ScoreText.text = info.Score.ToString();
            KillsText.text = info.Kills.ToString();
            DeathsText.text = info.Deaths.ToString();
            CheckStatus(info);
        }

        /// <summary>
        /// 
        /// </summary>
        void CheckStatus(LoginUserInfo account)
        {
            if (!bl_LoginProDataBase.Instance.showStatusPrefix) return;

            if (account.Role.Role.InsertRoleInNickName)
            {
                PlayerNameText.text = $"{account.Role.Role.GetRolePrefix()} {PlayerNameText.text}";
            }
        }
    }
}