using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

/// <summary>
/// This script handle the instantiation of network items
/// Network items are those game objects that instantiation and or destruction is replicated on all other clients in the room.
/// With this script, those network items doesn't have to have a PhotonView or be located in a Resources folder
/// Making way more network and memory efficient.
/// 
/// In order to instance a network item, there's only one requirement:
/// 1. the item prefab has to be listed in the 'Network Items Prefabs' list
/// </summary>
public class bl_ItemManager : bl_ItemManagerBase
{
    [Header("Network Prefabs")]
    public List<bl_NetworkItem> networkItemsPrefabs = new List<bl_NetworkItem>();

    [Header("Settings")]
    public float respawnItemsAfter = 12;

    //private
    private readonly Dictionary<string, bl_NetworkItem> networkItemsPool = new();
    private readonly Dictionary<string, GameObject> genericItems = new();
    private readonly List<RespawnItems> respawnItems = new();

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        if (!bl_PhotonNetwork.IsConnected) return;

        base.OnEnable();
        bl_PhotonNetwork.Instance.AddCallback(PropertiesKeys.NetworkItemInstance, OnNetworkItemInstance);
        bl_PhotonNetwork.Instance.AddCallback(PropertiesKeys.NetworkItemChange, OnNetworkItemChange);
        bl_PhotonNetwork.Instance.AddCallback(PropertiesKeys.EventItemSync, OnItemEvent);
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        bl_PhotonNetwork.Instance?.RemoveCallback(OnNetworkItemInstance);
        bl_PhotonNetwork.Instance?.RemoveCallback(OnNetworkItemChange);
        bl_PhotonNetwork.Instance?.RemoveCallback(OnItemEvent);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    void OnNetworkItemInstance(Hashtable data)
    {
        //don't instance for the player that create the item since it's already instance for him
        int actorID = (int)data["actorID"];
        if (bl_PhotonNetwork.LocalPlayer.ActorNumber == actorID) return;

        string prefabName = (string)data["prefab"];
        bl_NetworkItem prefab = networkItemsPrefabs.Find(x =>
        {
            return (x != null && x.gameObject.name == prefabName);
        });

        if (prefab == null)
        {
            Debug.LogWarning($"The network prefab {prefabName} is not listed in the bl_ItemManager of this scene.");
            return;
        }

        prefab = Instantiate(prefab.gameObject, (Vector3)data["position"], (Quaternion)data["rotation"]).GetComponent<bl_NetworkItem>();
        prefab.OnNetworkInstance(data);
        //pool this network item
        PoolItem(prefabName, prefab);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void PoolItem(string itemName, bl_NetworkItem item)
    {
        if (networkItemsPool.ContainsKey(itemName)) return;
        networkItemsPool.Add(itemName, item);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="uniqueName"></param>
    /// <param name="go"></param>
    public override void RegisterGeneric(string uniqueName, GameObject go)
    {
        if (genericItems.ContainsKey(uniqueName)) return;
        genericItems.Add(uniqueName, go);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="uniqueName"></param>
    public override void UnregisterGeneric(string uniqueName)
    {
        if (genericItems.ContainsKey(uniqueName))
        {
            genericItems.Remove(uniqueName);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    void OnItemEvent(Hashtable data)
    {
        byte command = (byte)data["c"];

        switch (command)
        {
            case 0: // sync projectile detonation
                OnSyncDetonation(data);
                break;
            default:
                Debug.LogWarning("Undefined item event " + command);
                break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    void OnSyncDetonation(Hashtable data)
    {
        string itemName = (string)data[(byte)0];
        Vector3 pos = (Vector3)data[(byte)1];
        Quaternion rot = (Quaternion)data[(byte)2];
        BulletData bulletData = (BulletData)data[(byte)3];

        if (!genericItems.ContainsKey(itemName))
        {
            Debug.LogWarning($"The network item {itemName} couldn't be found, maybe was instanced before this player enter in the room.");
            return;
        }
        if (genericItems[itemName] == null)
        {
            Debug.LogWarning("The item was null, maybe it was destroyed before the network command reach this client.");
            genericItems.Remove(itemName);
            return;
        }

        if (genericItems[itemName].TryGetComponent(out bl_ProjectileBase projectileBase))
        {
            projectileBase.Detonate(pos, rot, bulletData, false);
        }
    }

    /// <summary>
    /// Called when the state of a network item change, eg: OnDestroy, OnEnable or OnDisable
    /// </summary>
    void OnNetworkItemChange(Hashtable data)
    {
        string itemName = (string)data["name"];

        if (!networkItemsPool.ContainsKey(itemName))
        {
            Debug.LogWarning($"The network item {itemName} couldn't be found, maybe was instanced before this player enter in the room.");
            return;
        }

        int state = (int)data["active"];
        if (state == -1)
        {
            if (networkItemsPool[itemName] != null) Destroy(networkItemsPool[itemName].gameObject);
            networkItemsPool.Remove(itemName);
        }
        else
        {
            networkItemsPool[itemName].gameObject.SetActive(state == 1 ? true : false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnSlowUpdate()
    {
        CheckTimers();
    }

    /// <summary>
    /// 
    /// </summary>
    void CheckTimers()
    {
        if (respawnItems.Count <= 0) return;

        int c = respawnItems.Count;
        for (int i = c - 1; i >= 0; i--)
        {
            if (Time.time - respawnItems[i].AddedTime >= respawnItems[i].RespawnAfter)
            {
                respawnItems[i].Item.SetActiveSync(true);
                respawnItems.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Add an item to the waiting list to respawn it after certain time
    /// </summary>
    public override void RespawnAfter(bl_NetworkItem item, float respawnAfter = 0)
    {
        respawnItems.Add(new RespawnItems()
        {
            Item = item,
            AddedTime = Time.time,
            RespawnAfter = respawnAfter <= 0 ? respawnItemsAfter : respawnAfter
        });
        item.SetActiveSync(false);
    }

    /// <summary>
    /// 
    /// </summary>
    public class RespawnItems
    {
        public float AddedTime;
        public float RespawnAfter;
        public bl_NetworkItem Item;
    }
}