using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace BirdAI
{
    /// Checks if this bird currently has line-of-sight on the player.
    /// Returns Success if visible, Failure otherwise.
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Check Can See Player",
        description: "Succeeds if this bird has LOS on the player.",
        story: "[Self] can see the player",
        category: "Action/Bird",
        id: "bird-check-canseeplayer-001")]
    public partial class CheckCanSeePlayer : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Self;

        protected override Status OnStart()
        {
            if (Self?.Value == null)
                return Status.Failure;

            var bird = Self.Value.GetComponent<Bird>();
            if (bird == null || bird.Perception == null)
                return Status.Failure;

            if (bird.Perception.HasLineOfSight)
                return Status.Success;

            return Status.Failure;
        }
    }
}
