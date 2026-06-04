using UnityEngine;

namespace BirdAI
{
    public enum BirdMode
    {
        Idle,
        Roam,
        Seek,
        Dive
    }

    // Handles all bird movement. The behavior graph sets the Mode and Target,
    // and this script takes care of actually flying the bird each frame.
    public class MasterBird : MonoBehaviour
    {
        [Header("Speed")]
        public float cruiseSpeed = 12f;
        public float diveSpeed = 32f;
        [Tooltip("Maximum turn rate in degrees per second.")]
        public float turnRateDeg = 240f;

        [Header("Roam")]
        public float roamRadius = 60f;
        public float roamMinAltitude = 15f;
        public float roamMaxAltitude = 40f;
        public float roamArriveRadius = 5f;

        [Header("Obstacle Avoidance")]
        public float lookAheadDistance = 4f;
        public float avoidanceWeight = 12f;
        public LayerMask obstacleMask;
        public Vector3 RoamCenter { get; set; }
        public Vector3 Target { get; set; }
        public Vector3 Velocity => _velocity;

        private BirdMode _mode = BirdMode.Idle;
        public BirdMode Mode
        {
            get => _mode;
            set => _mode = value;
        }

        private Vector3 _velocity;
        private Vector3 _roamTarget;

        // Called once when the bird spawns.
        // Saves the starting position as the roam center, picks a first
        // random wander target, and gives the bird an initial velocity
        // so it starts moving right away.
        void Start()
        {
            if (RoamCenter == Vector3.zero) RoamCenter = transform.position;
            PickRoamTarget();

            _velocity = transform.forward * cruiseSpeed;
            if (_velocity.sqrMagnitude < 0.01f)
                _velocity = Random.onUnitSphere * cruiseSpeed;
        }

        // Runs every frame. If the bird is idle it stops moving.
        // Otherwise it avoids obstacles, picks a steering direction
        // based on the current mode, and moves the bird.
        void Update()
        {
            if (_mode == BirdMode.Idle)
            {
                _velocity = Vector3.zero;
                return;
            }
            AvoidObstacles();
            Vector3 steeringDirection = GetSteeringForMode(out float currentSpeed);
            ApplyMotion(steeringDirection, currentSpeed);
        }

        // Shoots a raycast forward to check for obstacles.
        // If something is in the way, it bounces the velocity away from the obstacle surface so the bird steers around it. Like a ball hitting a wall.
        // It steers of the bird a little for each frame
        void AvoidObstacles()
        {
            if (!Physics.Raycast(transform.position, _velocity.normalized, out RaycastHit obstacleHit, lookAheadDistance, obstacleMask))
                return;

            Vector3 reflectedDirection = Vector3.Reflect(_velocity.normalized, obstacleHit.normal);
            _velocity += reflectedDirection * avoidanceWeight * Time.deltaTime;
        }

        // Picks the right steering method and speed based on the current mode.
        // Roam and Seek use cruise speed. Dive uses the faster dive speed.
        Vector3 GetSteeringForMode(out float currentSpeed)
        {
            currentSpeed = cruiseSpeed;
            switch (_mode)
            {
                case BirdMode.Roam: return RoamSteering();
                case BirdMode.Seek: return SeekSteering();
                case BirdMode.Dive:
                    currentSpeed = diveSpeed;
                    return DiveSteering();
                default: return Vector3.zero;
            }
        }

        // Wander behavior. Flies toward a random point in the air.
        // When the bird gets close enough, it picks a new random point.
        // when roam mode is called from behavior graph this is what it activates. 
        Vector3 RoamSteering()
        {
            if ((transform.position - _roamTarget).sqrMagnitude
                < roamArriveRadius * roamArriveRadius)
                PickRoamTarget();

            return Seek(_roamTarget);
        }

        // Fly toward the Target at cruise speed.
        // Used by FlyToRallyPoint and FlyToRetreatPoint.
        Vector3 SeekSteering() => Seek(Target);

        // Fly toward the Target at dive speed.
        // Same direction as Seek, but the speed is set to diveSpeed
        // by GetSteeringForMode.
        Vector3 DiveSteering() => Seek(Target);

        // Moves and rotates the bird each frame.
        // Smoothly turns toward the desired direction (limited by turn rate),
        // locks the speed to the exact value, and moves the bird forward.
        // If the velocity becomes invalid (NaN/zero), it resets to a random direction.
        void ApplyMotion(Vector3 steeringDirection, float currentSpeed)
        {
            Vector3 desiredVelocity = steeringDirection.sqrMagnitude > 0.0001f
                ? steeringDirection.normalized * currentSpeed
                : _velocity;

            float maxTurnRadians = turnRateDeg * Mathf.Deg2Rad * Time.deltaTime;

            // maxMagnitudeDelta must be 0 — we enforce speed separately below.
            // Passing speed here causes the velocity magnitude to swing wildly
            // each frame, which makes the bird circle close-range targets.
            _velocity = Vector3.RotateTowards(_velocity, desiredVelocity, maxTurnRadians, 0f);

            // Enforce exact speed every frame
            _velocity = _velocity.normalized * currentSpeed;

            if (!IsFinite(_velocity) || _velocity.sqrMagnitude < 0.0001f)
                _velocity = Random.onUnitSphere * currentSpeed;

            transform.position += _velocity * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(_velocity) * Quaternion.Euler(0, 90, 0);
        }

        // Picks a new random point in the air for the bird to wander toward.
        // at a random altitude between min and max.
        void PickRoamTarget()
        {
            Vector2 randomOffset = Random.insideUnitCircle * roamRadius;
            float randomAltitude = Random.Range(roamMinAltitude, roamMaxAltitude);
            _roamTarget = RoamCenter + new Vector3(randomOffset.x, randomAltitude, randomOffset.y);
        }

        // Returns the direction from the bird to a target position.
        // This is the core steering calculation — just "where do I need to go."
        Vector3 Seek(Vector3 targetPosition) => targetPosition - transform.position;

        // Safety check — returns true if a vector has valid numbers.
        // Catches NaN or Infinity values that can happen from division
        // by zero or other math edge cases.
        static bool IsFinite(Vector3 vector) =>
            !(float.IsNaN(vector.x)      || float.IsNaN(vector.y)      || float.IsNaN(vector.z)
           || float.IsInfinity(vector.x) || float.IsInfinity(vector.y) || float.IsInfinity(vector.z));
    
    
    }
    
}
