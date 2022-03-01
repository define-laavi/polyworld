using System.Linq;
using System.Collections.Generic;
using UnityEngine;


namespace Quixl
{
    public class GameCameraManager : MonoBehaviour
    {
        private static GameCameraManager singleton;

        public Camera cameraObject;

        public float DragSensitivity = 30.0f;
        public float ZoomSensitivity = 0.25f;

        public float MinZoom = 2.0f;
        public float MaxZoom = 10.0f;

        Vector3 PrevMousePos;
        float CurrentZoom;

        private void Start()
        {
            singleton = this;
            CurrentZoom = Mathf.Lerp(MinZoom, MaxZoom, 0.5f);
            cameraObject.transform.position = cameraObject.transform.forward * -CurrentZoom;
        }

        public static void RotateCamera(float yawVal, float pitchVal, float rollVal)
        {
            singleton.RotateCam(yawVal,pitchVal,rollVal);
        }
        public static void ZoomCamera(float zoomVal)
        {
            singleton.ZoomCam(zoomVal);
        }
        public static Camera GetCamera() => singleton.cameraObject;

        private void RotateCam(float yV,float pV,float rV)
        {
            Quaternion yaw = Quaternion.AngleAxis(yV * DragSensitivity * CurrentZoom, cameraObject.transform.up);
            Quaternion pitch = Quaternion.AngleAxis(pV * DragSensitivity * CurrentZoom, -cameraObject.transform.right);
            Quaternion roll = Quaternion.AngleAxis(rV * DragSensitivity, -cameraObject.transform.forward);

            cameraObject.transform.localRotation = yaw * pitch * roll * cameraObject.transform.localRotation;
            cameraObject.transform.position = cameraObject.transform.forward * -CurrentZoom;
        }
        private void ZoomCam(float zV)
        {
            CurrentZoom /= (zV * ZoomSensitivity + 1) / (ZoomSensitivity + 1);
            CurrentZoom = Mathf.Clamp(CurrentZoom, MinZoom, MaxZoom);

            cameraObject.transform.position = cameraObject.transform.forward * -CurrentZoom;
        }
    }
}