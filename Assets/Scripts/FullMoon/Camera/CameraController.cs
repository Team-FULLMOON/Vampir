using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using FullMoon.UI;
using FullMoon.Input;
using FullMoon.Entities.Unit;
using FullMoon.Entities;
using FullMoon.Util;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using System.Linq.Expressions;

namespace FullMoon.Camera
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private CinemachineFreeLook freeLookCamera;

        [SerializeField] private bool enableCursorMovement;
        
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 12f;
        [SerializeField] private float shiftMoveSpeed = 25f;
        [SerializeField] private float cameraAreaLimit = 20f;
    
        [Header("Zoom")]
        [SerializeField] private float zoomSensitivity = 5f;
        [SerializeField] private float zoomSpeed = 10f;
        [SerializeField] private float minFov = 20f;
        [SerializeField] private float maxFov = 55f;
        
        [Header("Rotation")]
        [SerializeField] private float rotationSensitivity = 3f;
        
        [Header("Tile Map")]
        [SerializeField] private GameObjectDictionary tileMap;
        
        [Header("UI")]
        [SerializeField] private CursorController cursor;
        [SerializeField] private List<ButtonUnlock> buttonUnlock;
        [SerializeField] private Button tileButton;
        
        private UnityEngine.Camera mainCamera;
        private float targetFov;
        
        private Vector3 mousePos;
        private Ray mouseRay;
        
        private bool altRotation;

        private bool canCraft;
        private BuildingType buildingType;
        
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
            ButtonAction();
        }
        
        private Vector3 ClampToRotatedSquare(Vector3 position, Vector3 center, float limit, float angle)
        {
            // 각도를 라디안으로 변환
            float rad = angle * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);

            // 원점 중심의 회전된 좌표계로 변환
            Vector3 dir = position - center;
            float rotatedX = cos * dir.x - sin * dir.z;
            float rotatedZ = sin * dir.x + cos * dir.z;

            // 회전된 좌표계를 기준으로 클램프
            rotatedX = Mathf.Clamp(rotatedX, -limit, limit);
            rotatedZ = Mathf.Clamp(rotatedZ, -limit, limit);

            // 다시 원래 좌표계로 변환
            position.x = cos * rotatedX + sin * rotatedZ + center.x;
            position.z = -sin * rotatedX + cos * rotatedZ + center.z;

            return position;
        }
        
        private void FixedUpdate()
        {
            Vector3 moveDirection = AdjustMovementToCamera(PlayerInputManager.Instance.move);

            if (moveDirection == Vector3.zero && enableCursorMovement)
            {
                moveDirection = AdjustMovementToCamera(GetScreenMovementInput());
                cursor.SetCursorState(moveDirection != Vector3.zero ? CursorType.Camera : CursorType.Idle);
            }
        
            float movementSpeed = PlayerInputManager.Instance.shift ? shiftMoveSpeed : moveSpeed;
            Vector3 newPosition = transform.position + moveDirection * (movementSpeed * Time.fixedDeltaTime);

            newPosition = ClampToRotatedSquare(newPosition, Vector3.zero, cameraAreaLimit, freeLookCamera.m_XAxis.Value);
            
            transform.position = newPosition;
        }

        private Vector2 GetScreenMovementInput()
        {
            if (!Application.isFocused || Cursor.lockState != CursorLockMode.Confined)
            {
                return Vector2.zero;
            }

            Vector2 mousePosition = UnityEngine.Input.mousePosition;
            float normalizedX = (mousePosition.x / Screen.width) * 2 - 1;
            float normalizedY = (mousePosition.y / Screen.height) * 2 - 1;

            Vector2 normalizedPosition = new Vector2(normalizedX, normalizedY);
            normalizedPosition.Normalize();

            // 화면 가장자리에 있는지 확인
            if (Mathf.Abs(normalizedX) > 0.98f || Mathf.Abs(normalizedY) > 0.98f)
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
        
        public void CreateTileSetting(bool canCraft, BuildingType type)
        {
            this.canCraft = canCraft;
            buildingType = type;
        }

        #region Mouse

        /// <summary>
        /// 마우스 클릭 액션
        /// </summary>
        private void MouseAction()
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
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }
                HandleRightClick();
            }
        }
        
        private void HandleBuildingTile(RaycastHit hitInfo)
        {
            if (!UnityEngine.AI.NavMesh.SamplePosition(hitInfo.point, out var samplePoint, 0.1f, (1 << UnityEngine.AI.NavMesh.GetAreaFromName("Walkable"))))
            {
                CancelCrafting("지을 수 있는 공간이 없습니다.", "red");
                return;
            }

            var unitList = GetAvailableUnits();
            if (unitList.Count < 6)
            {
                CancelCrafting("자원 유닛이 부족합니다.", "red");
                return;
            }

            var sampleCellPosition = tileMap.GetComponentByName<Tilemap>("Building").WorldToCell(samplePoint.position);
            if (tileMap.GetComponentByName<Tilemap>("Building").HasTile(sampleCellPosition))
            {
                CancelCrafting("지을 수 있는 공간이 없습니다. 이미 건물이 존재합니다.", "red");
                return;
            }

            CraftBuilding(unitList, hitInfo.point, samplePoint.position);
        }

        private void HandleGroundTile(RaycastHit hitInfo)
        {
            var sampleCellPosition = tileMap.GetComponentByName<Tilemap>("Ground").WorldToCell(hitInfo.point);
            if (tileMap.GetComponentByName<Tilemap>("Ground").HasTile(sampleCellPosition) || hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                CancelCrafting("해당 위치에 이미 땅이 존재합니다.", "red");
                return;
            }

            canCraft = false;
            TileController.Instance.CreateTile(hitInfo.point, buildingType);
        }

        private List<CommonUnitController> GetAvailableUnits()
        {
            return FindObjectsByType<CommonUnitController>(FindObjectsSortMode.None)
                .Where(t => t.UnitType is "Player" && t.gameObject.activeInHierarchy && t.Alive)
                .Take(6)
                .ToList();
        }

        private void CraftBuilding(List<CommonUnitController> unitList, Vector3 buildPoint, Vector3 samplePoint)
        {
            foreach (var unit in unitList)
            {
                unit.CraftBuilding(buildPoint);
            }

            canCraft = false;
            TileController.Instance.CreateTile(samplePoint, buildingType);
        }

        private void CancelCrafting(string message, string color)
        {
            canCraft = false;
            Debug.LogWarning(message);
            ToastManager.Instance.ShowToast(message, color);
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

            if (canCraft)
            {
                if (Physics.Raycast(mouseRay, out var hitInfo, Mathf.Infinity, (1 << LayerMask.NameToLayer("Ground"))) && buildingType is not BuildingType.Ground)
                {
                    HandleBuildingTile(hitInfo);
                }
                else if (Physics.Raycast(mouseRay, out var waterHitInfo, Mathf.Infinity, (1 << LayerMask.NameToLayer("Water")) | (1 << LayerMask.NameToLayer("Ground"))) && buildingType is BuildingType.Ground)
                {
                    HandleGroundTile(waterHitInfo);
                }
                else
                {
                    CancelCrafting("생성할 수 없는 위치입니다.", "red");
                }
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
                    // cursor.SetMoveAniTarget(hit.point);
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

        #endregion Mouse

        #region Button

        private void ButtonAction()
        {
            OnCancelAction();

            buttonUnlock.Where(button => button.unlockButton.interactable)
                        .ToList()
                        .ForEach(button =>
                        {
                            button.unlockButton.GetComponentInChildren<Text>().text = button.buttonName + "\n건설하기";
                        });
        }

        private void OnCancelAction()
        {
            if (PlayerInputManager.Instance.cancel)
            {
                canCraft = false;
                buildingType = BuildingType.None;

                tileButton.interactable = false;
                tileButton.interactable = true;
                buttonUnlock.Where(btn => btn.unlockButton.interactable)
                            .ToList()
                            .ForEach(btn =>
                            {
                                btn.unlockButton.interactable = false;
                                btn.unlockButton.interactable = true;
                            });
            }
        }

        #endregion Button
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;

            Vector3 center = Vector3.zero;
            Vector3 size = new Vector3(cameraAreaLimit * 2, 1, cameraAreaLimit * 2);
        
            Gizmos.matrix = Matrix4x4.TRS(center, Quaternion.Euler(0, freeLookCamera.m_XAxis.Value, 0), Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, size);
        }
#endif
    }
}