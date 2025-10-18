using Unity.Entities;
using UnityEngine.InputSystem;

namespace Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
    public partial class InputSystem : SystemBase
    {
        private InputSystem_Actions _inputControls;

        protected override void OnCreate()
        {
            _inputControls = new InputSystem_Actions();
            EntityManager.CreateSingleton<LeftPaddleMovement>();
            EntityManager.CreateSingleton<RightPaddleMovement>();
            
            _inputControls.Player.LeftPaddle.performed += OnLeftMovePerformed;
            _inputControls.Player.RightPaddle.performed += OnRightMovePerformed;

            _inputControls.Player.LeftPaddle.canceled += OnLeftMovePerformed;
            _inputControls.Player.RightPaddle.canceled += OnRightMovePerformed;
        }

        protected override void OnStartRunning() => _inputControls.Enable();
        
        private void OnLeftMovePerformed(InputAction.CallbackContext context)
        {
            SystemAPI.SetSingleton(new LeftPaddleMovement
            {
                Value = context.ReadValue<float>()
            });
        }
        
        private void OnRightMovePerformed(InputAction.CallbackContext context)
        {
            SystemAPI.SetSingleton(new RightPaddleMovement
            {
                Value = context.ReadValue<float>()
            });
        }

        protected override void OnUpdate() { }
        protected override void OnStopRunning() => _inputControls.Disable();
    }

    public struct LeftPaddleMovement : IComponentData
    {
        public float Value;
    }

    public struct RightPaddleMovement : IComponentData
    {
        public float Value;
    }
}