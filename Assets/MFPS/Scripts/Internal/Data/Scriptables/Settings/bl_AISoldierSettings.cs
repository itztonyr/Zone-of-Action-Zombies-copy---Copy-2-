using UnityEngine;

namespace MFPS.Runtime.AI
{
    [CreateAssetMenu(fileName = "AI Soldier Settings", menuName = "MFPS/AI/Soldier Settings")]
    public class bl_AISoldierSettings : ScriptableObject
    {
        [Header("Speeds")]
        public float walkSpeed = 4;
        public float runSpeed = 8;
        public float crounchSpeed = 2;
        public float rotationSmoothing = 6.0f;

        [Header("Ranges")]
        public float visionRange = 10f;
        public float fieldOfView = 90f; // Field of view in degrees.
        public float closeRange = 10.0f;
        public float mediumRange = 25.0f;
        public float farRange = 20f;
        public float limitRange = 50f;

        [Header("Misc")]
        [Tooltip("The time in seconds that takes to the bot to react when have visual to the target, this determine the delay before start shooting.")]
        public float reactionTime = 0.5f;
        [Tooltip("Time in seconds that determine how long the bot will keep looking at the target after lost visual.")]
        public float targetLostFocusTime = 3.0f;
        [Tooltip("The health threshold to consider it as critical.")]
        [Range(1, 50)] public float criticalHealth = 30;
    }
}