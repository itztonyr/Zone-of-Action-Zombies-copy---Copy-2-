public class bl_GameModeObject : bl_PhotonHelper
{
    public GameMode m_GameMode = GameMode.FFA;

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        if (!bl_PhotonNetwork.IsConnected || !bl_PhotonNetwork.InRoom)
        {
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(GetGameMode == m_GameMode);
    }
}