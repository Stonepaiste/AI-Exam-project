using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace BirdAI
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Show Targeted Warning",
        description: "Shows the 'YOU ARE TARGETED' warning UI to the player.",
        story: "Show targeted warning to player",
        category: "Action/UI",
        id: "ui-action-show-warning-0001")]
    public partial class ShowTargetedWarningAction : Action
    {
        protected override Status OnStart()
        {
            if (WarningDisplay.Instance != null)
            {
                WarningDisplay.Instance.Show();
            }

            return Status.Success;
        }
    }
}
