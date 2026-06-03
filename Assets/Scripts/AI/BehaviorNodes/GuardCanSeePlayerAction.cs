using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace BirdAI
{
    /// Continuously checks LOS to the player every frame.
    /// Returns Running while the bird CAN see the player.
    /// Returns Failure the moment LOS is lost (player hid behind cover).
    ///
    /// Use this as a guard running in parallel with an attack sequence.
    /// When it fails, the parallel node aborts the attack.
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Guard Can See Player",
        description: "Stays Running while the bird has LOS. Fails when the player breaks line-of-sight (e.g. runs behind cover). Use in a parallel node to abort attacks.",
        story: "[Self] keeps eyes on the player",
        category: "Action/Bird",
        id: "bird-guard-canseeplayer-0001")]
    public partial class GuardCanSeePlayerAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Self;

        [Tooltip("Seconds of grace before failing after LOS is lost. Prevents flickering from brief occlusion.")]
        [SerializeReference] public BlackboardVariable<float> GracePeriod;
        [SerializeReference] public BlackboardVariable<bool> LineOfSight;
        private float _losLostTime;
        private bool _hadLOS;

        protected override Status OnStart()
        {
            if (Self?.Value == null) return Status.Failure;

            var bird = Self.Value.GetComponent<Bird>();
            if (bird == null || bird.Perception == null) return Status.Failure;
           
            
            _hadLOS = false;
            _losLostTime = 0f;

            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (Self?.Value == null) return Status.Failure;

            var bird = Self.Value.GetComponent<Bird>();
            if (bird == null || bird.Perception == null) return Status.Failure;

            var player = PlayerTarget.Instance;
            if (player == null || !player.IsAlive) return Status.Failure;

            bool canSee = bird.Perception.CanSeeTarget(player.CenterMass.position);

            if (canSee)
            {
                // Reset grace timer whenever we have LOS
                _hadLOS = true;
                _losLostTime = 0f;
                return Status.Running;
            }

            // LOS lost
            if (_hadLOS)
            {
                // First frame without LOS — start the grace timer
                _hadLOS = false;
                _losLostTime = Time.time;
            }

            float grace = GracePeriod != null ? GracePeriod.Value : 0f;

            if (Time.time - _losLostTime >= grace)
            {
                // Grace period expired — player is behind cover
                return Status.Failure;
            }

            // Still within grace period 
            return Status.Running;
        }
    }
}
