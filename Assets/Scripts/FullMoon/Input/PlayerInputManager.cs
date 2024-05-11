using System;
using UnityEngine;
using FullMoon.Util;
using MyBox;
using UnityEngine.Serialization;

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
        public bool ctrl;
        public Vector2 zoom;
        public bool stop;
        public bool hold;
        public bool rotation;
        public bool attackMove;
        public bool normalMove;
        public bool cancel;
        public bool mainSelect;
        public bool swordSelect;
        public bool crossbowSelect;
        public bool spearSelect;

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

        public void OnCtrl(InputValue value)
        {
            CtrlInput(value.isPressed);
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
        
        public void OnAttackMove(InputValue value)
        {
            AttackMoveInput(value.isPressed);
        }

        public void OnNormalMove(InputValue value)
        {
            NormalMoveInput(value.isPressed);
        }

        public void OnCancel(InputValue value)
        {
            CancelInput(value.isPressed);
        }

        public void OnMainSelect(InputValue value)
        {
            MainSelectInput(value.isPressed);
        }

        public void OnSwordSelect(InputValue value)
        {
            SwordSelectInput(value.isPressed);
        }

        public void OnCrossbowSelect(InputValue value)
        {
            CrossbowSelectInput(value.isPressed);
        }

        public void OnSpearSelect(InputValue value)
        {
            SpearSelectInput(value.isPressed);
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

        public readonly GenericEventSystem<bool> CtrlEvent = new();
        public void CtrlInput(bool input)
        {
            ctrl = input;
            CtrlEvent.TriggerEvent(input);
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
        
        public readonly GenericEventSystem<bool> AttackMoveEvent = new();
        public void AttackMoveInput(bool input)
        {
            attackMove = input;
            AttackMoveEvent.TriggerEvent(input);
        } 

        public readonly GenericEventSystem<bool> NormalMoveEvent = new();
        public void NormalMoveInput(bool input)
        {
            normalMove = input;
            NormalMoveEvent.TriggerEvent(input);
        }

        public readonly GenericEventSystem<bool> CancelEvent = new();
        public void CancelInput(bool input)
        {
            cancel = input;
            CancelEvent.TriggerEvent(input);
        }

        public readonly GenericEventSystem<bool> MainSelectEvent = new();
        public void MainSelectInput(bool input)
        {
            mainSelect = input;
            MainSelectEvent.TriggerEvent(input);
        }

        public readonly GenericEventSystem<bool> SwordSelectEvent = new();
        public void SwordSelectInput(bool input)
        {
            swordSelect = input;
            SwordSelectEvent.TriggerEvent(input);
        }
        
        public readonly GenericEventSystem<bool> CrossbowSelectEvent = new();
        public void CrossbowSelectInput(bool input)
        {
            crossbowSelect = input;
            CrossbowSelectEvent.TriggerEvent(input);
        }

        public readonly GenericEventSystem<bool> SpearSelectEvent = new();
        public void SpearSelectInput(bool input)
        {
            spearSelect = input;
            SpearSelectEvent.TriggerEvent(input);
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