using MFPS.Internal.Structures;
using Photon.Realtime;

/// <summary>
/// use to get all room properties easily
/// usage:  RoomProperties props = PhotonNetwork.CurrentRoom.GetRoomInfo();
/// </summary>
public class MFPSRoomInfo
{
    public string roomName { get; set; }
    public int sceneId { get; set; }
    public string password { get; set; }
    public GameMode gameMode { get; set; }
    public int goal { get; set; }
    public int time { get; set; }
    public int maxPing { get; set; }
    public Room room { get; set; }
    public int maxPlayers { get; set; }
    public bool friendlyFire { get; set; }
    public bool withBots { get; set; }
    public bool autoTeamSelection { get; set; }
    public RoundStyle roundStyle { get; set; }
    public byte WeaponOption { get; set; }

    public bool isPrivate { get { return !string.IsNullOrEmpty(password); } }

    public MFPSRoomInfo() { }

    public MFPSRoomInfo(Room roomTarget)
    {
        room = roomTarget;
        roomName = room.Name;
        sceneId = (int)roomTarget.CustomProperties[PropertiesKeys.RoomSceneID];
        password = (string)roomTarget.CustomProperties[PropertiesKeys.RoomPassword];
        gameMode = (GameMode)room.CustomProperties[PropertiesKeys.GameModeKey];
        time = (int)room.CustomProperties[PropertiesKeys.TimeRoomKey];
        goal = (int)room.CustomProperties[PropertiesKeys.RoomGoal];
        maxPing = (int)room.CustomProperties[PropertiesKeys.MaxPing];
        maxPlayers = room.MaxPlayers;
        friendlyFire = (bool)room.CustomProperties[PropertiesKeys.RoomFriendlyFire];
        withBots = (bool)room.CustomProperties[PropertiesKeys.WithBotsKey];
        autoTeamSelection = (bool)room.CustomProperties[PropertiesKeys.TeamSelectionKey];
        roundStyle = (RoundStyle)room.CustomProperties[PropertiesKeys.RoomRoundKey];
        WeaponOption = (byte)room.CustomProperties[PropertiesKeys.RoomWeaponOption];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public MapInfo GetMapInfo()
    {
        return bl_GameData.Instance.AllScenes[sceneId];
    }
}