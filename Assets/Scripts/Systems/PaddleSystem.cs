using Authoring;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Systems
{
    public partial struct PaddleSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            var builder = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<PaddleTag, Move, LocalTransform>();
            
            state.RequireForUpdate(state.GetEntityQuery(builder));
            state.RequireForUpdate<RightPaddleMovement>();
            state.RequireForUpdate<LeftPaddleMovement>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var leftPaddleInput = SystemAPI.GetSingleton<LeftPaddleMovement>();
            var rightPaddleInput = SystemAPI.GetSingleton<RightPaddleMovement>();

            foreach (var (move, trans) in SystemAPI
                         .Query<RefRW<Move>, RefRO<LocalTransform>>()
                         .WithAll<PaddleTag>())
            {
                move.ValueRW.MoveDirection.y = trans.ValueRO.Position.x < 0f ? 
                    leftPaddleInput.Value : rightPaddleInput.Value;

                if (trans.ValueRO.Position.y > 7 && move.ValueRO.MoveDirection.y > 0f ||
                    trans.ValueRO.Position.y < -7f && move.ValueRO.MoveDirection.y < 0f)
                    move.ValueRW.MoveDirection.y = 0;
            }
        }
    }
}