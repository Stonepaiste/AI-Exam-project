using UnityEngine;

namespace BirdAI
{
    //The glue component for a single boid.
  
    // Keeps references to Motor and Perception so behavior-tree nodes have a
    // single entry point.
    
    public class Bird : MonoBehaviour
    {
        public MasterBird Motor { get; private set; }
        public BirdPerception Perception { get; private set; }
        
        public float diveDamage = 8f;

       //How close to LastKnownPosition a dive needs to pass for the bird to give damage and retreat.
        public float diveContactRadius = 1.5f;

        void Awake()
        {
            Motor = GetComponent<MasterBird>();
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
