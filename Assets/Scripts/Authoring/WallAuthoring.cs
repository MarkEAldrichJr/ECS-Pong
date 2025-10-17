using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    public class WallAuthoring : MonoBehaviour
    {
        public class WallAuthoringBaker : Baker<WallAuthoring>
        {
            public override void Bake(WallAuthoring authoring)
            {
                var e =  GetEntity(authoring, TransformUsageFlags.WorldSpace);
                AddComponent<WallTag>(e);
            }
        }
    }

    public struct WallTag :  IComponentData { }
}