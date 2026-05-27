using UnityEngine;

namespace BirdAI
{
    /// Tag component that tells the bird AI "this is the player".
    /// Exposes the transforms that birds raycast against for line-of-sight
    /// (torso + head), and the callback for taking damage from dive hits.
    ///
    /// Setup: drop on the root of the ThirdPersonController prefab instance
    /// in your scene. Assign CenterMass (torso bone or child empty) and
    /// Head (head bone or child empty). If both are left null they fall back
    /// to the transform itself.
    public class PlayerTarget : MonoBehaviour
    {
        private static PlayerTarget _instance;
        public static PlayerTarget Instance => _instance;

        [Header("LOS Anchors")]
        [Tooltip("Torso point birds raycast against.")]
        public Transform centerMass;

        [Tooltip("Head point birds raycast against (second LOS ray).")]
        public Transform head;

        [Header("Health")]
        public float maxHealth = 100f;

        public float Health { get; private set; }
        public bool IsAlive => Health > 0f;

        public Transform CenterMass => centerMass != null ? centerMass : transform;
        public Transform Head => head != null ? head : transform;

        void Awake()
        {
            _instance = this;
            Health = maxHealth;
        }

        void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        /// Called by birds when a dive pass connects.
        public void TakeDamage(float amount)
        {
            if (!IsAlive) return;
            Health = Mathf.Max(0f, Health - amount);
        }
    }
}
