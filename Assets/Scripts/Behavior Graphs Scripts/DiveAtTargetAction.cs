
using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace BirdAI
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Dive At Target",
        description: "High-speed dive at the swarm's last known player position.",
        story: "[Self] dives at the target",
        category: "Action/Bird",
        id: "bird-action-dive-0004")]
    public partial class DiveAtTargetAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Self;
        [SerializeReference] public BlackboardVariable<GameObject> Player;

        private Vector3 _committedTarget;
        private bool _hit;

        // Checks for player and self exists at Onstart in the behavior Graph
        // Gets the bird component script so it can call the damage method.
        protected override Status OnStart()
        {
            if (Self?.Value == null) return Status.Failure;
            if (Player?.Value == null)
            {
                Debug.Log("[Dive] Player is NULL");
                return Status.Failure;
            }

            var bird = Self.Value.GetComponent<Bird>();
            if (bird == null) return Status.Failure;

            // Lock position at dive start — player can dodge by moving after this
            _committedTarget = Player.Value.transform.position;
            _hit = false;
            // starts the dive! 
            bird.Motor.Mode = BirdMode.Dive;
            bird.Motor.Target = _committedTarget;

            return Status.Running;
        }
            // Checks if the bird has reached the target (committed target) We don't set the target to the player
            // so the player still has a change to get away after being spottet.
        protected override Status OnUpdate()
        {
            var bird = Self.Value.GetComponent<Bird>();
            if (bird == null) return Status.Failure;

            float hitRadius = bird.diveContactRadius;
            Vector3 directionToTarget = _committedTarget - bird.transform.position;
            float distanceSquared = directionToTarget.sqrMagnitude;

            if (!_hit && distanceSquared < hitRadius * hitRadius)
            {
                bird.ApplyDiveHit();
                _hit = true;
            }
            // Checks if the bird has passed the committed target.
            // Without this they sometimes never fly away gettin stuck in a circle loop. 

            float passedByRadius = hitRadius * 4f;
            bool closeEnough = distanceSquared < passedByRadius * passedByRadius;
            bool flyingAwayFromTarget = Vector3.Dot(bird.Motor.Velocity, directionToTarget) < 0f;

            if (_hit || (closeEnough && flyingAwayFromTarget))
            {
                return Status.Success;
            }

            return Status.Running;
        }
    }
}