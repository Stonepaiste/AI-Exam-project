using System.Collections.Generic;
using UnityEngine;

namespace BirdAI
{
    public enum HiveState
    {
        /// No target known. Birds wander solo.
        Roaming,
        /// A bird has spotted the player. Everyone forms up at the rally point.
        Rallying,
        /// Enough birds have arrived at the rally point. The flock dives.
        Diving,
        /// Player has slipped out of sight. Flock flies to last known position.
        Investigating
    }

    /// Shared "awareness" layer for the flock.
    ///
    /// Individual birds report what they see here. The hive decides the
    /// group-level state (roam / rally / dive / investigate) and publishes
    /// that + a rally point + a last-known-position for each bird's
    /// behavior tree to react to.
    public class HiveMind : MonoBehaviour
    {
        private static HiveMind _instance;
        public static HiveMind Instance => _instance;

        [Header("Memory")]
        [Tooltip("Seconds after losing LOS before we drop from Rallying/Diving to Investigating.")]
        public float memoryDuration = 8f;

        [Tooltip("Extra seconds spent investigating the last known position before giving up.")]
        public float giveUpDuration = 5f;

        [Header("Rally")]
        [Tooltip("How many birds must be near the rally point before the flock commits to a dive.")]
        public int minBirdsToDive = 4;

        [Tooltip("A bird counts as 'at the rally point' when within this distance of it.")]
        public float rallyCompleteRadius = 12f;

        [Tooltip("Height above the last known position where the flock forms up.")]
        public float rallyHeight = 40f;

        [Tooltip("Horizontal offset away from the player used when positioning the rally point.")]
        public float rallyOffset = 15f;

        [Header("Dive")]
        [Tooltip("Max duration of a single coordinated dive pass before the flock reassesses.")]
        public float diveDuration = 4f;

        [Header("Debug")]
        public bool drawGizmos = true;

        public HiveState State { get; private set; } = HiveState.Roaming;
        public Vector3 LastKnownPosition { get; private set; }
        public float LastSeenTime { get; private set; } = -999f;
        public Vector3 RallyPoint { get; private set; }
        public IReadOnlyList<Bird> Birds => _birds;

        private readonly List<Bird> _birds = new List<Bird>();
        private float _diveStartTime;
        private float _stateEnteredTime;

        void Awake()
        {
            _instance = this;
        }

        void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        public void Register(Bird b)
        {
            if (b != null && !_birds.Contains(b)) _birds.Add(b);
        }

        public void Unregister(Bird b)
        {
            _birds.Remove(b);
        }

        /// Called by a bird's perception system when it has LOS on the player.
        public void ReportSighting(Vector3 playerPosition)
        {
            LastKnownPosition = playerPosition;
            LastSeenTime = Time.time;

            if (State == HiveState.Roaming || State == HiveState.Investigating)
            {
                EnterState(HiveState.Rallying);
            }
        }

        void Update()
        {
            switch (State)
            {
                case HiveState.Roaming:
                    // Nothing to do — birds drive themselves.
                    break;
                case HiveState.Rallying:
                    UpdateRallyPoint();
                    TickRallying();
                    break;
                case HiveState.Diving:
                    TickDiving();
                    break;
                case HiveState.Investigating:
                    TickInvestigating();
                    break;
            }
        }

        void UpdateRallyPoint()
        {
            // Rally above and slightly offset from last known position, so the
            // dive comes in at an angle rather than straight down.
            Vector3 offsetDir = Vector3.forward; // deterministic offset; could use wind/camera
            RallyPoint = LastKnownPosition + Vector3.up * rallyHeight + offsetDir * rallyOffset;
        }

        void TickRallying()
        {
            // Forget the target if we've been out of LOS too long.
            if (Time.time - LastSeenTime > memoryDuration)
            {
                EnterState(HiveState.Investigating);
                return;
            }

            int ready = 0;
            for (int i = 0; i < _birds.Count; i++)
            {
                var b = _birds[i];
                if (b == null) continue;
                if ((b.transform.position - RallyPoint).sqrMagnitude
                    < rallyCompleteRadius * rallyCompleteRadius)
                {
                    ready++;
                }
            }

            if (ready >= minBirdsToDive)
            {
                EnterState(HiveState.Diving);
                _diveStartTime = Time.time;
            }
        }

        void TickDiving()
        {
            if (Time.time - _diveStartTime < diveDuration) return;

            // Dive over. If the player is still fresh in memory, pull back up
            // for another pass. Otherwise drop to investigation.
            if (Time.time - LastSeenTime > memoryDuration)
                EnterState(HiveState.Investigating);
            else
                EnterState(HiveState.Rallying);
        }

        void TickInvestigating()
        {
            if (Time.time - LastSeenTime > memoryDuration + giveUpDuration)
                EnterState(HiveState.Roaming);
        }

        void EnterState(HiveState next)
        {
            State = next;
            _stateEnteredTime = Time.time;
        }

        void OnDrawGizmos()
        {
            if (!drawGizmos || !Application.isPlaying) return;

            if (State != HiveState.Roaming)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(LastKnownPosition, 1.5f);
            }
            if (State == HiveState.Rallying || State == HiveState.Diving)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(RallyPoint, rallyCompleteRadius);
            }
        }
    }
}
