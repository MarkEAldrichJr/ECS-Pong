using Authoring;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.InputSystem;

namespace Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class StartGameSystem : SystemBase
    {
        private InputSystem_Actions _inputControls;
        
        protected override void OnCreate()
        {
            _inputControls = new InputSystem_Actions();
            _inputControls.Player.StartGame.performed += OnStartGame;
        }

        protected override void OnStartRunning() => _inputControls.Enable();
        protected override void OnUpdate() { }
        protected override void OnStopRunning() => _inputControls.Disable();
        
        private void OnStartGame(InputAction.CallbackContext context)
        {
            if (SystemAPI.TryGetSingleton<GameStartedFlag>(out _)) return;
            
            var spawn = SystemAPI.GetSingleton<BulletSpawn>().Value;
            
            var firstBullet = EntityManager
                .Instantiate(spawn, 1, Allocator.Temp);

            SystemAPI.GetComponentRW<LocalTransform>(firstBullet[0])
                .ValueRW.Position = float3.zero;

            EntityManager.CreateSingleton<GameStartedFlag>();
        }
    }

    public struct GameStartedFlag : IComponentData
    {
        public bool Value; //purely here because TryGetSingleton doesn't work for flags
    }
}