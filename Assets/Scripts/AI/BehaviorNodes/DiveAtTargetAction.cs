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
            
            _committedTarget = Player.Value.transform.position;
            Debug.Log($"[Dive] starting — player at {_committedTarget}, bird at {bird.transform.position}, distance {Vector3.Distance(_committedTarget, bird.transform.position):F1}");

            // Lock position at dive start — player can dodge by moving after this
            _committedTarget = Player.Value.transform.position;
            _hit = false;

            bird.Motor.Mode = BirdMode.Dive;
            bird.Motor.Target = _committedTarget;

            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            var bird = Self.Value.GetComponent<Bird>();
            if (bird == null) return Status.Failure;

            float r = bird.diveContactRadius;
            Vector3 toTarget = _committedTarget - bird.transform.position;
            float distSq = toTarget.sqrMagnitude;

            if (!_hit && distSq < r * r)
            {
                bird.ApplyDiveHit();
                _hit = true;
            }

            bool closeEnough = distSq < (r * 4f) * (r * 4f);
            if (_hit || (closeEnough && Vector3.Dot(bird.Motor.Velocity, toTarget) < 0f))
            {
               // bird.Motor.BeginPostHitRoam();
                return Status.Success;
            }

            return Status.Running;
        }
    }
}