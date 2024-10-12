using System;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.Internal.Scriptables
{
    public abstract class bl_DatabaseBase : ScriptableObject
    {
        public struct OperationResult
        {
            public string Message;
            public bool Success;
        }

        // create a delegate event for operation result
        public delegate void OnOperationResult(OperationResult result);

        private static float startTime = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract bool IsUserLogged();

        /// <summary>
        /// 
        /// </summary>
        public abstract string NickName { get; }

        /// <summary>
        /// 
        /// </summary>
        public virtual string GetRolePrefix()
        {
            return string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public abstract string GetString(string key, string defaultValue = "");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public abstract int GetInt(string key, int defaultValue = 0);

        /// <summary>
        /// Store the player match stats (kills, deaths, score, etc)
        /// </summary>
        public abstract void StorePlayerMatchStats(int overrideScore = -1, Action<bool> onComplete = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemType"></param>
        /// <param name="itemID"></param>
        /// <returns></returns>
        public abstract bool IsItemPurchased(int itemType, int itemID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loadoutSlot"></param>
        /// <param name="defaultLoadout"></param>
        /// <returns></returns>
        public abstract bl_PlayerClassLoadout GetLoadout(PlayerClass loadoutSlot, bl_PlayerClassLoadout defaultLoadout = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loadouts"></param>
        public abstract void StoreLoadouts(bl_PlayerClassLoadout[] loadouts, Action onComplete = null);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract int[] GetCoins(MFPSCoin[] coinsToGet, string endPoint = "", OnOperationResult onFinish = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coinsToAdd"></param>
        /// <param name="userName"></param>
        public abstract void AddCoins(int coinsToAdd, MFPSCoin coinToAdd, OnOperationResult onFinish = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coinsToRemove"></param>
        public abstract void RemoveCoins(int coinsToRemove, MFPSCoin coinToRemoveFrom, OnOperationResult onFinish = null);

        /// <summary>
        /// 
        /// </summary>
        public abstract List<string> GetFriends();

        /// <summary>
        /// 
        /// </summary>
        public abstract void StoreFriends(List<string> friends);

        /// <summary>
        /// Check if the user exist in the database
        /// </summary>
        /// <param name="where"></param>
        /// <param name="index"></param>
        /// <param name="callback"></param>
        public virtual void CheckIfUserExist(string where, string index, Action<bool> callback)
        {
            // determine if the user exist in the database
            callback?.Invoke(false);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void StartRecordingPlayTime()
        {
            startTime = Time.realtimeSinceStartup;
        }

        /// <summary>
        /// Return the time in seconds since the start of the match
        /// </summary>
        public virtual int StopRecordingPlayTime()
        {
            float playTime = Time.realtimeSinceStartup - startTime;
            return Mathf.FloorToInt(playTime);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void SignOut()
        {
            bl_PhotonNetwork.NickName = string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual bool IsLocalInClan() { return false; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual string GetClanTag() { return string.Empty; }
    }
}