using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace BirdAI
{
    /// Player has slipped out of LOS but we remember where they were.
    /// The bird flies to that last-known position with light flocking.
    /// If perception re-acquires LOS on the way, the swarm flips back to
    /// Rallying automatically and a higher-priority branch preempts this one.
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Investigate Last Known",
        description: "Fly to HiveMind.LastKnownPosition to try and re-acquire the player.",
        story: "[Self] investigates the last known position",
        category: "Action/Bird",
        id: "bird-action-investigate-0005")]
    public partial class InvestigateAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Self;

        protected override Status OnStart()
        {
            if (Self?.Value == null) return Status.Failure;
            if (HiveMind.Instance == null) return Status.Failure;
            var bird = Self.Value.GetComponent<Bird>();
            if (bird == null) return Status.Failure;

            bird.Motor.Mode = BirdMode.Seek;
            bird.Motor.Target = HiveMind.Instance.LastKnownPosition;
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (HiveMind.Instance == null) return Status.Failure;
            var bird = Self.Value.GetComponent<Bird>();
            if (bird == null) return Status.Failure;

            // Self-abort if the swarm re-acquired the player (→ Rallying/Diving)
            // or gave up entirely (→ Roaming).
            if (HiveMind.Instance.State != HiveState.Investigating)
                return Status.Failure;

            bird.Motor.Target = HiveMind.Instance.LastKnownPosition;
            // We never return Success — the swarm's giveUpDuration is what ends the search.
            return Status.Running;
        }
    }
}
