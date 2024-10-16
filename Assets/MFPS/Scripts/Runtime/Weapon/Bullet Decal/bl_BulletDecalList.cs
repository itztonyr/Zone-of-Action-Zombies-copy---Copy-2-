using System;
using UnityEngine;

namespace MFPS.Internal.Scriptables
{
    [CreateAssetMenu(fileName = "Bullet Decal List", menuName = "MFPS/Weapons/Decal/List")]
    public class bl_BulletDecalList : ScriptableObject
    {
        public int genericSurfaceId = 0;
        public SurfaceDecal[] surfaceDecals;

        /// <summary>
        /// Get a random material from the given surface
        /// </summary>
        /// <param name="surfaceTag"></param>
        /// <returns></returns>
        public SurfaceDecal GetDecalForSurface(string surfaceTag)
        {
            for (int i = 0; i < surfaceDecals.Length; i++)
            {
                if (surfaceDecals[i].SurfaceTag == surfaceTag)
                {
                    return surfaceDecals[i];
                }
            }
            return surfaceDecals[genericSurfaceId];
        }

        /// <summary>
        /// Get a random material from the given surface
        /// </summary>
        /// <param name="surfaceTag"></param>
        /// <returns></returns>
        public SurfaceDecal GetDecalForTag(Transform trans)
        {
            for (int i = 0; i < surfaceDecals.Length; i++)
            {
                if (trans.CompareTag(surfaceDecals[i].SurfaceTag))
                {
                    return surfaceDecals[i];
                }
            }
            return surfaceDecals[genericSurfaceId];
        }

        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        public class SurfaceDecal
        {
            public string SurfaceTag;
            public Material[] DecalMaterials;
            [Tooltip("The name of the pooled particle prefab, which is the Key assigned in the Pooled prefab container list.")]
            public string HitParticleName = "bullet_hit";
            public Vector2 SizeRange = new Vector2(0.9f, 1.2f);

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public Material GetMaterial()
            {
                return DecalMaterials[UnityEngine.Random.Range(0, DecalMaterials.Length)];
            }

            /// <summary>
            /// Does this surface require decals?
            /// </summary>
            /// <returns></returns>
            public bool HasDecals()
            {
                return DecalMaterials != null && DecalMaterials.Length > 0;
            }

            /// <summary>
            /// Does this surface require particles?
            /// </summary>
            /// <returns></returns>
            public bool HasHitParticle()
            {
                return !string.IsNullOrEmpty(HitParticleName);
            }
        }
    }
}