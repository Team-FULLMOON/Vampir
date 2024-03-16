using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraClick : MonoBehaviour
{
    [Header("UnitList")]
    [SerializeField] UnitSpawner                unitSpawner;
    [SerializeField] List<UnitController>       selectedUnitList;               // Ŭ�� or �巡�׷� ���õ� ����
    public List<UnitController>                 unitList { private set; get; }  // ��� ����

    [Header("Layer")]
    [SerializeField] LayerMask layerUnit;
    [SerializeField] LayerMask layerGround;

    [Header("DragInfo")]
    [SerializeField] RectTransform dragRectangle;                    // ���콺�� �巡���� ������ ����ȭ�ϴ� Image UI�� RectTransform
    private Rect dragRect;                                  // ���콺�� �巡�� �� ���� (xMin~xMax, yMin~yMax)
    private Vector2 start = Vector2.zero;                   // �巡�� ���� ��ġ
    private Vector2 end = Vector2.zero;                     // �巡�� ���� ��ġ

    [Header("Button")]
    private bool isShift = false;

    private Camera mainCamera;

    public Vector3 mousePos;

    private void Awake()
    {
        mainCamera = Camera.main;
        selectedUnitList = new List<UnitController>();
        unitList = unitSpawner.SpawnUnits();

        // start, end�� (0, 0)�� ���·� �̹����� ũ�⸦ (0, 0)���� ������ ȭ�鿡 ������ �ʵ��� ��
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
        // �巡�� ������ ��Ÿ���� Image UI�� ��ġ
        dragRectangle.position = (start + end) * 0.5f;
        // �巡�� ������ ��Ÿ���� Image UI�� ũ��
        dragRectangle.sizeDelta = new Vector2(Mathf.Abs(start.x - end.x), Mathf.Abs(start.y - end.y));
    }

    private void CalculateDragRect()
    {
        if (Input.mousePosition.x < start.x)
        {
            dragRect.xMin = Input.mousePosition.x;
            dragRect.xMax = start.x;
        }
        else
        {
            dragRect.xMin = start.x;
            dragRect.xMax = Input.mousePosition.x;
        }

        if (Input.mousePosition.y < start.y)
        {
            dragRect.yMin = Input.mousePosition.y;
            dragRect.yMax = start.y;
        }
        else
        {
            dragRect.yMin = start.y;
            dragRect.yMax = Input.mousePosition.y;
        }
    }

    private void SelectUnits()
    {
        // ��� ������ �˻�
        foreach (UnitController unit in unitList)
        {
            // ������ ���� ��ǥ�� ȭ�� ��ǥ�� ��ȯ�� �巡�� ���� ���� �ִ��� �˻�
            if (dragRect.Contains(mainCamera.WorldToScreenPoint(unit.transform.position)))
            {
                DragSelectUnit(unit);
            }
        }
    }
    #endregion Drag

    #region Mouse

    /// <summary>
    /// ���콺 ��ȣ�ۿ� �Լ�
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

            // ������ �ε����� ������Ʈ�� ���� �� (=������ Ŭ������ ��)
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
            // ������ �ε����� ������Ʈ�� ���� ��
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

            // ���콺�� Ŭ���� ���·� �巡�� �ϴ� ���� �巡�� ������ �̹����� ǥ��
            DrawDragRectangle();
        }

        // GetMouseButtonUp(0)
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            // ���콺 Ŭ���� ������ �� �巡�� ���� ���� �ִ� ���� ����
            CalculateDragRect();
            SelectUnits();

            // ���콺 Ŭ���� ������ �� �巡�� ������ ������ �ʵ���
            // start, end ��ġ�� (0, 0)���� �����ϰ� �巡�� ������ �׸���
            start = end = Vector2.zero;
            DrawDragRectangle();
        }

        // GetMouseButtonDown(1)
        // ���콺 ������ Ŭ������ ���� �̵�
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.value);

            // ���� ������Ʈ(layerUnit)�� Ŭ������ ��
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerGround))
            {
                MoveSelectedUnits(hit.point);
            }
        }
    }

    /// <summary>
    /// ���콺 Ŭ������ ������ ������ �� ȣ��
    /// </summary>
    public void ClickSelectUnit(UnitController newUnit)
    {
        // ������ ���õǾ� �ִ� ��� ���� ����
        DeselectAll();

        SelectUnit(newUnit);
    }

    /// <summary>
    /// Shift+���콺 Ŭ������ ������ ������ �� ȣ��
    /// </summary>
    public void ShiftClickSelectUnit(UnitController newUnit)
    {
        // ������ ���õǾ� �ִ� ������ ����������
        if (selectedUnitList.Contains(newUnit))
        {
            DeselectUnit(newUnit);
        }
        // ���ο� ������ ����������
        else
        {
            SelectUnit(newUnit);
        }
    }

    /// <summary>
    /// ���콺 �巡�׷� ������ ������ �� ȣ��
    /// </summary>
    public void DragSelectUnit(UnitController newUnit)
    {
        // ���ο� ������ ����������
        if (!selectedUnitList.Contains(newUnit))
        {
            SelectUnit(newUnit);
        }
    }

    /// <summary>
    /// ���õ� ��� ������ �̵��� �� ȣ��
    /// </summary>
    public void MoveSelectedUnits(Vector3 end)
    {
        for (int i = 0; i < selectedUnitList.Count; ++i)
        {
            selectedUnitList[i].MoveTo(end);
        }
    }

    /// <summary>
    /// ��� ������ ������ ������ �� ȣ��
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
    /// �Ű������� �޾ƿ� newUnit ���� ����
    /// </summary>
    private void SelectUnit(UnitController newUnit)
    {
        // ������ ���õǾ��� �� ȣ���ϴ� �޼ҵ�
        newUnit.SelectUnit();
        // ������ ���� ������ ����Ʈ�� ����
        selectedUnitList.Add(newUnit);
    }

    /// <summary>
    /// �Ű������� �޾ƿ� newUnit ���� ���� ����
    /// </summary>
    private void DeselectUnit(UnitController newUnit)
    {
        // ������ �����Ǿ��� �� ȣ���ϴ� �޼ҵ�
        newUnit.DeSelectUnit();
        // ������ ���� ������ ����Ʈ���� ����
        selectedUnitList.Remove(newUnit);
    }
    #endregion Mouse

    private void OnShift(InputValue value)
    {
        isShift = value.isPressed;
    }
}
