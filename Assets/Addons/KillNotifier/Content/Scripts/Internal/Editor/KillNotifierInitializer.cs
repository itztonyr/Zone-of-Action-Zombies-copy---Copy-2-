using MFPSEditor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class KillNotifierInitializer
{
    private const string DEFINE_KEY = "KILL_NOTIFIER";

#if !KILL_NOTIFIER
    [MenuItem("MFPS/Addons/Kill Notifier/Enable")]
    private static void Enable()
    {
        EditorUtils.SetEnabled(DEFINE_KEY, true);
    }
#endif
#if KILL_NOTIFIER
    [MenuItem("MFPS/Addons/Kill Notifier/Disable")]
    private static void Disable()
    {
        EditorUtils.SetEnabled(DEFINE_KEY, false);
    }
#endif

    [MenuItem("MFPS/Addons/Kill Notifier/Integrate")]
    private static void Instegrate()
    {
        bl_KillStreakManager km = GameObject.FindObjectOfType<bl_KillStreakManager>();
        if (km == null)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath("Assets/Addons/KillNotifier/Content/Prefab/Kill Streak Manager.prefab", typeof(GameObject)) as GameObject;
            if (prefab != null)
            {
                GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                instance.transform.SetParent(bl_UIReferences.Instance.noInteractableCanvas.transform, false);
                instance.transform.SetSiblingIndex(11);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                EditorUtility.SetDirty(instance);
                Debug.Log("<color=green>Kill Streak Notifier Integrated</color>");
            }
        }
    }

    [MenuItem("MFPS/Addons/Kill Notifier/Integrate", true)]
    private static bool InstegrateValidate()
    {
        bl_KillStreakManager km = GameObject.FindObjectOfType<bl_KillStreakManager>();
        bl_GameManager gm = GameObject.FindObjectOfType<bl_GameManager>();
        return (km == null && gm != null);
    }
}