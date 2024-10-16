﻿using UnityEngine;
using UnityEngine.SceneManagement;
using MFPS.Runtime.Level;
using System.Linq;
using UnityEngine.Networking;
using System.Runtime.CompilerServices;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#if !UNITY_WEBGL
using System.IO;
#endif
#endif

/// <summary>
/// Static class with utility functions
/// </summary>
public static class bl_UtilityHelper
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="scene"></param>
    public static void LoadLevel(string scene)
    {
        if (bl_GlobalReferences.I.sceneLoader != null)
        {
            bl_GlobalReferences.I.sceneLoader.LoadScene(scene);
        }
        else
            SceneManager.LoadScene(scene, LoadSceneMode.Single);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="scene"></param>
    public static void LoadLevelAdditive(string scene)
    {
        SceneManager.LoadScene(scene, LoadSceneMode.Additive);
    }

    /// <summary>
    /// Sort Player by Kills,for more info watch this: http://answers.unity3d.com/questions/233917/custom-sorting-function-need-help.html
    /// </summary>
    /// <returns></returns>
    public static int GetSortPlayerByKills(MFPSPlayer player1, MFPSPlayer player2)
    {
        return player1 != null && player2 != null
            ? (int)player2.GetPlayerPropertie(PropertiesKeys.KillsKey) - (int)player1.GetPlayerPropertie(PropertiesKeys.KillsKey)
            : 0;
    }

    /// <summary>
    /// Helper for Cursor locked in Unity 5
    /// </summary>
    /// <param name="mLock">cursor state</param>
    public static void LockCursor(bool mLock)
    {
        if (BlockCursorForUser) return;
        if (mLock == true)
        {
            CursorLockMode cm = isMobile ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = false;
            Cursor.lockState = cm;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
    public static bool BlockCursorForUser = false;

    /// <summary>
    /// 
    /// </summary>
    public static bool GetCursorState
    {
        get
        {
#if (UNITY_ANDROID || UNITY_IOS || UNITY_IPHONE) && !UNITY_EDITOR
            return true;
#else
            return !Cursor.visible || Cursor.lockState == CursorLockMode.Locked;
#endif
        }
    }

    /// <summary>
    /// Are we currently playing in a mobile build or using Unity Remote in editor
    /// </summary>
    public static bool isMobile
    {
        get
        {
#if (UNITY_ANDROID || UNITY_IOS || UNITY_IPHONE) && !UNITY_EDITOR
            return true;
#else
#if MFPSM && UNITY_EDITOR
            if (EditorApplication.isRemoteConnected) return true;
#endif
            return false;
#endif
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static AudioSource PlayClipAtPoint(AudioClip clip, Vector3 position, float volume, AudioSource sourc)
    {
        var obj2 = new GameObject("One shot audio")
        {
            transform = { position = position }
        };
        AudioSource source = (AudioSource)obj2.AddComponent(typeof(AudioSource));
        if (sourc != null)
        {
            source.minDistance = sourc.minDistance;
            source.maxDistance = sourc.maxDistance;
            source.panStereo = sourc.panStereo;
            source.spatialBlend = sourc.spatialBlend;
            source.rolloffMode = sourc.rolloffMode;
            source.priority = sourc.priority;
        }
        source.clip = clip;
        source.volume = volume;
        source.Play();
        Object.Destroy(obj2, clip.length * Time.timeScale);
        return source;
    }

    /// <summary>
    /// 
    /// </summary>

    public static void DrawWireCircle(Vector3 center, float radius, int segments = 20, Quaternion rotation = default)
    {
        DrawWireArc(center, radius, 360, segments, rotation);
    }

    /// <summary>
    /// 
    /// </summary>
    public static void DrawWireArc(Vector3 center, float radius, float angle, int segments = 20, Quaternion rotation = default)
    {
        var old = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(center, rotation, Vector3.one);
        Vector3 from = Vector3.forward * radius;
        var step = Mathf.RoundToInt(angle / segments);
        for (int i = 0; i <= angle; i += step)
        {
            var to = new Vector3(radius * Mathf.Sin(i * Mathf.Deg2Rad), 0, radius * Mathf.Cos(i * Mathf.Deg2Rad));
            Gizmos.DrawLine(from, to);
            from = to;
        }

        Gizmos.matrix = old;
    }

    /// <summary>
    /// Calculate distance from two vectors
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Distance(Vector3 from, Vector3 to)
    {
        var v = new Vector3(from.x - to.x, from.y - to.y, from.z - to.z);
        return Mathf.Sqrt((v.x * v.x) + (v.y * v.y) + (v.z * v.z));
    }

    /// <summary>
    /// Calculate distance from two vectors without square root
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float SquaredDistance(Vector3 a, Vector3 b)
    {
        return (a - b).sqrMagnitude;
    }

    /// <summary>
    /// Check if two points are within a distance using squared distance for faster calculation.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="distance"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsWithinDistance(Vector3 a, Vector3 b, float distance)
    {
        float squaredDistance = SquaredDistance(a, b);
        float squaredThreshold = distance * distance;
        return squaredDistance <= squaredThreshold;
    }

#if UNITY_EDITOR
    public static string CreateAsset<T>(string path = "", bool autoFocus = true, string customName = "") where T : ScriptableObject
    {
        T asset = ScriptableObject.CreateInstance<T>();

        if (string.IsNullOrEmpty(path))
            path = AssetDatabase.GetAssetPath(Selection.activeObject);

        if (string.IsNullOrEmpty(path))
        {
            path = "Assets";
        }
#if !UNITY_WEBGL
        else if (Path.GetExtension(path) != string.Empty)
        {
            path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
        }
#endif
        string fileName = string.IsNullOrEmpty(customName) ? $"New {typeof(T)}" : customName;
        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath($"{path}/{fileName}.asset");

        AssetDatabase.CreateAsset(asset, assetPathAndName);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        if (autoFocus)
        {
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
        return assetPathAndName;
    }
#endif

    /// <summary>
    /// Calculate the center pivot from the child meshes of a object
    /// </summary>
    /// <param name="aObjects"></param>
    /// <returns></returns>
    public static Vector3 CalculateCenter(params Transform[] aObjects)
    {
        var b = new Bounds();
        foreach (var o in aObjects)
        {
            var renderers = o.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                if (r.GetComponent<ParticleSystem>() != null) continue;
                if (b.size == Vector3.zero)
                    b = r.bounds;
                else
                    b.Encapsulate(r.bounds);
            }
            var colliders = o.GetComponentsInChildren<Collider>();
            foreach (var c in colliders)
            {
                if (b.size == Vector3.zero)
                    b = c.bounds;
                else
                    b.Encapsulate(c.bounds);
            }
        }
        return b.center;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="go"></param>
    /// <param name="name"></param>
    /// <param name="inactiveObjects"></param>
    /// <returns></returns>
    public static GameObject FindInChildren(this GameObject go, string name, bool inactiveObjects = true)
    {
        return (from x in go.GetComponentsInChildren<Transform>(inactiveObjects)
                where x.gameObject.name == name
                select x.gameObject).First();
    }

    /// <summary>
    /// Create a Photon hashtable
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ExitGames.Client.Photon.Hashtable CreatePhotonHashTable()
    {
        return new ExitGames.Client.Photon.Hashtable();
    }

    //Use this instead of Vector3.forward since that return a new Vector3 each time that is called
    private static Vector3 m_VectorForward = Vector3.forward;
    public static Vector3 VectorForward => m_VectorForward;

    /// <summary>
    /// Check if one of the two teams reach the max score
    /// </summary>
    /// <param name="maxScore"></param>
    /// <returns></returns>
    public static Team CheckIfATeamWon(int maxScore)
    {
        var team1Score = bl_PhotonNetwork.CurrentRoom.GetRoomScore(Team.Team1);
        var team2Score = bl_PhotonNetwork.CurrentRoom.GetRoomScore(Team.Team2);

        if (team1Score < maxScore && team2Score < maxScore) return Team.None;
        if (team1Score > team2Score) return Team.Team1;
        if (team2Score > team1Score) return Team.Team2;
        return Team.All;//tie
    }

    /// <summary>
    /// Reset all the decals in this object recursively
    /// </summary>
    public static void DetachChildDecals(GameObject go)
    {
        if (go == null) return;

        var all = go.transform.GetComponents<bl_BulletDecalBase>();
        for (int i = 0; i < all.Length; i++)
        {
            all[i].BackToOrigin();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static bool IsNetworkError(UnityWebRequest w)
    {
#if UNITY_2020_1_OR_NEWER
        return w.result != UnityWebRequest.Result.Success && w.result != UnityWebRequest.Result.InProgress;
#else
        return w.isNetworkError || w.isHttpError;
#endif
    }

    /// <summary>
    /// Is the animator playing or in transition
    /// </summary>
    /// <param name="animator"></param>
    /// <returns></returns>
    public static bool IsPlaying(this Animator animator)
    {
        return !(animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !animator.IsInTransition(0));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    public static void CopyComponentFields(Object source, Object target)
    {
        // Get the types of source and target objects
        System.Type typeSource = source.GetType();
        System.Type typeTarget = target.GetType();

        // Ensure both objects are of the same type
        if (typeSource != typeTarget)
        {
            Debug.LogError("Source and target must be of the same type.");
            return;
        }

        // Iterate through all the fields in the source object
        foreach (FieldInfo field in typeSource.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            // Check if the field is public or is tagged with [SerializeField]
            if (field.IsPublic || System.Attribute.IsDefined(field, typeof(SerializeField)))
            {
                // Copy the value from the source to the target
                field.SetValue(target, field.GetValue(source));
            }
        }
    }
}