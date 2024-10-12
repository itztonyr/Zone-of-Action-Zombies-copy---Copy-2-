using System;
using System.Collections.Generic;
using UnityEngine;
#if ACTK_IS_HERE
using CodeStage.AntiCheat.ObscuredTypes;
#endif

namespace MFPS.ULogin
{
    /// <summary>
    /// Class containing all the data of a player account
    /// </summary>
    [Serializable]
    public class LoginUserInfo
    {
        /// <summary>
        /// This is the Database unique player ID
        /// Use to identify the player on the database
        /// </summary>
        public int ID = 0;

#if !ACTK_IS_HERE
        /// <summary>
        /// The username with which the player login
        /// Only used to login
        /// </summary>
        public string LoginName;

        /// <summary>
        /// The player nickname
        /// This is the one that other players can see
        /// </summary>
        public string NickName;

        /// <summary>
        /// Player all time kills count
        /// </summary>
        public int Kills = 0;

        /// <summary>
        /// Player all time deaths count
        /// </summary>
        public int Deaths = 0;

        /// <summary>
        /// Player all time score
        /// </summary>
        public int Score = 0;

        /// <summary>
        /// Player all time assists count
        /// </summary>
        public int Assist = 0;

        /// <summary>
        /// Player saved coins
        /// </summary>
        public int[] Coins;

        /// <summary>
        /// Total time played 
        /// </summary>
        public int PlayTime = 0;
#else
        /// <summary>
        /// The username with which the player login
        /// Only used to login
        /// </summary>
        public ObscuredString LoginName;

        /// <summary>
        /// The player nickname
        /// This is the one that other players can see
        /// </summary>
        public ObscuredString NickName;

        /// <summary>
        /// Player all time kills count
        /// </summary>
        public ObscuredInt Kills = 0;

        /// <summary>
        /// Player all time deaths count
        /// </summary>
        public ObscuredInt Deaths = 0;

        /// <summary>
        /// Player all time score
        /// </summary>
        public ObscuredInt Score = 0;

        /// <summary>
        /// Player all time assists count
        /// </summary>
        public ObscuredInt Assist = 0;

        /// <summary>
        /// Player saved coins
        /// </summary>
        public ObscuredInt[] Coins;

        /// <summary>
        /// Total time played 
        /// </summary>
        public ObscuredInt PlayTime = 0;
#endif

        /// <summary>
        /// Last player detected IP
        /// </summary>
        public string IP;

        /// <summary>
        /// The IP that is registered in the database for this player
        /// </summary>
        public string SavedIP;

        /// <summary>
        /// Player account meta data
        /// </summary>
        public bl_UserMetaData metaData;

        /// <summary>
        /// Player's friend list
        /// </summary>
        public List<string> FriendList = new List<string>();

        /// <summary>
        /// Account role
        /// </summary>
        public ULoginAccountRoleRef Role;

        /// <summary>
        /// The date of the registration of this player account
        /// </summary>
        public string UserDate => rawData["user_date"];

        /// <summary>
        /// Raw data from the server
        /// This data is not updated during the game and is only assigned when the player login
        /// </summary>
        public Dictionary<string, string> rawData = new Dictionary<string, string>();

        /// <summary>
        /// The authentication method that this player used to sign in on this session
        /// </summary>
        public AuthenticationType authenticationType = AuthenticationType.ULogin;

#if CLANS
        /// <summary>
        /// Player clan info
        /// </summary>
        public Addon.Clan.bl_ClanInfo Clan = null;
#endif

#if SHOP
        /// <summary>
        /// Player shop data
        /// </summary>
        public Shop.bl_ShopUserData ShopData = null;
#endif

        /// <summary>
        /// Add a friend (LOCALLY)
        /// </summary>
        public void SetFriends(string line)
        {
            if (!string.IsNullOrEmpty(line))
            {
                FriendList.Clear();
                string[] splitFriends = line.Split('/');
                FriendList.AddRange(splitFriends);
            }
        }

        /// <summary>
        /// Set to overwrite a coin value
        /// </summary>
        /// <param name="coinID">the coin ID/Index</param>
        /// <param name="cointAmount">The value that will overwrite the current value.</param>
        public void SetCoinLocally(int coinID, int cointAmount)
        {
            Coins[coinID] = cointAmount;
        }

