using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Post Wwise Event",
    description: "Posts a Wwise event on a GameObject.",
    story: "[Self] plays [EventName]",
    category: "Action/Audio",
    id: "audio-action-wwise-0001")]
public partial class PostWwiseEventAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<string> EnvironmentSound;

    protected override Status OnStart()
    {
        if (Self?.Value == null || string.IsNullOrEmpty(EnvironmentSound?.Value))
            return Status.Failure;

        AkSoundEngine.PostEvent(EnvironmentSound.Value, Self.Value);
        return Status.Success;
    }
}