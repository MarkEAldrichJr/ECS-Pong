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

        private enum CollisionType
        {
            Paddle, Wall
        }
        
        public void OnCreate(ref SystemState state)
        {
            var builder = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<BounceFlag, Move>().WithNone<InitializeFlag>();
            
            state.RequireForUpdate(state.GetEntityQuery(builder));

            // Cache entity queries for walls and paddles
            _wallQuery = state.GetEntityQuery(
                ComponentType.ReadOnly<WallTag>(),
                ComponentType.ReadOnly<LocalTransform>());
            _paddleQuery = state.GetEntityQuery(
                ComponentType.ReadOnly<PaddleTag>(),
                ComponentType.ReadOnly<LocalTransform>());

            state.RequireForUpdate<BounceSound>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            //Get Bounce Sound Singleton
            var soundSingleton = SystemAPI.GetSingletonRW<BounceSound>();
            
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
                    Position = wallTransforms[i].Position
                };
            }
            
            for (var i = 0; i < paddleEntities.Length; i++)
            {
                paddleData[i] = new CollisionData
                {
                    Position = paddleTransforms[i].Position
                };
            }

            var collisionEvents = new NativeQueue<CollisionType>(Allocator.TempJob);
            var collisionWriter = collisionEvents.AsParallelWriter();
            
            // Schedule parallel job
            var job = new BounceJob
            {
                WallData = wallData,
                PaddleData = paddleData,
                CollisionEvents = collisionWriter,
                BulletRadius = 0.5f * 0.5f,
                WallHalfExtents = new float3(17f, 0.25f, 0.5f),
                PaddleHalfExtents = new float3(0.5f, 2f, 0.5f)
            };
            
            state.Dependency = job.ScheduleParallel(state.Dependency);
            state.Dependency.Complete(); // Ensure completion before cleanup
            
            while (collisionEvents.TryDequeue(out var collisionType))
            {
                if (collisionType == CollisionType.Paddle)
                    soundSingleton.ValueRW.Paddle = true;
                else if (collisionType == CollisionType.Wall)
                    soundSingleton.ValueRW.Wall = true;
            }
            
            // Cleanup
            wallEntities.Dispose();
            wallTransforms.Dispose();
            paddleEntities.Dispose();
            paddleTransforms.Dispose();
            wallData.Dispose();
            paddleData.Dispose();
            collisionEvents.Dispose();
        }

        private struct CollisionData
        {
            public float3 Position;
        }

        [BurstCompile]
        private partial struct BounceJob : IJobEntity
        {
            [ReadOnly] public NativeArray<CollisionData> WallData;
            [ReadOnly] public NativeArray<CollisionData> PaddleData;
            [ReadOnly] public float BulletRadius;
            [ReadOnly] public float3 WallHalfExtents;
            [ReadOnly] public float3 PaddleHalfExtents;
            [WriteOnly] public NativeQueue<CollisionType>.ParallelWriter CollisionEvents;

            private void Execute(
                ref Move move,
                in LocalTransform trans, in BounceFlag bounceFlag)
            {
                var bulletPos = trans.Position;
                var hasCollided = false;

                // Check wall collisions
                foreach (var wall in WallData)
                {
                    
                    if (IsBoxColliding(bulletPos, BulletRadius, wall.Position, WallHalfExtents))
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

                        CollisionEvents.Enqueue(CollisionType.Wall);
                        if (hasCollided) return;
                    }
                }

                // Check paddle collisions
                foreach (var paddle in PaddleData)
                {
                    if (IsBoxColliding(bulletPos, BulletRadius, paddle.Position, 
                            PaddleHalfExtents))
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

                            //ensure bullets keep moving at a minimum lateral speed
                            if (move.MoveDirection.x < 0)
                            {
                                move.MoveDirection.x = math.clamp(move.MoveDirection.x,
                                    -math.INFINITY, -0.5f);
                            }
                            else
                            {
                                move.MoveDirection.x = math.clamp(move.MoveDirection.x, 0.5f,
                                    -math.INFINITY);
                            }
                            
                            CollisionEvents.Enqueue(CollisionType.Paddle);
                            return;
                        }
                    }
                }
            }

            // AABB vs Sphere collision detection
            private static bool IsBoxColliding(
                float3 spherePos, 
                float sphereRadiusSquared,
                float3 boxCenter,
                float3 boxHalfExtents)
            {
                // Find the closest point on the box to the sphere
                var closestPoint = math.clamp(spherePos, boxCenter - boxHalfExtents, boxCenter + boxHalfExtents);
                
                // Calculate distance between sphere center and closest point
                var distance = math.distancesq(spherePos, closestPoint);
                
                return distance < sphereRadiusSquared;
            }
        }
    }
}