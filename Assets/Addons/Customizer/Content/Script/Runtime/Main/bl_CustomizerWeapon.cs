using MFPS.Addon.Customizer;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class bl_CustomizerWeapon : MonoBehaviour
{
    #region Public members
    public int WeaponID = 0;
    public bool ApplyOnStart = true;
    public CustomizerAttachments Attachments;
    public CustomizerCamoRender CamoRender;

    public string WeaponName => bl_CustomizerData.Instance.Weapons[WeaponID].WeaponName;
    #endregion

    #region Private members
    static readonly string CUSTOMIZER_RPC_NAME = "SyncCustomizer";
    private bl_Gun m_gun = null;
    private bl_Gun Gun { get { if (m_gun == null) { m_gun = GetComponent<bl_Gun>(); } return m_gun; } }
    private bool isFPWeapon = false;
    private int[] AttachmentsIds = new int[] { 0, 0, 0, 0, 0 };
    private bool isSync = false;
    private PhotonView photonView;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        isFPWeapon = Gun != null;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        if (photonView == null) { photonView = transform.GetComponentInParent<PhotonView>(); }
        if (photonView != null && !photonView.IsMine)
        {
            return;
        }

        if (!isSync)
        {
            if (isFPWeapon)
            {
                SyncToOthers(Gun.GunID);
                bl_PhotonCallbacks.PlayerEnteredRoom += OnNewPlayerEnter;
            }
            else
            {
#if MFPSTPV
                if (bl_CameraViewSettings.IsThirdPerson())
                {
                    var netGun = GetComponent<bl_NetworkGun>();
                    if (netGun != null && netGun.LocalGun != null)
                    {
                        SyncToOthers(netGun.LocalGun.GunID);
                    }
                }
#endif
            }
        }
    }

    /// <summary> 
    /// 
    /// </summary>
    public void SyncToOthers(int gunID)
    {
        LoadAttachments();
        ApplyAttachments();
        string line = bl_CustomizerData.Instance.CompileArray(AttachmentsIds);
        photonView.RPC(CUSTOMIZER_RPC_NAME, RpcTarget.Others, gunID, line);
        isSync = true;
    }

    /// <summary>
    /// 
    /// </summary>
    public void LoadAttachments()
    {
        AttachmentsIds = bl_CustomizerData.Instance.LoadAttachmentsForWeapon(WeaponName);
    }

    /// <summary>
    /// 
    /// </summary>
    public void ApplyAttachments()
    {
        Attachments.Apply(AttachmentsIds);
        CamoRender.ApplyCamo(WeaponName, AttachmentsIds[(int)bl_AttachType.Camo]);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="customIds"></param>
    public void ApplyAttachments(int[] customIds)
    {
        Attachments.Apply(customIds);
        CamoRender.ApplyCamo(WeaponName, customIds[(int)bl_AttachType.Camo]);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="line"></param>
    public void ApplyAttachments(string line)
    {
        AttachmentsIds = bl_CustomizerData.Instance.DecompileLine(line);
        ApplyAttachments();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    void OnNewPlayerEnter(Player player)
    {
        if (isFPWeapon && Gun != null)
        {
            string line = bl_CustomizerData.Instance.CompileArray(AttachmentsIds);
            photonView.RPC(CUSTOMIZER_RPC_NAME, RpcTarget.Others, Gun.GunID, line);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public bool RenderIsAttachment(GameObject model)
    {
        if (model == null) return false;
        return Attachments.CheckIfIsAttachment(model);
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDestroy()
    {
        bl_PhotonCallbacks.PlayerEnteredRoom -= OnNewPlayerEnter;
    }

    public bool ISFPWeapon() => Gun != null;

#if UNITY_EDITOR

    public void BuildAttachments()
    {
        if (Attachments == null) { Attachments = new CustomizerAttachments(); }
        Attachments.Suppressers.Clear();
        Attachments.Sights.Clear();
        Attachments.Foregrips.Clear();
        Attachments.Magazines.Clear();
        CustomizerInfo info = bl_CustomizerData.Instance.GetWeapon(WeaponName);

        if (info == null)
        {
            Debug.LogWarning($"{WeaponName} was not found in the Customizer data, make sure you have set up the weapon data first.");
        }

        for (int i = 0; i < info.Attachments.Suppressers.Count; i++)
        {
            Attachments.Suppressers.Add(new CustomizerModelInfo());
            Attachments.Suppressers[i].SetInfo(info.Attachments.Suppressers[i]);
        }
        for (int i = 0; i < info.Attachments.Sights.Count; i++)
        {
            Attachments.Sights.Add(new CustomizerModelInfo());
            Attachments.Sights[i].SetInfo(info.Attachments.Sights[i]);
        }
        for (int i = 0; i < info.Attachments.Foregrips.Count; i++)
        {
            Attachments.Foregrips.Add(new CustomizerModelInfo());
            Attachments.Foregrips[i].SetInfo(info.Attachments.Foregrips[i]);
        }
        for (int i = 0; i < info.Attachments.Magazines.Count; i++)
        {
            Attachments.Magazines.Add(new CustomizerModelInfo());
            Attachments.Magazines[i].SetInfo(info.Attachments.Magazines[i]);
        }
    }

    public void RefreshAttachments()
    {
        CustomizerInfo info = bl_CustomizerData.Instance.GetWeapon(WeaponName);
        if (Attachments.Suppressers.Count != info.Attachments.Suppressers.Count)
        {
            for (int i = 0; i < info.Attachments.Suppressers.Count; i++)
            {
                if (Attachments.Suppressers.Count - 1 < i)
                {
                    Attachments.Suppressers.Add(new CustomizerModelInfo());
                }
                Attachments.Suppressers[i].SetInfo(info.Attachments.Suppressers[i]);
            }
        }
        if (Attachments.Sights.Count != info.Attachments.Sights.Count)
        {
            for (int i = 0; i < info.Attachments.Sights.Count; i++)
            {
                if (Attachments.Sights.Count - 1 < i)
                {
                    Attachments.Sights.Add(new CustomizerModelInfo());
                }
                Attachments.Sights[i].SetInfo(info.Attachments.Sights[i]);
            }
        }
        if (Attachments.Foregrips.Count != info.Attachments.Foregrips.Count)
        {
            for (int i = 0; i < info.Attachments.Foregrips.Count; i++)
            {
                if (Attachments.Foregrips.Count - 1 < i)
                {
                    Attachments.Foregrips.Add(new CustomizerModelInfo());
                }
                Attachments.Foregrips[i].SetInfo(info.Attachments.Foregrips[i]);
            }
        }
        if (Attachments.Magazines.Count != info.Attachments.Magazines.Count)
        {
            for (int i = 0; i < info.Attachments.Magazines.Count; i++)
            {
                if (Attachments.Magazines.Count - 1 < i)
                {
                    Attachments.Magazines.Add(new CustomizerModelInfo());
                }
                Attachments.Magazines[i].SetInfo(info.Attachments.Magazines[i]);
            }
        }
    }
#endif
}