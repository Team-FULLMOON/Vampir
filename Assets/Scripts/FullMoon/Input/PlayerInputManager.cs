using System;
using UnityEngine;
using FullMoon.Util;
using MyBox;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace FullMoon.Input
{
    [Serializable]
    public enum CursorLockType
    {
        None,
        Locked,
        Confined,
    }

    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputManager : ComponentSingleton<PlayerInputManager>
    {
        [Header("Camera Input Values")]
        public Vector2 move;
        public bool analogMovement;
        public bool shift;
        public Vector2 zoom;
        public bool stop;
        public bool hold;
        public bool rotation;
        public bool respawn;
        public bool attack;
        public bool moveKey;

        [Header("Mouse Cursor Lock Settings")] 
        public CursorLockType cursorLockType;

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

        public void OnStop(InputValue value)
        {
            StopInput(value.isPressed);
        }

        public void OnHold(InputValue value)
        {
            HoldInput(value.isPressed);
        }
        
        public void OnRotation(InputValue value)
        {
            RotationInput(value.isPressed);
        }
        
        public void OnRespawn(InputValue value)
        {
            RespawnInput(value.isPressed);
        }

        public void OnAttack(InputValue value)
        {
            AttackInput(value.isPressed);
        }

        public void OnMoveKey(InputValue value)
        {
            MoveKeyInput(value.isPressed);
        }
#endif
		
        public readonly GenericEventSystem<Vector2> MoveEvent = new();
        public void MoveInput(Vector2 input)
        {
            move = input;
            MoveEvent.TriggerEvent(input);
        } 
        
        public readonly GenericEventSystem<bool> ShiftEvent = new();
        public void ShiftInput(bool input)
        {
            shift = input;
            ShiftEvent.TriggerEvent(input);
        }
        
        public readonly GenericEventSystem<Vector2> ZoomEvent = new();
        public void ZoomInput(Vector2 input)
        {
            zoom = input;
            ZoomEvent.TriggerEvent(input);
        }

        public readonly GenericEventSystem<bool> StopEvent = new();
        public void StopInput(bool input)
        {
            stop = input;
            StopEvent.TriggerEvent(stop);
        }

        public readonly GenericEventSystem<bool> HoldEvent = new();
        public void HoldInput(bool input)
        {
            hold = input;
            HoldEvent.TriggerEvent(hold);
        }
        
        public readonly GenericEventSystem<bool> RotationEvent = new();
        public void RotationInput(bool input)
        {
            rotation = input;
            RotationEvent.TriggerEvent(input);
        } 
        
        public readonly GenericEventSystem<bool> RespawnEvent = new();
        public void RespawnInput(bool input)
        {
            respawn = input;
            RespawnEvent.TriggerEvent(input);
        } 

        public readonly GenericEventSystem<bool> AttackEvent = new();
        public void AttackInput(bool input)
        {
            attack = input;
            AttackEvent.TriggerEvent(input);
        } 

        public readonly GenericEventSystem<bool> MoveKeyEvent = new();
        public void MoveKeyInput(bool input)
        {
            moveKey = input;
            MoveKeyEvent.TriggerEvent(input);
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorLockState(cursorLockType);
        }

        private void SetCursorLockState(CursorLockType type)
        {
            switch (type)
            {
                case CursorLockType.None:
                    Cursor.lockState = CursorLockMode.None;
                    break;
                case CursorLockType.Locked:
                    Cursor.lockState = CursorLockMode.Locked;
                    break;
                case CursorLockType.Confined:
                    Cursor.lockState = CursorLockMode.Confined;
                    break;
            }
        }
    }
}