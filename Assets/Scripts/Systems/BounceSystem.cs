//produced with Claude

using Authoring;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct BounceSystem : ISystem
    {
        private EntityQuery _wallQuery;
        private EntityQuery _paddleQuery;
        
        public void OnCreate(ref SystemState state)
        {
            var builder = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<BounceFlag, Move>();
            
            state.RequireForUpdate(state.GetEntityQuery(builder));

            // Cache entity queries for walls and paddles
            _wallQuery = state.GetEntityQuery(
                ComponentType.ReadOnly<WallTag>(),
                ComponentType.ReadOnly<LocalTransform>());
            _paddleQuery = state.GetEntityQuery(
                ComponentType.ReadOnly<PaddleTag>(),
                ComponentType.ReadOnly<LocalTransform>());
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Get all walls with positions
            var wallEntities = _wallQuery.ToEntityArray(Allocator.TempJob);
            var wallTransforms = _wallQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
            
            // Get all paddles with positions
            var paddleEntities = _paddleQuery.ToEntityArray(Allocator.TempJob);
            var paddleTransforms = _paddleQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);

            // Create collision data arrays
            var wallData = new NativeArray<CollisionData>(wallEntities.Length, Allocator.TempJob);
            var paddleData = new NativeArray<CollisionData>(paddleEntities.Length, Allocator.TempJob);
            
            for (var i = 0; i < wallEntities.Length; i++)
            {
                wallData[i] = new CollisionData
                {
                    Position = wallTransforms[i].Position,
                    Scale = wallTransforms[i].Scale,
                    Entity = wallEntities[i]
                };
            }
            
            for (var i = 0; i < paddleEntities.Length; i++)
            {
                paddleData[i] = new CollisionData
                {
                    Position = paddleTransforms[i].Position,
                    Scale = paddleTransforms[i].Scale,
                    Entity = paddleEntities[i]
                };
            }

            // Schedule parallel job
            var job = new BounceJob
            {
                WallData = wallData,
                PaddleData = paddleData
            };

            state.Dependency = job.ScheduleParallel(state.Dependency);
            state.Dependency.Complete(); // Ensure completion before cleanup

            // Cleanup
            wallEntities.Dispose();
            wallTransforms.Dispose();
            paddleEntities.Dispose();
            paddleTransforms.Dispose();
            wallData.Dispose();
            paddleData.Dispose();
        }

        private struct CollisionData
        {
            public float3 Position;
            public float Scale;
            public Entity Entity;
        }

        [BurstCompile]
        private partial struct BounceJob : IJobEntity
        {
            [ReadOnly] public NativeArray<CollisionData> WallData;
            [ReadOnly] public NativeArray<CollisionData> PaddleData;

            private void Execute(
                ref Move move,
                in LocalTransform trans,
                in BounceFlag bounceFlag)
            {
                var bulletPos = trans.Position;
                const float bulletRadius = 0.5f; // Half the bullet size
                var hasCollided = false;

                // Check wall collisions
                for (var i = 0; i < WallData.Length; i++)
                {
                    var wall = WallData[i];
                    var wallHalfExtents = new float3(17f, 0.25f, 0.5f); // 34 * 0.5, 0.5 * 0.5
                    
                    if (IsBoxColliding(bulletPos, bulletRadius, wall.Position, wallHalfExtents))
                    {
                        // Only bounce if we're moving towards the wall
                        if (bulletPos.y > wall.Position.y && move.MoveDirection.y < 0)
                        {
                            move.MoveDirection.y = -move.MoveDirection.y;
                            hasCollided = true;
                        }
                        else if (bulletPos.y < wall.Position.y && move.MoveDirection.y > 0)
                        {
                            move.MoveDirection.y = -move.MoveDirection.y;
                            hasCollided = true;
                        }
                        
                        if (hasCollided) return;
                    }
                }

                // Check paddle collisions
                for (var i = 0; i < PaddleData.Length; i++)
                {
                    var paddle = PaddleData[i];
                    var paddleHalfExtents = new float3(0.5f, 2f, 0.5f); // 1 * 0.5, 4 * 0.5
                    
                    if (IsBoxColliding(bulletPos, bulletRadius, paddle.Position, paddleHalfExtents))
                    {
                        // Only bounce if we're moving towards the paddle
                        var shouldBounce = false;
                        if (bulletPos.x > paddle.Position.x && move.MoveDirection.x < 0)
                        {
                            move.MoveDirection.x = -move.MoveDirection.x;
                            shouldBounce = true;
                        }
                        else if (bulletPos.x < paddle.Position.x && move.MoveDirection.x > 0)
                        {
                            move.MoveDirection.x = -move.MoveDirection.x;
                            shouldBounce = true;
                        }

                        if (shouldBounce)
                        {
                            // Calculate vertical direction based on paddle hit position
                            var hitDistance = bulletPos.y - paddle.Position.y;
                            move.MoveDirection.y = math.clamp(
                                math.remap(-2.5f, 2.5f, -4f, 4f, hitDistance),
                                -5f,
                                5f);

                            // Normalize direction
                            move.MoveDirection = math.normalize(move.MoveDirection);
                            return;
                        }
                    }
                }
            }

            // AABB vs Sphere collision detection
            private bool IsBoxColliding(float3 spherePos, float sphereRadius, float3 boxCenter, float3 boxHalfExtents)
            {
                // Find the closest point on the box to the sphere
                var closestPoint = math.clamp(spherePos, boxCenter - boxHalfExtents, boxCenter + boxHalfExtents);
                
                // Calculate distance between sphere center and closest point
                var distance = math.distance(spherePos, closestPoint);
                
                return distance < sphereRadius;
            }
        }
    }
}