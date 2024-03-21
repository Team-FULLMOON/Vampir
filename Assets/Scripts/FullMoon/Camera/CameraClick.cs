using System.Collections.Generic;
using FullMoon.Unit;
using FullMoon.Util;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FullMoon.Camera
{
    public class CameraClick : MonoBehaviour
    {
        [Header("UnitList")]
        [SerializeField] UnitSpawner                unitSpawner;
        [SerializeField] List<UnitController>       selectedUnitList;               // 플레이어가 클릭 or 드래그로 선택한 유닛
        public List<UnitController>                 unitList { private set; get; }  // 맵에 존재하는 모든 유닛

        [Header("Layer")]
        [SerializeField] LayerMask layerUnit;
        [SerializeField] LayerMask layerGround;

        [Header("DragInfo")]
        [SerializeField] RectTransform dragRectangle;                    // 마우스로 드래그한 범위를 가시화하는 Image UI의 RectTransform
        private Rect dragRect;                                           // 마우스로 드래그 한 범위 (xMin~xMax, yMin~yMax)
        private Vector2 start = Vector2.zero;                            // 드래그 시작 위치
        private Vector2 end = Vector2.zero;                              // 드래그 종료 위치

        [Header("Button")]
        private bool isShift = false;

        private UnityEngine.Camera mainCamera;

        public Vector3 mousePos;

        private void Awake()
        {
            mainCamera = UnityEngine.Camera.main;
            selectedUnitList = new List<UnitController>();
            unitList = unitSpawner.SpawnUnits();

            // 드래그 모형 초기화
            DrawDragRectangle();
        }

        private void Update()
        {
            MouseAction();
            mousePos = Mouse.current.position.value;
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
            // 모든 유닛을 검사
            foreach (UnitController unit in unitList)
            {
                // 유닛의 월드 좌표를 화면 좌표로 변환해 드래그 범위 내에 있는지 검사
                if (dragRect.Contains(mainCamera.WorldToScreenPoint(unit.transform.position)))
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
            // GetMouseButtonDown(0)
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                RaycastHit hit;
                Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.value);
                start = Mouse.current.position.value;
                dragRect = new Rect();
                GameManage.PrintLog("작동");
                // 마우스 왼쪽 클릭으로 유닛 선택 or 해제
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerUnit))
                {
                    if (hit.transform.GetComponent<UnitController>() == null) return;

                    if (isShift)
                    {
                        ShiftClickSelectUnit(hit.transform.GetComponent<UnitController>());
                    }
                    else
                    {
                        ClickSelectUnit(hit.transform.GetComponent<UnitController>());
                    }
                }
                // 광선에 부딪히는 오브젝트가 없을 때
                else
                {
                    if (!isShift)
                    {
                        DeselectAll();
                    }
                }
            }

            // GetMouseButton(0)
            if (Mouse.current.leftButton.isPressed)
            {
                end = Mouse.current.position.value;

                // start, end가 (0, 0)인 상태로 이미지의 크기를 (0, 0)으로 설정해 화면에 보이지 않도록 함
                DrawDragRectangle();
            }

            // GetMouseButtonUp(0)
            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                // 마우스 클릭을 종료할 때 드래그 범위 내에 있는 유닛 선택
                CalculateDragRect();
                SelectUnits();

                // 마우스 클릭을 종료할 때 드래그 범위가 보이지 않도록
                // start, end 위치를 (0, 0)으로 설정하고 드래그 범위를 그린다
                start = end = Vector2.zero;
                DrawDragRectangle();
            }

            // GetMouseButtonDown(1)
            // 마우스 오른쪽 클릭으로 유닛 이동
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                RaycastHit hit;
                Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.value);

                // 유닛 오브젝트(layerUnit)를 클릭했을 때
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerGround))
                {
                    MoveSelectedUnits(hit.point);
                }
            }
        }

        /// <summary>
        /// 마우스 클릭으로 유닛을 선택할 때 호출
        /// </summary>
        public void ClickSelectUnit(UnitController newUnit)
        {
            // 기존에 선택되어 있는 모든 유닛 해제
            DeselectAll();

            SelectUnit(newUnit);
        }

        /// <summary>
        /// Shift+마우스 클릭으로 유닛을 선택할 때 호출
        /// </summary>
        public void ShiftClickSelectUnit(UnitController newUnit)
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
        public void DragSelectUnit(UnitController newUnit)
        {
            // 새로운 유닛을 선택했으면
            if (!selectedUnitList.Contains(newUnit))
            {
                SelectUnit(newUnit);
            }
        }

        /// <summary>
        /// 선택된 모든 유닛을 이동할 때 호출
        /// </summary>
        public void MoveSelectedUnits(Vector3 end)
        {
            for (int i = 0; i < selectedUnitList.Count; ++i)
            {
                selectedUnitList[i].MoveTo(end);
            }
        }

        /// <summary>
        /// 모든 유닛의 선택을 해제할 때 호출
        /// </summary>
        public void DeselectAll()
        {
            for (int i = 0; i < selectedUnitList.Count; ++i)
            {
                selectedUnitList[i].DeSelectUnit();
            }

            selectedUnitList.Clear();
        }

        /// <summary>
        /// 매개변수로 받아온 newUnit 선택 설정
        /// </summary>
        private void SelectUnit(UnitController newUnit)
        {
            // 유닛이 선택되었을 때 호출하는 메소드
            newUnit.SelectUnit();
            // 선택한 유닛 정보를 리스트에 저장
            selectedUnitList.Add(newUnit);
        }

        /// <summary>
        /// 매개변수로 받아온 newUnit 선택 해제 설정
        /// </summary>
        private void DeselectUnit(UnitController newUnit)
        {
            // 유닛이 해제되었을 때 호출하는 메소드
            newUnit.DeSelectUnit();
            // 선택한 유닛 정보를 리스트에서 삭제
            selectedUnitList.Remove(newUnit);
        }
        #endregion Mouse

        private void OnShift(InputValue value)
        {
            isShift = value.isPressed;
        }
    }
}
