using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace BirdAI
{
    /// Fly toward the swarm's rally point. Succeeds when the bird is within
    /// the swarm's rallyCompleteRadius of that point (so the flock knows this
    /// bird is "ready" for the dive).
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Fly To Rally Point",
        description: "Steer toward HiveMind.RallyPoint. Succeeds when within rallyCompleteRadius.",
        story: "[Self] flies to the rally point",
        category: "Action/Bird",
        id: "bird-action-flytorally-0002")]
    public partial class FlyToRallyPointAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Self;

        protected override Status OnStart()
        {
            if (Self?.Value == null) return Status.Failure;
            if (HiveMind.Instance == null) return Status.Failure;
            var bird = Self.Value.GetComponent<Bird>();
            if (bird == null) return Status.Failure;

            bird.Motor.Mode = BirdMode.Seek;
            bird.Motor.Target = HiveMind.Instance.RallyPoint;
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            var bird = Self.Value.GetComponent<Bird>();
            if (bird == null || HiveMind.Instance == null) return Status.Failure;

            // Self-abort if the swarm moved on (e.g. flipped to Diving or lost the target)
            // so the Selector can pick the now-correct branch. Without this, a "sticky"
            // Selector implementation would keep us flying to rally during a dive.
            if (HiveMind.Instance.State != HiveState.Rallying)
                return Status.Failure;

            // Follow the rally point as it's updated each frame by the swarm.
            bird.Motor.Target = HiveMind.Instance.RallyPoint;

            float r = HiveMind.Instance.rallyCompleteRadius;
            if ((bird.transform.position - HiveMind.Instance.RallyPoint).sqrMagnitude < r * r)
                return Status.Success;

            return Status.Running;
        }
    }
}
