using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MFPS.Addon.GameResumePro;

public class GameResumeProAddon
{
    const string prefabPath = "Assets/Addons/GameResumePro/Prefab/Match Final Resume Pro.prefab";

    [MenuItem("MFPS/Addons/GameResumePro/Integrate")]
    static void Integrate()
    {
        var prefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject));
        if (prefab == null)
        {
            Debug.LogWarning("Can't found the addon prefab!");
            return;
        }

        if (GameObject.FindObjectOfType<bl_GameResumePro>() != null)
        {
            Debug.LogWarning("Game Resume Pro has been integrated in this scene already.");
            return;
        }

        if (bl_UIReferences.Instance == null)
        {
            Debug.LogWarning("This is not a map scene, therefore this addon can't be integrated here.");
            return;
        }

        var gm = bl_UIReferences.Instance.GetComponentInChildren<bl_GameFinish>(true);
        GameObject objRef = gm == null ? null : gm.gameObject;
        if (objRef != null)
        {
            objRef.gameObject.SetActive(false);
            EditorUtility.SetDirty(objRef);
        }
        else
        {
            objRef = bl_UIReferences.Instance.addonReferences[0];
        }

        var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        instance.transform.SetParent(objRef.transform.parent, false);
        instance.transform.SetSiblingIndex(10);
        EditorUtility.SetDirty(instance);

        Selection.activeGameObject = instance;
        EditorGUIUtility.PingObject(instance);
        Debug.Log("<color=green>Game Resume Pro integrated!</color>", instance);
    }

    [MenuItem("MFPS/Addons/GameResumePro/Integrate", true)]
    static bool IntegrationVerify()
    {
        var script = GameObject.FindObjectOfType<bl_GameResumePro>();
        return bl_UIReferences.Instance != null && script == null;
    }
}