using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using MFPSEditor;
#endif

public class bl_SimpleGizmoDrawer : MonoBehaviour
{
    [System.Serializable]
    public enum Shape
    {
        Cube
    }
    public Color IconColor;
    private Shape shape = Shape.Cube;
    private float DistanceDraw = 1f;

#if UNITY_EDITOR
    DomeGizmo _gizmo = null;
    private void OnDrawGizmos()
    {
        Draw();
    }
    void Draw()
    {
        if (Application.isPlaying) return;

        Color c = IconColor;
        Gizmos.color = c;

        if (shape == Shape.Cube)
        {
            var matrix = Gizmos.matrix;

            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            c.a = 0.3f;
            Gizmos.color = c;
            Vector3 basePos = Vector3.zero;
            basePos.y += 1.1f;
            Gizmos.DrawCube(basePos, new Vector3(transform.localScale.x, 2.2f, transform.localScale.z));
            Gizmos.DrawWireCube(basePos, new Vector3(transform.localScale.x, 2.2f, transform.localScale.z));
            Gizmos.matrix = matrix;
        }

    }
#endif
}
