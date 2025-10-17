using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class BulletMoveAuthoring : MonoBehaviour
{
    public float initialSpeed;
    public float speedDelta;

    public class BulletMoveBaker : Baker<BulletMoveAuthoring>
    {
        public override void Bake(BulletMoveAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            
            AddComponent<InitializeFlag>(entity);
            AddComponent<BounceFlag>(entity);
            AddComponent(entity, new Move
            {
                MoveSpeed = authoring.initialSpeed,
                MoveDirection = float2.zero,
                SpeedDelta = authoring.speedDelta
            });
        }
    }
}

public struct Move : IComponentData
{
    public float MoveSpeed;
    public float SpeedDelta;
    public float2 MoveDirection;
}

public struct BounceFlag : IComponentData {}
public struct InitializeFlag : IComponentData {}