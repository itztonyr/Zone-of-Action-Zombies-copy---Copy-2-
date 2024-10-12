using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MFPS.ULogin
{
    public class bl_SupportTicketManager : bl_LoginProBase
    {
        [SerializeField] private GameObject TicketViewUI = null;
        [SerializeField] private GameObject TicketPrefab = null;
        [SerializeField] private Transform TicketsPanel = null;
        [SerializeField] private Text MessageText = null;
        [SerializeField] private Text TitleText = null;
        [SerializeField] private Text NameText = null;
        [SerializeField] private InputField ReplyInput = null;
        [SerializeField] private GameObject noTicketsUI = null;

        private bl_SupportTicket CurrentTicket;
        private List<GameObject> cachedList;

        /// <summary>
        /// 
        /// </summary>
        private void Start()
        {
            TicketViewUI.SetActive(false);
            GetAllTickets();
        }

        /// <summary>
        /// 
        /// </summary>
        private void GetAllTickets()
        {
            var wf = new WWWForm();
            wf.AddField("hash", bl_LoginProDataBase.GetAPIToken());
            wf.AddField("type", DBCommands.SUPPORT_GET_TICKETS);

            WebRequest.POST(bl_LoginProDataBase.GetUrl("support"), wf, (result) =>
            {
                if (result.isError)
                {
                    result.PrintError();
                    return;
                }

                result.Print();
                var tickets = result.FromJson<ULoginTicketList>();
                InstanceTickets(tickets);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ticket"></param>
        public void SelectTicket(bl_SupportTicket ticket)
        {
            CurrentTicket = ticket;
            MessageText.text = ticket.cacheInfo.GetChatAsText(true);
            TitleText.text = ticket.cacheInfo.title;
            NameText.text = ticket.cacheInfo.GetCreatorName();
            TicketViewUI.SetActive(true);

            // force updated the parent canvas
            TicketViewUI.GetComponentInParent<Canvas>().enabled = false;
            TicketViewUI.GetComponentInParent<Canvas>().enabled = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Reply()
        {
            string reply = ReplyInput.text;
            if (CurrentTicket == null || string.IsNullOrEmpty(reply))
                return;

            reply = SanitazeText(reply);

            int userId = bl_DataBase.IsUserLogged ? bl_DataBase.LocalUserInstance.ID : 0;
            string userNick = bl_DataBase.IsUserLogged ? bl_DataBase.LocalUserInstance.NickName : "Editor";

            var wf = new WWWForm();
            wf.AddField("hash", bl_LoginProDataBase.GetAPIToken());
            wf.AddField("id", CurrentTicket.cacheInfo.id);
            wf.AddField("reply", reply);
            wf.AddField("userId", userId);
            wf.AddField("userNick", userNick);
            wf.AddField("type", DBCommands.SUPPORT_REPLY_TICKET);

            WebRequest.POST(bl_LoginProDataBase.GetUrl("support"), wf, (result) =>
            {
                if (result.isError)
                {
                    result.PrintError();
                    return;
                }

                if (result.HTTPCode == 202)
                {
                    ReplyInput.text = string.Empty;
                    CurrentTicket.cacheInfo.AddNewReply(userId, userNick, reply);
                    MessageText.text = CurrentTicket.cacheInfo.GetChatAsText(true);
                }
                else { result.Print(true); }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        public void DeleteTicket()
        {
            if (CurrentTicket == null) return;

            bl_AdminWindowManager.ShowConfirmationWindow("Are you sure you want to delete this ticket?", () =>
            {
                var wf = new WWWForm();
                wf.AddField("hash", bl_LoginProDataBase.GetAPIToken());
                wf.AddField("id", CurrentTicket.cacheInfo.id);
                wf.AddField("type", DBCommands.SUPPORT_DELETE_TICKET);

                WebRequest.POST(bl_LoginProDataBase.GetUrl("support"), wf, (result) =>
                {
                    if (result.isError)
                    {
                        result.PrintError();
                        return;
                    }

                    if (result.HTTPCode == 202)
                    {
                        Destroy(CurrentTicket.gameObject);
                        TicketViewUI.SetActive(false);
                        CleanList();
                        GetAllTickets();
                    }
                    else { result.Print(true); }
                });
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string SanitazeText(string input)
        {
            input = bl_DataBaseUtils.SanitazeString(input);
            return input;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        void InstanceTickets(ULoginTicketList tickets)
        {
            if (tickets == null || tickets.tickets == null)
            {
                TicketPrefab.SetActive(false);
                noTicketsUI.SetActive(true);
                return;
            }

            noTicketsUI.SetActive(false);
            cachedList = new List<GameObject>();
            var list = tickets.tickets;
            for (int i = 0; i < list.Count; i++)
            {
                GameObject g = Instantiate(TicketPrefab) as GameObject;
                g.transform.SetParent(TicketsPanel, false);
                g.GetComponent<bl_SupportTicket>().GetInfo(list[i], this);
                g.SetActive(true);
                cachedList.Add(g);
            }
            TicketPrefab.SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        public void CleanList()
        {
            if (cachedList == null || cachedList.Count == 0) return;

            for (int i = 0; i < cachedList.Count; i++)
            {
                Destroy(cachedList[i]);
            }
            cachedList.Clear();
        }
    }
}