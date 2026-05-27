using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace BirdAI
{
    /// Checks if the swarm is currently in Diving state.
    /// Returns Success if diving, Failure otherwise.
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Check Swarm Diving",
        description: "Succeeds if HiveMind.State == Diving.",
        story: "Swarm is diving",
        category: "Action/Bird",
        id: "bird-check-diving-001")]
    public partial class CheckSwarmDiving : Action
    {
        protected override Status OnStart()
        {
            if (HiveMind.Instance == null)
                return Status.Failure;

            if (HiveMind.Instance.State == HiveState.Diving)
                return Status.Success;

            return Status.Failure;
        }
    }
}