        /// <summary>
        /// 
        /// </summary>
        public void ParseFullData(Dictionary<string, string> data)
        {
            rawData = data;
            LoginName = data["name"];
            NickName = data["nick"];
            Kills = int.Parse(data["kills"]);
            Deaths = int.Parse(data["deaths"]);
            Score = int.Parse(data["score"]);
            if (data.ContainsKey("assist")) Assist = int.Parse(data["assist"]);
            PlayTime = int.Parse(data["playtime"]);
            Role = int.Parse(data["status"]);
            ID = int.Parse(data["id"]);//unique identifier of the player in database
            SetFriends(data["friends"]);

            // Parse coins
            Coins = ParseCoins(data["coins"]);

            SavedIP = data["ip"];
#if SHOP
            ShopData = new Shop.bl_ShopUserData();
            ShopData.SetRawData(data);
#endif
            string meta = data["meta"];
            metaData = new bl_UserMetaData();
            if (!string.IsNullOrEmpty(meta))
            {
                metaData = JsonUtility.FromJson<bl_UserMetaData>(meta);
            }
        }

#if !ACTK_IS_HERE
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int[] ParseCoins(string data)
        {
            int[] coins;
            if (data.Contains("&"))
            {
                var values = data.Split('&');
                coins = new int[values.Length];

                for (int i = 0; i < values.Length; i++)
                {
                    if (string.IsNullOrEmpty(values[i])) continue;

                    coins[i] = values[i].ToInt();
                }
            }
            else
            {
                coins = new int[1];
                coins[0] = int.Parse(data);
            }
            return coins;
        }
#else
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ObscuredInt[] ParseCoins(string data)
        {
            ObscuredInt[] coins;
            if (data.Contains("&"))
            {
                var values = data.Split('&');
                coins = new ObscuredInt[values.Length];

                for (int i = 0; i < values.Length; i++)
                {
                    if (string.IsNullOrEmpty(values[i])) continue;

                    coins[i] = values[i].ToInt();
                }
            }
            else
            {
                coins = new ObscuredInt[1];
                coins[0] = int.Parse(data);
            }
            return coins;
        }
#endif

        /// <summary>
        /// 
        /// </summary>
        public void SetUserDate(string date)
        {
            if (!rawData.ContainsKey("user_date")) rawData.Add("user_date", "");
            rawData["user_date"] = date;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool HaveModerationRights()
        {
            return Role.Role.HasModerationRights;
        }

        /// <summary>
        /// Does this user have a clan?
        /// </summary>
        /// <returns></returns>
        public bool HaveAClan()
        {
#if CLANS
            if (Clan == null || Clan.ID == -1) return false;
            return true;
#else
          return false;
#endif
        }

        /// <summary>
        /// Get the user account status prefix
        /// If Admin or Moderator, returns the text that will show next to the player name
        /// </summary>
        /// <returns></returns>
        public string GetStatusPrefix()
        {
            if (!bl_LoginProDataBase.Instance.showStatusPrefix) return "";

            return Role.Role.GetRolePrefix();
        }
    }

    [Serializable]
    public class CustomAuthCredentials
    {
        public string UserName;
        public string NickName;
        public string UniqueID;
        public string Email;
        public AuthenticationType authenticationType = AuthenticationType.ULogin;
        public bool RequireNickName = true;

        public string GetUniquePassword()
        {
            char[] charArray = UniqueID.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }

    [Serializable]
    public class ULoginDatabaseUser
    {
        public int id;
        public string name;
        public string nick;
        public int kills;
        public int deaths;
        public int score;
        public int assist;
        public string coins;
        public string purchases;
        public string meta;
        public string clan;
        public string clan_invitations;
        public int playtime;
        public string email;
        public int active;
        public string ip;
        public string friends;
        public int status;
        public string verify;
        public string user_date;

        /// <summary>
        /// Convert this data to a LoginUserInfo
        /// </summary>
        /// <returns></returns>
        public LoginUserInfo ToLoginUserInfo()
        {
            LoginUserInfo info = new LoginUserInfo
            {
                ID = id,
                NickName = nick,
                LoginName = name,
                Kills = kills,
                Deaths = deaths,
                Assist = assist,
                Score = score,
                Coins = LoginUserInfo.ParseCoins(coins),
                IP = ip,
                PlayTime = playtime,
                Role = status
            };
            info.SetUserDate(user_date);

#if SHOP
            info.ShopData = new Shop.bl_ShopUserData();
            info.ShopData.SetRawData(new Dictionary<string, string>() { { "purchases", purchases } });
#endif
            return info;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetPurchasesCount()
        {
            if (string.IsNullOrEmpty(purchases)) return 0;

            return purchases.Split('-').Length;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetPlaytimeString()
        {
            return bl_DataBaseUtils.TimeFormat(playtime);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ULoginAccountRoleRef GetRole()
        {
            return (ULoginAccountRoleRef)status;
        }
    }

    [Serializable]
    public class ULoginDatabaseUserList
    {
        public List<ULoginDatabaseUser> users;
        public int total;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="perPage"></param>
        /// <returns></returns>
        public int GetPagesCount(int perPage)
        {
            int pages = total / perPage;
            if (total % perPage != 0) pages++;
            return pages;
        }
    }
}