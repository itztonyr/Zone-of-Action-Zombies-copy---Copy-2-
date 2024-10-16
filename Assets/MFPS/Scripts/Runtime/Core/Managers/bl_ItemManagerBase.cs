using UnityEngine;

public abstract class bl_ItemManagerBase : bl_MonoBehaviour
{

    /// <summary>
    /// Queue the given item and respawn after the defined time
    /// </summary>
    /// <param name="item"></param>
    /// <param name="delay">if 0 the time defined in the bl_ItemManager inspector will be used</param>
    public abstract void RespawnAfter(bl_NetworkItem item, float delay = 0);

    /// <summary>
    /// Add the given item to the pool network items
    /// </summary>
    /// <param name="itemName">pool list name</param>
    /// <param name="item"></param>
    public abstract void PoolItem(string itemName, bl_NetworkItem item);

    /// <summary>
    /// Cache generic items
    /// Usefull when you are syncing items by name
    /// </summary>
    /// <param name="uniqueName"></param>
    /// <param name="go"></param>
    public abstract void RegisterGeneric(string uniqueName, GameObject go);

    /// <summary>
    /// Remove generic item from cache
    /// </summary>
    /// <param name="uniqueName"></param>
    public abstract void UnregisterGeneric(string uniqueName);

    private static bl_ItemManagerBase _instance;
    public static bl_ItemManagerBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_ItemManagerBase>(); }
            return _instance;
        }
    }
}