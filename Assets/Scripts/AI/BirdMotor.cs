using UnityEngine;

namespace BirdAI
{
    public enum BirdMode
    {
        /// Solo wander around the spawn area. No flocking.
        Roam,
        /// Head toward a target position with light flocking (used for rally, investigate, retreat).
        Seek,
        /// Hold near a target position with stronger flocking (ring around rally point).
        Hold,
        /// Full-speed line at a target position — the dive pass.
        Dive
    }

    /// Movement for a single bird. Mode-switchable — when the bird is Roaming
    /// it uses a simple random-wander; when it's hunting it participates in
    /// boids-style flocking with other hunting birds.
    ///
    /// The behavior-tree nodes set Mode and Target; this script produces
    /// motion. It does NOT make high-level decisions itself.
    public class BirdMotor : MonoBehaviour
    {
        [Header("Speed")]
        public float cruiseSpeed = 12f;
        public float diveSpeed = 32f;
        [Tooltip("Maximum turn rate in degrees per second.")]
        public float turnRateDeg = 240f;

        [Header("Roam")]
        [Tooltip("Radius (XZ) of the wander area around RoamCenter.")]
        public float roamRadius = 60f;
        public float roamMinAltitude = 15f;
        public float roamMaxAltitude = 40f;
        [Tooltip("How close the bird needs to get to a wander point before picking a new one.")]
        public float roamArriveRadius = 5f;

        [Header("Flocking (only active in Seek/Hold/Dive)")]
        public float neighborRadius = 15f;
        public float separationRadius = 4f;
        public float separationWeight = 2.5f;
        public float alignmentWeight = 1.0f;
        public float cohesionWeight = 1.0f;

        /// Where the bird wanders around when in Roam mode. Defaults to spawn position.
        public Vector3 RoamCenter { get; set; }
        public BirdMode Mode { get; set; } = BirdMode.Roam;
        public Vector3 Target { get; set; }
        public Vector3 Velocity => _velocity;

        private Vector3 _velocity;
        private Vector3 _roamTarget;

        void Start()
        {
            if (RoamCenter == Vector3.zero) RoamCenter = transform.position;
            PickRoamTarget();
            _velocity = transform.forward * cruiseSpeed;
            if (_velocity.sqrMagnitude < 0.01f)
                _velocity = Random.onUnitSphere * cruiseSpeed;
        }

        void Update()
        {
            Vector3 steer;
            float speed = cruiseSpeed;

            switch (Mode)
            {
                case BirdMode.Roam:
                    if ((transform.position - _roamTarget).sqrMagnitude
                        < roamArriveRadius * roamArriveRadius)
                    {
                        PickRoamTarget();
                    }
                    steer = Seek(_roamTarget);
                    break;

                case BirdMode.Seek:
                    steer = Seek(Target) + FlockForce(weightScale: 1.0f);
                    break;

                case BirdMode.Hold:
                    steer = Seek(Target) * 0.6f + FlockForce(weightScale: 1.4f);
                    break;

                case BirdMode.Dive:
                    // Commit hard to the dive line. Minimal flocking so birds don't
                    // pull each other off target.
                    steer = Seek(Target) + FlockForce(weightScale: 0.25f);
                    speed = diveSpeed;
                    break;

                default:
                    steer = Vector3.zero;
                    break;
            }

            // Convert the steering vector into a desired velocity at the target speed.
            Vector3 desired = steer.sqrMagnitude > 0.0001f
                ? steer.normalized * speed
                : _velocity;

            float turnRad = turnRateDeg * Mathf.Deg2Rad * Time.deltaTime;
            _velocity = Vector3.RotateTowards(_velocity, desired, turnRad, speed);

            if (!IsFinite(_velocity) || _velocity.sqrMagnitude < 0.0001f)
                _velocity = Random.onUnitSphere * cruiseSpeed;

            transform.position += _velocity * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(_velocity);
        }

        void PickRoamTarget()
        {
            Vector2 disc = Random.insideUnitCircle * roamRadius;
            float h = Random.Range(roamMinAltitude, roamMaxAltitude);
            _roamTarget = RoamCenter + new Vector3(disc.x, h, disc.y);
        }

        Vector3 Seek(Vector3 target) => target - transform.position;

        /// Classic boids forces, but only counts birds in a non-Roam mode —
        /// so solo wanderers never pull on each other. The hive effectively
        /// "condenses" when hunting.
        Vector3 FlockForce(float weightScale)
        {
            if (HiveMind.Instance == null || Mode == BirdMode.Roam) return Vector3.zero;

            Vector3 sep = Vector3.zero;
            Vector3 align = Vector3.zero;
            Vector3 centerSum = Vector3.zero;
            int count = 0;

            var birds = HiveMind.Instance.Birds;
            for (int i = 0; i < birds.Count; i++)
            {
                var other = birds[i];
                if (other == null || other.Motor == this) continue;
                if (other.Motor.Mode == BirdMode.Roam) continue;

                Vector3 offset = transform.position - other.transform.position;
                float sq = offset.sqrMagnitude;
                if (sq < 0.0001f || sq > neighborRadius * neighborRadius) continue;

                float dist = Mathf.Sqrt(sq);
                if (dist < separationRadius)
                    sep += (offset / dist) * (separationRadius - dist);

                align += other.Motor.Velocity;
                centerSum += other.transform.position;
                count++;
            }

            if (count == 0) return Vector3.zero;

            Vector3 alignForce = (align / count).normalized * cruiseSpeed;
            Vector3 cohesionForce = (centerSum / count) - transform.position;

            return (sep * separationWeight
                    + alignForce * alignmentWeight
                    + cohesionForce * cohesionWeight) * weightScale;
        }

        static bool IsFinite(Vector3 v)
        {
            return !(float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z)
                  || float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z));
        }
    }
}
