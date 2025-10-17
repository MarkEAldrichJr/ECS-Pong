using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Authoring
{
    public class PaddleAuthoring : MonoBehaviour
    {
        public float moveSpeed = 5f;
    
        public class PaddleAuthoringBaker : Baker<PaddleAuthoring>
        {
            public override void Bake(PaddleAuthoring authoring)
            {
                var e = GetEntity(authoring, TransformUsageFlags.Dynamic);
            
                AddComponent<PaddleTag>(e);
                AddComponent(e, new Move
                {
                    MoveDirection = float2.zero,
                    MoveSpeed = authoring.moveSpeed,
                    SpeedDelta = 0f
                });
            }
        }
    }



    public struct PaddleTag : IComponentData { }
}