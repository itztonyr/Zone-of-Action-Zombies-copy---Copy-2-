using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class bl_BaricadeManager : MonoBehaviourPun
{
    [Header("Main")]
    [Space(5)]
    public KeyCode interactionKey = KeyCode.F;
    public int RepairPoints = 10;
    public float interactRange = 3f;
    public List<bl_Baricades> baricadedBaricades;
    public List<bl_Baricades> destroyedBaricades;


    private bl_RoundManager roundManager;
    private void Awake()
    {
        roundManager = FindObjectOfType<bl_RoundManager>(true);
    }
    /// <summary>
    /// Add a perk to a active list
    /// </summary>
    /// <param name="field"></param>
    /// <param name="fromMaster"></param>
    [PunRPC]
    public void AddFieldToList(bl_Baricades field, bool fromMaster)
    {
        if (fromMaster)
        {
            photonView.RPC(nameof(AddFieldToList), RpcTarget.AllBuffered, field, false);
            return;
        }
        if (bl_PhotonNetwork.IsMasterClient)
        {
            baricadedBaricades.Add(field);
            destroyedBaricades.Remove(field);
        }
    }
    /// <summary>
    /// Remove a perk from being accessed
    /// </summary>
    /// <param name="field"></param>
    /// <param name="fromMaster"></param>
    [PunRPC]
    public void RemoveFieldToList(bl_Baricades field, bool fromMaster)
    {
        if (fromMaster)
        {
            photonView.RPC(nameof(RemoveFieldToList), RpcTarget.AllBuffered, field, false);
            return;
        }
        if (bl_PhotonNetwork.IsMasterClient)
        {
            baricadedBaricades.Remove(field);
            destroyedBaricades.Add(field);
        }
    }
    [PunRPC]
    public void DisambleNavMeshObsticle(bl_Baricades field, bool fromMaster)
    {
        if (fromMaster)
        {
            photonView.RPC(nameof(DisambleNavMeshObsticle), RpcTarget.AllBuffered, field, false);
            return;
        }
        if(PhotonNetwork.IsMasterClient)
        {
            field.obstacle.enabled = false;
        }
    }
    [PunRPC]
    public void RepairABaricade(bl_Baricades field, bool fromMaster)
    {
        if (fromMaster)
        {
            photonView.RPC(nameof(DisambleNavMeshObsticle), RpcTarget.All, field, false);
            return;
        }
        if (PhotonNetwork.IsMasterClient)
        {
            if (field.maxAmount >= field.AllBaricadeObjects.Count)
                return;

            this.InvokeAfter(1, () =>
            {
                field.obstacle.enabled = true;
                field.AddBaricade(field.AllBaricadeObjects[0]);
                roundManager.IncreaseScore(RepairPoints);
            });
        }
    }
}

# if UNITY_EDITOR
[CustomEditor(typeof(bl_BaricadeManager))]
class bl_FindAllBaricades : Editor
{
    private bl_BaricadeManager manager;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        manager = (bl_BaricadeManager)target;

        if (GUILayout.Button("Collect all baricades"))
        {
            AddFields();
        }
    }
    public void AddFields()
    {
        if (manager.baricadedBaricades.Count > 0)
        {
            Debug.Log($"<color=#00FF00>Already Integrated!</color>");
            return;
        }
        manager.baricadedBaricades.Clear();
        manager.baricadedBaricades.AddRange(FindObjectsOfType<bl_Baricades>());
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
        Debug.Log("sweet!, all barricades found and added: " + manager.baricadedBaricades.Count);
    }
}
#endif
