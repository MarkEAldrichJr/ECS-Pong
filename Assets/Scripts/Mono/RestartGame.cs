using Systems;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Mono
{
    public partial class RestartGame : SystemBase
    {
        private InputSystem_Actions _inputControls;
        private EntityQuery _query;
        
        protected override void OnCreate()
        {
            _inputControls = new InputSystem_Actions();
            _inputControls.Player.RestartGame.performed += Restart;
            _query = GetEntityQuery(
                new EntityQueryBuilder(Allocator.Temp)
                    .WithAll<GameStartedFlag>());
        }

        protected override void OnStartRunning()
        {
            _inputControls.Enable();
        }

        protected override void OnStopRunning()
        {
            _inputControls.Disable();
        }

        protected override void OnUpdate() { }        
        
        private void Restart(InputAction.CallbackContext context)
        {
            if (SystemAPI.TryGetSingleton<GameStartedFlag>(out _))
            {
                EntityManager.DestroyEntity(_query);
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }
}