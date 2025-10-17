using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial struct MoveSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        var builder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<Move>();
        
        state.RequireForUpdate(state.GetEntityQuery(builder));
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;
        
        foreach (var (move, trans) in SystemAPI
                     .Query<RefRO<Move>, RefRW<LocalTransform>>()
                     .WithNone<InitializeFlag>())
        {
            var moveDelta2D = move.ValueRO.MoveDirection * (move.ValueRO.MoveSpeed * deltaTime);
            var moveDelta3D = new float3(moveDelta2D.x, moveDelta2D.y, 0);
            
            trans.ValueRW.Position = trans.ValueRO.Position + moveDelta3D;
        }

        foreach (var move in SystemAPI
                     .Query<RefRW<Move>>()
                     .WithAll<BounceFlag>())
        {
            move.ValueRW.MoveSpeed += move.ValueRO.SpeedDelta * deltaTime;
        }
    }
}