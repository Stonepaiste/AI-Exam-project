using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace BirdAI
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Hide Targeted Warning",
        description: "Hides the 'YOU ARE TARGETED' warning UI.",
        story: "Hide targeted warning",
        category: "Action/UI",
        id: "ui-action-hide-warning-0002")]
    public partial class HideTargetedWarningAction : Action
    {
        protected override Status OnStart()
        {
            if (TargetedWarningUI.Instance != null)
            {
                TargetedWarningUI.Instance.Hide();
            }

            return Status.Success;
        }
    }
}
