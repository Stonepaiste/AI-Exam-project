using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace BirdAI
{
    /// Fly toward a rally point (read from the blackboard).
    /// Returns Success when the bird is within ArriveRadius of the point.
    /// No HiveMind dependency — all state lives on the blackboard.
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Fly To Rally Point",
        description: "Steers Self toward the RallyPoint blackboard variable. Succeeds when within ArriveRadius.",
        story: "[Self] flies to [RallyPoint]",
        category: "Action/Bird",
        id: "bird-action-flytorally-0002")]
    public partial class FlyToRallyPointAction : Action
    {
        [Tooltip("This bird.")]
        [SerializeReference] public BlackboardVariable<GameObject> Self;

        [Tooltip("Position to fly to (typically set by Set Rally Point).")]
        [SerializeReference] public BlackboardVariable<Vector3> RallyPoint;

        [Tooltip("How close the bird must get before this node succeeds.")]
        [SerializeReference] public BlackboardVariable<float> ArriveRadius;

        private Bird _bird;

        protected override Status OnStart()
        {
            if (Self?.Value == null)
            {
                Debug.LogWarning("[FlyToRallyPoint] Self is NULL.");
                return Status.Failure;
            }
            if (RallyPoint == null)
            {
                Debug.LogWarning("[FlyToRallyPoint] RallyPoint blackboard var not wired.");
                return Status.Failure;
            }

            _bird = Self.Value.GetComponent<Bird>();
            if (_bird == null || _bird.Motor == null)
            {
                Debug.LogWarning("[FlyToRallyPoint] Self has no Bird/BirdMotor component.");
                return Status.Failure;
            }

            _bird.Motor.Mode = BirdMode.Seek;
            _bird.Motor.Target = RallyPoint.Value;
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (_bird == null || _bird.Motor == null) return Status.Failure;

            // Follow any updates the tree makes to the rally point.
            _bird.Motor.Target = RallyPoint.Value;

            float arriveDistance = ArriveRadius != null ? ArriveRadius.Value : 5f;
            float distanceSquared = (_bird.transform.position - RallyPoint.Value).sqrMagnitude;

            if (distanceSquared < arriveDistance * arriveDistance)
                return Status.Success;

            return Status.Running;
        }
    }
}
