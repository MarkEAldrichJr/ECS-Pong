using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Systems
{
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
            foreach (var move in SystemAPI
                         .Query<RefRW<Move>>()
                         .WithAll<InitializeFlag, BounceFlag>())
            {
                var moveX = _rng.NextBool() ? 5 : -5;
                var moveY = _rng.NextFloat(-5f, 5f);
                var movement = new float2(moveX, moveY);
            
                move.ValueRW.MoveDirection = math.normalize(movement);
            }
        
            state.EntityManager.RemoveComponent<InitializeFlag>(_bulletQuery);
        }
    }
}