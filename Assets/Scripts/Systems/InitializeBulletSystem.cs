using Authoring;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

namespace Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct InitializeBulletSystem : ISystem
    {
        private Random _rng;
        private EntityQuery _bulletQuery;
        
        public void OnCreate(ref SystemState state)
        {
            _rng = new Random((uint)System.DateTime.Now.Millisecond);
        
            var builder = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<InitializeFlag, Move, BounceFlag>();
            _bulletQuery = state.GetEntityQuery(builder);
        
            state.RequireForUpdate(_bulletQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (move, color, trans) in SystemAPI
                         .Query<RefRW<Move>, RefRW<URPMaterialPropertyBaseColor>, RefRW<LocalTransform>>()
                         .WithAll<InitializeFlag, BounceFlag>())
            {
                var moveX = _rng.NextBool() ? 5 : -5;
                var moveY = _rng.NextFloat(-5f, 5f);
                var movement = new float2(moveX, moveY);
                move.ValueRW.MoveDirection = math.normalize(movement);
                trans.ValueRW.Position.z = _rng.NextFloat(-0.2f, 0.2f);

                var brightness = _rng.NextFloat(0f, 1.0f);
                color.ValueRW.Value = new float4(brightness, brightness, brightness, 1.0f);
            }
        
            state.EntityManager.RemoveComponent<InitializeFlag>(_bulletQuery);
        }
    }
}