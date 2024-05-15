using UnityEngine;

namespace FullMoon.UI
{
    public class LookAtCamera : MonoBehaviour
    {
        private UnityEngine.Camera Cam { get; set; }

        [SerializeField] private bool followX = true;
        [SerializeField] private bool followY = true;

        private void Start()
        {
            Cam = UnityEngine.Camera.main;
            UpdateRotation();
        }

        private void LateUpdate()
        {
            UpdateRotation();
        }
    
        private void UpdateRotation()
        {
            if (!followX && !followY)
                return;
        
            float cameraYRotation = followX ? Cam.transform.eulerAngles.y : transform.eulerAngles.y;
            float cameraXRotation = followY ? Cam.transform.eulerAngles.x : transform.eulerAngles.x;
        
            transform.rotation = Quaternion.Euler(cameraXRotation, cameraYRotation, transform.eulerAngles.z);
        }
    }
}