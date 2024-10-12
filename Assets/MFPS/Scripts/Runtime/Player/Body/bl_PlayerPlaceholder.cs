using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class bl_PlayerPlaceholder : MonoBehaviour, IRealPlayerReference
{
    public Camera playerCamera = null;
    [SerializeField] private Transform headTransform = null;
    [SerializeField] private Transform tpWeaponHolder = null;
    [SerializeField] private Transform fpWeaponHolder = null;

    public bool CalibratingAim { get; set; } = false;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="container"></param>
    public void PlaceFPWeaponContainer(Transform container)
    {
        container.SetParent(fpWeaponHolder, true);
        container.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="container"></param>
    public void PlaceTPWeaponContainer(Transform container)
    {
        if (tpWeaponHolder.childCount > 0)
        {
            Debug.LogWarning("The placeholder player does already have a weapon container instance.");
        }

        container.SetParent(tpWeaponHolder, true);
        container.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static bl_PlayerPlaceholder GetInstance()
    {
        var script = FindObjectOfType<bl_PlayerPlaceholder>();
        if (script != null) { return script; }

        var go = Instantiate(bl_GlobalReferences.I.PlayerPlaceholder);
        go.name = "Player Placeholder";

        return go.GetComponent<bl_PlayerPlaceholder>();
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnGUI()
    {
        DoDrawCenterLines();
    }

    /// <summary>
    /// 
    /// </summary>
    void DoDrawCenterLines()
    {
        if (!CalibratingAim) return;

        float lineThickness = 1;
        var rect = new Rect(0, (Screen.height * 0.5f) - (lineThickness * 0.5f), Screen.width, lineThickness);
        GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill);

        rect = new Rect((Screen.width * 0.5f) - (lineThickness * 0.5f), 0, lineThickness, Screen.height);
        GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bl_WorldWeaponsContainer GetTPContainerInstance()
    {
        return GetComponentInChildren<bl_WorldWeaponsContainer>(true);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bl_ViewWeaponsContainer GetFPContainerInstance()
    {
        return GetComponentInChildren<bl_ViewWeaponsContainer>(true);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Camera GetPlayerCamera()
    {
        return playerCamera;
    }

    public bl_PlayerReferences GetReferenceRoot() => null;

    private void OnDrawGizmos()
    {
        if (playerCamera == null) return;

        Gizmos.color = new Color(0.38394f, 1f, 0.1273585f, 0.33f);
        Gizmos.DrawLine(transform.position, headTransform.position);
        Gizmos.DrawLine(headTransform.position, headTransform.position + (headTransform.forward * 0.4f));

#if UNITY_EDITOR
        // show a handle circle at the head position
        Handles.color = new Color(0.38394f, 1f, 0.1273585f, 0.33f);
        Handles.DrawWireDisc(headTransform.position, headTransform.forward, 0.2f);
        // show a handle circle at the transform position facing up
        Handles.DrawWireDisc(transform.position, transform.up, 0.5f);
#endif
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(bl_PlayerPlaceholder))]
public class bl_PlayerPlaceholderEditor : Editor
{
    public bl_PlayerPlaceholder script;
    public bool showSky = false;
    private bool wasShowingSky = false;

    private void OnEnable()
    {
        script = (bl_PlayerPlaceholder)target;
        if (script.playerCamera != null)
        {
            showSky = script.playerCamera.clearFlags == CameraClearFlags.Skybox;
        }
        wasShowingSky = showSky;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.Space(EditorGUIUtility.singleLineHeight);

        showSky = GUILayout.Toggle(showSky, "Show Skybox", EditorStyles.miniButton);
        if (showSky != wasShowingSky)
        {
            if (script.playerCamera != null)
            {
                script.playerCamera.clearFlags = showSky ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
                script.playerCamera.cullingMask = showSky ? -1 : 1 << 8;
            }
            wasShowingSky = showSky;
        }
    }
}
#endif