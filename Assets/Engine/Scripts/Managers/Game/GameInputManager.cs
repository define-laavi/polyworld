using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigitalRubyShared;

namespace Quixl
{
    public class GameInputManager : MonoBehaviour
    {

        ScaleGestureRecognizer scale;
        PanGestureRecognizer swipe;
        RotateGestureRecognizer rotate;
        TapGestureRecognizer tap;

        private void Start()
        {
            SetupRotateGesture();
            SetupScaleGesture();
            SetupSwipeGesture();
            SetupTapGesture();
        }

        void SetupSwipeGesture()
        {
            swipe = new PanGestureRecognizer();
            swipe.MinimumNumberOfTouchesToTrack = swipe.MaximumNumberOfTouchesToTrack = 1;
            swipe.Updated += SwipeCallback;
            FindObjectOfType<FingersScript>().AddGesture(swipe);
        }
        void SetupScaleGesture()
        {
            scale = new ScaleGestureRecognizer();
            scale.MinimumNumberOfTouchesToTrack = scale.MaximumNumberOfTouchesToTrack = 2;
            scale.Updated += ScaleCallback;
            FindObjectOfType<FingersScript>().AddGesture(scale);
        }
        void SetupRotateGesture()
        {
            rotate = new RotateGestureRecognizer();
            rotate.MinimumNumberOfTouchesToTrack = rotate.MaximumNumberOfTouchesToTrack = 2;
            rotate.Updated += RotateCallback;
            FindObjectOfType<FingersScript>().AddGesture(rotate);
        }
        void SetupTapGesture()
        {
            tap = new TapGestureRecognizer();
            tap.MinimumNumberOfTouchesToTrack = tap.MaximumNumberOfTouchesToTrack = 1;
            tap.Updated += TapCallback;
            FindObjectOfType<FingersScript>().AddGesture(tap);
        }


        void SwipeCallback(GestureRecognizer gesture, ICollection<GestureTouch> touches)
        {
            if (gesture.State == GestureRecognizerState.Executing)
            {
                GameCameraManager.RotateCamera(swipe.DeltaX / Screen.width, swipe.DeltaY / Screen.height, 0);
            }
        }
        void ScaleCallback(GestureRecognizer gesture, ICollection<GestureTouch> touches)
        {
            if (gesture.State == GestureRecognizerState.Executing)
            {
                GameCameraManager.ZoomCamera(scale.ScaleMultiplier);
            }
        }
        void RotateCallback(GestureRecognizer gesture, ICollection<GestureTouch> touches)
        {
            if (gesture.State == GestureRecognizerState.Executing)
            {
                GameCameraManager.RotateCamera(0, 0, rotate.RotationRadiansDelta);
            }
        }
        void TapCallback(GestureRecognizer gesture, ICollection<GestureTouch> touches)
        {
            if (gesture.State == GestureRecognizerState.Ended)
            {
                Ray ray = GameCameraManager.GetCamera().ScreenPointToRay(new Vector2(tap.FocusX, tap.FocusY));
                RaycastHit hit;
                
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    GamePlayManager.SelectPolygonAt(hit.point);
                }
            }
        }
    }
}
