using Authoring;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems
{
    public partial struct PointAddSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BulletSpawn>();
            state.RequireForUpdate<Score>();
            state.RequireForUpdate<BounceFlag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var soundSingleton = SystemAPI.GetSingletonRW<BounceSound>();
            var scoreSingleton = SystemAPI.GetSingletonRW<Score>();
            var spawnSingleton = SystemAPI.GetSingleton<BulletSpawn>();
            var numSpawn = 0;
            
            foreach (var (trans, move) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<Move>>()
                         .WithAll<BounceFlag>())
            {
                var sideDist = trans.ValueRO.Position.x;

                switch (sideDist)
                {
                    case > 17:
                        scoreSingleton.ValueRW.Value++;
                        break;
                    case < -17:
                        scoreSingleton.ValueRW.Value--;
                        break;
                }

                if (sideDist is > 17 or < -17)
                {
                    trans.ValueRW.Position = float3.zero;
                    move.ValueRW.MoveSpeed = 5f; //Magic number.  Speed is set on Move IComponentData
                    numSpawn++;
                    soundSingleton.ValueRW.Goal = true;
                }
            }

            state.EntityManager.Instantiate(spawnSingleton.Value, numSpawn, Allocator.Temp);
        }
    }
}