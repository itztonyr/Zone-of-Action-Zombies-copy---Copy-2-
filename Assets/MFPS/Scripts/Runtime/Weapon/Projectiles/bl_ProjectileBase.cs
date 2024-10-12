using System;
using UnityEngine;

/// <summary>
/// Base class for all the weapon projectiles (bullets, grenades, etc...)
/// </summary>
public abstract class bl_ProjectileBase : bl_MonoBehaviour
{
    /// <summary>
    /// Called just before disable the projectile
    /// </summary>
    public Action onBeforeDisable;

    protected bool hasUniqueID = false;

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (hasUniqueID && bl_ItemManagerBase.Instance != null)
        {
            bl_ItemManagerBase.Instance.UnregisterGeneric(gameObject.name);
        }
    }

    /// <summary>
    /// Initialize the projectile
    /// </summary>
    public abstract void InitProjectile(BulletData data);

    /// <summary>
    /// Detonate the projectile
    /// </summary>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <param name="callFromLocal"></param>
    public virtual void Detonate(Vector3 position, Quaternion rotation, BulletData bulletData, bool callFromLocal = true)
    {
        Debug.LogWarning("Method not implemented.");
    }

    /// <summary>
    /// Set the unique id for this projectile
    /// </summary>
    /// <param name="id"></param>
    public virtual void SetUniqueID(string id)
    {
        gameObject.name = $"{gameObject.name.Replace("(Clone)", "")} [{id}]";
        hasUniqueID = true;
        if (bl_ItemManagerBase.Instance != null)
        {
            bl_ItemManagerBase.Instance.RegisterGeneric(gameObject.name, gameObject);
        }
    }
}