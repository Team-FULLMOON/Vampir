using Cinemachine;
using FullMoon.Input;
using UnityEngine;

namespace FullMoon.Camera
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private CinemachineFreeLook freeLookCamera;
    
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 12f;
        [SerializeField] private float shiftMoveSpeed = 25f;
    
        [Header("Zoom")]
        [SerializeField] private float zoomSensitivity = 5f; // 마우스 스크롤 감도
        [SerializeField] private float zoomSpeed = 10f; // 줌 속도
        [SerializeField] private float minFov = 20f;
        [SerializeField] private float maxFov = 55f;
    
        private float targetFov;

        private void Start()
        {
            targetFov = freeLookCamera.m_Lens.FieldOfView;
        
            PlayerInputManager.Instance.ZoomEvent.AddEvent(ZoomEvent);
        }

        private void FixedUpdate()
        {
            Vector3 moveDirection = AdjustMovementToCamera(PlayerInputManager.Instance.move);

            if (moveDirection == Vector3.zero)
            {
                moveDirection = AdjustMovementToCamera(GetScreenMovementInput());
            }
        
            float movementSpeed = PlayerInputManager.Instance.shift ? shiftMoveSpeed : moveSpeed;
            transform.position += moveDirection * (movementSpeed * Time.fixedDeltaTime);
        }

        private void Update()
        {
            // FOV를 목표값으로 부드럽게 조정
            freeLookCamera.m_Lens.FieldOfView = Mathf.Lerp(freeLookCamera.m_Lens.FieldOfView, targetFov, Time.deltaTime * zoomSpeed);
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
                targetFov -= (scrollValue.y > 0f ? 1f : -1f) * zoomSensitivity;
                targetFov = Mathf.Clamp(targetFov, minFov, maxFov);
            }
        }
    }
}
