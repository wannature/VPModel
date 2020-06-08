using System.Collections;
using System.Collections.Generic;

using HoloToolkit.Unity.InputModule;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.IO;


namespace Academy
{
    /// <summary>
    /// GestureAction performs custom actions based on 
    /// which gesture is being performed.
    /// </summary>
    public class GestureAction : MonoBehaviour, INavigationHandler, IManipulationHandler, ISpeechHandler
    {

        [Tooltip("Rotation max speed controls amount of rotation.")]
        [SerializeField]

        public GameObject Color1;
        public GameObject Color2;
        public Material ma1; public Material ma2;
        public Material mb1; public Material mb2;
        public Material mc1; public Material mc2;
        public GameObject camera1;
        public GameObject camera2;
        public GameObject colors;
        public GameObject camerparameter;
        public GameObject competitormodel;
        public GameObject competitorparameter;
        public GameObject competitorname;
        public GameObject quad;
        public GameObject notes;
        public Sprite daiji;
        public Sprite jujiao;
        public GameObject photobtn;
        public GameObject note;
        string openpicname = null;
        string speech = null;
        private float RotationSensitivity = 10.0f;

        private bool isNavigationEnabled = false;
        private bool isManipulationEnabled = false;
        private bool isReset = true;
        public bool IsNavigationEnabled
        {
            get { return isNavigationEnabled; }
            set { isNavigationEnabled = value; }
        }
        public bool IsManipulationEnabled
        {
            get { return isManipulationEnabled; }
            set { isManipulationEnabled = value; }
        }



        private Vector3 manipulationOriginalPosition = Vector3.zero;

        void ResetEnable()
        {
            if (isReset)
            {
                // GameObject currentModel = InputManager.Instance.gameObject;
                // currentModel.transform.position = new Vector3(0, 0, 2);
                // currentModel.transform.Rotate(-90 - currentModel.transform.localEulerAngles.x, -currentModel.transform.localEulerAngles.y, -currentModel.transform.localEulerAngles.z);
            }
        }
        void INavigationHandler.OnNavigationStarted(NavigationEventData eventData)
        {
            InputManager.Instance.PushModalInputHandler(gameObject);
        }

        void INavigationHandler.OnNavigationUpdated(NavigationEventData eventData)
        {
            if (isNavigationEnabled)
            {
                /* TODO: DEVELOPER CODING EXERCISE 2.c */

                // 2.c: Calculate a float rotationFactor based on eventData's NormalizedOffset.x multiplied by RotationSensitivity.
                // This will help control the amount of rotation.
                float rotationFactorz = eventData.NormalizedOffset.x * RotationSensitivity;
                float rotationFactorx = eventData.NormalizedOffset.z * RotationSensitivity;

                // 2.c: transform.Rotate around the Y axis using rotationFactor.
                transform.Rotate(new Vector3(-1 * rotationFactorx, 0, -1 * rotationFactorz));
            }
        }

        void INavigationHandler.OnNavigationCompleted(NavigationEventData eventData)
        {
            InputManager.Instance.PopModalInputHandler();
        }

        void INavigationHandler.OnNavigationCanceled(NavigationEventData eventData)
        {
            InputManager.Instance.PopModalInputHandler();
        }

        void IManipulationHandler.OnManipulationStarted(ManipulationEventData eventData)
        {
            if (isManipulationEnabled)
            {
                InputManager.Instance.PushModalInputHandler(gameObject);

                manipulationOriginalPosition = transform.position;
            }
        }

        void IManipulationHandler.OnManipulationUpdated(ManipulationEventData eventData)
        {
            if (isManipulationEnabled)
            {
                /* TODO: DEVELOPER CODING EXERCISE 4.a */

                // 4.a: Make this transform's position be the manipulationOriginalPosition + eventData.CumulativeDelta
                transform.position = manipulationOriginalPosition + eventData.CumulativeDelta;
            }
        }

        void IManipulationHandler.OnManipulationCompleted(ManipulationEventData eventData)
        {
            InputManager.Instance.PopModalInputHandler();
        }

        void IManipulationHandler.OnManipulationCanceled(ManipulationEventData eventData)
        {
            InputManager.Instance.PopModalInputHandler();
        }

