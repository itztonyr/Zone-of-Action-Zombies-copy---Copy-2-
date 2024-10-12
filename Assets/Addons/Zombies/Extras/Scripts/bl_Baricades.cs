using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class bl_Baricades : MonoBehaviour
{
    [Header("Baricade Settings")]
    [Space(5)]
    public GameObject BoardsPositions;
    public List<bl_Board> AllBaricadeObjects;
    [Header("References Settings")]
    [Space(5)]
    public NavMeshObstacle obstacle;


    private int totalHealth;
    [HideInInspector] public int maxAmount;
    private int BaricadeHealth = 10;
    private Image UI;
    private bl_RoundManager roundmanager;
    private bl_BaricadeManager Manager;
    // Start is called before the first frame update
    private void Awake()
    {
        totalHealth = AllBaricadeObjects.Count * BaricadeHealth;
        maxAmount = AllBaricadeObjects.Count;
        UI = GetComponentInChildren<Image>();
        Manager = FindObjectOfType<bl_BaricadeManager>();
        roundmanager = FindObjectOfType<bl_RoundManager>();
        UI.gameObject.SetActive(false);
    }

    // Update is called once per frame
    private void Update()
    {
        if (bl_GameManager.Instance.LocalPlayerReferences == null) return;
        bool isInRange = Vector3.Distance(transform.position, bl_GameManager.Instance.LocalPlayer.transform.position) <= Manager.interactRange;
        if (isInRange && AllBaricadeObjects.Count < maxAmount)
        {
            UI.gameObject.SetActive(true);
        }
        else
        {
            UI.gameObject.SetActive(false);
        }
        if (isInRange && Input.GetKeyDown(Manager.interactionKey))
        {
            AttemptToRepair();
        }
        if (AllBaricadeObjects.Count <= 0) 
        {
            obstacle.enabled = false;
        }
    }

    private void AttemptToRepair()
    {
        if (maxAmount == AllBaricadeObjects.Count) return;
        Manager.RepairABaricade(this, true);
    }

    public void AddBaricade(bl_Board board)
    {
        board.gameObject.SetActive(true);
        AllBaricadeObjects.Add(board);
    }

    public void RemoveBaricade(bl_Board board)
    {
        board.gameObject.SetActive(false);
        AllBaricadeObjects.Remove(board);
    }
}
# if UNITY_EDITOR
[CustomEditor(typeof(bl_Baricades))]
class bl_FindAllBoards : Editor
{
    private bl_Baricades manager;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        manager = (bl_Baricades)target;

        if (GUILayout.Button("Collect all boards"))
        {
            AddFields();
        }
    }
    public void AddFields()
    {
        if (manager.AllBaricadeObjects.Count > 0)
        {
            Debug.Log($"<color=#00FF00>Already Integrated!</color>");
            return;
        }
        manager.AllBaricadeObjects.Clear();
        manager.AllBaricadeObjects.AddRange(manager.BoardsPositions.GetComponentsInChildren<bl_Board>());
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
        Debug.Log("sweet!, all boards found and added: " + manager.AllBaricadeObjects.Count);
    }
}
#endif