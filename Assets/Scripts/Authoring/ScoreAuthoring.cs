using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    public class ScoreAuthoring : MonoBehaviour
    {
        public int maxScore;
        public GameObject bulletPrefab;
        
        private class ScoreAuthoringBaker : Baker<ScoreAuthoring>
        {
            public override void Bake(ScoreAuthoring authoring)
            {
                var e = GetEntity(authoring, TransformUsageFlags.None);
                var entityPrefab = GetEntity(authoring.bulletPrefab, TransformUsageFlags.Dynamic);
                
                AddComponent(e, new Score
                {
                    Value = 0,
                    MaxScore = authoring.maxScore
                });
                AddComponent(e, new BulletSpawn
                {
                    Value = entityPrefab
                });
                AddComponent<BounceSound>(e);
            }
        }
    }

    public struct BulletSpawn : IComponentData
    {
        public Entity Value;
    }
    
    public struct Score : IComponentData
    {
        public int Value;
        public int MaxScore;
    }

    public struct BounceSound : IComponentData
    {
        public bool Wall;
        public bool Paddle;
        public bool Goal;
    }
}