using Unity.Entities;
using UnityEngine;

public class GoalAuthoring : MonoBehaviour
{
    private class GoalAuthoringBaker : Baker<GoalAuthoring>
    {
        public override void Bake(GoalAuthoring authoring)
        {
            var e = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent<GoalTag>(e);
        }
    }
}

public struct GoalTag : IComponentData {}