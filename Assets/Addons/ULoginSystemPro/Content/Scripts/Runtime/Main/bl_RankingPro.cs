using MFPS.ULogin;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class bl_RankingPro : MonoBehaviour
{

    [Serializable]
    public class Players
    {
        public Player[] players;

        [Serializable]
        public class Player
        {
            public string name;
            public string nick;
            public int kills;
            public int deaths;
            public int score;
            public int status;
            public int clan;

            public LoginUserInfo GetLoginUserInfo()
            {
                var user = new LoginUserInfo()
                {
                    LoginName = name,
                    NickName = nick,
                    Kills = kills,
                    Deaths = deaths,
                    Score = score,
                    Role = status
                };
                return user;
            }
        }
    }

    [Header("Settings")]
    public bool FetchOnStart = true;
    [SerializeField] private int Top = 100;
    [Header("References")]
    public GameObject Content;
    [SerializeField] private GameObject RankingUIPrefab = null;
    [SerializeField] private Transform RankingPanel = null;

    private Players players;
    private bl_LoginProDataBase LoginDataBase;
    private readonly List<GameObject> currentList = new();
    private bool Requesting = false;
    private bool firstFetchComplete = false;

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        LoginDataBase = bl_LoginProDataBase.Instance;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        if (FetchOnStart && !firstFetchComplete)
        {
            StartCoroutine(GetRanking());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void Open()
    {
        if (Content != null) Content.SetActive(true);
        StartCoroutine(GetRanking());
    }

    /// <summary>
    /// 
    /// </summary>
    public void Refresh()
    {
        if (Requesting)
            return;

        Clean();
        StartCoroutine(GetRanking());
    }

    /// <summary>
    /// 
    /// </summary>
    IEnumerator GetRanking()
    {
        Requesting = true;
        Dictionary<string, string> wf = new()
        {
            { "top", Top.ToString() }
        };

        UnityWebRequest w = UnityWebRequest.Post(bl_LoginProDataBase.GetUrl("ranking"), wf);
        yield return w.SendWebRequest();

        firstFetchComplete = true;
        if (!bl_DataBaseUtils.IsNetworkError(w))
        {
            string result = w.downloadHandler.text;
            if (w.responseCode == 200)
            {
                players = JsonUtility.FromJson<Players>(result);
                if (players != null)
                {
                    var alluser = new List<LoginUserInfo>();
                    for (int i = 0; i < players.players.Length; i++)
                    {
                        alluser.Add(players.players[i].GetLoginUserInfo());
                    }
                    InstanceUI(alluser);
                }
                else
                {
                    Debug.LogWarning("Unexpected response: " + result);
                }
            }
            else if (w.responseCode == 204)
            {
                Debug.Log("Not player registered yet.");
            }
        }
        else
        {
            Debug.LogError(w.error);
        }
        w.Dispose();
        Requesting = false;
    }

    /// <summary>
    /// 
    /// </summary>
    public void InstanceUI(List<LoginUserInfo> users)
    {
        Clean();
        GameObject g;
        for (int i = 0; i < users.Count; i++)
        {
            g = Instantiate(RankingUIPrefab) as GameObject;
            g.GetComponent<bl_RankingUIPro>().SetInfo(users[i], i + 1);
            g.transform.SetParent(RankingPanel, false);
            currentList.Add(g);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void Clean()
    {
        for (int i = 0; i < currentList.Count; i++)
        {
            Destroy(currentList[i]);
        }
        currentList.Clear();
    }
}