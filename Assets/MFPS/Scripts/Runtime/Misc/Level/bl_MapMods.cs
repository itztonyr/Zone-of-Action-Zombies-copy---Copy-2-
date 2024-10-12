using UnityEngine;

public class bl_MapMods : MonoBehaviour
{
    [LovattoToogle] public bool infinityAmmo = false;

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        bl_EventHandler.Player.onLocalWeaponLoadoutReady += OnLocalWeaponLoadoutReady;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        bl_EventHandler.Player.onLocalWeaponLoadoutReady -= OnLocalWeaponLoadoutReady;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnLocalWeaponLoadoutReady()
    {
        var p = bl_MFPS.LocalPlayerReferences;

        if (infinityAmmo) p.gunManager.SetInfinityAmmoToAllEquippeds(true);
    }
}