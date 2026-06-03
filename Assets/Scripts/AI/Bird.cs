using UnityEngine;

namespace BirdAI
{
    /// The glue component for a single boid.
    ///
    /// Keeps references to Motor and Perception so behavior-tree nodes have a
    /// single entry point.
    ///
    /// Require component makes sure the scripts are setup correctly 
    [RequireComponent(typeof(BirdMotor))]
    [RequireComponent(typeof(BirdPerception))]
    public class Bird : MonoBehaviour
    {
        public BirdMotor Motor { get; private set; }
        public BirdPerception Perception { get; private set; }

        [Header("Combat")]
        [Tooltip("Damage applied to the player on a dive contact.")]
        public float diveDamage = 8f;

        [Tooltip("How close to LastKnownPosition a dive needs to pass for the bird to 'peck' and retreat.")]
        public float diveContactRadius = 1.5f;

        [Tooltip("HP the bird starts with (for a future damage system).")]
        public float health = 1f;

        void Awake()
        {
            Motor = GetComponent<BirdMotor>();
            Perception = GetComponent<BirdPerception>();
        }


        /// Called by DiveAtTargetAction when the bird's dive pass crosses close
        /// enough to the player to register a hit.
        public void ApplyDiveHit()
        {
            var player = PlayerTarget.Instance;
            if (player != null) player.TakeDamage(diveDamage);
        }
    }
}
