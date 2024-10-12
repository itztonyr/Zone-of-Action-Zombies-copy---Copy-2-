using MFPS.Addon.Customizer;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class bl_CustomizerSetupHelper : EditorWindow
{
    public bl_Customizer customizerTarget;

    public static void Open(bl_Customizer customizer)
    {
        var window = GetWindow<bl_CustomizerSetupHelper>();
        window.customizerTarget = customizer;
    }

    private void OnGUI()
    {
        customizerTarget = EditorGUILayout.ObjectField("Customizer Target", customizerTarget, typeof(bl_Customizer), true) as bl_Customizer;
        GUILayout.Space(20);
        if (GUILayout.Button("Fetch Attachments From Hierarchy"))
        {
            if (EditorUtility.DisplayDialog("Confirm Action", "This operation will overwrite the current attachment list with the attachments transform of the hierarchy, are you sure to do this?", "Yes", "Cancel"))
            {
                var list = FetchAttachmentsChilds(customizerTarget.Positions.BarrelRoot, "Empty");
                customizerTarget.Attachments.Suppressers = list;

                list = FetchAttachmentsChilds(customizerTarget.Positions.CylinderRoot);
                customizerTarget.Attachments.Magazines = list;

                list = FetchAttachmentsChilds(customizerTarget.Positions.OpticsRoot, "Iron Sight");
                customizerTarget.Attachments.Sights = list;

                list = FetchAttachmentsChilds(customizerTarget.Positions.FeederRoot, "Empty");
                customizerTarget.Attachments.Foregrips = list;

                EditorUtility.SetDirty(customizerTarget);
            }
        }
    }

    private List<CustomizerModelInfo> FetchAttachmentsChilds(Object attachmentRoot, string firstEmpty = "")
    {
        if (attachmentRoot == null) return null;

        var root = (attachmentRoot as GameObject).transform;

        int childCount = root.childCount;
        var list = new List<CustomizerModelInfo>();

        if (!string.IsNullOrEmpty(firstEmpty))
        {
            list.Add(new CustomizerModelInfo()
            {
                Name = firstEmpty,
                ID = 0,
            });
        }

        int extraID = string.IsNullOrEmpty(firstEmpty) ? 0 : 1;
        for (int i = 0; i < childCount; i++)
        {
            list.Add(new CustomizerModelInfo()
            {
                Name = root.GetChild(i).name,
                ID = i + extraID,
                Model = root.GetChild(i).gameObject
            });
        }

        return list;
    }
}