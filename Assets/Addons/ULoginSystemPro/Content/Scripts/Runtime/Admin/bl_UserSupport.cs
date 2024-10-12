using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MFPS.ULogin
{
    public class bl_UserSupport : bl_LoginProBase
    {
        public float WindowSize = 141;
        [Range(1, 100)] public float ShowSpeed = 100;
        public MonoBehaviour[] disableOnOpen;

        [Header("References")]
        [SerializeField] private GameObject LoginBlock = null;
        [SerializeField] private GameObject ReplyWindow = null;
        [SerializeField] private TMP_InputField TitleInput = null;
        [SerializeField] private TMP_InputField ContentInput = null;
        [SerializeField] private Button SummitButton = null;
        [SerializeField] private Button CloseButton = null;
        [SerializeField] private TextMeshProUGUI MessageText = null;
        [SerializeField] private TextMeshProUGUI ReplyText = null;
        [SerializeField] private GameObject Loading = null;
        [SerializeField] private RectTransform WindowTransform = null;
        [SerializeField] private GameObject[] awaitingReplyUI = null;
        [SerializeField] private GameObject[] sendReplyUI = null;

        private bl_LoginPro LoginPro;
        private bool sending = false;
        private bool ShowWindow = false;
        private ULoginTicket ticket;

        /// <summary>
        /// 
        /// </summary>
        private void Awake()
        {
            LoginPro = FindObjectOfType<bl_LoginPro>();
            LoginBlock.SetActive(true);
            ReplyWindow.SetActive(false);
            Loading.SetActive(false);
            SummitButton.interactable = false;
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnLogin()
        {
            LoginBlock.SetActive(false);
            ticket = null;
            MessageText.text = string.Empty;
            TitleInput.text = string.Empty;
            ContentInput.text = string.Empty;
            ReplyWindow.SetActive(false);

            CheckPersonalTicket();
        }

        /// <summary>
        /// 
        /// </summary>
        void CheckPersonalTicket()
        {
            var wf = new WWWForm();
            wf.AddField("type", DBCommands.SUPPORT_CHANGE_TICKET);
            wf.AddField("hash", bl_LoginProDataBase.GetAPIToken());
            wf.AddField("userId", DataBase.LocalUser.ID);

            Loading.SetActive(true);
            WebRequest.POST(GetURL("support"), wf, (result) =>
            {
                Loading.SetActive(false);
                if (result.isError)
                {
                    result.PrintError();
                    return;
                }

                if (result.HTTPCode == 201)
                {
                    try
                    {
                        result.Print();
                        ticket = result.FromJson<ULoginTicket>();
                        if (ticket != null)
                        {
                            ticket.Init();
                            MessageText.text = ticket.GetChatAsText();
                            if (ticket.IsLastReplyFromUser(DataBase.LocalUser.ID))
                            {
                                ReplyText.text = "Awaiting for reply...";
                                SetActiveWaitingUI(true);
                            }
                            else
                            {
                                CloseButton.interactable = true;
                                ReplyText.text = "Replied here...";
                                SetActiveWaitingUI(false);
                            }

                            ReplyWindow.SetActive(true);
                        }
                    }
                    catch
                    {
                        result.Print(true);
                    }
                }
                else if (result.HTTPCode == 204)
                {
                    // when there's no ticket from this user
                    ReplyWindow.SetActive(false);
                    if (ULoginSettings.FullLogs) { Debug.Log("No ticket found for this user."); }
                }
                else
                {
                    result.Print(true);
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        void SubmitTicket()
        {
            // we sanitize the input to avoid any kind of injection, we do it in the server side too.
            string title = bl_DataBaseUtils.SanitazeString(TitleInput.text);
            string content = bl_DataBaseUtils.SanitazeString(ContentInput.text);

            if (ticket == null || string.IsNullOrEmpty(ticket.title))
            {
                ticket = new ULoginTicket()
                {
                    user_id = DataBase.LocalUser.ID,
                    title = title,
                    status = ULoginTicketStatus.Open,
                };
            }

            ticket.AddNewReply(DataBase.LocalUser.ID, DataBase.LocalUser.NickName, content);

            var wf = new WWWForm();
            wf.AddField("type", DBCommands.SUPPORT_SUBMIT_TICKET);
            wf.AddField("hash", bl_LoginProDataBase.GetAPIToken());
            wf.AddField("userId", DataBase.LocalUser.ID);
            wf.AddField("title", TitleInput.text);
            wf.AddField("chat", JsonUtility.ToJson(ticket.ChatData));

            Loading.SetActive(true);
            WebRequest.POST(GetURL("support"), wf, (result) =>
            {
                Loading.SetActive(false);
                if (result.isError)
                {
                    result.PrintError();
                    return;
                }

                if (result.HTTPCode == 202)
                {
                    LoginPro.SetLogText("Ticket Submitted");
                    MessageText.text = ticket.GetChatAsText();
                    ReplyText.text = "AWAITING FOR REPLY...";
                    ReplyWindow.SetActive(true);
                }
                else
                {
                    result.Print(true);
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        public void SubmitReply(TMP_InputField input)
        {
            string reply = input.text;

            if (ticket == null || string.IsNullOrEmpty(reply))
                return;

            reply = bl_DataBaseUtils.SanitazeString(reply);

            var wf = new WWWForm();
            wf.AddField("hash", bl_LoginProDataBase.GetAPIToken());
            wf.AddField("id", ticket.id);
            wf.AddField("reply", reply);
            wf.AddField("userId", DataBase.LocalUser.ID);
            wf.AddField("userNick", DataBase.LocalUser.NickName);
            wf.AddField("isUser", "1");
            wf.AddField("type", DBCommands.SUPPORT_REPLY_TICKET);

            Loading.SetActive(true);
            WebRequest.POST(GetURL("support"), wf, (result) =>
            {
                Loading.SetActive(false);
                if (result.isError)
                {
                    result.PrintError();
                    return;
                }

                if (result.HTTPCode == 202)
                {
                    input.text = string.Empty;
                    ticket.AddNewReply(DataBase.LocalUser.ID, DataBase.LocalUser.NickName, reply);
                    MessageText.text = ticket.GetChatAsText(true);
                    SetActiveWaitingUI(true);
                }
                else
                {
                    result.Print(true);
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        public void CloseTicket()
        {
            CloseButton.interactable = false;

            var wf = new WWWForm();
            wf.AddField("hash", bl_LoginProDataBase.GetAPIToken());
            wf.AddField("id", ticket.id);
            wf.AddField("status", 2);
            wf.AddField("type", DBCommands.SUPPORT_CLOSE_TICKET);

            Loading.SetActive(true);
            WebRequest.POST(GetURL("support"), wf, (result) =>
            {
                Loading.SetActive(false);
                if (result.isError)
                {
                    result.PrintError();
                    return;
                }

                if (result.HTTPCode == 202)
                {
                    ticket.status = ULoginTicketStatus.Closed;
                    ReplyText.text = "CLOSED";
                    CloseButton.interactable = true;
                    ReplyWindow.SetActive(false);
                }
                else
                {
                    result.Print(true);
                }
            });
        }

        public void OnLogOut()
        {
            LoginBlock.SetActive(true);
        }

        public void Send()
        {
            if (sending || DataBase == null || !DataBase.isLogged)
                return;
            if (string.IsNullOrEmpty(TitleInput.text) || string.IsNullOrEmpty(ContentInput.text))
                return;

            SubmitTicket();
        }

        public void CheckTexts()
        {
            SummitButton.interactable = (TitleInput.text.Length > 2 && ContentInput.text.Length > 7);
        }

        public void Show()
        {
            ShowWindow = !ShowWindow;
            StopCoroutine(nameof(ShowWindowIE));
            StartCoroutine(nameof(ShowWindowIE), ShowWindow);

            foreach (var item in disableOnOpen)
            {
                if (item == null) continue;
                item.enabled = !ShowWindow;
            }
        }

        IEnumerator ShowWindowIE(bool show)
        {
            Vector2 v = WindowTransform.anchoredPosition;
            if (show)
            {
                while (v.x > 0)
                {
                    v.x -= Time.deltaTime * (ShowSpeed * 10);
                    WindowTransform.anchoredPosition = v;
                    yield return null;
                }
                v.x = 0;
                WindowTransform.anchoredPosition = v;
            }
            else
            {
                while (v.x < WindowSize)
                {
                    v.x += Time.deltaTime * (ShowSpeed * 10);
                    WindowTransform.anchoredPosition = v;
                    yield return null;
                }
                v.x = WindowSize;
                WindowTransform.anchoredPosition = v;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        private void SetActiveWaitingUI(bool active)
        {
            foreach (var item in awaitingReplyUI)
            {
                if (item == null) continue;
                item.SetActive(active);
            }
            foreach (var item in sendReplyUI)
            {
                if (item == null) continue;
                item.SetActive(!active);
            }
        }
    }
}