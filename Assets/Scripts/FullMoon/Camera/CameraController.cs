using System;
using UnityEngine;
using System.Collections.Generic;
using Cinemachine;
using FullMoon.Input;
using FullMoon.Entities.Unit;

namespace FullMoon.Camera
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private CinemachineFreeLook freeLookCamera;

        [SerializeField] private bool enableCursorMovement;
        
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 12f;
        [SerializeField] private float shiftMoveSpeed = 25f;
    
        [Header("Zoom")]
        [SerializeField] private float zoomSensitivity = 5f; // 마우스 스크롤 감도
        [SerializeField] private float zoomSpeed = 10f; // 줌 속도
        [SerializeField] private float minFov = 20f;
        [SerializeField] private float maxFov = 55f;
        
        [Header("ClickSetting")]
        List<BaseUnitController> selectedUnitList; // 플레이어가 클릭 or 드래그로 선택한 유닛
        private UnityEngine.Camera mainCamera;
        private Vector3 mousePos;

        [Header("DragInfo")]
        [SerializeField] RectTransform dragRectangle; // 마우스로 드래그한 범위를 가시화하는 Image UI의 RectTransform
        private Rect dragRect; // 마우스로 드래그 한 범위 (xMin~xMax, yMin~yMax)
        private Vector2 start = Vector2.zero; // 드래그 시작 위치
        private Vector2 end = Vector2.zero; // 드래그 종료 위치
    
        private float _targetFov;

        private void Awake()
        {
            mainCamera = UnityEngine.Camera.main;
            selectedUnitList = new List<BaseUnitController>();

            // 드래그 모형 초기화
            DrawDragRectangle();
        }

        private void Start()
        {
            _targetFov = freeLookCamera.m_Lens.FieldOfView;
        
            PlayerInputManager.Instance.ZoomEvent.AddEvent(ZoomEvent);
        }

        private void FixedUpdate()
        {
            Vector3 moveDirection = AdjustMovementToCamera(PlayerInputManager.Instance.move);

            if (moveDirection == Vector3.zero && enableCursorMovement)
            {
                moveDirection = AdjustMovementToCamera(GetScreenMovementInput());
            }
        
            float movementSpeed = PlayerInputManager.Instance.shift ? shiftMoveSpeed : moveSpeed;
            transform.position += moveDirection * (movementSpeed * Time.fixedDeltaTime);
        }

        private void Update()
        {
            // FOV를 목표값으로 부드럽게 조정
            freeLookCamera.m_Lens.FieldOfView = Mathf.Lerp(freeLookCamera.m_Lens.FieldOfView, _targetFov, Time.deltaTime * zoomSpeed);
            
            MouseAction();
            mousePos = UnityEngine.InputSystem.Mouse.current.position.value;
        }
    
        private Vector2 GetScreenMovementInput()
        {
            if (Cursor.lockState != CursorLockMode.Confined)
            {
                return Vector2.zero;
            }

            Vector2 mousePosition = UnityEngine.Input.mousePosition;
            float normalizedX = (mousePosition.x / Screen.width) * 2 - 1;
            float normalizedY = (mousePosition.y / Screen.height) * 2 - 1;

            Vector2 normalizedPosition = new Vector2(normalizedX, normalizedY);
            normalizedPosition.Normalize();

            // 화면 가장자리에 있는지 확인
            if (Mathf.Abs(normalizedX) > 0.95f || Mathf.Abs(normalizedY) > 0.95f)
            {
                return normalizedPosition;
            }

            return Vector2.zero;
        }


        private Vector3 AdjustMovementToCamera(Vector2 input)
        {
            Vector3 forward = freeLookCamera.transform.forward;
            Vector3 right = freeLookCamera.transform.right;

            forward.y = 0;
            right.y = 0;

            forward.Normalize();
            right.Normalize();

            return (forward * input.y + right * input.x);
        }

        private void ZoomEvent(Vector2 scrollValue)
        {
            if (scrollValue.y != 0f)
            {
                _targetFov -= (scrollValue.y > 0f ? 1f : -1f) * zoomSensitivity;
                _targetFov = Mathf.Clamp(_targetFov, minFov, maxFov);
            }
        }
        
        #region Drag
        private void DrawDragRectangle()
        {
            // 드래그 범위를 나타내는 Image UI의 위치
            dragRectangle.position = (start + end) * 0.5f;
            // 드래그 범위를 나타내는 Image UI의 크기
            dragRectangle.sizeDelta = new Vector2(Mathf.Abs(start.x - end.x), Mathf.Abs(start.y - end.y));
        }

        private void CalculateDragRect()
        {
            if (UnityEngine.Input.mousePosition.x < start.x)
            {
                dragRect.xMin = UnityEngine.Input.mousePosition.x;
                dragRect.xMax = start.x;
            }
            else
            {
                dragRect.xMin = start.x;
                dragRect.xMax = UnityEngine.Input.mousePosition.x;
            }

            if (UnityEngine.Input.mousePosition.y < start.y)
            {
                dragRect.yMin = UnityEngine.Input.mousePosition.y;
                dragRect.yMax = start.y;
            }
            else
            {
                dragRect.yMin = start.y;
                dragRect.yMax = UnityEngine.Input.mousePosition.y;
            }
        }

        private void SelectUnits()
        {
            foreach (var unit in FindObjectsByType<BaseUnitController>(FindObjectsSortMode.None))
            {
                Vector3 screenPosition = mainCamera.WorldToScreenPoint(unit.transform.position);
                Vector2 position2D = new Vector2(screenPosition.x, screenPosition.y);

                if (dragRect.Contains(position2D))
                {
                    DragSelectUnit(unit);
                }
            }
        }
        #endregion Drag
        
        #region Mouse

        /// <summary>
        /// 마우스 클릭 액션
        /// </summary>
        public void MouseAction()
        {
            // 마우스 왼쪽 버튼 처리
            if (UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
            {
                HandleLeftClick();
            }
            else if (UnityEngine.InputSystem.Mouse.current.leftButton.isPressed)
            {
                HandleLeftDrag();
            }
            else if (UnityEngine.InputSystem.Mouse.current.leftButton.wasReleasedThisFrame)
            {
                HandleLeftRelease();
            }

            // 마우스 오른쪽 버튼 처리
            if (UnityEngine.InputSystem.Mouse.current.rightButton.wasPressedThisFrame)
            {
                HandleRightClick();
            }
        }

        private void HandleLeftClick()
        {
            Ray ray = mainCamera.ScreenPointToRay(mousePos);
            
            start = mousePos;
            dragRect = new Rect();

            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, (1 << LayerMask.NameToLayer("Unit")) | (1 << LayerMask.NameToLayer("Ground"))))
            {
                var unitController = hit.transform.GetComponent<BaseUnitController>();
                if (unitController != null)
                {
                    if (PlayerInputManager.Instance.shift)
                    {
                        ShiftClickSelectUnit(unitController);
                    }
                    else
                    {
                        ClickSelectUnit(unitController);
                    }
                }
                else if (!PlayerInputManager.Instance.shift)
                {
                    DeselectAll();
                }
            }
        }

        private void HandleLeftDrag()
        {
            end = mousePos;
            DrawDragRectangle();
        }

        private void HandleLeftRelease()
        {
            CalculateDragRect();
            SelectUnits();
            start = Vector2.zero;
            end = Vector2.zero;
            DrawDragRectangle();
        }

        private void HandleRightClick()
        {
            Ray ray = mainCamera.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Ground")))
            {
                var rangedUnitController = hit.transform.GetComponent<BaseUnitController>();
                if (rangedUnitController != null)
                {
                    AttackSelectedUnits(rangedUnitController);
                }
                else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                {
                    MoveSelectedUnits(hit.point);
                }
            }
        }

        /// <summary>
        /// 마우스 클릭으로 유닛을 선택할 때 호출
        /// </summary>
        private void ClickSelectUnit(BaseUnitController newUnit)
        {
            // 기존에 선택되어 있는 모든 유닛 해제
            DeselectAll();
            
            SelectUnit(newUnit);
        }

        /// <summary>
        /// Shift+마우스 클릭으로 유닛을 선택할 때 호출
        /// </summary>
        private void ShiftClickSelectUnit(BaseUnitController newUnit)
        {
            // 유닛이 리스트에 있다면
            if (selectedUnitList.Contains(newUnit))
            {
                DeselectUnit(newUnit);
            }
            // 유닛이 리스트에 없다면
            else
            {
                SelectUnit(newUnit);
            }
        }

        /// <summary>
        /// 마우스 드래그로 유닛을 선택할 때 호출
        /// </summary>
        private void DragSelectUnit(BaseUnitController newUnit)
        {
            if (newUnit.unitType.Equals("Enemy") || selectedUnitList.Contains(newUnit))
            {
                return;
            }

            SelectUnit(newUnit);
        }
        
        /// <summary>
        /// 선택된 모든 유닛을 이동할 때 호출
        /// </summary>
        private void MoveSelectedUnits(Vector3 end)
        {
            foreach (var unit in selectedUnitList)
            {
                if (unit.unitType.Equals("Enemy"))
                    continue;
                
                unit.MoveToPosition(end);
            }
        }

        /// <summary>
        /// 모든 유닛의 선택을 해제할 때 호출
        /// </summary>
        private void DeselectAll()
        {
            foreach (var unit in selectedUnitList)
            {
                unit.Deselect();
            }

            selectedUnitList.Clear();
        }

        /// <summary>
        /// 매개변수로 받아온 newUnit 선택 설정
        /// </summary>
        private void SelectUnit(BaseUnitController newUnit)
        {
            // 유닛이 선택되었을 때 호출하는 메소드
            newUnit.Select();
            // 선택한 유닛 정보를 리스트에 저장
            selectedUnitList.Add(newUnit);
        }

        /// <summary>
        /// 매개변수로 받아온 newUnit 선택 해제 설정
        /// </summary>
        private void DeselectUnit(BaseUnitController newUnit)
        {
            // 유닛이 해제되었을 때 호출하는 메소드
            newUnit.Deselect();
            // 선택한 유닛 정보를 리스트에서 삭제
            selectedUnitList.Remove(newUnit);
        }
        
        /// <summary>
        /// 선택된 유닛들에게 강제공격 명령
        /// </summary>
        public void AttackSelectedUnits(BaseUnitController unit)
        {
            // for (int index = 0; index < selectedUnitList.Count; ++index)
            //     selectedUnitList[index].SetTarget(unit);
        }
        
        #endregion Mouse
    }
}
