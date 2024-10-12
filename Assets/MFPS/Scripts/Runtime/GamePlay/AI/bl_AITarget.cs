using UnityEngine;

public class bl_AITarget : MonoBehaviour
{
    public bl_PlayerReferencesCommon playerReferences = null;

    private Transform m_Transform;
    private string targetName;
    private bool isDeath = false;

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        m_Transform = transform;
        targetName = playerReferences != null ? playerReferences.PlayerName : m_Transform.root.name;
        if (playerReferences != null) { playerReferences.onDie += OnDie; }
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDie()
    {
        isDeath = true;
    }

    /// <summary>
    /// This should be called when this target is attacked (hit or almost hit)
    /// </summary>
    public void OnAttacked(bl_PlayerReferencesCommon attacker)
    {
        if (playerReferences != null)
        {
            playerReferences.OnAttacked(attacker);
        }
    }

    public Vector3 position
    {
        get
        {
            return m_Transform.position;
        }
    }

    public string Name
    {
        get
        {
            return targetName;
        }
    }

    public Transform Transform
    {
        get
        {
            return m_Transform;
        }
    }

    public bool IsDeath
    {
        get
        {
            return isDeath;
        }
    }
}