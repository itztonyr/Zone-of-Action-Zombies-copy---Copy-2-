using UnityEngine;
using Photon.Pun;

public class ZombieSync : MonoBehaviourPun, IPunObservable
{
    private bl_AttackPlayer attack;
    private AIController ai;
    [HideInInspector] public PhotonView view;
    public static ZombieSync Instance;
    const float maxLerpTime = 0.5f;
    Vector3 latestPosition = Vector3.zero;
    Vector3 positionAtLastUpdate = Vector3.zero;

    float timer = 0;
    private void Awake()
    {
        Instance = this;
        // Disable the script on the local player's zombie
        if (!photonView.IsMine)
        {
            this.enabled = false;
        }
    }


    private void Update()
    {
        if (!photonView.IsMine)
        {
            if (timer < maxLerpTime)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / maxLerpTime);
                transform.position = Vector3.Lerp(positionAtLastUpdate, latestPosition, t);
            }
        }
    }

    // Called to send data to the network
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

        if (stream.IsWriting)
        {
            // This is the local player, send data to others
            //general position
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {

            timer = 0;
            latestPosition = (Vector3)stream.ReceiveNext();
            transform.rotation = (Quaternion)stream.ReceiveNext();
            positionAtLastUpdate = transform.position;
        }
    }
}
