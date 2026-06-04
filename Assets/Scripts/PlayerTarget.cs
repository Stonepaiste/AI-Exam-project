using UnityEngine;

namespace BirdAI
{
    // playertag component that our bird AI can read and call. Also holds the player health method 
    public class PlayerTarget : MonoBehaviour
    {
        private static PlayerTarget _instance;
        public static PlayerTarget Instance => _instance;
        public Transform centerMass;
        public Transform head;
        
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

        /// Called by birds when a dive pass connects. The damage amount is set in bird script. (the bridge script)
        public void TakeDamage(float amount)
        {
            if (!IsAlive) return;
            Health = Mathf.Max(0f, Health - amount);
        }
    }
}
