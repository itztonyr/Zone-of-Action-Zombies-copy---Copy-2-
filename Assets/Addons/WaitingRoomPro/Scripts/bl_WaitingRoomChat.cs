using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using HashTable = ExitGames.Client.Photon.Hashtable;

public class bl_WaitingRoomChat : bl_PhotonHelper
{
    public TextMeshProUGUI ChatText;
    public TMP_InputField chatInput;

    static readonly RaiseEventOptions EventsAll = new();
    private string team1Color, team2Color;

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        ChatText.text = string.Empty;
        EventsAll.Receivers = ReceiverGroup.All;
        PhotonNetwork.NetworkingClient.EventReceived += OnEventCustom;
        if (bl_GameData.isDataCached)
        {
            team1Color = MFPSTeam.Get(Team.Team1).GetColorString();
            team2Color = MFPSTeam.Get(Team.Team2).GetColorString();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEventCustom;
    }

    /// <summary>
    /// From Server
    /// </summary>
    public void OnChatReceive(HashTable data)
    {
        string msg = (string)data["chat"];
        Player sender = (Player)data["player"];
        string txt;
        if (isOneTeamModeUpdate)
        {
            txt = string.Format("{0}: {1}", sender.NickNameAndRole(), msg);
        }
        else
        {
            string tc = sender.GetPlayerTeam() == Team.Team1 ? team1Color : team2Color;
            txt = string.Format("<color=#{2}>{0}</color>: {1}", sender.NickNameAndRole(), msg, tc);
        }
        AddChat(txt);
    }

    /// <summary>
    /// 
    /// </summary>
    public void SendMessage(TMP_InputField field)
    {
        if (!bl_PhotonNetwork.InRoom) return;

        string str = field.text;
        if (string.IsNullOrEmpty(str)) return;

        if (bl_GameData.CoreSettings.filterProfanityWords)
        {
            if (bl_StringUtility.ContainsProfanity(str, out string filterText))
            {
                str = filterText;
            }
        }

        var table = new HashTable
        {
            { "chat", str },
            { "player", bl_PhotonNetwork.LocalPlayer }
        };
        PhotonNetwork.RaiseEvent(PropertiesKeys.ChatEvent, table, EventsAll, SendOptions.SendUnreliable);
        field.text = string.Empty;
    }

    /// <summary>
    /// 
    /// </summary>
    public void AddChat(string txt)
    {
        ChatText.text += "\n" + txt;
    }

    /// <summary>
    /// 
    /// </summary>
    private void Update()
    {
        if (chatInput == null) return;
        if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter))
        {
            if (chatInput.isFocused || !string.IsNullOrEmpty(chatInput.text))
            {
                SendMessage(chatInput);
            }
        }
    }

    /// <summary>
    /// RaiseEvent = RPC, I just used this cuz I like it more :)
    /// </summary>
    public void OnEventCustom(EventData data)
    {
        HashTable t = (HashTable)data.CustomData;
        switch (data.Code)
        {
            case PropertiesKeys.ChatEvent:
                OnChatReceive(t);
                break;
        }
    }
}