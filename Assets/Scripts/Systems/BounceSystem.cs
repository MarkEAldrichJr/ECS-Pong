using Authoring;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
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
                        if (trans.ValueRO.Position.y > 0)
                            move.ValueRW.MoveDirection.y = -math.abs(move.ValueRO.MoveDirection.y);
                        else
                            move.ValueRW.MoveDirection.y = math.abs(move.ValueRO.MoveDirection.y);
                        
                        //TODO: Add wall bonk noise
                    } 
                    else if (state.EntityManager.HasComponent<PaddleTag>(distanceHit.Entity))
                    {
                        //change ball direction left or right based on height.
                        if (trans.ValueRO.Position.x > 0)
                            move.ValueRW.MoveDirection.x = -math.abs(move.ValueRO.MoveDirection.x);
                        else
                            move.ValueRW.MoveDirection.x = math.abs(move.ValueRO.MoveDirection.x);

                        //change ball direction up or down based on distance to the center of the paddle
                        var hitHeight = trans.ValueRO.Position.y;
                        var hitCenter = state.EntityManager.GetComponentData<LocalTransform>(distanceHit.Entity).Position.y;
                        var hitDistance = hitHeight - hitCenter;
                        move.ValueRW.MoveDirection.y = math.clamp(
                            math.remap(
                                -2.5f, 2.5f, -4f, 4f, hitDistance),
                            -5f,
                            5f);

                        
                        //normalize
                        move.ValueRW.MoveDirection = math.normalize(move.ValueRO.MoveDirection);
                        
                        //TODO: add paddle bonk noise
                    }
                }
            }
        }
    }
}