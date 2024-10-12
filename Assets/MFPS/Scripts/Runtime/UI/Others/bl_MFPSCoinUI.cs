﻿using MFPS.Internal.Scriptables;
using MFPSEditor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MFPS.Runtime.UI
{
    public class bl_MFPSCoinUI : MonoBehaviour
    {
        [MFPSCoinID] public int coin;
        [SerializeField] private TextMeshProUGUI coinText = null;
        [SerializeField] private Text coinTextUGUI = null;
        [SerializeField] private Image coinIconImg = null;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            if (bl_GameData.isDataCached) OnCoinUpdate(null);
            bl_EventHandler.onCoinUpdate += OnCoinUpdate;
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDisable()
        {
            bl_EventHandler.onCoinUpdate -= OnCoinUpdate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="updatedCoin"></param>
        void OnCoinUpdate(MFPSCoin updatedCoin)
        {
            var coinData = bl_MFPS.Coins.GetCoinData(coin);
            if (coinData == null)
            {
                if (coinText != null) { coinText.gameObject.SetActive(false); }
                if (coinTextUGUI != null) { coinTextUGUI.gameObject.SetActive(false); }
                if (coinIconImg != null) { coinIconImg.gameObject.SetActive(false); }
                return;
            }

            if (coinText != null) coinText.text = coinData.GetCoins(bl_PhotonNetwork.NickName).ToString();
            if (coinTextUGUI != null) coinTextUGUI.text = coinData.GetCoins(bl_PhotonNetwork.NickName).ToString();
            if (coinIconImg != null) coinIconImg.sprite = coinData.CoinIcon;
        }
    }
}