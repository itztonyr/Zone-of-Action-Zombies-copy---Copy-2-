using UnityEngine;

namespace MFPS.Runtime.Level
{
    [RequireComponent(typeof(AudioSource))]
    public class bl_JumpPlatform : MonoBehaviour
    {
        public Vector3 ForceDirection;
        public float ForceMultiplier = 1;
        [SerializeField] private Transform directionIndicator = null;
        [SerializeField] private AudioClip JumpSound;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter(Collider other)
        {
            if (other.isLocalPlayerCollider())
            {
                var fpc = other.GetComponent<bl_FirstPersonControllerBase>();
                fpc.AddForce(ForceDirection * ForceMultiplier, true);
                if (JumpSound != null) { AudioSource.PlayClipAtPoint(JumpSound, transform.position); }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, ForceDirection.normalized);

            if (directionIndicator != null)
            {
                directionIndicator.LookAt(transform.position + (ForceDirection * ForceMultiplier));
            }
        }
    }
}