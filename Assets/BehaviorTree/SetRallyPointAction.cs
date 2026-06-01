using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

/// Reads the player's current position and stores it in the RallyPoint
/// blackboard variable so later nodes (e.g. Dive At Target) can read it.
[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Set Rally point",
    story: "Sets rally point at [Player] position",
    category: "Action",
    id: "96db7080101c38fd52e34190680cd6d0")]
public partial class SetRallyPointAction : Action
{
    [Tooltip("The player (used as the basis for the rally position).")]
    [SerializeReference] public BlackboardVariable<GameObject> Player;

    [Tooltip("Output: where the flock should gather before diving.")]
    [SerializeReference] public BlackboardVariable<Vector3> RallyPoint;

   /* protected override Status OnStart()
    {
        if (Player?.Value == null)
        {
            Debug.LogWarning("[SetRallyPointAction] Player blackboard variable is NULL — wire it up in the graph or use a 'Find GameObject With Tag' node first.");
            return Status.Failure;
        }
        if (RallyPoint == null)
        {
            Debug.LogWarning("[SetRallyPointAction] RallyPoint blackboard variable is NULL — add a Vector3 var on the blackboard and wire it to the node's RallyPoint slot.");
            return Status.Failure;
        }

        RallyPoint.Value = Player.Value.transform.position;
        return Status.Success;
    }
    */
    
    protected override Status OnStart()
    {
        
        Vector3 fromBlackboard = Player.Value.transform.position;
        GameObject freshLookup = GameObject.FindWithTag("PlayerTarget");
        Vector3 fromTag = freshLookup != null ? freshLookup.transform.position : Vector3.zero;
        Debug.Log($"[Rally] Blackboard player pos: {fromBlackboard}  |  Fresh tag lookup pos: {fromTag}");
        
        var player = GameObject.FindWithTag("PlayerTarget");
        if (player == null) return Status.Failure;
        if (RallyPoint == null) return Status.Failure;

        RallyPoint.Value = player.transform.position;
        return Status.Success;
    }
    
    
}