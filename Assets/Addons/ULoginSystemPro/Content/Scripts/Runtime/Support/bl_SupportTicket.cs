using UnityEngine;
using UnityEngine.UI;

namespace MFPS.ULogin
{
    public class bl_SupportTicket : MonoBehaviour
    {
        [SerializeField] private Text TitleText = null;
        [SerializeField] private Text statusText = null;

        public ULoginTicket cacheInfo { get; set; }
        private bl_SupportTicketManager Manager;

        public void GetInfo(ULoginTicket info, bl_SupportTicketManager ma)
        {
            Manager = ma;
            cacheInfo = info;
            TitleText.text = info.title;
            statusText.text = $"[{info.status.ToString().ToUpper()}]";
        }

        public void Select()
        {
            Manager.SelectTicket(this);
        }

        [System.Serializable]
        public class Ticket
        {
            public string Title;
            public string Message;
            public string Reply;
            public bool isClose;
            public string User;
            public int ID;
        }
    }
}