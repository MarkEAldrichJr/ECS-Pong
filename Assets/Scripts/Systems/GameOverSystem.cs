using Authoring;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct GameOverSystem : ISystem
    {
        private EntityQuery _bounceQuery;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            var builder = new EntityQueryBuilder(Allocator.Temp).WithAll<BounceFlag>();
            _bounceQuery = state.GetEntityQuery(builder);
            
            state.RequireForUpdate<Score>();
            state.RequireForUpdate(_bounceQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var score = SystemAPI.GetSingleton<Score>();
            if (math.abs(score.Value) < score.MaxScore) return;
            
            state.EntityManager.DestroyEntity(_bounceQuery);
        }
    }
}