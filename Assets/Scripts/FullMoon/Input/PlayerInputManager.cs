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

        [Header("Mouse Cursor Settings")] 
        public CursorType cursorType;

#if ENABLE_INPUT_SYSTEM
        public void OnMove(InputValue value)
        {
            MoveInput(value.Get<Vector2>());
        }
#endif
		
        public void MoveInput(Vector2 newMoveDirection)
        {
            move = newMoveDirection;
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