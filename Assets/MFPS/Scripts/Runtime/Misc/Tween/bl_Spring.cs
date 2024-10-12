using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MFPS/Motion/Spring", fileName = "Spring")]
public class bl_Spring : ScriptableObject
{
    public float stiffness = 169f;
    public float damping = 26f;
    public float mass = 1f;

    public float Target { get; set; }
    public float Current { get; set; }

    public float Velocity { get; private set; }
    private List<TargetLayer> layers;
    private float defaultStiffness = -1;
    private float defaultDamping = -1;

    /// <summary>
    /// 
    /// </summary>
    public class TargetLayer
    {
        public float Target { get; set; }

        private bool autoReset = false;
        private float resetProgress = 0;

        /// <summary>
        /// 
        /// </summary>
        public void Update()
        {
            if (!autoReset) return;

            resetProgress += Time.deltaTime;
            Target = Mathf.Lerp(Target, 0, resetProgress);

            if (resetProgress >= 1)
            {
                autoReset = false;
                Target = 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void AutoReset()
        {
            autoReset = true;
            resetProgress = 0;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bl_Spring InitAndGetInstance()
    {
        var copy = Instantiate(this);
        copy.defaultStiffness = -1;
        copy.defaultDamping = -1;

        return copy;
    }

    /// <summary>
    /// Advance a step by deltaTime(seconds).
    /// </summary>
    /// <param name="deltaTime">Delta time since previous frame</param>
    /// <returns>Evaluated value</returns>
    public void UpdateSpring(float deltaTime)
    {
        const float maxDeltaTime = 0.0333f; // This corresponds to ~30 FPS
        deltaTime = Mathf.Min(deltaTime, maxDeltaTime);

        UpdateLayers();

        float target = Target + GetLayersTarget();
        // Hooke's Law
        float springForce = -stiffness * (Current - target);

        // Damping force
        float dampingForce = -damping * Velocity;

        // Net force
        float netForce = springForce + dampingForce;

        // Acceleration
        float acceleration = netForce / mass;

        // Update velocity
        Velocity += acceleration * deltaTime;

        // Update the current value (position)
        Current += Velocity * deltaTime;
    }

    /// <summary>
    /// 
    /// </summary>
    private void UpdateLayers()
    {
        if (layers == null || layers.Count == 0) return;

        for (int i = layers.Count - 1; i >= 0; i--)
        {
            layers[i].Update();
        }
    }

    /// <summary>
    /// Add a additive target
    /// </summary>
    /// <param name="layer"></param>
    /// <param name="target"></param>
    public TargetLayer SetTargetLayer(int layer, float target, bool autoReset = false)
    {
        if (layer == -1)
        {
            Target = target;
            return null;
        }

        layers ??= new List<TargetLayer>();

        // make sure the layer is assigned
        while ((layers.Count - 1) <= layer)
        {
            layers.Add(new TargetLayer());
        }

        layers[layer].Target = target;
        if (autoReset) layers[layer].AutoReset();
        return layers[layer];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private float GetLayersTarget()
    {
        if (layers == null || layers.Count == 0) return 0;

        float accumulate = 0;
        for (int i = layers.Count - 1; i >= 0; i--)
        {
            accumulate += layers[i].Target;
        }
        return accumulate;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="multiplier"></param>
    public void SetMultiplier(float multiplier)
    {
        if (defaultStiffness <= 0)
        {
            defaultStiffness = stiffness;
            defaultDamping = damping;
        }

        stiffness = defaultStiffness * multiplier;
        damping = defaultDamping * multiplier;
    }
}