using MFPS.Internal.Scriptables;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.ULogin
{
    [CreateAssetMenu(fileName = "ULogin Database Handler", menuName = "MFPS/Database/ULogin Handler")]

    public class bl_ULoginDatabase : bl_DatabaseBase
    {
        /// <summary>
        /// 
        /// </summary>
        public override string NickName
        {
            get
            {
                if (!IsUserLogged()) return bl_PhotonNetwork.NickName;
                return LocalUser.NickName;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string GetRolePrefix()
        {
            if (!IsUserLogged()) return string.Empty;
            return LocalUser.GetStatusPrefix();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public override int GetInt(string key, int defaultValue = 0)
        {
            if (!IsUserLogged()) return defaultValue;

            switch (key)
            {
                case "score":
                    return LocalUser.Score;
                case "kills":
                    return LocalUser.Kills;
                case "deaths":
                    return LocalUser.Deaths;
                case "assists":
                    return LocalUser.Assist;
                case "id":
                    return LocalUser.ID;
                case "playtime":
                    return LocalUser.PlayTime;
                default:
                    Debug.LogWarning($"The key '{key}' has not been defined.");
                    return defaultValue;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public override string GetString(string key, string defaultValue = "")
        {
            if (!IsUserLogged()) return defaultValue;

            switch (key)
            {
                case "username":
                    return LocalUser.LoginName;
                case "nickname":
                    return LocalUser.NickName;
                case "ip":
                    return LocalUser.IP;
                default:
                    Debug.LogWarning($"The key '{key}' has not been defined.");
                    return defaultValue;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="onComplete"></param>
        public override void StorePlayerMatchStats(int overrideScore = -1, Action<bool> onComplete = null)
        {
            if (!IsUserLogged()) return;

            var lp = bl_PhotonNetwork.LocalPlayer;
            var lu = LocalUser;
            int scoreToAdd = overrideScore == -1 ? lp.GetPlayerScore() : overrideScore;

            if (!lu.SendNewData(lp.GetKills(), lp.GetDeaths(), scoreToAdd, lp.GetAssists()))
            {
                // if there's no data to update
                return;
            }

            var fields = new ULoginUpdateFields();
            fields.AddField("kills", lu.Kills);
            fields.AddField("deaths", lu.Deaths);
            fields.AddField("score", lu.Score);
            fields.AddField("assist", lu.Assist);

            bl_DataBase.Instance.UpdateUserData(fields, onComplete);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemType"></param>
        /// <param name="itemID"></param>
        /// <returns></returns>
        public override bool IsItemPurchased(int itemType, int itemID)
        {
            if (!IsUserLogged()) return false;

#if SHOP
            return LocalUser.ShopData.isItemPurchase(itemType, itemID);
#else
            return false;
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loadoutSlot"></param>
        /// <param name="defaultLoadout"></param>
        /// <returns></returns>
        public override bl_PlayerClassLoadout GetLoadout(PlayerClass loadoutSlot, bl_PlayerClassLoadout defaultLoadout = null)
        {
            var instance = Instantiate(defaultLoadout);

            if (!IsUserLogged())
            {
                string nick = string.IsNullOrEmpty(bl_PhotonNetwork.NickName) ? "Offline Player" : bl_PhotonNetwork.NickName;
                string key = PropertiesKeys.GetUniqueKeyForPlayer("loadouts", nick);
                if (!PlayerPrefs.HasKey(key)) return instance;

                string loadoutData = PlayerPrefs.GetString(key);
                instance.FromString(loadoutData, (int)loadoutSlot);
                return instance;
            }

            string allLoadouts = LocalUser.metaData.rawData.WeaponsLoadouts;
            if (string.IsNullOrEmpty(allLoadouts)) return instance;

            instance.FromString(allLoadouts, (int)loadoutSlot);
            return instance;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loadouts"></param>
        public override void StoreLoadouts(bl_PlayerClassLoadout[] loadouts, Action onComplete = null)
        {
            string nick = string.IsNullOrEmpty(bl_PhotonNetwork.NickName) ? "Offline Player" : bl_PhotonNetwork.NickName;
            string key = PropertiesKeys.GetUniqueKeyForPlayer("loadouts", nick);

            string compiled = string.Empty;
            for (int i = 0; i < loadouts.Length; i++)
            {
                compiled += $"{loadouts[i].ToString()},";
            }

            compiled = compiled.Remove(compiled.Length - 1);
            if (IsUserLogged())
            {
                LocalUser.metaData.rawData.WeaponsLoadouts = compiled;
                bl_DataBase.Instance.SaveUserMetaData(onComplete);
            }
            else
            {
                PlayerPrefs.SetString(key, compiled);
                onComplete?.Invoke();
            }
        }

        #region Coins
        /// <summary>
        /// 
        /// </summary>
        /// <param name="coinsToGet"></param>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        public override int[] GetCoins(MFPSCoin[] coinsToGet, string endPoint = "", OnOperationResult onFinish = null)
        {
            if (!IsUserLogged()) return null;

            int[] amounts = new int[coinsToGet.Length];
            var userCoins = LocalUser.Coins;

            for (int i = 0; i < coinsToGet.Length; i++)
            {
                int indexOfCoin = bl_MFPS.Coins.GetIndexOfCoin(coinsToGet[i]);

                if (indexOfCoin >= userCoins.Length)
                {
                    Debug.LogWarning($"Local user doesn't have data for this coin '{coinsToGet[i].CoinName} with ID {indexOfCoin}'.");
                    continue;
                }

                amounts[i] = userCoins[indexOfCoin];
            }
            return amounts;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coinsToAdd"></param>
        /// <param name="coinToAdd"></param>
        public override void AddCoins(int coinsToAdd, MFPSCoin coinToAdd, OnOperationResult onFinish = null)
        {
            if (!IsUserLogged()) return;

            bl_DataBase.Instance.SaveNewCoins(coinsToAdd, (int)coinToAdd);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coinsToRemove"></param>
        /// <param name="coinToRemoveFrom"></param>
        public override void RemoveCoins(int coinsToRemove, MFPSCoin coinToRemoveFrom, OnOperationResult onFinish = null)
        {
            if (!IsUserLogged()) return;

            bl_DataBase.Instance.SubtractCoins(coinsToRemove, (int)coinToRemoveFrom);
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override List<string> GetFriends()
        {
            if (!IsUserLogged()) return new List<string>();

            return LocalUser.FriendList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="friends"></param>
        public override void StoreFriends(List<string> friends)
        {
            if (!IsUserLogged()) return;

            string friendsString = string.Join("/", friends.ToArray());
            bl_DataBase.Instance.SaveValue("friends", friendsString, () =>
            {
                // update local user data
                LocalUser.SetFriends(friendsString);
            }, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int StopRecordingPlayTime()
        {
            if (!IsUserLogged()) return 0;

            int playTime = base.StopRecordingPlayTime();
            if (playTime <= 0) return 0;

            int totalPlaytime = GetInt("playtime");

            // the total play time returned is in seconds, but we store it in minutes, so we need to convert it
            playTime = Mathf.FloorToInt(totalPlaytime / 60);
            if (playTime <= 0) return 0;

            totalPlaytime += playTime;
            bl_DataBase.Instance.SaveValue("playtime", totalPlaytime.ToString());
            return totalPlaytime;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="where"></param>
        /// <param name="index"></param>
        /// <param name="callback"></param>
        public override void CheckIfUserExist(string where, string index, Action<bool> callback)
        {
            if (!IsUserLogged())
            {
                callback?.Invoke(false);
                return;
            }

            bl_DataBase.CheckIfUserExist(bl_PhotonNetwork.Instance, where, index, callback);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void SignOut()
        {
            base.SignOut();
            bl_LoginProDataBase.SignOut();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool IsLocalInClan()
        {
            if (!IsUserLogged()) return false;
            return LocalUser.HaveAClan();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string GetClanTag()
        {
            if (!IsUserLogged()) return string.Empty;
#if CLANS
            return LocalUser.Clan.GetTagPrefix();
#else
            return string.Empty;
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        private LoginUserInfo LocalUser
        {
            get => bl_DataBase.LocalLoggedUser;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool IsUserLogged()
        {
            return bl_DataBase.IsUserLogged;
        }
    }
}