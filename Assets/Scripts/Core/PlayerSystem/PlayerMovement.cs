using System;
using Core.Interfaces;
using UnityEngine;

namespace Core.PlayerSystem
{
    [RequireComponent(typeof(PlayerInputHandler))]
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement: MonoBehaviour, IUpdatable
    {
        [Header("References")]
        [SerializeField] private PlayerInputHandler inputHandler;
        [SerializeField] private CharacterController characterController;

        [Header("Settings")]
        [SerializeField] private float moveSpeed = 5.0f;

        private void Awake()
        {
            if (inputHandler == null)
                inputHandler = GetComponent<PlayerInputHandler>();

            if (characterController == null)
                characterController = GetComponent<CharacterController>();
        }

        private void OnEnable()
        {
            Player.Instance.RegisterUpdatable(this);
        }
        
        private void OnDisable()
        {
            Player.Instance.UnregisterUpdatable(this);
        }

        public void OnUpdate()
        {
            Vector2 input = inputHandler.MoveInput;
            Debug.Log("X is " + input.x + "  Y is " + input.y);
            Vector3 moveDirection = new Vector3(input.x, 0, input.y);
            
            characterController.Move(moveDirection * (moveSpeed * Time.deltaTime));
        }
    }
}