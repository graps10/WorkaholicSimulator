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

        // Actions
		private static InputAction horizontalAxis, verticalAxis, 
			jumpAction, sprintAction, interactAction;
		
        public static InputDeviceType CurrentInputDevice { get; private set; }

        public delegate void ButtonDelegate();
        public delegate void AxisDelegate(float axis);
        
        #region Events
        
        // Movement Events
        public static event AxisDelegate OnHorizontalAxis, OnVerticalAxis;
        public static event ButtonDelegate OnJumpPerformed;
        public static event ButtonDelegate OnSprintStarted, OnSprintCanceled;
        
        // Interaction Events
        public static event ButtonDelegate OnInteractPerformed;
        
        // System Events
        public static event Action<InputDeviceType> OnInputDeviceChanged;
        
        #endregion
		
		private static float mouseSensitivity = 1f;
		private static bool isControlledByMouse;
		
		internal static void Initialize()
        {
            inputActions = new();
            inputActions.Enable();
            ChangeControlsToMain();
			
			Player.Instance.OnUpdateEvent += OnUpdate;

			AddCheckDeviceSubscriptions();
			
			jumpAction.performed += OnJumpInput;
			
			sprintAction.performed += OnSprintInputStarted;
			sprintAction.canceled += OnSprintInputCanceled;
			
			interactAction.performed += OnInteractInput;
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

        #region Update Loop
        
        private static void OnUpdate()
        {
	        if(Player.Instance == null || SceneManager.IsChangingPlaymode || !Application.isPlaying)
		        Dispose();
	        
	        if(Player.Instance.EntityGameObjectIsNull) 
		        return;

	        HandleMovementInputs();
        }
        
        private static void HandleMovementInputs()
        {
	        // Horizontal
	        float horizontalInput = 0f;

	        if (horizontalAxis.ReadValue<float>() != 0)
		        horizontalInput = horizontalAxis.ReadValue<float>();
			
	        horizontalInput = Mathf.Clamp(horizontalInput, -1f, 1f);
	        OnHorizontalAxis?.Invoke(horizontalInput);

	        // Vertical
	        float verticalInput = 0f;

	        if (verticalAxis.ReadValue<float>() != 0)
		        verticalInput = verticalAxis.ReadValue<float>();

	        verticalInput = Mathf.Clamp(verticalInput, -1f, 1f);
	        OnVerticalAxis?.Invoke(verticalInput);
        }
        
        #endregion
		
		#region Input Callbacks
		
		private static void OnJumpInput(InputAction.CallbackContext context) => OnJumpPerformed?.Invoke();
		private static void OnSprintInputStarted(InputAction.CallbackContext context) => OnSprintStarted?.Invoke();
		private static void OnSprintInputCanceled(InputAction.CallbackContext context) => OnSprintCanceled?.Invoke();
        
		private static void OnInteractInput(InputAction.CallbackContext context) => OnInteractPerformed?.Invoke();
		
		#endregion
		
		#region System Logic
		
		private static void ChangeControlsToMain()
		{
			horizontalAxis = inputActions.MainControls.MovementHorizontal;
			verticalAxis = inputActions.MainControls.MovementVertical;
			jumpAction = inputActions.MainControls.Jump; 
			sprintAction = inputActions.MainControls.Sprint;
			interactAction = inputActions.MainControls.Interact;
		}

		private static void AddCheckDeviceSubscriptions()
		{
			horizontalAxis.performed += CheckDevice;
			verticalAxis.performed += CheckDevice;
			jumpAction.performed += CheckDevice;
			sprintAction.performed += CheckDevice;
			interactAction.performed += CheckDevice;
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
	            horizontalAxis.performed -= CheckDevice;
	            horizontalAxis.Dispose();
	            horizontalAxis = null;
            }
	        
	        if (verticalAxis != null)
	        {
		        verticalAxis.performed -= CheckDevice;
		        verticalAxis.Dispose();
		        verticalAxis = null;
	        }
	        
	        if (jumpAction != null)
	        {
		        jumpAction.performed -= CheckDevice;
		        jumpAction.performed -= OnJumpInput;
		        jumpAction.Dispose();
		        jumpAction = null;
	        }
	        
	        if (sprintAction != null)
	        {
		        sprintAction.performed -= CheckDevice;
		        sprintAction.performed -= OnSprintInputStarted;
		        sprintAction.canceled -= OnSprintInputCanceled;
		        sprintAction.Dispose();
		        sprintAction = null;
	        }
	        
	        if (interactAction != null)
	        {
		        interactAction.performed -= CheckDevice;
		        interactAction.performed -= OnInteractInput;
		        interactAction.Dispose();
		        interactAction = null;
	        }
        }
		
		#endregion
	}
}
		
		