using UnityEngine;
using System.Collections;

namespace BirdAI
{
    public enum BirdMode
    {
        Idle,
        Roam,
        Seek,
        Hold,
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

        [Header("Post-Hit Roam")]
        public float postHitRoamDuration = 3f;

        public Vector3 RoamCenter { get; set; }
        public Vector3 Target { get; set; }
        public Vector3 Velocity => _velocity;

        private BirdMode _mode = BirdMode.Idle;
        public BirdMode Mode
        {
            get => _mode;
            set
            {
                if (_isPostHitRoaming && value != BirdMode.Roam) return;
                _mode = value;
            }
        }

        private Vector3 _velocity;
        private Vector3 _roamTarget;
        private bool _isPostHitRoaming;

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

            Vector3 steer = GetSteeringForMode(out float speed);
            ApplyMotion(steer, speed);
        }

        void OnTriggerEnter(Collider other)
        {
            // FIX: tag is PlayerTarget, not Player
            if (other.CompareTag("PlayerTarget"))
                StartCoroutine(PostHitRoam());
        }

        private IEnumerator PostHitRoam()
        {
            _isPostHitRoaming = true;
            _mode = BirdMode.Roam;

            Vector3 awayDir = (transform.position - Target).normalized;
            if (awayDir.sqrMagnitude < 0.01f) awayDir = Random.onUnitSphere;
            awayDir.y = 0;
            awayDir.Normalize();

            float h = Random.Range(roamMinAltitude, roamMaxAltitude);
            _roamTarget = transform.position + awayDir * roamRadius + Vector3.up * h;

            float elapsed = 0f;
            while (elapsed < postHitRoamDuration)
            {
                if ((transform.position - _roamTarget).sqrMagnitude < roamArriveRadius * roamArriveRadius)
                {
                    _roamTarget = transform.position + awayDir * roamRadius + Vector3.up * Random.Range(roamMinAltitude, roamMaxAltitude);
                }
                elapsed += Time.deltaTime;
                yield return null;
            }

            _isPostHitRoaming = false;
            _mode = BirdMode.Roam;
        }

        Vector3 GetSteeringForMode(out float speed)
        {
            speed = cruiseSpeed;
            switch (_mode)
            {
                case BirdMode.Roam: return RoamSteering();
                case BirdMode.Seek: return SeekSteering();
                case BirdMode.Hold: return HoldSteering();
                case BirdMode.Dive:
                    speed = diveSpeed;
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
        Vector3 HoldSteering()  => Seek(Target) * 0.6f;
        Vector3 DiveSteering()  => Seek(Target);

        void ApplyMotion(Vector3 steer, float speed)
        {
            Vector3 desired = steer.sqrMagnitude > 0.0001f
                ? steer.normalized * speed
                : _velocity;

            float turnRad = turnRateDeg * Mathf.Deg2Rad * Time.deltaTime;

            // FIX: maxMagnitudeDelta must be 0 — we enforce speed separately below.
            // Passing `speed` here causes the velocity magnitude to swing wildly
            // each frame, which makes the bird circle close-range targets.
            _velocity = Vector3.RotateTowards(_velocity, desired, turnRad, 0f);

            // Enforce exact speed every frame
            _velocity = _velocity.normalized * speed;

            if (!IsFinite(_velocity) || _velocity.sqrMagnitude < 0.0001f)
                _velocity = Random.onUnitSphere * speed;

            transform.position += _velocity * Time.deltaTime;
            transform.rotation  = Quaternion.LookRotation(_velocity);
        }

        void PickRoamTarget()
        {
            Vector2 disc = Random.insideUnitCircle * roamRadius;
            float h = Random.Range(roamMinAltitude, roamMaxAltitude);
            _roamTarget = RoamCenter + new Vector3(disc.x, h, disc.y);
        }

        Vector3 Seek(Vector3 target) => target - transform.position;

        static bool IsFinite(Vector3 v) =>
            !(float.IsNaN(v.x)      || float.IsNaN(v.y)      || float.IsNaN(v.z)
           || float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z));
    }
}