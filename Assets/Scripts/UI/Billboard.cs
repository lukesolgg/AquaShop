using UnityEngine;

namespace AquariumShop
{
    public class Billboard : MonoBehaviour
    {
        Transform _mainCamera;

        void Start()
        {
            if (Camera.main != null)
                _mainCamera = Camera.main.transform;
        }

        void LateUpdate()
        {
            if (_mainCamera == null) return;
            
            // Make this object match the camera's rotation exactly
            transform.rotation = _mainCamera.rotation;
        }
    }
}