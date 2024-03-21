using System;
using UnityEngine;
using FullMoon.Util;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace FullMoon.Input
{
    [Serializable]
    public enum CursorType
    {
        None,
        Locked,
        Confined
    }
    
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputManager : ComponentSingleton<PlayerInputManager>
    {
        [Header("Camera Input Values")]
        public Vector2 move;
        public bool analogMovement;
        public bool shift;
        public Vector2 zoom;

        [Header("Mouse Cursor Settings")] 
        public CursorType cursorType;

#if ENABLE_INPUT_SYSTEM
        public void OnMove(InputValue value)
        {
            MoveInput(value.Get<Vector2>());
        }
        
        private void OnShift(InputValue value)
        {
            ShiftInput(value.isPressed);
        }

        public void OnZoom(InputValue value)
        {
            ZoomInput(value.Get<Vector2>());
        }
#endif
		
        public readonly GenericEventSystem<Vector2> moveEvent = new();
        public void MoveInput(Vector2 input)
        {
            move = input;
            moveEvent.TriggerEvent(input);
        } 
        
        public readonly GenericEventSystem<bool> shiftEvent = new();
        public void ShiftInput(bool input)
        {
            shift = input;
            shiftEvent.TriggerEvent(input);
        }
        
        public readonly GenericEventSystem<Vector2> ZoomEvent = new();
        public void ZoomInput(Vector2 input)
        {
            zoom = input;
            ZoomEvent.TriggerEvent(input);
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorState(cursorType);
        }

        private void SetCursorState(CursorType type)
        {
            switch (type)
            {
                case CursorType.None:
                    Cursor.lockState = CursorLockMode.None;
                    break;
                case CursorType.Locked:
                    Cursor.lockState = CursorLockMode.Locked;
                    break;
                case CursorType.Confined:
                    Cursor.lockState = CursorLockMode.Confined;
                    break;
            }
        }
    }
}