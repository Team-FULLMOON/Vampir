using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    [SerializeField] GameObject unitMarker;
    [SerializeField] float speed;
    private Vector3 movePos;
    public bool isMove = false;

    private void Update()
    {
        if (isMove)
        {
            transform.position = Vector3.MoveTowards(transform.position, movePos, speed * Time.deltaTime);
            if (transform.position.x == movePos.x && transform.position.z == movePos.z)
                isMove = false;
        }
    }

    public void SelectUnit()
    {
        unitMarker.SetActive(true);
    }

    public void DeSelectUnit()
    {
        unitMarker.SetActive(false);
    }

    public void MoveTo(Vector3 pos)
    {
        isMove = true;
        movePos = pos;
        movePos.y = 1;
    }
}
