using UnityEngine;
using FullMoon.Unit;
using FullMoon.Input;
using System.Collections.Generic;
using FullMoon.Entities.Unit;

namespace FullMoon.Camera
{
    public class CameraClick : MonoBehaviour
    {
        [Header("UnitList")]
        [SerializeField] UnitSpawner unitSpawner;
        [SerializeField] List<BaseUnitController> selectedUnitList; // 플레이어가 클릭 or 드래그로 선택한 유닛
        private List<BaseUnitController> UnitList; // 맵에 존재하는 모든 유닛

        [Header("Layer")]
        [SerializeField] LayerMask layerPlayerUnit;

        [SerializeField] LayerMask layerEnemyUnit;
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

        private void Start()
        {
            // UnitList = unitSpawner.GetUnitList();
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
            // // 모든 유닛을 검사
            // foreach (BaseUnitController unit in UnitList)
            // {
            //     // 유닛의 월드 좌표를 화면 좌표로 변환해 드래그 범위 내에 있는지 검사
            //     if (dragRect.Contains(mainCamera.WorldToScreenPoint(unit.transform.position)))
            //     {
            //         DragSelectUnit(unit);
            //     }
            // }
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
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(UnityEngine.InputSystem.Mouse.current.position.value);
            start = UnityEngine.InputSystem.Mouse.current.position.value;
            dragRect = new Rect();

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                var rangedUnitController = hit.transform.GetComponent<RangedUnitController>();
                if (rangedUnitController != null)
                {
                    if (PlayerInputManager.Instance.shift)
                    {
                        ShiftClickSelectUnit(rangedUnitController);
                    }
                    else
                    {
                        ClickSelectUnit(rangedUnitController);
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
            start = end = Vector2.zero;
            DrawDragRectangle();
        }

        private void HandleRightClick()
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(UnityEngine.InputSystem.Mouse.current.position.value);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerEnemyUnit | layerGround))
            {
                var rangedUnitController = hit.transform.GetComponent<RangedUnitController>();
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
        public void ClickSelectUnit(BaseUnitController newUnit)
        {
            // // 기존에 선택되어 있는 모든 유닛 해제
            // DeselectAll();
            //
            // SelectUnit(newUnit);
        }

        /// <summary>
        /// Shift+마우스 클릭으로 유닛을 선택할 때 호출
        /// </summary>
        public void ShiftClickSelectUnit(BaseUnitController newUnit)
        {
            // // 유닛이 리스트에 있다면
            // if (selectedUnitList.Contains(newUnit))
            //     DeselectUnit(newUnit);
            // // 유닛이 리스트에 없다면
            // else
            //     SelectUnit(newUnit);
        }

        /// <summary>
        /// 마우스 드래그로 유닛을 선택할 때 호출
        /// </summary>
        public void DragSelectUnit(BaseUnitController newUnit)
        {
            // // 새로운 유닛을 선택했으면
            // if (!selectedUnitList.Contains(newUnit) || newUnit.GetUnitHandType() == BaseUnit.UnithandType.Player)
            // {
            //     SelectUnit(newUnit);
            // }
        }
        
        /// <summary>
        /// 선택된 모든 유닛을 이동할 때 호출
        /// </summary>
        public void MoveSelectedUnits(Vector3 end)
        {
            // for (int i = 0; i < selectedUnitList.Count; ++i)
            // {
            //     if (selectedUnitList[i].GetUnitHandType() == BaseUnit.UnithandType.Enemy)
            //         continue;
            //     
            //     selectedUnitList[i].MoveTo(end, true);
            // }
        }

        /// <summary>
        /// 모든 유닛의 선택을 해제할 때 호출
        /// </summary>
        public void DeselectAll()
        {
            // for (int i = 0; i < selectedUnitList.Count; ++i)
            // {
            //     selectedUnitList[i].DeSelectUnit();
            // }
            //
            // selectedUnitList.Clear();
        }

        /// <summary>
        /// 매개변수로 받아온 newUnit 선택 설정
        /// </summary>
        private void SelectUnit(BaseUnitController newUnit)
        {
            // // 유닛이 선택되었을 때 호출하는 메소드
            // newUnit.SelectUnit();
            // // 선택한 유닛 정보를 리스트에 저장
            // selectedUnitList.Add(newUnit);
        }

        /// <summary>
        /// 매개변수로 받아온 newUnit 선택 해제 설정
        /// </summary>
        private void DeselectUnit(BaseUnitController newUnit)
        {
            // // 유닛이 해제되었을 때 호출하는 메소드
            // newUnit.DeSelectUnit();
            // // 선택한 유닛 정보를 리스트에서 삭제
            // selectedUnitList.Remove(newUnit);
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
