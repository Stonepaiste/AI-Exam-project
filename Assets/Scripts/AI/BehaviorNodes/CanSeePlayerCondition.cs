using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace BirdAI
{
    /// True when the bird's own perception currently has LOS on the player.
    /// Sightings are also automatically reported to the HiveMind by
    /// BirdPerception, so this condition is useful mostly for bird-specific
    /// reactions (e.g. play an alarm call) rather than triggering the swarm.
    [Serializable]
    [Condition(
        name: "Can See Player",
        description: "True when this bird has line-of-sight on the player.",
        story: "[Self] can see the player",
        category: "Condition/Bird",
        id: "bird-cond-canseeplayer-0101")]
    public class CanSeePlayerCondition : Condition
    {
        [SerializeReference] public BlackboardVariable<GameObject> Self;

        public override bool IsTrue()
        {
            if (Self?.Value == null) return false;
            var bird = Self.Value.GetComponent<Bird>();
            if (bird == null || bird.Perception == null) return false;

            var player = PlayerTarget.Instance;
            if (player == null || !player.IsAlive) return false;

            return bird.Perception.CanSeeTarget(player.CenterMass.position);
        }
    }
}
