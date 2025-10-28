using Authoring;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct MoveSystem : ISystem
    {
        private EntityQuery _query;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            var builder = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<Move, LocalTransform>().WithNone<InitializeFlag>();
            
            _query = state.GetEntityQuery(builder);
        
            state.RequireForUpdate(_query);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;

            new MoveJob
            {
                DeltaTime = deltaTime
            }.ScheduleParallel(_query);
            
            state.Dependency.Complete();

            foreach (var move in SystemAPI
                         .Query<RefRW<Move>>()
                         .WithAll<BounceFlag>())
            {
                move.ValueRW.MoveSpeed += move.ValueRO.SpeedDelta * deltaTime;
            }
        }
    }

    public partial struct MoveJob : IJobEntity
    {
        [ReadOnly] public float DeltaTime;
        
        public void Execute(ref LocalTransform trans, in Move move)
        {
            var moveDelta2D = move.MoveDirection * (move.MoveSpeed * DeltaTime);
            var moveDelta3D = new float3(moveDelta2D.x, moveDelta2D.y, 0);
            
            trans.Position += moveDelta3D;
        }
    }
}