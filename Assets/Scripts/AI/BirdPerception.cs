using UnityEngine;

namespace BirdAI
{
    /// Periodic line-of-sight probe against the player.
    ///
    /// Uses two rays (torso + head) so a player who is half-occluded by low
    /// cover can still be spotted. Any collider on coverMask blocks the ray.
    ///
    /// Polled, not frame-by-frame — one bird shouldn't pay the cost of 60
    /// raycasts per second for detection that updates usefully at ~7Hz.
    public class BirdPerception : MonoBehaviour
    {
        [Header("Vision")]
        public float viewRange = 80f;
        [Range(10f, 360f)] public float viewAngleDeg = 180f;
        [Tooltip("Layers whose colliders block line-of-sight (buildings, rocks, trees, doors).")]
        public LayerMask coverMask = ~0;

        [Tooltip("Empty child at the bird's eye position. Defaults to the bird transform.")]
        public Transform eye;

        [Header("Polling")]
        [Tooltip("Seconds between LOS checks. Staggered per-bird at Start to avoid frame spikes.")]
        public float checkInterval = 0.15f;

        [Header("Debug")]
        public bool drawGizmos = false;

        public bool HasLineOfSight { get; private set; }
        public Vector3 LastSeenPosition { get; private set; }

        private float _nextCheckTime;

        void Start()
        {
            if (eye == null) eye = transform;
            // Stagger the first check so a whole flock doesn't poll on the same frame.
            _nextCheckTime = Time.time + Random.Range(0f, checkInterval);
        }

        void Update()
        {
            if (Time.time < _nextCheckTime) return;
            _nextCheckTime = Time.time + checkInterval;

            HasLineOfSight = false;

            var player = PlayerTarget.Instance;
            if (player == null)
            {
                Debug.LogWarning($"[BirdPerception] PlayerTarget.Instance is NULL on bird {gameObject.name}");
                return;
            }
            if (!player.IsAlive)
            {
                Debug.LogWarning($"[BirdPerception] Player is dead on bird {gameObject.name}");
                return;
            }

            Vector3 eyePos = eye.position;

            // Quick distance/angle rejection before paying for a raycast.
            Vector3 toPlayer = player.CenterMass.position - eyePos;
            float sq = toPlayer.sqrMagnitude;
            float dist = Mathf.Sqrt(sq);

            if (sq > viewRange * viewRange)
            {
                Debug.LogWarning($"[BirdPerception] Player too far ({dist:F1} > {viewRange}) on bird {gameObject.name}");
                return;
            }

            float ang = Vector3.Angle(transform.forward, toPlayer);
            if (ang > viewAngleDeg * 0.5f)
            {
                Debug.LogWarning($"[BirdPerception] Player outside view angle ({ang:F1} > {viewAngleDeg * 0.5f}) on bird {gameObject.name}");
                return;
            }

            Debug.Log($"[BirdPerception] Checking LOS for bird {gameObject.name}...");

            // Two probes: if either clears, the bird has seen the player.
            if (TryProbe(eyePos, player.CenterMass.position)
                || TryProbe(eyePos, player.Head.position))
            {
                HasLineOfSight = true;
                LastSeenPosition = player.CenterMass.position;
                Debug.Log($"[BirdPerception] SPOTTED PLAYER! Bird {gameObject.name}");
                if (HiveMind.Instance != null)
                    HiveMind.Instance.ReportSighting(LastSeenPosition);
            }
            else
            {
                Debug.Log($"[BirdPerception] LOS blocked for bird {gameObject.name}");
            }
        }

        bool TryProbe(Vector3 from, Vector3 to)
        {
            Vector3 dir = to - from;
            float dist = dir.magnitude;
            if (dist < 0.01f) return true;
            dir /= dist;

            // If nothing on the cover layers is between bird and player, LOS is clear.
            // Use a larger offset to safely avoid hitting the bird's own collider.
            Vector3 origin = from + dir * 0.5f;
            float adjustedDist = dist - 0.5f;

            if (adjustedDist <= 0) return true;

            // Exclude the bird's own collider to avoid the assertion.
            var birdCollider = GetComponent<Collider>();
            return !Physics.Raycast(
                origin, dir, adjustedDist, coverMask,
                QueryTriggerInteraction.Ignore);
        }

        void OnDrawGizmosSelected()
        {
            if (!drawGizmos) return;
            Gizmos.color = HasLineOfSight ? Color.green : Color.gray;
            Gizmos.DrawWireSphere(transform.position, viewRange);
        }
    }
}
