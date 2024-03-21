using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private CinemachineFreeLook freeLookCamera;
    
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private float shiftMoveSpeed = 25f;
    
    [Header("Zoom")]
    [SerializeField] private float zoomSensitivity = 5f; // 마우스 스크롤 감도
    [SerializeField] private float zoomSpeed = 10f; // 줌 속도
    [SerializeField] private float minFov = 15f;
    [SerializeField] private float maxFov = 90f;
    private float targetFov;

    private Vector2 _movementInput;
    private bool _shiftInput;

    private void Start()
    {
        targetFov = freeLookCamera.m_Lens.FieldOfView;
    }

    private void FixedUpdate()
    {
        Vector3 moveDirection = AdjustMovementToCamera(_movementInput);
        float movementSpeed = _shiftInput ? shiftMoveSpeed : moveSpeed;
        transform.position += moveDirection * (movementSpeed * Time.fixedDeltaTime);
    }

    private void Update()
    {
        // FOV를 목표값으로 부드럽게 조정
        freeLookCamera.m_Lens.FieldOfView = Mathf.Lerp(freeLookCamera.m_Lens.FieldOfView, targetFov, Time.deltaTime * zoomSpeed);
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

    public void OnMove(InputValue value)
    {
        _movementInput = value.Get<Vector2>();
    }
    
    private void OnShift(InputValue value)
    {
        _shiftInput = value.isPressed;
    }

    public void OnZoom(InputValue value)
    {
        Vector2 scrollValue = value.Get<Vector2>();
        if (scrollValue.y != 0f)
        {
            targetFov -= (scrollValue.y > 0f ? 1f : -1f) * zoomSensitivity;
            targetFov = Mathf.Clamp(targetFov, minFov, maxFov);
        }
    }
}