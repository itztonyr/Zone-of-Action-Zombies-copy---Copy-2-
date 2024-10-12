using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MFPS.Addon.Customizer
{
    [CustomEditor(typeof(bl_Customizer))]
    public class bl_CustomizerEditor : Editor
    {

        bl_Customizer script;
        SerializedProperty attac;
        SerializedProperty camor;
        SerializedProperty positions;
        private string weaponName = "";
        private bool editOpen = false;
        bl_CustomizerManager customizerManager;

        private void OnEnable()
        {
            attac = serializedObject.FindProperty("Attachments");
            positions = serializedObject.FindProperty("Positions");
            camor = serializedObject.FindProperty("CamoRender");
            customizerManager = FindObjectOfType<bl_CustomizerManager>();
        }

        public override void OnInspectorGUI()
        {
            script = (bl_Customizer)target;
            weaponName = script.WeaponName;

            if (HasAttachmentsDifferences())
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.HelpBox($"CustomizerData attachment list and this attachments list are not synced, do you want to sync them automatically", MessageType.Warning);
                if (GUILayout.Button("Sync Attachment List"))
                {
                    SyncAttachmentList();
                }
                EditorGUILayout.EndVertical();
            }

            GUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal("box");
            script.WeaponID = EditorGUILayout.Popup("Customizer ID", script.WeaponID, bl_CustomizerData.Instance.GetWeaponStringArray(), EditorStyles.toolbarDropDown);
            GUILayout.Space(5);
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                script.RefreshAttachments();
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.LabelField(script.WeaponName);
            if (customizerManager != null && !customizerManager.AllCustom.Exists(x =>
            {
                if (x == null) return false;
                return x.WeaponID == script.WeaponID;
            }))
            {
                if (GUILayout.Button("Listed Customizer Weapon"))
                {
                    customizerManager.AllCustom.Add(script);
                    EditorUtility.SetDirty(customizerManager);
                }
            }
            if (GUI.changed)
            {
                script.WeaponName = bl_CustomizerData.Instance.Weapons[script.WeaponID].WeaponName;
                if (script.WeaponName != weaponName)
                {
                    script.BuildAttachments();
                    weaponName = script.WeaponName;
                }
            }

            serializedObject.Update();
            GUILayout.BeginHorizontal("box");
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(camor, true);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("box");
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(attac, true);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("box");
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(positions, true);
            GUILayout.EndHorizontal();
            serializedObject.ApplyModifiedProperties();
            GUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            script.SeparateCurve = EditorGUILayout.CurveField("Separate Curve", script.SeparateCurve);
            script.ChangeMovementPath = EditorGUILayout.CurveField("Change Movement Path", script.ChangeMovementPath);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal("box");
            if (GUILayout.Button("Open Helper"))
            {
                bl_CustomizerSetupHelper.Open(script);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.EndChangeCheck();
            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void SyncAttachmentList()
        {
            var data = bl_CustomizerData.Instance.Weapons[script.WeaponID];
            var list = script.Attachments.Sights;

            data.Attachments.Sights = new List<AttachInfo>();
            for (int i = 0; i < list.Count; i++)
            {
                data.Attachments.Sights.Add(new AttachInfo()
                {
                    Name = list[i].Name,
                    ID = list[i].ID
                });
            }

            list = script.Attachments.Suppressers;
            data.Attachments.Suppressers = new List<AttachInfo>();
            for (int i = 0; i < list.Count; i++)
            {
                data.Attachments.Suppressers.Add(new AttachInfo()
                {
                    Name = list[i].Name,
                    ID = list[i].ID
                });
            }

            list = script.Attachments.Foregrips;
            data.Attachments.Foregrips = new List<AttachInfo>();
            for (int i = 0; i < list.Count; i++)
            {
                data.Attachments.Foregrips.Add(new AttachInfo()
                {
                    Name = list[i].Name,
                    ID = list[i].ID
                });
            }

            list = script.Attachments.Magazines;
            data.Attachments.Magazines = new List<AttachInfo>();
            for (int i = 0; i < list.Count; i++)
            {
                data.Attachments.Magazines.Add(new AttachInfo()
                {
                    Name = list[i].Name,
                    ID = list[i].ID
                });
            }

            EditorUtility.SetDirty(bl_CustomizerData.Instance);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool HasAttachmentsDifferences()
        {
            var data = bl_CustomizerData.Instance.Weapons[script.WeaponID];

            var list = script.Attachments.Sights;
            var dataList = data.Attachments.Sights;
            if (list.Count != dataList.Count) return true;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Name != dataList[i].Name)
                {
                    return true;
                }
            }

            list = script.Attachments.Suppressers;
            dataList = data.Attachments.Suppressers;
            if (list.Count != dataList.Count) return true;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Name != dataList[i].Name)
                {
                    return true;
                }
            }

            list = script.Attachments.Foregrips;
            dataList = data.Attachments.Foregrips;
            if (list.Count != dataList.Count) return true;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Name != dataList[i].Name)
                {
                    return true;
                }
            }

            list = script.Attachments.Magazines;
            dataList = data.Attachments.Magazines;
            if (list.Count != dataList.Count) return true;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Name != dataList[i].Name)
                {
                    return true;
                }
            }

            return false;
        }

        private void OnSceneGUI()
        {
            script = (bl_Customizer)target;

            if (editOpen)
            {
                script.Positions.BarrelPosition.position = Handles.PositionHandle(script.Positions.BarrelPosition.position, Quaternion.identity);
                script.Positions.OpticPosition.position = Handles.PositionHandle(script.Positions.OpticPosition.position, Quaternion.identity);
                script.Positions.FeederPosition.position = Handles.PositionHandle(script.Positions.FeederPosition.position, Quaternion.identity);
                script.Positions.CylinderPosition.position = Handles.PositionHandle(script.Positions.CylinderPosition.position, Quaternion.identity);

                if (GUI.changed)
                {
                    EditorUtility.SetDirty(target);
                }
            }
            if (Handles.Button(script.transform.position, Quaternion.identity, 0.05f, 0.1f, Handles.RectangleHandleCap))
            {
                editOpen = !editOpen;
            }
        }
    }
}