        void ISpeechHandler.OnSpeechKeywordRecognized(SpeechEventData eventData)
        {
            speech = eventData.RecognizedText.ToLower();
            Debug.Log(speech);
            if (speech.Equals("move camera"))
            {
                isNavigationEnabled = false;
                isReset = false;
                isManipulationEnabled = true;
            }
            else if (speech.Equals("rotate camera"))
            {
                isNavigationEnabled = true;
                isManipulationEnabled = false;
                isReset = false;
            }
            else if (speech.Equals("select camera"))
            {
                isNavigationEnabled = false;
                isManipulationEnabled = false;
                isReset = true;

            }
            else if (speech.Equals("color one"))
            {
                Color1.GetComponent<MeshRenderer>().material = ma1;
                Color2.GetComponent<MeshRenderer>().material = ma2;

            }
            else if (speech.Equals("color two"))
            {
                Color1.GetComponent<MeshRenderer>().material = mb1;
                Color2.GetComponent<MeshRenderer>().material = mb2;

            }
            else if (speech.Equals("color three"))
            {
                Color1.GetComponent<MeshRenderer>().material = mc1;
                Color2.GetComponent<MeshRenderer>().material = mc2;

            }
            else if (speech.Equals("first camera"))
            {
                camera1.SetActive(true);
                camera2.SetActive(false);

            }
            else if (speech.Equals("second camera"))
            {
                camera1.SetActive(false);
                camera2.SetActive(true);

            }
            else if (speech.Equals("set color"))
            {
                colors.SetActive(true);
            }
            else if (speech.Equals("close color"))
            {
                colors.SetActive(false);
            }
            else if (speech.Equals("product one"))
            {
                camerparameter.SetActive(true);
            }
            else if (speech.Equals("close one"))
            {
                camerparameter.SetActive(false);
            }
            else if (speech.Equals("product two"))
            {
                competitormodel.SetActive(true);
                competitorparameter.SetActive(true);
                competitorname.SetActive(true);
            }
            else if (speech.Equals("close two"))
            {
                competitormodel.SetActive(false);
                competitorparameter.SetActive(false);
                competitorname.SetActive(false);
            }
            else if (speech.Equals("take"))
            {
                GameObject g = GameObject.FindGameObjectWithTag("shutter");
                StatusManager.Instance.SetStatus(CurrentStatus.Ready);
                quad.SetActive(false);
                ModelManager.Instance.imageObject.SetActive(true);
                ModelManager.Instance.imageObject.GetComponent<Image>().sprite = jujiao;
                photobtn.GetComponent<MeshRenderer>().material.color = Color.yellow;
                g.GetComponent<PhotoCaptureFinal>().ReadyToPhoto();
            }
            else if (speech.Equals("play"))
            {
                GameObject g = GameObject.FindGameObjectWithTag("play");
                StatusManager.Instance.SetStatus(CurrentStatus.ReadPhoto);
                ModelManager.Instance.imageObject.SetActive(true);
                quad.SetActive(false);
                openpicname = string.Format(@"CapturedImage{0}.jpg", PhotoCaptureFinal.openpicnum);
                PhotoCaptureFinal.openpicpath = Path.Combine(Application.persistentDataPath, openpicname);
                //g.GetComponent<PhotoCaptureFinal>().ShowPhoto(openpicname, PhotoCaptureFinal.openpicpath);
            }
            else if (speech.Equals("open"))
            {
                GameObject g = GameObject.FindGameObjectWithTag("open");
                StatusManager.Instance.currentstatus = CurrentStatus.Ready;
                g.GetComponent<StatusManager>().OpenCamera();

            }
            else if (speech.Equals("close"))
            {
                GameObject g = GameObject.FindGameObjectWithTag("open");
                StatusManager.Instance.currentstatus = CurrentStatus.Close;
                g.GetComponent<StatusManager>().cameraIsColosed();
            }

            else if (speech.Equals("next"))
            {
                GameObject g = GameObject.FindGameObjectWithTag("back");
                if (StatusManager.Instance.currentstatus == CurrentStatus.ReadPhoto)
                {
                    g.GetComponent<PhotoCaptureFinal>().NextPhoto();
                }
            }

            else if (speech.Equals("previous"))
            {
                GameObject g = GameObject.FindGameObjectWithTag("back");
                if (StatusManager.Instance.currentstatus == CurrentStatus.ReadPhoto)
                {
                    g.GetComponent<PhotoCaptureFinal>().BackPhoto();
                }
            }

            else if (speech.Equals("take video"))
            {
                GameObject g = GameObject.FindGameObjectWithTag("record");
                if (StatusManager.Instance.currentstatus != CurrentStatus.CaptureVideo)
                {

                    StatusManager.Instance.SetStatus(CurrentStatus.CaptureVideo);
                    quad.SetActive(false);
                    ModelManager.Instance.imageObject.SetActive(true);
                    ModelManager.Instance.imageObject.GetComponent<Image>().sprite = jujiao;
                    g.GetComponent<PhotoCaptureFinal>().TakeVideo();
                }
                else
                {
                    PhotoCaptureFinal.m_VideoCapture.StopRecordingAsync(g.GetComponent<PhotoCaptureFinal>().OnStoppedRecordingVideo);
                    quad.SetActive(false);
                    ModelManager.Instance.imageObject.SetActive(true);
                    ModelManager.Instance.imageObject.GetComponent<Image>().sprite = daiji;
                    StatusManager.Instance.SetStatus(CurrentStatus.Ready);

                }
            }
            else if (speech.Equals("close video"))
            {
                GameObject g = GameObject.FindGameObjectWithTag("back");

                PhotoCaptureFinal.m_VideoCapture.StopRecordingAsync(g.GetComponent<PhotoCaptureFinal>().OnStoppedRecordingVideo);
                quad.SetActive(false);
                modelma.Instance.imageObject.SetActive(true);
                modelma.Instance.imageObject.GetComponent<Image>().sprite = daiji;
                statusma.Instance.SetStatus(CurrentStatus.Ready);

                PhotoCaptureFinal.m_VideoCapture.Dispose();
                PhotoCaptureFinal.m_VideoCapture = null;
                Debug.Log("停止录像模式!");
                StatusManager.Instance.currentstatus = CurrentStatus.Ready;
                g.GetComponent<PhotoCaptureFinal>().StopVideo();
            }

            else if (speech.Equals("play video"))
            {
                GameObject g = GameObject.FindGameObjectWithTag("video");
                if (StatusManager.Instance.GetStatus() != CurrentStatus.WatchVideo)
                {
                    quad.SetActive(true);
                    ModelManager.Instance.imageObject.SetActive(false);
                    StatusManager.Instance.currentstatus = CurrentStatus.WatchVideo;
                    string filename = string.Format("TestVideo_{0}.mp4", PhotoCaptureFinal.openVdieonum);
                    string filepath = Path.Combine(Application.persistentDataPath, filename);
                    filepath = filepath.Replace("/", @"\");
                    g.GetComponent<PhotoCaptureFinal>().PlayVideo(filepath);//

                }
                else
                {
                    quad.SetActive(false);
                    ModelManager.Instance.imageObject.SetActive(true);
                    ModelManager.Instance.imageObject.GetComponent<Image>().sprite = daiji;
                    StatusManager.Instance.currentstatus = CurrentStatus.Ready;
                    quad.SetActive(false);
                }
            }
            else if (speech.Equals("note"))
            {
                GameObject g = GameObject.FindGameObjectWithTag("light");
                g.GetComponent<Notes>().enabled = true;
            }
            else if (speech.Equals("close note"))
            {
                Notes tempNote = new Notes();
                tempNote.tipText = null;
                note.SetActive(false);
            }
            else if (speech.Equals("record"))
            {
                notes.SetActive(true);
            }
            else if (speech.Equals("close record"))
            {
                notes.SetActive(false);
            }

            //vuforia开启关闭
            else if (speech.Equals("start"))
            {
                if (Vuforia.CameraDevice.Instance.IsActive())
                {
                    GameObject.Find("ARCamera").GetComponent<Vuforia.VuforiaBehaviour>().enabled = false;
                }
            }
            else if (speech.Equals("end"))
            {
                GameObject.Find("ARCamera").GetComponent<Vuforia.VuforiaBehaviour>().enabled = true;
            }
            else if (speech.Equals("time"))
            {
                StatusManager.Instance.SetStatus(CurrentStatus.ChangeTime);
                ModelManager.Instance.SetTipText("Change Time");
                PhotoCaptureFinal co = new PhotoCaptureFinal();
                co.showtime();
            }

            else
            {
                return;
            }

            eventData.Use();
        }

        //***************************************************************************************

        /*
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



                private void GestureRecognizer_TappedEvent(InteractionSourceKind source, int tapCount, Ray headRay)

                {

                    if (tapCount == 1)

                    {

                        OnTap();

                    }

                    else

                    {

                        OnDoubleTap();

                    }

                }*/
    }
}