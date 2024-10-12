using System;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.Addon.Customizer
{
    public class bl_MultiCamoRenders : MonoBehaviour
    {
        public List<RenderInfo> renders = new List<RenderInfo>();

        /// <summary>
        /// 
        /// </summary>
        public void ApplyCammo(Material camoMat)
        {
            for (int i = 0; i < renders.Count; i++)
            {
                var render = renders[i];
                if (render.MeshRender == null) continue;

                var mats = render.MeshRender.materials;
                mats[render.MaterialIndex] = camoMat;
                render.MeshRender.materials = mats;
            }
        }

        [Serializable]
        public class RenderInfo
        {
            public Renderer MeshRender;
            public int MaterialIndex = 0;
        }
    }
}