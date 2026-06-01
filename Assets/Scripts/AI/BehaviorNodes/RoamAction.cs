using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace BirdAI
{
    /// Default / fallback behavior: the bird wanders solo around its spawn area.
    /// Returns Success periodically so the parent BT can re-evaluate distance/LOS.
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Roam",
        description: "Bird wanders randomly around its spawn area (no flocking).",
        story: "[Self] roams solo",
        category: "Action/Bird",
        id: "bird-action-roam-0001")]
    public partial class RoamAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Self;

        [Tooltip("How often (seconds) the BT should re-evaluate distance/LOS while roaming.")]
        [SerializeReference] public BlackboardVariable<float> RecheckInterval;

        private float _elapsed;

        protected override Status OnStart()
        {
            if (Self?.Value == null) return Status.Failure;

            var bird = Self.Value.GetComponent<Bird>();
            if (bird == null) return Status.Failure;

            bird.Motor.Mode = BirdMode.Roam;
            _elapsed = 0f;
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            _elapsed += Time.deltaTime;

            float interval = RecheckInterval != null && RecheckInterval.Value > 0f
                ? RecheckInterval.Value
                : 0.5f;

            if (_elapsed >= interval)
            {
                var player = GameObject.FindWithTag("PlayerTarget");
                
                if (player != null)
                {
                    float d = Vector3.Distance(Self.Value.transform.position, player.transform.position);
                    Debug.Log($"[Roam] returning Success — actual distance to player: {d:F1}");
                }

                Debug.Log("[Roam] returning Success — BT should re-check distance now");
                return Status.Success;
            }

            return Status.Running;
        }
    }
}