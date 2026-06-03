using UnityEngine;
using System.Collections;

namespace BirdAI
{
    public enum BirdMode
    {
        Idle,
        Roam,
        Seek,
        Dive
    }

    public class BirdMotor : MonoBehaviour
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
            if (_mode == BirdMode.Idle)
            {
                _velocity = Vector3.zero;
                return;
            }
            AvoidObstacles();
            Vector3 steeringDirection = GetSteeringForMode(out float currentSpeed);
            ApplyMotion(steeringDirection, currentSpeed);
        }
        

        void AvoidObstacles()
        {
            if (!Physics.Raycast(transform.position, _velocity.normalized, out RaycastHit obstacleHit, lookAheadDistance, obstacleMask))
                return;

            Vector3 reflectedDirection = Vector3.Reflect(_velocity.normalized, obstacleHit.normal);
            _velocity += reflectedDirection * avoidanceWeight * Time.deltaTime;
        }
       

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

        Vector3 RoamSteering()
        {
            if ((transform.position - _roamTarget).sqrMagnitude
                < roamArriveRadius * roamArriveRadius)
                PickRoamTarget();

            return Seek(_roamTarget);
        }

        Vector3 SeekSteering()  => Seek(Target);
        Vector3 DiveSteering()  => Seek(Target);

        void ApplyMotion(Vector3 steeringDirection, float currentSpeed)
        {
            Vector3 desiredVelocity = steeringDirection.sqrMagnitude > 0.0001f
                ? steeringDirection.normalized * currentSpeed
                : _velocity;

            float maxTurnRadians = turnRateDeg * Mathf.Deg2Rad * Time.deltaTime;

            // FIX: maxMagnitudeDelta must be 0 — we enforce speed separately below.
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

        void PickRoamTarget()
        {
            Vector2 randomOffset = Random.insideUnitCircle * roamRadius;
            float randomAltitude = Random.Range(roamMinAltitude, roamMaxAltitude);
            _roamTarget = RoamCenter + new Vector3(randomOffset.x, randomAltitude, randomOffset.y);
        }

        Vector3 Seek(Vector3 targetPosition) => targetPosition - transform.position;

        static bool IsFinite(Vector3 vector) =>
            !(float.IsNaN(vector.x)      || float.IsNaN(vector.y)      || float.IsNaN(vector.z)
           || float.IsInfinity(vector.x) || float.IsInfinity(vector.y) || float.IsInfinity(vector.z));
    }
}