using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MFPSEditor;
using MFPSEditor.Addons;

public class WaitingRoomProIntegration : AddonIntegrationWizard
{
    private const string ADDON_NAME = "Waiting Room Pro";
    private const string ADDON_KEY = "MFPSWRP";

    /// <summary>
    /// 
    /// </summary>
    public override void OnEnable()
    {
        base.OnEnable();
        addonName = ADDON_NAME;
        addonKey = ADDON_KEY;
        // number of steps for the integration.
        allSteps = 2;

        int indexOf = MFPSAddonsData.Instance.Addons.FindIndex(x => x.NiceName == ADDON_NAME);
        if (indexOf == -1)
        {
            var aInfo = new MFPSAddonsInfo();
            aInfo.NiceName = ADDON_NAME;
            aInfo.KeyName = addonKey;
            aInfo.FolderName = "FOLDER_NAME";
            aInfo.MinVersion = "1.9";
            MFPSAddonsData.Instance.Addons.Add(aInfo);
            EditorUtility.SetDirty(MFPSAddonsData.Instance);
        }
        else
        {
            var addon = MFPSAddonsData.Instance.Addons[indexOf];
            if (addon.KeyName != addonKey)
            {
                addon.KeyName = addonKey;
                EditorUtility.SetDirty(MFPSAddonsData.Instance);
            }
        }

        MFPSAddonsInfo addonInfo = MFPSAddonsData.Instance.Addons.Find(x => x.KeyName == addonKey);
        Dictionary<string, string> info = new Dictionary<string, string>();
        if (addonInfo != null) { info = addonInfo.GetInfoInDictionary(); }
        Initializate(info);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stepID">Start from 1</param>
    public override void DrawWindow(int stepID)
    {
        if(stepID == 1)
        {
            DrawFirstWindow();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawFirstWindow()
    {
        DrawText("<b>INTEGRATION</b>\n \n<color=#BCBCBCFF><size=11>This addon doesn't require to be enabled, you simply have to integrate it once in the MainMenu scene.</size></color>");
        GUILayout.Space(20);
        EditorGUILayout.BeginHorizontal("box");
        EditorGUILayout.BeginVertical();
        {
            DrawText("<b><size=14>MainMenu Integration</size></b>.");
            GUILayout.Space(10);
            DrawText("<color=#939393FF><size=10><i>Open the <b>MainMenu</b> scene and then click on the <b>Integrate</b> button.</i></size></color>");
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        {
            GUILayout.Space(10);
            GUI.enabled = bl_Lobby.Instance == null;
            if (DrawButton("Open MainMenu"))
            {
                OpenMainMenuScene();
            }
            GUI.enabled = true;

            GUI.enabled = bl_LobbyUI.Instance != null;
            if (DrawButton("Integrate"))
            {
                if (IntegrateInMainMenu())
                {
                    Finish();
                }
            }
            GUI.enabled = true;
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// 
    /// </summary>
    bool IntegrateInMainMenu()
    {
        var lobbyUI = bl_LobbyUI.Instance;
        if(lobbyUI == null)
        {
            Debug.LogWarning("The MainMenu scene is not open yet or the Lobby UI is disabled in the editor.");
            return false;
        }

        var wrp = lobbyUI.GetComponentInChildren<bl_WaitingRoomPro>(true);
        if(wrp != null)
        {
            Debug.Log("Waiting Room Pro is already integrated, if you want to integrate again, delete the current instance first.");
            return true;
        }

        var instance = InstancePrefab("Assets/Addons/WaitingRoomPro/Prefabs/Waiting Room Pro.prefab");
        instance.transform.SetParent(lobbyUI.FadeAlpha.transform, false);
        instance.transform.SetSiblingIndex(9);

        var defaultWRP = lobbyUI.transform.GetComponentInChildren<bl_WaitingRoomBase>(true);
        if(defaultWRP != null)
        {
            if (!defaultWRP.name.Contains("[Default]"))
            {
                defaultWRP.name += " [Default]";
            }
            defaultWRP.gameObject.SetActive(false);
            EditorUtility.SetDirty(defaultWRP);
        }

        EditorUtility.SetDirty(instance);
        MarkSceneDirty();
        ShowSuccessIntegrationLog(instance);
        return true;
    }

    [MenuItem("MFPS/Addons/Waiting Room/Integrate")]
    static void Open()
    {
        GetWindow<WaitingRoomProIntegration>();
    }
}