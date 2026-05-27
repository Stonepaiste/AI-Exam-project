using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace BirdAI
{
    /// Checks if the swarm is currently in Rallying state.
    /// Returns Success if rallying, Failure otherwise.
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Check Swarm Rallying",
        description: "Succeeds if HiveMind.State == Rallying.",
        story: "Swarm is rallying",
        category: "Action/Bird",
        id: "bird-check-rallying-001")]
    public partial class CheckSwarmRallying : Action
    {
        protected override Status OnStart()
        {
            if (HiveMind.Instance == null)
                return Status.Failure;

            if (HiveMind.Instance.State == HiveState.Rallying)
                return Status.Success;

            return Status.Failure;
        }
    }
}
