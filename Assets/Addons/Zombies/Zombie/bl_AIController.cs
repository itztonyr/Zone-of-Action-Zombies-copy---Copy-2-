using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using ZombiesGameMode;

public class AIController : bl_MonoBehaviour
{
    //Public
    #region Public
    [Header("General Setings")]
    [Space(5)]
    public float ZombieUpdateRate = 3f;
    public float WaitTimeForSpawn;
    public float WaitForRoundEnd;
    public Animator animator;
    [Header("Audio Setings")]
    [Space(5)]
    public AudioSource Source;
    public List<AudioClip> RandomScream;
    public float timeBetweenScreems;
    [Header("MFPS Refrences")]
    [Space(5)]
    [LovattoToogle] public bool AllowZombieFootSteps;
    public bl_Footstep footstep;
    [Header("Debug")]

    #endregion
    //Private
    #region Private
    private float velocityMagnitud = 0;
    private bool isScreaming;
    [HideInInspector] public AudioClip scream;
    [HideInInspector] public AudioClip randomscream;
    [HideInInspector] public NavMeshAgent agent;
    private bool canMove = false;
    private bool canMoveOnSpawn = false;
    // Declare sync variables
    private Vector3 networkPosition;
    private Quaternion networkRotation;
    [HideInInspector] public MFPSPlayer ClosestPlayer;
    private List<MFPSPlayer> PlayerList = new List<MFPSPlayer>();
    private List<MFPSPlayer> AlivePlayerList = new List<MFPSPlayer>();
    #endregion


    // Start is called before the first frame update
    protected override void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        //spawn zombie anim :D
        animator.Play("SpawnIn", 0, 0);
        this.InvokeAfter(5, () => { Invoke(nameof(StartFunction), 0); });
        this.InvokeAfter(5, () => { canMove = true; });
        this.InvokeAfter(5, () => { canMoveOnSpawn = true; });
    }
    protected override void OnEnable()
    {
        bl_EventHandler.onLocalPlayerDeath += OnLocalPlayerDeath;
        bl_EventHandler.onLocalPlayerSpawn += OnLocalPlayerSpawn;
    }

    protected override void OnDisable()
    {
        bl_EventHandler.onLocalPlayerDeath -= OnLocalPlayerDeath;
        bl_EventHandler.onLocalPlayerSpawn -= OnLocalPlayerSpawn;
    }

    void OnLocalPlayerDeath()
    {
        bl_Zombies.Instance.UpdateTargetList();
    }
    void OnLocalPlayerSpawn()
    {
        bl_Zombies.Instance.UpdateTargetList();
    }
    private void StartFunction()
    {
        InvokeRepeating("PlayRandomScream", 0f, timeBetweenScreems);
        InvokeRepeating("Base", 0f, ZombieUpdateRate); //unoptimized
    }
    //called each second soo we dont put footstep in normal update
    private void Update()
    {
        PlayerList = bl_Zombies.Instance.PlayerSort;
        for(int i = 0; i < PlayerList.Count; i++)
        {
            if (PlayerList[i].isAlive)
            {
                AlivePlayerList.Add(PlayerList[i]);
            }
            else
            {
                AlivePlayerList.Remove(PlayerList[i]);
            }
        }
        if (PlayerList.Count <= 0)
            return;
        ClosestPlayer = GetClosestEnemy(AlivePlayerList);
    }
    public override void OnSlowUpdate()
    {
        velocityMagnitud = agent.velocity.magnitude;
        FootStep();
    }

    MFPSPlayer GetClosestEnemy(List<MFPSPlayer> enemies)
    {
        MFPSPlayer bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;
        foreach(MFPSPlayer potentialTarget in enemies)
        {
            Vector3 directionToTarget = potentialTarget.Actor.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = potentialTarget;
            }
        }
        return bestTarget;
    }

    public void Base()
    {
        if (!canMove || !canMoveOnSpawn) return;
        if (ClosestPlayer == null)
        {
            animator.Play("Idle");
        }

        if (ClosestPlayer == null)
            return;
        float distance = Vector3.Distance(transform.position, ClosestPlayer.Actor.position);

        if (distance < agent.stoppingDistance)
        {
            canMove = false;
        }
        else
        {
            animator.Play("Run");

            canMove = true;
        }

        if (canMoveOnSpawn && canMove)
        {
            agent.SetDestination(ClosestPlayer.Actor.position);
        }

    }
    public void PlayRandomScream()
    {
        if (!isScreaming)
        {
            randomscream = RandomScream[Random.Range(0, RandomScream.Count)];
            Source.clip = randomscream;
            Source.Play();
            isScreaming = true;

            Invoke(nameof(ResetScream), timeBetweenScreems);
        }

    }
    public void FootStep()
    {
        if (AllowZombieFootSteps)
        {
            if (velocityMagnitud > 0.2f)
                footstep.UpdateStep(agent.speed);
        }
    }
    private void ResetScream()
    {
        isScreaming = false;
    }
}
