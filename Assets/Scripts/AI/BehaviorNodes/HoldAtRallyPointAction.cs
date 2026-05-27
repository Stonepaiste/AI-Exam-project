using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace BirdAI
{
    /// Circle close to the rally point waiting for the rest of the flock to
    /// arrive. Runs indefinitely; the parent Selector re-evaluates conditions
    /// (e.g. HiveIsDiving) and preempts this node when the swarm commits to a dive.
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Hold At Rally Point",
        description: "Loiter near HiveMind.RallyPoint with tight flocking.",
        story: "[Self] holds at the rally point",
        category: "Action/Bird",
        id: "bird-action-holdrally-0003")]
    public partial class HoldAtRallyPointAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Self;

        protected override Status OnStart()
        {
            if (Self?.Value == null) return Status.Failure;
            if (HiveMind.Instance == null) return Status.Failure;
            var bird = Self.Value.GetComponent<Bird>();
            if (bird == null) return Status.Failure;

            bird.Motor.Mode = BirdMode.Hold;
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (HiveMind.Instance == null) return Status.Failure;
            var bird = Self.Value.GetComponent<Bird>();
            if (bird == null) return Status.Failure;

            // Self-abort the moment the swarm commits to a dive (or loses the target)
            // so the Selector re-picks the Dive branch / drops to Investigate or Roam.
            if (HiveMind.Instance.State != HiveState.Rallying)
                return Status.Failure;

            bird.Motor.Target = HiveMind.Instance.RallyPoint;
            return Status.Running;
        }
    }
}
