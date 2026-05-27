using System;
using Unity.Behavior;
using Unity.Properties;

namespace BirdAI
{
    /// True while the swarm is executing a coordinated dive pass.
    [Serializable]
    [Condition(
        name: "Hive Is Diving",
        description: "True while HiveMind.State == Diving.",
        story: "The hive is diving",
        category: "Condition/Hive",
        id: "bird-cond-hivediving-0103")]
    public class HiveIsDivingCondition : Condition
    {
        public override bool IsTrue()
        {
            return HiveMind.Instance != null
                && HiveMind.Instance.State == HiveState.Diving;
        }
    }
}
