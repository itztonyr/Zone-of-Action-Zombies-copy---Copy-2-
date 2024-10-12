using MFPS.Addon.Customizer;
using MFPS.Core;
using MFPSEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class bl_CustomizerData : ScriptableObject
{
    public List<CustomizerInfo> Weapons = new();
    [Space(10)]
    public List<GlobalCamo> GlobalCamos = new();
    [Header("Settings")]
    [LovattoToogle] public bool returnToLobbyAfterSave = true;

    [HideInInspector] public int OpenedWeapon = 0;
    public const string CURRENT_CUSTOMIZER = "cmz.cweaponid";

    public CustomizerInfo GetWeapon(string weaponName)
    {
        for (int i = 0; i < Weapons.Count; i++)
        {
            if (Weapons[i].WeaponName == weaponName)
            {
                return Weapons[i];
            }
        }
        return null;
    }

    public int GetCustomizerID(int GunID)
    {
        for (int i = 0; i < Weapons.Count; i++)
        {
            if (Weapons[i].GunID == GunID) return i;
        }
        return 0;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="camoName"></param>
    /// <returns></returns>
    public int GetGlobalCamoID(string camoName)
    {
        for (int i = 0; i < GlobalCamos.Count; i++)
        {
            if (GlobalCamos[i].Name == camoName) return i;
        }
        return -1;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public GlobalCamo GetGlobalCamo(int index)
    {
        return index >= GlobalCamos.Count ? null : GlobalCamos[index];
    }

    public string[] GetWeaponStringArray()
    {
        string[] array = new string[Weapons.Count];
        for (int i = 0; i < Weapons.Count; i++)
        {
            array[i] = Weapons[i].WeaponName;
        }
        return array;
    }

    public int[] LoadAttachmentsForWeapon(string weapon)
    {
        int[] array = new int[5] { 0, 0, 0, 0, 0 };
        var key = GetWeaponKey(weapon);
        if (PlayerPrefs.HasKey(key))
        {
            string t = PlayerPrefs.GetString(key);
            array = DecompileLine(t);
        }
        return array;
    }

    public int[] DecompileLine(string line)
    {
        int[] array = new int[5] { 0, 0, 0, 0, 0 };
        string[] split = line.Split(","[0]);
        array[0] = int.Parse(split[0]);
        array[1] = int.Parse(split[1]);
        array[2] = int.Parse(split[2]);
        array[3] = int.Parse(split[3]);
        array[4] = int.Parse(split[4]);
        return array;
    }

    public string CompileArray(int[] array)
    {
        string line = string.Join(",", array.Select(x => x.ToString()).ToArray());
        return line;
    }

    public void SaveAttachmentsForWeapon(string weapon, int[] ids)
    {
        string t = GetWeaponKey(weapon);
        string line = string.Join(",", ids.Select(x => x.ToString()).ToArray());
        PlayerPrefs.SetString(t, line);
        PlayerPrefs.SetInt(CURRENT_CUSTOMIZER, Weapons.FindIndex(x => x.WeaponName == weapon));
    }

    public string GetWeaponKey(string wname)
    {
        string nick = bl_PhotonNetwork.NickName;
        if (!bl_PhotonNetwork.IsConnected || bl_PhotonNetwork.OfflineMode)
        {
            nick = "Offline Player";
        }
        string t = string.Format("{0}.{1}.cmz.att.{2}", nick, Application.productName, wname);
        return t;
    }

    public string[] GetGlobalCamosStringArray()
    {
        List<string> list = new();
        list = GlobalCamos.Select(x => x.Name).ToList();
        return list.ToArray();
    }
    /// <summary>
    /// 
    /// </summary>
    void OnValidate()
    {
        for (int i = 0; i < Weapons.Count; i++)
        {
            var w = Weapons[i];
            w.WeaponName = bl_GameData.Instance.GetWeapon(w.GunID).Name;
            for (int e = 0; e < w.Camos.Count; e++)
            {
                w.Camos[e].ID = e;
                w.Camos[e].ofWeaponID = i;
            }
        }
    }

    [MFPSGameItemCollector]
    private static List<MFPSGameItem> CollectDefaultItems()
    {
        var list = new List<MFPSGameItem>();
        var all = Instance.GlobalCamos;

        for (int i = 0; i < all.Count; i++)
        {
            var w = all[i];
            list.Add(w);
        }

        return list;
    }

    private static bl_CustomizerData _data;
    public static bl_CustomizerData Instance
    {
        get
        {
            if (_data == null)
            {
                _data = Resources.Load<bl_CustomizerData>("CustomizerData");
            }
            return _data;
        }
    }
}