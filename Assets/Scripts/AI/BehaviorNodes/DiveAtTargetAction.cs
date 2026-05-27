using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace BirdAI
{
    /// The attack itself. All participating birds dive together at the swarm's
    /// LastKnownPosition (NOT the current player position — this is what lets
    /// a player who slips behind cover during the rally actually dodge).
    ///
    /// On close pass the bird calls Bird.ApplyDiveHit() and the node succeeds,
    /// ending the individual bird's dive so it starts to retreat/reform. The
    /// hive-level state machine handles gathering the flock for another pass.
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

        private Vector3 _committedTarget;
        private bool _hit;

        protected override Status OnStart()
        {
            if (Self?.Value == null) return Status.Failure;
            if (HiveMind.Instance == null) return Status.Failure;
            var bird = Self.Value.GetComponent<Bird>();
            if (bird == null) return Status.Failure;

            // Lock the dive target when the dive starts so late-breaking LOS
            // updates don't snake the bird's path mid-pass.
            _committedTarget = HiveMind.Instance.LastKnownPosition;
            _hit = false;

            bird.Motor.Mode = BirdMode.Dive;
            bird.Motor.Target = _committedTarget;
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            var bird = Self.Value.GetComponent<Bird>();
            if (bird == null) return Status.Failure;

            // Contact check: if the bird's pass crosses within diveContactRadius
            // of the locked target, register the hit once.
            if (!_hit)
            {
                float r = bird.diveContactRadius;
                if ((bird.transform.position - _committedTarget).sqrMagnitude < r * r)
                {
                    bird.ApplyDiveHit();
                    _hit = true;
                }
            }

            // After passing the target (bird is moving away from it), the dive
            // is over — succeed so the BT can route the bird back up to rally.
            Vector3 toTarget = _committedTarget - bird.transform.position;
            if (_hit || Vector3.Dot(bird.Motor.Velocity, toTarget) < 0f)
                return Status.Success;

            return Status.Running;
        }
    }
}
