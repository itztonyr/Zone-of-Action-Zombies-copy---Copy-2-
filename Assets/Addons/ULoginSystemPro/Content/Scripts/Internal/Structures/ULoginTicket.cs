using System;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.ULogin
{
    [Serializable]
    public class ULoginTicket
    {
        public int id;
        public int user_id;
        public string title;
        public string chat;
        public ULoginTicketStatus status;
        public string last_update;

        public ULoginTicketChat ChatData;

        public void Init()
        {
            try
            {
                ChatData = JsonUtility.FromJson<ULoginTicketChat>(chat);
            }
            catch
            {
                Debug.LogError("Ticket chat format is corrupted.");
                AddNewReply(0, "Corrupted Chat", chat);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void AddNewReply(int fromUserId, string fromNick, string text)
        {
            ChatData ??= new ULoginTicketChat() { chat = new List<ULoginTicketReply>() };
            ChatData.chat ??= new List<ULoginTicketReply>();
            ChatData.chat.Add(new ULoginTicketReply() { user_id = fromUserId, nick = fromNick, text = text });
            chat = JsonUtility.ToJson(ChatData);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetChatAsText(bool inverted = false)
        {
            Init();

            string chatText = "";
            var chat = ChatData.chat;
            if (inverted)
            {
                for (int i = chat.Count - 1; i >= 0; i--)
                {
                    var reply = chat[i];
                    chatText += string.Format("<b>{0}:</b> {1}\n\n", reply.nick, reply.text);
                }
            }
            else
            {
                for (int i = 0; i < chat.Count; i++)
                {
                    var reply = chat[i];
                    chatText += string.Format("<b>{0}:</b> {1}\n\n", reply.nick, reply.text);
                }
            }
            return chatText;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool IsLastReplyFromUser(int userId)
        {
            if (ChatData.chat.Count == 0) return false;
            return ChatData.chat[ChatData.chat.Count - 1].user_id == userId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetCreatorName()
        {
            if (ChatData.chat.Count == 0) return "Unknown";
            return ChatData.chat[0].nick;
        }
    }

    [Serializable]
    public class ULoginTicketList
    {
        public List<ULoginTicket> tickets;
    }

    [Serializable]
    public class ULoginTicketChat
    {
        public List<ULoginTicketReply> chat;
    }

    [Serializable]
    public class ULoginTicketReply
    {
        public int user_id;
        public string nick;
        public string text;
    }

    public enum ULoginTicketStatus
    {
        None = 0,
        Open = 1,
        Closed = 2,
        Pending = 3, // waiting for user reply
    }
}