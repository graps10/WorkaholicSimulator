using System;
using Core.PlayerSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.InputSystem
{
    public enum InputDeviceType { Keyboard, Gamepad }

    public static class InputManager
    {
        private static GameInput inputActions;

		private static InputAction horizontalAxis, verticalAxis, leftMouseButton, jumpAction;
		
        public static InputDeviceType CurrentInputDevice { get; private set; }

        public delegate void ButtonDelegate();
        public delegate void AxisDelegate(float axis);

        public static event ButtonDelegate OnJumpPerformed;
        public static event AxisDelegate OnHorizontalAxis, OnVerticalAxis;
		
		private static float mouseSensitivity = 1f;
		private static bool isControledByMouse;
		
		public static event Action<InputDeviceType> OnInputDeviceChanged;
		
		internal static void Initialize()
        {
            inputActions = new();
            inputActions.Enable();
            ChangeControlsToMain();

			Player.Instance.OnUpdateEvent += OnUpdate;
			
			horizontalAxis.performed += CheckDevice;
			verticalAxis.performed += CheckDevice;
			jumpAction.performed += CheckDevice;
        }

        private static void CheckDevice(InputAction.CallbackContext context)
        {
	        var currentDevice = context.action.activeControl.device;
	        var newDevice = currentDevice switch
            {
	            Keyboard or Mouse => InputDeviceType.Keyboard,
	            _ => InputDeviceType.Gamepad
            };

            if (newDevice == CurrentInputDevice) 
	            return;

            CurrentInputDevice = newDevice;
            OnInputDeviceChanged?.Invoke(newDevice);

            Cursor.visible = newDevice == InputDeviceType.Keyboard;
        }

        private static void OnUpdate()
        {
	        if(Player.Instance == null || SceneManager.IsChangingPlaymode || !Application.isPlaying)
		        Dispose();
	        
	        if(Player.Instance.EntityGameObjectIsNull) 
		        return;

            VerticalAxis();
            HorizontalAxis();
		}
        
		private static void HorizontalAxis()
		{
			float horizontalInput = 0f;

			if (horizontalAxis.ReadValue<float>() != 0)
				horizontalInput = horizontalAxis.ReadValue<float>();
			
			horizontalInput = Mathf.Clamp(horizontalInput, -1f, 1f);
            OnHorizontalAxis?.Invoke(horizontalInput);
        }
		
		private static void VerticalAxis()
		{
			float verticalInput = 0f;

			if (verticalAxis.ReadValue<float>() != 0)
				verticalInput = verticalAxis.ReadValue<float>();

			verticalInput = Mathf.Clamp(verticalInput, -1f, 1f);
			OnVerticalAxis?.Invoke(verticalInput);
        }
		
		private static void OnJumpInput(InputAction.CallbackContext context)
		{
			OnJumpPerformed?.Invoke();
		}
		
		private static void ChangeControlsToMain()
		{
			horizontalAxis = inputActions.MainControls.MovementHorizontal;
			verticalAxis = inputActions.MainControls.MovementVertical;
			
			jumpAction = inputActions.MainControls.AdditionalUse; 
			jumpAction.performed += OnJumpInput;
		}

		private static void Dispose()
        {
	        if(Player.Instance != null)
		        Player.Instance.OnUpdateEvent -= OnUpdate;
	        
	        if (inputActions != null)
	        {
		        inputActions.Disable();
		        inputActions.Dispose();
		        inputActions = null;
	        }

	        if (horizontalAxis != null)
            {
	            horizontalAxis.Dispose();
	            horizontalAxis = null;
            }
	        
	        if (verticalAxis != null)
	        {
		        verticalAxis.Dispose();
		        verticalAxis = null;
	        }
	        
	        if (jumpAction != null)
	        {
		        jumpAction.performed -= OnJumpInput;
		        jumpAction.Dispose();
		        jumpAction = null;
	        }
        }
	}
}