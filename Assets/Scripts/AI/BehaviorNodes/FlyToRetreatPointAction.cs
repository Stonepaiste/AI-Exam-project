using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace BirdAI
{
    /// After a dive, fly to a retreat point far from the player before
    /// the bird is allowed to re-enter the detection/attack loop.
    /// Uses BirdMotor (Seek mode) — no NavMeshAgent.
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Fly To Retreat Point",
        description: "Picks a point away from the player and flies there at cruise speed. Succeeds on arrival.",
        story: "[Self] retreats from the player",
        category: "Action/Bird",
        id: "bird-action-retreat-0006")]
    public partial class FlyToRetreatPointAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Self;
        [SerializeReference] public BlackboardVariable<GameObject> Player;

        [Tooltip("How far from the player the retreat point is placed.")]
        [SerializeReference] public BlackboardVariable<float> RetreatDistance;

        [Tooltip("Altitude range for the retreat point.")]
        [SerializeReference] public BlackboardVariable<float> MinAltitude;
        [SerializeReference] public BlackboardVariable<float> MaxAltitude;

        [Tooltip("How close the bird must get to the retreat point to count as arrived.")]
        [SerializeReference] public BlackboardVariable<float> ArriveRadius;

        private Vector3 _retreatTarget;
        private Bird _bird;

        protected override Status OnStart()
        {
            if (Self?.Value == null || Player?.Value == null)
                return Status.Failure;

            _bird = Self.Value.GetComponent<Bird>();
            if (_bird == null) return Status.Failure;

            float dist = RetreatDistance != null ? RetreatDistance.Value : 60f;
            float minAlt = MinAltitude != null ? MinAltitude.Value : 15f;
            float maxAlt = MaxAltitude != null ? MaxAltitude.Value : 40f;

            // Pick a direction away from the player, flatten to horizontal,
            // then add a random altitude.
            Vector3 awayDir = (_bird.transform.position - Player.Value.transform.position);
            awayDir.y = 0f;
            if (awayDir.sqrMagnitude < 0.01f)
                awayDir = UnityEngine.Random.insideUnitSphere;
            awayDir.Normalize();

            // Add some random spread so birds don't all retreat on the same line.
            Vector3 spread = UnityEngine.Random.insideUnitSphere;
            spread.y = 0f;
            awayDir = (awayDir + spread * 0.4f).normalized;

            float h = UnityEngine.Random.Range(minAlt, maxAlt);
            _retreatTarget = _bird.transform.position + awayDir * dist + Vector3.up * h;

            _bird.Motor.Mode = BirdMode.Seek;
            _bird.Motor.Target = _retreatTarget;

            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (_bird == null) return Status.Failure;

            // Keep the motor pointed at the retreat target in case something
            // else briefly overwrites Target.
            _bird.Motor.Target = _retreatTarget;

            float r = ArriveRadius != null ? ArriveRadius.Value : 8f;
            if ((_bird.transform.position - _retreatTarget).sqrMagnitude < r * r)
                return Status.Success;

            return Status.Running;
        }
    }
}
