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
using Unity.VisualScripting;
using Cysharp.Threading.Tasks;
using System.Threading;
using FullMoon.Util;

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
        [SerializeField] private float zoomSensitivity = 5f;
        [SerializeField] private float zoomSpeed = 10f;
        [SerializeField] private float minFov = 20f;
        [SerializeField] private float maxFov = 55f;
        
        [Header("Rotation")]
        [SerializeField] private float rotationSensitivity = 3f;
        
        [Header("UI")]
        [SerializeField] private CursorController cursor;
        
        private UnityEngine.Camera mainCamera;
        public float targetFov;
        
        private Vector3 mousePos;
        private Ray mouseRay;
        
        private bool normalMove;
        private bool attackMove;
        private bool altRotation;

        private bool isCraft = false;
        private Vector3 hitPoint;
        private BuildingType buildingType;
        private CancellationTokenSource cancel = new CancellationTokenSource();
        
        private List<BaseUnitController> selectedUnitList;
        
        private void Awake()
        {
            mainCamera = UnityEngine.Camera.main;
            selectedUnitList = new List<BaseUnitController>();
        }

        private void Start()
        {
            targetFov = freeLookCamera.m_Lens.OrthographicSize;
        
            PlayerInputManager.Instance.ZoomEvent.AddEvent(ZoomEvent);
        }

        private void Update()
        {
            selectedUnitList.RemoveAll(unit => unit == null || !unit.gameObject.activeInHierarchy);
            
            freeLookCamera.m_Lens.OrthographicSize = Mathf.Lerp(freeLookCamera.m_Lens.OrthographicSize, targetFov, Time.deltaTime * zoomSpeed);
            
            mousePos = UnityEngine.InputSystem.Mouse.current.position.value;
            mouseRay = mainCamera.ScreenPointToRay(mousePos);
            
            MouseAction();
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

        async UniTaskVoid StartTileTimer(List<CommonUnitController> unitList)
        {
            while (true)
            {
                for (int i = 0; i < unitList.Count; ++i)
                {
                    var unit = unitList[i];
                    if (!unit.gameObject.activeInHierarchy)
                    {
                        unitList.Remove(unit);
                    }
                }

                if (unitList.Count.Equals(0))
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(2));

                    HammerUnitController[] units = FindObjectsByType<HammerUnitController>(FindObjectsSortMode.None);
                    for (int i = 0; i < units.Count(); ++i)
                    {
                        ObjectPoolManager.Instance.ReturnObjectToPool(units[i].gameObject);
                    }

                    TileController.Instance.CreateTile(hitPoint, buildingType);
                    return;
                }

                await UniTask.Delay(500);
            }
        }
        
        public void CreateTileSetting(bool isCraft, BuildingType type)
        {
            this.isCraft = isCraft;
            buildingType = type;
        }

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

            if (PlayerInputManager.Instance.rotation)
            {
                altRotation = true;
                return;
            }

            if (isCraft && Physics.Raycast(mouseRay, out var hg, Mathf.Infinity, (1 << LayerMask.NameToLayer("Ground"))))
            {
                List<CommonUnitController> tempList = new List<CommonUnitController>();
                List<CommonUnitController> unitList = new List<CommonUnitController>();
                isCraft = false;

                tempList = FindObjectsByType<CommonUnitController>(FindObjectsSortMode.None).ToList();
                if (tempList.Count < 6)
                {
                    return;
                }
                
                for (int i = 0; i < 6; ++i)
                {
                    unitList.Add(tempList[i]);
                }

                foreach (var u in unitList)
                {
                    u.GetComponent<CommonUnitController>().CraftBuilding(hg.point, buildingType);
                }

                hitPoint = hg.point;

                StartTileTimer(unitList).Forget();
            }

            DeselectAll();
            
            if (Physics.Raycast(mouseRay, out var hit, Mathf.Infinity, (1 << LayerMask.NameToLayer("Unit"))))
            {
                var unitController = hit.transform.GetComponent<BaseUnitController>();
                ClickSelectUnit(unitController);
            }
        }

        private void HandleLeftDrag()
        {
            if (altRotation == false)
            {
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
            altRotation = false;
        }

        private void HandleRightClick()
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            
            if (Physics.Raycast(mouseRay, out var hit, Mathf.Infinity, (1 << LayerMask.NameToLayer("Ground"))))
            {
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                {
                    MoveSelectedUnits(hit.point);
                    cursor.SetMoveAniTarget(hit.point);
                }
            }
        }

        private void MoveSelectedUnits(Vector3 hitInfoPoint)
        {
            foreach (var unit in selectedUnitList)
            {
                if (unit.UnitType is "Enemy")
                {
                    continue;
                }

                if (unit.Flag is null)
                {
                    unit.MoveToPosition(hitInfoPoint);
                    return;
                }
                
                unit.Flag.MoveToPosition(hitInfoPoint);
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
        
        private void CheckCursorUnit()
        {
            if (attackMove || normalMove)
            {
                return;
            }

            cursor.SetCursorState(
                Physics.Raycast(mouseRay, out var _, Mathf.Infinity, 1 << LayerMask.NameToLayer("Unit"))
                    ? CursorType.Unit
                    : CursorType.Idle);
        }

        #endregion Mouse
    }
}