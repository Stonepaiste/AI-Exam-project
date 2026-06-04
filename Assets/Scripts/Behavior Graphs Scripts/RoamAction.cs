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
        
        [SerializeReference] public BlackboardVariable<float> RecheckInterval;
        

        private float _elapsed;

        protected override Status OnStart()
        {
            if (Self?.Value == null) return Status.Failure;

            var bird = Self.Value.GetComponent<Bird>();
            if (bird == null) return Status.Failure;

            bird.Motor.Mode = BirdMode.Roam;
            return Status.Running;
            
        }

        protected override Status OnUpdate()
        {
            
            
            
            return Status.Running;
        }
    }
}