using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

namespace HoloToolkit.Unity.InputModule
{

    
    [RequireComponent(typeof(GazeManager))]
    public partial class GestureManager : Singleton<GestureManager>
   
    {
        public KeyCode EditorSelectKey = KeyCode.Space;
        public GameObject obj;
        public GameObject obj2;
        public GameObject obj3;
        public GameObject obj4;
        public GameObject obj5;
        public GameObject obj6;
        /// <summary>
        /// To select even when a hologram is not being gazed at,
        /// set the override focused object.
        /// If its null, then the gazed at object will be selected.
        /// </summary>
        
        public GameObject OverrideFocusedObject
        {
            get; set;
        }

        public GameObject FocusedObject
        {
            get { return focusedObject; }
        }

        private GestureRecognizer gestureRecognizer;
        private GameObject focusedObject;
        public bool IsNavigating { get; private set; }
        public Vector3 NavigationPosition { get; private set; }
        void Start()
        {
            gestureRecognizer = new GestureRecognizer();
            gestureRecognizer.SetRecognizableGestures(GestureSettings.Tap

                | GestureSettings.DoubleTap);

            //  订阅手势事件

            gestureRecognizer.TappedEvent += GestureRecognizer_TappedEvent;
            //  开始手势识别

            gestureRecognizer.StartCapturingGestures();

        }
        private void OnTap()
        {
            if (focusedObject != null)
            {
                focusedObject.SendMessage("OnTap");
            }
        }
        private void OnDoubleTap()
        {
            if (focusedObject != null)
            {
                focusedObject.SendMessage("OnDoubleTap");
            }
        }
        private void OnMultiTap()
        {
            if (focusedObject != null)
            {
                focusedObject.SendMessage("OnMultiTap");
            }
        }
        private void GestureRecognizer_TappedEvent(InteractionSourceKind source, int tapCount, Ray headRay)
        {
            if (tapCount == 1)
            {
                OnTap();
            }
            else if(tapCount==2)
            {
                OnDoubleTap();
            }
            else
            {
                OnMultiTap();
            }
        }
        void LateUpdate()
        {
            GameObject oldFocusedObject = focusedObject;

            if (GazeManager.Instance.HitObject &&
                OverrideFocusedObject == null &&
                GazeManager.Instance.HitInfo.collider != null)
            {
                // If gaze hits a hologram, set the focused object to that game object.
                // Also if the caller has not decided to override the focused object.
                focusedObject = GazeManager.Instance.HitInfo.collider.gameObject;
            }
            else
            {
                // If our gaze doesn't hit a hologram, set the focused object to null or override focused object.
                focusedObject = OverrideFocusedObject;
            }

            if (focusedObject != oldFocusedObject)
            {
                // If the currently focused object doesn't match the old focused object, cancel the current gesture.

                // Start looking for new gestures.  This is to prevent applying gestures from one hologram to another.
                gestureRecognizer.CancelGestures();
                gestureRecognizer.StartCapturingGestures();
            }

#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(EditorSelectKey))
            {
                focusedObject = obj;
                OnTap();
            }
#endif
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.A))
            {
                focusedObject = obj2;
                OnTap();
            }
#endif
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.S))
            {
                focusedObject = obj3;
                OnTap();
            }
#endif
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.W))
            {
                focusedObject = obj4;
                OnTap();
            }
#endif
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Z))
            {
                focusedObject = obj5;
                OnTap();
            }
#endif
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.D))
            {
                focusedObject = obj6;
                OnTap();
            }
#endif
        }

        /*  void OnDestroy()
          {
              gestureRecognizer.StopCapturingGestures();
              gestureRecognizer.TappedEvent -= GestureRecognizer_TappedEvent;
          }*/
    }
}