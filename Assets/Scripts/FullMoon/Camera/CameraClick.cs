using UnityEngine;
using FullMoon.Input;
using System.Collections.Generic;
using FullMoon.Entities.Unit;

namespace FullMoon.Camera
{
    public class CameraClick : MonoBehaviour
    {
        [Header("UnitList")]
        [SerializeField] List<BaseUnitController> selectedUnitList; // 플레이어가 클릭 or 드래그로 선택한 유닛

        [Header("Layer")]
        [SerializeField] LayerMask layerGround;

        [Header("DragInfo")]
        [SerializeField] RectTransform dragRectangle; // 마우스로 드래그한 범위를 가시화하는 Image UI의 RectTransform
        private Rect dragRect; // 마우스로 드래그 한 범위 (xMin~xMax, yMin~yMax)
        private Vector2 start = Vector2.zero; // 드래그 시작 위치
        private Vector2 end = Vector2.zero; // 드래그 종료 위치

        private UnityEngine.Camera mainCamera;

        public Vector3 mousePos;

        private void Awake()
        {
            mainCamera = UnityEngine.Camera.main;
            selectedUnitList = new List<BaseUnitController>();

            // 드래그 모형 초기화
            DrawDragRectangle();
        }

        private void Update()
        {
            MouseAction();
            mousePos = UnityEngine.InputSystem.Mouse.current.position.value;
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
            Ray ray = mainCamera.ScreenPointToRay(UnityEngine.InputSystem.Mouse.current.position.value);
            
            start = UnityEngine.InputSystem.Mouse.current.position.value;
            dragRect = new Rect();

            if (Physics.Raycast(ray, out var hit, Mathf.Infinity))
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
            end = UnityEngine.InputSystem.Mouse.current.position.value;
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
            Ray ray = mainCamera.ScreenPointToRay(UnityEngine.InputSystem.Mouse.current.position.value);
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, layerGround))
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
