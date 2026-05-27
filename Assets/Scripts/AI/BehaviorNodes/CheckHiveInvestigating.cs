using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace BirdAI
{
    /// Checks if the swarm is currently in Investigating state.
    /// Returns Success if investigating, Failure otherwise.
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Check Swarm Investigating",
        description: "Succeeds if HiveMind.State == Investigating.",
        story: "Swarm is investigating",
        category: "Action/Bird",
        id: "bird-check-investigating-001")]
    public partial class CheckSwarmInvestigating : Action
    {
        protected override Status OnStart()
        {
            if (HiveMind.Instance == null)
                return Status.Failure;

            if (HiveMind.Instance.State == HiveState.Investigating)
                return Status.Success;

            return Status.Failure;
        }
    }
}
