using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


namespace FullMoon.Camera
{
    public class CoverController : MonoBehaviour
    {
        public CameraController mainCamera;

        void Awake()
        {
            mainCamera = UnityEngine.Camera.main.GetComponentInParent<CameraController>();
        }

        void OnEnable()
        {
            mainCamera.SetCoverList(gameObject);
        }

        void OnTriggerEnter(Collider collider)
        {
            if (collider.gameObject.layer == LayerMask.NameToLayer("Unit"))
            {
                mainCamera.RemoveCover(gameObject);
            }
        }

        void OnTriggerExit(Collider collider)
        {
            if (collider.gameObject.layer == LayerMask.NameToLayer("Unit"))
            {
                mainCamera.SetCoverList(gameObject);
            }
        }
    }
}
