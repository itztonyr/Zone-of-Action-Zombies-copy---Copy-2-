﻿using MFPS.Audio;
using MFPS.Internal.Scriptables;
using MFPSEditor;
using UnityEngine;
using HashData = ExitGames.Client.Photon.Hashtable;

/// <summary>
/// This script take care of instantiate and synchronize drop/packages requested by players.
/// this class doesn't have direct references hence can be just replaced with a custom script
/// </summary>
public class bl_DropDispacher : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Time that takes to deliver the item after requested.")]
    public float DeliveryTime = 10;
    [Tooltip("Force applied to the drop indicator object when instantiated.")]
    public float ForceImpulse = 500;
    [Tooltip("Delay to request the item after instantiate the indicator object.")]
    [Range(1, 4)] public float CallDelay = 1.4f;
    [Tooltip("Distance from the player camera view to instantiate the indicator.")]
    public float dropInstanceDistance = 1f;

    [Header("References")]
    [SerializeField] private GameObject DropDeliveryPrefab = null;
    [SerializeField] private GameObject dropIndicatorPrefab = null;
    [ScriptableDrawer] public bl_ItemDropContainer dropContainer;

    /// <summary>
    /// when activated, record this in the event
    /// </summary>
    void OnEnable()
    {
        bl_EventHandler.onAirKit += SendDevilery;
        bl_PhotonNetwork.AddNetworkCallback(PropertiesKeys.DropManager, OnNetworkMessage);
    }

    /// <summary>
    /// when disabled, quit this in the event
    /// </summary>
    void OnDisable()
    {
        bl_EventHandler.onAirKit -= SendDevilery;
        bl_PhotonNetwork.RemoveNetworkCallback(OnNetworkMessage);
    }

    /// <summary>
    /// Instance the drop indicator object in the map to request the item.
    /// </summary>
    public void ThrowIndicator(bl_PlayerReferences requester, int itemId)
    {
        Vector3 point = (requester.Position + (requester.PlayerCameraTransform.forward * dropInstanceDistance)) + requester.transform.up;
        GameObject kit = Instantiate(dropIndicatorPrefab, point, Quaternion.identity) as GameObject;
        kit.GetComponent<bl_DropCallerBase>().SetUp(new bl_DropCallerBase.DropData()
        {
            KitID = itemId,
            Delay = CallDelay
        });
        if (kit.TryGetComponent<Rigidbody>(out var rigidbody))
        {
            rigidbody.AddForce(requester.PlayerCameraTransform.forward * ForceImpulse);
        }

        bl_AudioController.PlayClipAtPoint("drop-indicator-spawn", requester.Position);
    }

    /// <summary>
    /// 
    /// </summary>
    void OnNetworkMessage(HashData data)
    {
        GameObject newInstance = Instantiate(DropDeliveryPrefab, transform.position, Quaternion.identity) as GameObject;

        newInstance.GetComponent<bl_DropBase>().Dispatch(new bl_DropBase.DropData()
        {
            DropPrefab = dropContainer.GetItem((int)data["kit"]).Prefab,
            DropPosition = (Vector3)data["pos"],
            DeliveryDuration = DeliveryTime,
        });
    }

    /// <summary>
    /// This is called by an internal event
    /// </summary>
    public void SendDevilery(Vector3 position, int kitID)
    {
        var data = bl_UtilityHelper.CreatePhotonHashTable();
        data.Add("pos", position);
        data.Add("kit", kitID);

        bl_PhotonNetwork.Instance.SendDataOverNetwork(PropertiesKeys.DropManager, data);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bl_ItemDropContainer GetItemContainer()
    {
        return dropContainer;
    }

    private static bl_DropDispacher _instance = null;
    public static bl_DropDispacher Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<bl_DropDispacher>();
            }
            return _instance;
        }
    }

    [System.Serializable]
    public class DropInfo
    {
        public string Name;
        public GameObject Prefab;
    }
}