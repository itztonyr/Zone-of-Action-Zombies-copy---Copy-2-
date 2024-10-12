using UnityEngine;
using MFPS.Runtime.FriendList;

public class bl_WaitingPlayerUIPro : MonoBehaviour
{
    public GameObject AddFriendButton;
    private bl_WaitingPlayerUIBase WaitingUI;

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        WaitingUI = GetComponent<bl_WaitingPlayerUIBase>();
        AddFriendButton.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    private void Start()
    {
        bool isf = bl_FriendListBase.Instance.IsPlayerFriend(WaitingUI.GetPlayer().NickName);
        AddFriendButton.SetActive(bl_FriendListBase.Instance.CanAddMoreFriends() && !WaitingUI.GetPlayer().IsLocal && !isf);
    }

    /// <summary>
    /// 
    /// </summary>
    public void AddFriend()
    {
        string playerName = WaitingUI.GetPlayer().NickName;
        bl_FriendListBase.Instance.AddFriend(playerName);
    }
}