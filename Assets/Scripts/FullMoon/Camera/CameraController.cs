using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Cinemachine;
using FullMoon.Input;
using FullMoon.Entities.Unit;
using FullMoon.UI;
using UniRx;
using UniRx.Triggers;
using System.Linq;
using UnityEngine.EventSystems;

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
        
        [Header("Rotation")]
        [SerializeField] private float rotationSensitivity = 3f; // 회전 감도

        [Header("DragInfo")]
        [SerializeField] RectTransform dragRectangle; // 마우스로 드래그한 범위를 가시화하는 Image UI의 RectTransform
        
        [Header("UI")]
        [SerializeField] private CursorController cursor;
        
        private UnityEngine.Camera mainCamera;
        private float targetFov;
        
        private Vector3 mousePos;
        private Ray mouseRay;
        
        private bool normalMove;
        private bool attackMove;
        private bool createUnit;
        private bool altRotation;
        
        private Rect dragRect; // 마우스로 드래그 한 범위 (xMin~xMax, yMin~yMax)
        private Vector2 dragStart = Vector2.zero; // 드래그 시작 위치
        private Vector2 dragEnd = Vector2.zero; // 드래그 종료 위치
        
        private List<BaseUnitController> selectedUnitList; // 플레이어가 클릭 or 드래그로 선택한 유닛
        
        private void Awake()
        {
            mainCamera = UnityEngine.Camera.main;
            selectedUnitList = new List<BaseUnitController>();

            DrawDragRectangle();
        }

        private void Start()
        {
            targetFov = freeLookCamera.m_Lens.FieldOfView;
        
            PlayerInputManager.Instance.ZoomEvent.AddEvent(ZoomEvent);
            DoubleClickAction();

            MainUIController.Instance.ShortcutCancelButton.RegisterCallback<ClickEvent>(evt =>
            {
                OnCancelAction();
            });
            MainUIController.Instance.ShortcutAttackMoveButton.RegisterCallback<ClickEvent>(evt =>
            {
                if (selectedUnitList.Count is not 0)
                {
                    OnAttackMoveAction();
                }
            });
            MainUIController.Instance.ShortcutHoldButton.RegisterCallback<ClickEvent>(evt =>
            {
                if (selectedUnitList.Count is not 0)
                {
                    HoldSelectUnits();
                }
            });
            MainUIController.Instance.ShortcutMoveButton.RegisterCallback<ClickEvent>(evt =>
            {
                if (selectedUnitList.Count is not 0)
                {
                    OnNormalMoveAction();
                }
            });
            MainUIController.Instance.ShortcutRespawnButton.RegisterCallback<ClickEvent>(evt =>
            {
                if (selectedUnitList.Count is not 0)
                {
                    OnRespawnAction();
                }
            });
            MainUIController.Instance.ShortcutStopButton.RegisterCallback<ClickEvent>(evt =>
            {
                if (selectedUnitList.Count is not 0)
                {
                    StopSelectUnits();
                }
            });
        }

        private void Update()
        {
            freeLookCamera.m_Lens.FieldOfView = Mathf.Lerp(freeLookCamera.m_Lens.FieldOfView, targetFov, Time.deltaTime * zoomSpeed);
            
            mousePos = UnityEngine.InputSystem.Mouse.current.position.value;
            mouseRay = mainCamera.ScreenPointToRay(mousePos);
            
            MouseAction();
            ButtonAction();
        }
        
        private void FixedUpdate()
        {
            Vector3 moveDirection = AdjustMovementToCamera(PlayerInputManager.Instance.move);

            if (moveDirection == Vector3.zero && enableCursorMovement)
            {
                moveDirection = AdjustMovementToCamera(GetScreenMovementInput());
                if (moveDirection != Vector3.zero)
                {
                    cursor.SetCursorState(CursorType.Camera);
                }
            }
        
            float movementSpeed = PlayerInputManager.Instance.shift ? shiftMoveSpeed : moveSpeed;
            transform.position += moveDirection * (movementSpeed * Time.fixedDeltaTime);
        }

        private void LateUpdate()
        {
            selectedUnitList.RemoveAll(unit => unit == null || !unit.gameObject.activeInHierarchy);
        }

        private Vector2 GetScreenMovementInput()
        {
            if (UnityEngine.Cursor.lockState != CursorLockMode.Confined)
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
            Vector3 forward = Vector3.Scale(mainCamera.transform.forward, new Vector3(1, 0, 1)).normalized;
            Vector3 right = mainCamera.transform.right;

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
                targetFov -= (scrollValue.y > 0f ? 1f : -1f) * zoomSensitivity;
                targetFov = Mathf.Clamp(targetFov, minFov, maxFov);
            }
        }

        #region Drag
        private void DrawDragRectangle()
        {
            // 드래그 범위를 나타내는 Image UI의 위치
            dragRectangle.position = (dragStart + dragEnd) * 0.5f;
            // 드래그 범위를 나타내는 Image UI의 크기
            dragRectangle.sizeDelta = new Vector2(Mathf.Abs(dragStart.x - dragEnd.x), Mathf.Abs(dragStart.y - dragEnd.y));
        }

        private void CalculateDragRect()
        {
            if (UnityEngine.Input.mousePosition.x < dragStart.x)
            {
                dragRect.xMin = UnityEngine.Input.mousePosition.x;
                dragRect.xMax = dragStart.x;
            }
            else
            {
                dragRect.xMin = dragStart.x;
                dragRect.xMax = UnityEngine.Input.mousePosition.x;
            }

            if (UnityEngine.Input.mousePosition.y < dragStart.y)
            {
                dragRect.yMin = UnityEngine.Input.mousePosition.y;
                dragRect.yMax = dragStart.y;
            }
            else
            {
                dragRect.yMin = dragStart.y;
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
        private void MouseAction()
        {
            CheckCursorUnit();

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
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }
                HandleRightClick();
            }
        }

        private void HandleLeftClick()
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            
            dragStart = mousePos;
            dragRect = new Rect();

            if (PlayerInputManager.Instance.rotation)
            {
                altRotation = true;
                return;
            }

            if (Physics.Raycast(mouseRay, out var hit, Mathf.Infinity, (1 << LayerMask.NameToLayer("Unit")) | (1 << LayerMask.NameToLayer("Ground"))))
            {
                var unitController = hit.transform.GetComponent<BaseUnitController>();

                if (normalMove)
                {
                    ForceMoveSelectedUnites(hit.point);
                    cursor.SetMoveAniTarget(hit.point);
                    normalMove = false;
                }
                else if (attackMove)
                {
                    if (unitController is not null)
                    {
                        ForceAttackSelectUnits(unitController);
                        return;
                    }
                    AttackSelectedUnits(hit.point);
                    cursor.SetMoveAniTarget(hit.point);
                }
                else if (createUnit)
                {
                    var deadUnit = hit.transform.GetComponent<RespawnController>();
                    if (deadUnit != null)
                    {
                        ReviveUnit(deadUnit);
                        createUnit = false;
                    }
                }
                else
                {
                    if (unitController != null)
                    {
                        if (PlayerInputManager.Instance.shift)
                        {
                            ShiftClickSelectUnit(unitController);
                        }
                        else if (PlayerInputManager.Instance.ctrl)
                        {
                            SelectAllUnit(unitController.UnitClass);
                        }
                        else
                        {
                            ClickSelectUnit(unitController);
                        }
                    }
                    else if (!PlayerInputManager.Instance.shift)
                    {
                        DeselectAll();

                        cursor.SetCursorState(CursorType.Idle);
                    }
                }
            }
        }

        private void HandleLeftDrag()
        {
            if (dragStart == Vector2.zero)
            {
                return;
            }
            
            if (altRotation == false)
            {
                dragEnd = mousePos;
                DrawDragRectangle();
                return;
            }
            
            if (PlayerInputManager.Instance.rotation == false)
            {
                return;
            }

            freeLookCamera.m_XAxis.Value += freeLookCamera.m_XAxis.m_InputAxisValue * rotationSensitivity;
        }

        private void HandleLeftRelease()
        {
            CalculateDragRect();
            SelectUnits();
            dragStart = Vector2.zero;
            dragEnd = Vector2.zero;
            altRotation = false;
            DrawDragRectangle();
        }

        private void HandleRightClick()
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            
            if (normalMove || attackMove || createUnit)
            {
                OnCancelAction();
                return;
            }

            if (Physics.Raycast(mouseRay, out var hit, Mathf.Infinity, (1 << LayerMask.NameToLayer("Unit")) | (1 << LayerMask.NameToLayer("Ground"))))
            {
                var unitController = hit.transform.GetComponent<BaseUnitController>();
                var deadUnit = hit.transform.GetComponent<RespawnController>();

                if (deadUnit is not null)
                {
                    ReviveUnit(deadUnit);
                }
                else if (unitController is not null && selectedUnitList.Count != 0)
                {
                    ForceAttackSelectUnits(unitController);
                }
                else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                {
                    ForceMoveSelectedUnites(hit.point);
                    cursor.SetMoveAniTarget(hit.point);
                }
            }
        }

        /// <summary>
        /// 더블 클릭 상호작용
        /// </summary>
        private void DoubleClickAction()
        {
            var clickStream = this.UpdateAsObservable().Where(_ => UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame);

            clickStream
                .Buffer(clickStream.Throttle(TimeSpan.FromMilliseconds(200)))
                .Where(_ => EventSystem.current is not null && EventSystem.current.IsPointerOverGameObject() == false)
                .Where(x => x.Count >= 2)
                .Where(count => selectedUnitList.Count != 0)
                .Subscribe(_ => SelectAllUnit(selectedUnitList.First().UnitClass));
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
            if (newUnit.UnitType.Equals("Enemy") || selectedUnitList.Contains(newUnit))
            {
                return;
            }

            SelectUnit(newUnit);
        }
        
        /// <summary>
        /// 선택된 모든 유닛을 강제로 이동
        /// </summary>
        private void ForceMoveSelectedUnites(Vector3 end)
        {
            foreach (var unit in selectedUnitList)
            {
                if (unit.UnitType.Equals("Enemy"))
                    continue;

                unit.AttackMove = false;
                unit.MoveToPosition(end);
            }

            MainUIController.Instance.canMove.Value = false;
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

            MainUIController.Instance.OffButton();
            selectedUnitList.Clear();
        }

        /// <summary>
        /// 매개변수로 받아온 newUnit 선택 설정
        /// </summary>
        private void SelectUnit(BaseUnitController newUnit)
        {
            if (newUnit.UnitType is "Player")
            {
                if (newUnit.UnitClass is "Main")
                {
                    MainUIController.Instance.isMainUnit = true;
                    MainUIController.Instance.MainUnitActiveButton();
                }
                else
                    MainUIController.Instance.UnitActiveButton();
            }

            // 유닛이 선택되었을 때 호출하는 메소드
            newUnit.Select();
            // 선택한 유닛 정보를 리스트에 저장
            selectedUnitList.Add(newUnit);
        }

        /// <summary>
        /// 화면 안에 있는 모든 유닛 선택 (직군 별로)
        /// </summary>
        private void SelectAllUnit(string unitClass)
        {
            var units = FindObjectsByType<BaseUnitController>(FindObjectsSortMode.None);
            foreach (var unit in units.Where(u => u.UnitClass == unitClass))
            {
                Vector3 screenPosition = mainCamera.WorldToScreenPoint(unit.transform.position);
                Vector2 position2D = new Vector2(screenPosition.x, screenPosition.y);

                Rect screenRect = new Rect(0, 0, mainCamera.pixelWidth, mainCamera.pixelHeight);
                if (screenRect.Contains(position2D))
                {
                    DragSelectUnit(unit);
                }
            }
        }

        /// <summary>
        /// 매개변수로 받아온 newUnit 선택 해제 설정
        /// </summary>
        private void DeselectUnit(BaseUnitController newUnit)
        {
            if (newUnit.UnitType is "Player")
            {
                if (newUnit.UnitClass is "Main")
                {
                    MainUIController.Instance.isMainUnit = false;
                    MainUIController.Instance.ShortcutRespawnButton.SetEnabled(false);
                }
            }

            // 유닛이 해제되었을 때 호출하는 메소드
            newUnit.Deselect();
            // 선택한 유닛 정보를 리스트에서 삭제
            selectedUnitList.Remove(newUnit);
        }
        
        /// <summary>
        /// 선택된 유닛들에게 어택땅 명령
        /// </summary>
        private void AttackSelectedUnits(Vector3 targetPosition)
        {
            foreach (var unit in selectedUnitList)
            {
                if (unit.UnitType.Equals("Enemy"))
                {
                    continue;
                }
                
                unit.OnUnitAttack(targetPosition);
            }

            attackMove = false;
            MainUIController.Instance.canAttack.Value = false;
        }

        private void ForceAttackSelectUnits(BaseUnitController target)
        {
            foreach (var unit in selectedUnitList)
            {
                if (unit.UnitType.Equals("Enemy") || unit.UnitType == target.UnitType)
                {
                    continue;
                }

                unit.OnUnitForceAttack(target);
            }
        }

        private void CheckCursorUnit()
        {
            if (attackMove || normalMove || createUnit)
            {
                return;
            }

            cursor.SetCursorState(
                Physics.Raycast(mouseRay, out var _, Mathf.Infinity, 1 << LayerMask.NameToLayer("Unit"))
                    ? CursorType.Unit
                    : CursorType.Idle);
        }

        private void ReviveUnit(RespawnController unit)
        {
            // foreach (var controller in selectedUnitList
            //     .Where(controller => controller.GetComponent<BaseUnitController>() is MainUnitController)
            //     .Select(controller => controller.GetComponent<MainUnitController>()))
            // {
            //     controller.CheckAbleToRespawn(unit);
            // }

            cursor.SetCursorState(CursorType.Idle);

            MainUIController.Instance.canRespawn.Value = false;
        }
        
        #endregion Mouse

        #region Button

        private void ButtonAction()
        {
            if (PlayerInputManager.Instance.stop)
            {
                StopSelectUnits();
            }
            else if (PlayerInputManager.Instance.hold)
            {
                HoldSelectUnits();
            }
            else if (PlayerInputManager.Instance.cancel)
            {
                OnCancelAction();
            }
            else if (PlayerInputManager.Instance.mainSelect)
            {
                DeselectAll();
                OnCancelAction();
                SelectAllUnit("Main");
            }
            else if (PlayerInputManager.Instance.shieldSelect)
            {
                DeselectAll();
                OnCancelAction();
                SelectAllUnit("Shield");
            }
            else if (PlayerInputManager.Instance.rangedSelect)
            {
                DeselectAll();
                OnCancelAction();
                SelectAllUnit("Ranged");
            }
            else if (PlayerInputManager.Instance.meleeSelect)
            {
                DeselectAll();
                OnCancelAction();
                SelectAllUnit("Melee");
            }
            else if (selectedUnitList.Count != 0)
            {
                if (PlayerInputManager.Instance.attackMove)
                {
                    OnAttackMoveAction();
                }
                else if (PlayerInputManager.Instance.normalMove)
                {
                    OnNormalMoveAction();
                }
                else if (PlayerInputManager.Instance.respawn)
                {
                    OnRespawnAction();
                }
            }
        }

        private void StopSelectUnits()
        {
            foreach(var unit in selectedUnitList)
            {
                unit.OnUnitStop();
            }
        }

        private void HoldSelectUnits()
        {
            foreach(var unit in selectedUnitList)
            {
                unit.OnUnitHold();
            }
        }

        private void OnNormalMoveAction()
        {
            normalMove = true;
            cursor.SetCursorState(CursorType.Move);
            MainUIController.Instance.canMove.Value = true;
        }

        private void OnAttackMoveAction()
        {
            attackMove = true;
            cursor.SetCursorState(CursorType.Attack);
            MainUIController.Instance.canAttack.Value = true;
        }

        private void OnCancelAction()
        {
            normalMove = false;
            attackMove = false;
            createUnit = false;
            cursor.SetCursorState(CursorType.Idle);

            MainUIController.Instance.canAttack.Value = false;
            MainUIController.Instance.canMove.Value = false;
            MainUIController.Instance.canRespawn.Value = false;
        }

        private void OnRespawnAction()
        {
            foreach (var controller in selectedUnitList
                .Select(u => u.GetComponent<MainUnitController>())
                .Where(c => c != null))
            {
                createUnit = true;
                cursor.SetCursorState(CursorType.Create);
                MainUIController.Instance.canRespawn.Value = true;
            }
            
            PlayerInputManager.Instance.respawn = false;
        }

        #endregion Button
    }
}