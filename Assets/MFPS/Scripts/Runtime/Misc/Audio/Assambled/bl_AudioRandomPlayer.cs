using UnityEngine;

namespace MFPS.Audio
{
    public class bl_AudioRandomPlayer : MonoBehaviour
    {
        [LovattoToogle] public bool playOnEnable = true;
        [SerializeField] private AudioClip[] clips = null;

        private AudioSource audioSource;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            if (playOnEnable) { PlayRandom(); }
        }

        /// <summary>
        /// 
        /// </summary>
        public void PlayRandom()
        {
            if (clips.Length <= 0) return;

            if (audioSource == null) { audioSource = GetComponent<AudioSource>(); }
            if (audioSource == null) { audioSource = gameObject.AddComponent<AudioSource>(); }

            int r = Random.Range(0, clips.Length);

            audioSource.clip = clips[r];
            audioSource.Play();
        }
    }
}