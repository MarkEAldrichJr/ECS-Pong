using Systems;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Mono
{
    public class RemoveOnPlay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        
        private EntityQuery _query;

        private void Start()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            _query = em.CreateEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                .WithAll<GameStartedFlag>());
        }
        
        private void Update()
        {
            if (_query.TryGetSingleton<GameStartedFlag>(out _))
            {
                text.gameObject.SetActive(false);
                //deactivate self
                gameObject.SetActive(false);
            }
        }
    }
}
