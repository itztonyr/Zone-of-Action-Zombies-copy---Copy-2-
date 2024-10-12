using UnityEngine;

public class bl_RandomZombieAnimation : StateMachineBehaviour
{
    [SerializeField] string[] m_StateNames = new string[0];

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var index = UnityEngine.Random.Range(0, m_StateNames.Length);
        var stateName = m_StateNames[index];

        animator.Play(stateName, layerIndex);
    }
}
