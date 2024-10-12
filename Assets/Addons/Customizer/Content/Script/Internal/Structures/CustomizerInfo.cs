using MFPSEditor;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.Addon.Customizer
{
    [System.Serializable]
    public class CustomizerInfo
    {
        [HideInInspector] public string WeaponName;
        [GunID] public int GunID = 0;
        public List<CamoInfo> Camos = new List<CamoInfo>();
        public ListCustomizer Attachments;
    }

    [System.Serializable]
    public class ListCustomizer
    {
        public List<AttachInfo> Suppressers = new List<AttachInfo>();
        public List<AttachInfo> Sights = new List<AttachInfo>();
        public List<AttachInfo> Foregrips = new List<AttachInfo>();
        public List<AttachInfo> Magazines = new List<AttachInfo>();
    }

    [System.Serializable]
    public class CustomizerCamoRender
    {
        public int MaterialID = 0;
        public Renderer Render;

        [HideInInspector] public CamoInfo Info;

        public void SetInfo(CamoInfo _info)
        {
            Info = _info;
        }

        public Material ApplyCamo(string weapon, int camoID)
        {
            if (Render == null) return null;

            var weaponData = bl_CustomizerData.Instance.GetWeapon(weapon);
            if (weaponData == null)
            {
                Debug.LogWarning($"Weapon '{weapon}' is not setup in the Customizer data.");
                return null;
            }
            var camoData = weaponData.Camos.Find(x => x.ID == camoID);
            if (camoData == null)
            {
                Debug.LogWarning($"Camo {camoID} for Weapon '{weapon}' is not setup in the Customizer data.");
                return null;
            }

            Material m = camoData.Camo;
            var customRender = Render.GetComponent<bl_MultiCamoRenders>();
            if (customRender != null)
            {
                customRender.ApplyCammo(m);
                return m;
            }

            Material[] mats = Render.materials;
            mats[MaterialID] = m;
            Render.materials = mats;
            return m;
        }
    }


    [System.Serializable]
    public class AttachInfo
    {
        public string Name;
        public int ID;
        public string Description;
    }

    [System.Serializable]
    public class CamoInfo
    {
        public string Name;
        public int GlobalID = 0;
        public int ID;
        public Material Camo;
        [SpritePreview(50)] public Texture2D OverridePreview;

        public int ofWeaponID = 0;

        public Texture2D Preview
        {
            get
            {
                if (OverridePreview != null) { return OverridePreview; }
                return bl_CustomizerData.Instance.GlobalCamos[GlobalID].Icon.texture;
            }
        }
    }
}