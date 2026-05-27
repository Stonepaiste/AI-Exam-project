using System;
using Unity.Behavior;
using Unity.Properties;

namespace BirdAI
{
    /// True while the swarm has a memory of the player but no current LOS —
    /// the flock is flying to the last known position hoping to re-spot them.
    [Serializable]
    [Condition(
        name: "Hive Is Investigating",
        description: "True while HiveMind.State == Investigating.",
        story: "The hive is investigating",
        category: "Condition/Hive",
        id: "bird-cond-hiveinvestigating-0104")]
    public class HiveIsInvestigatingCondition : Condition
    {
        public override bool IsTrue()
        {
            return HiveMind.Instance != null
                && HiveMind.Instance.State == HiveState.Investigating;
        }
    }
}
