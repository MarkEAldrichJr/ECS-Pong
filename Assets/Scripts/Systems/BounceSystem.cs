using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Systems
{
    public partial struct BounceSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            var builder = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<BounceFlag, Move>();
            
            state.RequireForUpdate(state.GetEntityQuery(builder));
            state.RequireForUpdate<PhysicsWorldSingleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>()
                .CollisionWorld;
            var collisionList = new NativeList<DistanceHit>(Allocator.Temp);
            var filter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = ~0u,
                GroupIndex = 0
            };
            
            foreach (var (move, trans) in SystemAPI
                         .Query<RefRW<Move>, RefRO<LocalTransform>>()
                         .WithAll<BounceFlag>())
            {
                collisionList.Clear();
                
                var collision = collisionWorld.OverlapBox(
                    trans.ValueRO.Position,
                    quaternion.identity,
                    new float3(.5f, .5f, .5f),
                    ref collisionList, 
                    filter,
                    QueryInteraction.IgnoreTriggers);

                if (!collision) continue;

                foreach (var distanceHit in collisionList)
                {
                    if (state.EntityManager.HasComponent<WallTag>(distanceHit.Entity))
                    {
                        move.ValueRW.MoveDirection.y = -move.ValueRO.MoveDirection.y;
                        //TODO: Add wall bonk noise
                    } 
                    else if (state.EntityManager.HasComponent<PaddleTag>(distanceHit.Entity))
                    {
                        move.ValueRW.MoveDirection.x = -move.ValueRO.MoveDirection.x;
                        //TODO: set move angle based on height compared to the paddle
                        //TODO: add paddle bonk noise
                    }
                    else if (state.EntityManager.HasComponent<GoalTag>(distanceHit.Entity))
                    {
                        //destroy the entity
                        //tick the player's side by one
                        //play goal noise
                    }
                    else
                    {
                        //error
                    }
                }
            }
        }
    }
}