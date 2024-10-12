using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class bl_ZombieRagdoll : MonoBehaviourPun, IPunObservable
{
    public float DestroyTime = 30f;
    [HideInInspector] public Rigidbody[] rigidbodys;
    private AIController cont;
    private bl_AttackPlayer attack;
    private bl_ZombiesHealthBar healthBar;
    private PhotonAnimatorView photonAnim;
    private Animator animator;
    private bool isRagdolled;

    // Start is called before the first frame update
    void Start()
    {
        cont = GetComponent<AIController>();
        attack = GetComponent<bl_AttackPlayer>();
        animator = GetComponent<Animator>();
        healthBar = GetComponent<bl_ZombiesHealthBar>();    
        rigidbodys = GetComponentsInChildren<Rigidbody>();
        photonAnim = GetComponent<PhotonAnimatorView>();
        DeactivateRagdoll();
    }

    public void DeactivateRagdoll()
    {
        isRagdolled = false;
        foreach (var rigidbody in rigidbodys)
        {
            rigidbody.isKinematic = true;
        }
        animator.enabled = true;
        cont.enabled = true;
        healthBar.enabled = true;
        attack.enabled = true;
    }
    [PunRPC]
    public void ActivateRagdoll()
    {
        photonView.RPC(nameof(DestoryZombieSync), RpcTarget.All);
    }

    [PunRPC]
    public void DestoryZombieSync()
    {
        isRagdolled = true;
        if (isRagdolled)
        {
            foreach (var rigidbody in rigidbodys)
            {
                rigidbody.isKinematic = false;
            }
            attack.CancelInvoke("DealDamageFunction");
            attack.CancelInvoke("PlayAttackAnimation");
            animator.enabled = false;
            healthBar.Content.SetActive(false);
            if (gameObject.CompareTag("Zombie"))
            {
                gameObject.tag = "IgnoreBullet";
            }
            else
            {
                Debug.LogError("MFPS Tags are not setup");
            }

            cont.CancelInvoke(nameof(cont.PlayRandomScream));
            cont.CancelInvoke(nameof(cont.Base));
            cont.ClosestPlayer = null;
            cont.agent.SetDestination(cont.gameObject.transform.position);
            if (cont.AllowZombieFootSteps)
            {
                Destroy(cont.footstep);
            }
            Destroy(attack, 0f);
            Destroy(healthBar, 0f);
            Destroy(animator, 0f);
            Destroy(cont, 0f);
            Destroy(photonAnim, 0f);
            Destroy(gameObject, DestroyTime);
        }
    }
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isRagdolled);
        }
        else
        {
           isRagdolled = (bool)stream.ReceiveNext();
        }
    }
}
