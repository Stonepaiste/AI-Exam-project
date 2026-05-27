using System;
using Unity.Behavior;
using Unity.Properties;

namespace BirdAI
{
    /// True while the swarm is assembling the flock for a coordinated dive.
    [Serializable]
    [Condition(
        name: "Hive Is Rallying",
        description: "True while HiveMind.State == Rallying.",
        story: "The hive is rallying",
        category: "Condition/Hive",
        id: "bird-cond-hiverallying-0102")]
    public class HiveIsRallyingCondition : Condition
    {
        public override bool IsTrue()
        {
            return HiveMind.Instance != null
                && HiveMind.Instance.State == HiveState.Rallying;
        }
    }
}
