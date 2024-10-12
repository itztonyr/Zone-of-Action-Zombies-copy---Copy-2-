using MFPS.Internal;
using TMPro;
using UnityEngine;

namespace MFPS.ULogin
{
    public class bl_AdminUserList : bl_LoginProBase
    {
        public int usersPerPage = 10;

        public UIListHandler listHandler;
        public bl_AdminUserPanel userPanel = null;
        [SerializeField] private TextMeshProUGUI paginationText = null;

        private int currentPage = 1;
        private int pages = 0;
        private bool firstFetch = false;

        /// <summary>
        /// 
        /// </summary>
        private void Start()
        {
            listHandler.Initialize();
            if (!firstFetch)
            {
                GetUserList();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void GetUserList()
        {
            bl_AdminWindowManager.SetLoading(true);
            var wf = bl_DataBaseUtils.CreateWWWForm(FormHashParm.Name, true);
            wf.AddSecureField("type", DBCommands.ADMIN_GET_USERLIST);
            wf.AddSecureField("count", usersPerPage);
            wf.AddSecureField("page", currentPage);

            WebRequest.POST(bl_LoginProDataBase.GetUrl("admin"), wf, (result) =>
            {
                firstFetch = true;
                bl_AdminWindowManager.SetLoading(false);

                if (result.isError)
                {
                    result.PrintError();
                    return;
                }

                if (result.HTTPCode == 204)
                {
                    Debug.Log("No users found");
                    return;
                }

                try
                {
                    var user = result.FromJson<ULoginDatabaseUserList>();
                    if (user.users == null || user.users.Count == 0)
                    {
                        Debug.Log("No users found");
                        return;
                    }

                    ShowList(user);
                }
                catch
                {
                    result.Print(true);
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="users"></param>
        private void ShowList(ULoginDatabaseUserList users)
        {
            listHandler.Clear();
            for (int i = 0; i < users.users.Count; i++)
            {
                var script = listHandler.InstatiateAndGet<bl_AdminUserListRow>();
                script.Set(users.users[i], this);
            }

            pages = users.GetPagesCount(usersPerPage);
            paginationText.text = $"{currentPage}/{pages}";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="forward"></param>
        public void ChangePage(bool forward)
        {
            if (pages <= 0) return;

            if (forward)
            {
                currentPage++;
            }
            else if (currentPage > 1)
            {
                currentPage--;
            }
            else return;

            GetUserList();
        }
    }
}