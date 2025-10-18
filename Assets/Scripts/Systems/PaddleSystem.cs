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
                if (trans.ValueRO.Position.x < 0f)
                {
                    move.ValueRW.MoveDirection.y = leftPaddleInput.Value;
                }
                else
                {
                    move.ValueRW.MoveDirection.y = rightPaddleInput.Value;
                }
            }
        }
    }
}
