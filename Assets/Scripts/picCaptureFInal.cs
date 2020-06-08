using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.XR.WSA.WebCam;
using System.IO;
using UnityEngine.UI;
using UnityEngine.Video;

public class picCaptureFInal : MonoBehaviour {

    PhotoCapture photoCaptureObject = null;
    public AudioClip captureAudioClip;
    public AudioClip failedAudioClip;
    private AudioSource audioSource;

    static readonly int TotalImagesToCapture = 1;
    int capturedImageCount = 0;
    public GameObject photobtn;

    string openpicpath = null;
    static int openpicnum = 0;
    static int sumpic = 0;
    string openpicname = null;
    static bool isPhotoExist = false;

    VideoCapture m_VideoCapture = null;
    static bool isRecording;
    static readonly float MaxRecordingTime = 5.0f;
    float m_stopRecordingTimer = float.MaxValue;

    private VideoPlayer videoplayer;
    public GameObject quad;
    int VideoCount = 0;
    static int openVdieonum = 0;
    static int sumVideo = 0;
    public Sprite daiji;
    public Sprite jujiao;
    // Use this for initialization]
    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        modelma.Instance.SetTipText("");
        statusma.Instance.currentstatus = CurrentStatus.Close;
        quad.SetActive(false);

    }

    void Update()
    {
        statusma.Instance.currentstatus = statusma.Instance.GetStatus();
        if (m_VideoCapture == null || !m_VideoCapture.IsRecording)
        {
            return;
        }

        /*  if (Time.time > m_stopRecordingTimer)
          {
              m_VideoCapture.StopRecordingAsync(OnStoppedRecordingVideo);

          }*/

        if (statusma.Instance.currentstatus == CurrentStatus.Ready)
            modelma.Instance.imageObject.GetComponent<Image>().sprite = daiji;
    }
    private void OnTap()
    {
        if (statusma.Instance.currentstatus != CurrentStatus.Close)
        {
            if (gameObject == GameObject.FindGameObjectWithTag("shutter"))
            {
                statusma.Instance.SetStatus(CurrentStatus.Ready);
                // photobtn.GetComponent<MeshRenderer>().material.color = Color.red;
                // gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
                // ReadyToPhoto();
                quad.SetActive(false);
                modelma.Instance.imageObject.SetActive(true);
                modelma.Instance.imageObject.GetComponent<Image>().sprite = jujiao;
                Debug.Log("tap");
            }
            else if (gameObject == GameObject.FindGameObjectWithTag("play"))
            {
                Debug.Log("play");
                if (statusma.Instance.currentstatus != CurrentStatus.ReadPhoto)
                {
                    statusma.Instance.SetStatus(CurrentStatus.ReadPhoto);
                    modelma.Instance.imageObject.SetActive(true);
                    quad.SetActive(false);
                    openpicname = string.Format(@"CapturedImage{0}.jpg", openpicnum);
                    openpicpath = Path.Combine(Application.persistentDataPath, openpicname);
                    ShowPhoto(openpicname, openpicpath);
                }
                else
                {
                    statusma.Instance.SetStatus(CurrentStatus.Ready);
                   modelma.Instance.imageObject.SetActive(true);
                    modelma.Instance.imageObject.GetComponent<Image>().sprite = daiji;
                }
            }
            else if (gameObject == GameObject.FindGameObjectWithTag("remove"))
            {
                Debug.Log("remove");
                if (statusma.Instance.currentstatus == CurrentStatus.ReadPhoto)
                {
                    openpicname = string.Format(@"CapturedImage{0}.jpg", openpicnum);
                    openpicpath = Path.Combine(Application.persistentDataPath, openpicname);
                    File.Delete(openpicpath);
                    if (openpicnum > 1)
                    {

                        BackPhoto();
                    }
                    else if (openpicnum == 1)
                    {
                        modelma.Instance.imageObject.GetComponent<Image>().sprite = null;
                        modelma.Instance.SetTipText("Nothing.Please air tap to take photos");
                    }
                }
                else if (statusma.Instance.currentstatus == CurrentStatus.WatchVideo)
                {

                    string filename = string.Format("TestVideo_{0}.mp4", openVdieonum);
                    string filepath = Path.Combine(Application.persistentDataPath, filename);
                    filepath = filepath.Replace("/", @"\");
                    File.Delete(filepath);
                    if (openVdieonum > 1)
                    {

                        PreviousVideo();
                    }
                    else if (openVdieonum == 1)
                    {
                        quad.GetComponent<Image>().sprite = null;
                        modelma.Instance.SetTipText("Nothing.Please air tap to record videos");
                    }
                }

            }
            else if (gameObject == GameObject.FindGameObjectWithTag("back"))
            {
                Debug.Log("back");
                if (statusma.Instance.currentstatus == CurrentStatus.ReadPhoto)
                {
                    BackPhoto();
                }

            }
            else if (gameObject == GameObject.FindGameObjectWithTag("next"))
            {
                Debug.Log("next");
                if (statusma.Instance.currentstatus == CurrentStatus.ReadPhoto)
                {
                    NextPhoto();
                }
            }
            else if (gameObject == GameObject.FindGameObjectWithTag("record"))
            {
                if (statusma.Instance.currentstatus != CurrentStatus.CaptureVideo)
                {

                    statusma.Instance.SetStatus(CurrentStatus.CaptureVideo);
                    quad.SetActive(false);
                    modelma.Instance.imageObject.SetActive(true);
                    modelma.Instance.imageObject.GetComponent<Image>().sprite = jujiao;
                    TakeVideo();
                }
                else
                {
                    m_VideoCapture.StopRecordingAsync(OnStoppedRecordingVideo);
                    quad.SetActive(false);
                   modelma.Instance.imageObject.SetActive(true);
                    modelma.Instance.imageObject.GetComponent<Image>().sprite = daiji;
                    statusma.Instance.SetStatus(CurrentStatus.Ready);
                }
            }
            else if (gameObject == GameObject.FindGameObjectWithTag("video"))
            {
                if (statusma.Instance.GetStatus() != CurrentStatus.WatchVideo)
                {
                    quad.SetActive(true);
                   modelma.Instance.imageObject.SetActive(false);
                    statusma.Instance.currentstatus = CurrentStatus.WatchVideo;
                    string filename = string.Format("TestVideo_{0}.mp4", openVdieonum);
                    string filepath = Path.Combine(Application.persistentDataPath, filename);
                    filepath = filepath.Replace("/", @"\");
                    PlayVideo(filepath);

                }
                else
                {
                    quad.SetActive(false);
                    modelma.Instance.imageObject.SetActive(true);
                   modelma.Instance.imageObject.GetComponent<Image>().sprite = daiji;
                    statusma.Instance.currentstatus = CurrentStatus.Ready;
                    quad.SetActive(false);
                }
            }
            else if (gameObject == GameObject.FindGameObjectWithTag("previous"))
            {
                Debug.Log("previous");
                if (statusma.Instance.currentstatus == CurrentStatus.WatchVideo)
                {
                    PreviousVideo();
                }
            }
            else if (gameObject == GameObject.FindGameObjectWithTag("behind"))
            {
                Debug.Log("behind");
                if (statusma.Instance.currentstatus == CurrentStatus.WatchVideo)
                {
                    NextVideo();
                }
            }
        }
    }

    private void OnDoubleTap()
    {
        if (gameObject == GameObject.FindGameObjectWithTag("shutter"))
        {
            statusma.Instance.SetStatus(CurrentStatus.Ready);
            photobtn.GetComponent<MeshRenderer>().material.color = Color.yellow;
            ReadyToPhoto();
            Debug.Log("tap");
        }
        //gameObject.GetComponent<MeshRenderer>().material.color = Color.yellow;

    }

    void ReadyToPhoto()
    {
        statusma.Instance.currentstatus = CurrentStatus.TakingPhoto;
        Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
        Texture2D targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);
        PhotoCapture.CreateAsync(false, delegate (PhotoCapture captureObject) {
            Debug.Log("Created PhotoCapture Object");
            photoCaptureObject = captureObject;

            CameraParameters c = new CameraParameters();
            c.hologramOpacity = 0.0f;
            c.cameraResolutionWidth = targetTexture.width;
            c.cameraResolutionHeight = targetTexture.height;
            c.pixelFormat = CapturePixelFormat.BGRA32;

            captureObject.StartPhotoModeAsync(c, delegate (PhotoCapture.PhotoCaptureResult result) {
                Debug.Log("Started Photo Capture Mode");
                TakePicture();
            });
        });
    }

    void OnCapturedPhotoToDisk(PhotoCapture.PhotoCaptureResult result)
    {
        Debug.Log("Saved Picture To Disk!");

        /* if (capturedImageCount < TotalImagesToCapture)
         {
             TakePicture();
         }
         else
         {
             photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
         }*/
        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }

    void TakePicture()
    {
        capturedImageCount++;
        getPicNum();
        Debug.Log(string.Format("Taking Picture ({0}/{1})...", capturedImageCount, TotalImagesToCapture));
        string filename = string.Format(@"CapturedImage{0}.jpg", capturedImageCount);
        string filePath = Path.Combine(Application.persistentDataPath, filename);
        audioSource.clip = captureAudioClip;
        audioSource.Play();
        Debug.Log("take picture " + filePath);

        photoCaptureObject.TakePhotoAsync(filePath, PhotoCaptureFileOutputFormat.JPG, OnCapturedPhotoToDisk);
        ShowPhoto(filename, filePath);
    }
    void getPicNum()
    {
        openpicnum = capturedImageCount;
        sumpic = capturedImageCount;
        openVdieonum = VideoCount;
        sumVideo = VideoCount;
    }
    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
        photobtn.GetComponent<MeshRenderer>().material.color = Color.gray;
        Debug.Log("Captured images have been saved at the following path.");
        Debug.Log(Application.persistentDataPath);
        statusma.Instance.currentstatus = CurrentStatus.Ready;
    }

    public void ShowPhoto(string file, string path)

    {
        Debug.Log("opencount" + openpicnum.ToString() + "sumpic" + sumpic.ToString());
        modelma.Instance.imageObject.SetActive(true);
        StartCoroutine(LoadPic(file, path));
        modelma.Instance.SetTipText("air tap to take a photo");

    }
    private IEnumerator LoadPic(string picname, string path)
    {
        yield return new WaitForSeconds(2.0f);
        // string path = Application.persistentDataPath + "/" + picname + ".jpg";
        //string path = "C:\\Data\\Users\\ediso\\Pictures\\" + picname + ".jpg";
        if (File.Exists(path))
        {
            isPhotoExist = true;
            WWW www = new WWW("file:///" + path);
            yield return www;
            Texture2D dynaPic = www.texture;
            modelma.Instance.imageObject.GetComponent<Image>().sprite = Sprite.Create(dynaPic, new Rect(0, 0, dynaPic.width, dynaPic.height), new Vector2(0.5f, 0.5f));
            print("读取成功");
            modelma.Instance.SetTipText("load picture successfully");
        }
        else
        {
            isPhotoExist = false;
            print("图片不存在！");
            modelma.Instance.SetTipText("none");
        }
        if (statusma.Instance.currentstatus != CurrentStatus.ReadPhoto)
        {
            yield return new WaitForSeconds(5.0f);
            modelma.Instance.imageObject.SetActive(true);
            modelma.Instance.imageObject.GetComponent<Image>().sprite = daiji;

        }
    }
    void BackPhoto()
    {
        if (openpicnum > 1)
        {
            openpicnum--;
            Debug.Log("back" + openpicnum.ToString());
            openpicname = string.Format(@"CapturedImage{0}.jpg", openpicnum);
            openpicpath = Path.Combine(Application.persistentDataPath, openpicname);
            if (File.Exists(openpicpath))
                ShowPhoto(openpicname, openpicpath);
            else
                BackPhoto();
        }
        else modelma.Instance.SetTipText("It's has been the first picture.");
    }
    void NextPhoto()
    {
        if (openpicnum < sumpic)
        {
            openpicnum++;
            Debug.Log("next" + openpicnum.ToString());
            openpicname = string.Format(@"CapturedImage{0}.jpg", openpicnum);
            openpicpath = Path.Combine(Application.persistentDataPath, openpicname);
            if (File.Exists(openpicpath))
                ShowPhoto(openpicname, openpicpath);
            else
                NextPhoto();
        }
        else modelma.Instance.SetTipText("It's has been the last picture.");
    }
    void NextVideo()
    {
        if (openVdieonum < sumVideo)
        {
            openVdieonum++;
            Debug.Log("next" + openVdieonum.ToString());
            string filename = string.Format("TestVideo_{0}.mp4", openVdieonum);
            string filepath = Path.Combine(Application.persistentDataPath, filename);
            filepath = filepath.Replace("/", @"\");
            if (File.Exists(filepath))
                PlayVideo(filepath);
            else
                NextVideo();
        }
        else modelma.Instance.SetTipText("It's has been the last video.");

    }
    void PreviousVideo()
    {
        if (openVdieonum > 1)
        {
            openVdieonum--;
            Debug.Log("back" + openVdieonum.ToString());
            string filename = string.Format("TestVideo_{0}.mp4", openVdieonum);
            string filepath = Path.Combine(Application.persistentDataPath, filename);
            filepath = filepath.Replace("/", @"\");
            if (File.Exists(filepath))
                PlayVideo(filepath);
            else
                PreviousVideo();
        }
        else modelma.Instance.SetTipText("It's has been the first video.");
    }
    //************************************录像***************************************************


    public void StopVideo()
    {
        if (isRecording)
        {
            isRecording = false;
            print("停止录像...");
            if (Application.platform == RuntimePlatform.WSAPlayerX86)
                m_VideoCapture.StopRecordingAsync(OnStoppedRecordingVideo);
        }
    }
    public void TakeVideo()
    {

        isRecording = true;
        print("开始录像...");
        if (Application.platform == RuntimePlatform.WSAPlayerX86)
            VideoCapture.CreateAsync(false, delegate (VideoCapture videoCapture)
            {
                if (videoCapture != null)
                {
                    m_VideoCapture = videoCapture;
                    Debug.Log("Created VideoCapture Instance!");
                    StartVideoCapture(videoCapture);
                }
                else
                {
                    Debug.LogError("Failed to create VideoCapture Instance!");
                }
            });

    }
    void StartVideoCapture(VideoCapture videoCapture)
    {

        // if (videoCapture != null)
        // {
        // m_VideoCapture = videoCapture;
        // Debug.Log("Created VideoCapture Instance!");

        Resolution cameraResolution = VideoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
        float cameraFramerate = VideoCapture.GetSupportedFrameRatesForResolution(cameraResolution).OrderByDescending((fps) => fps).First();
        Debug.Log("刷新率：" + cameraFramerate);

        CameraParameters cameraParameters = new CameraParameters();
        cameraParameters.hologramOpacity = 0.0f;
        cameraParameters.frameRate = cameraFramerate;
        cameraParameters.cameraResolutionWidth = cameraResolution.width;
        cameraParameters.cameraResolutionHeight = cameraResolution.height;
        cameraParameters.pixelFormat = CapturePixelFormat.BGRA32;

        m_VideoCapture.StartVideoModeAsync(cameraParameters,
            VideoCapture.AudioState.ApplicationAndMicAudio,
            OnStartedVideoCaptureMode);
        //  }

    }

    void OnStartedVideoCaptureMode(VideoCapture.VideoCaptureResult result)
    {
        Debug.Log("开始录像模式!");
        VideoCount++;
        getPicNum();
        //timeStamp = Time.time.ToString().Replace(".", "").Replace(":", "");

        string filename = string.Format("TestVideo_{0}.mp4", VideoCount);
        //string filename = "TestVideo.mp4";
        string filepath = Path.Combine(Application.persistentDataPath, filename);
        filepath = filepath.Replace("/", @"\");
        m_VideoCapture.StartRecordingAsync(filepath, OnStartedRecordingVideo);
        print("videopath:" + filepath);
    }

    void OnStoppedVideoCaptureMode(VideoCapture.VideoCaptureResult result)
    {
        m_VideoCapture.Dispose();
        m_VideoCapture = null;
        Debug.Log("停止录像模式!");
        statusma.Instance.currentstatus = CurrentStatus.Ready;
        modelma.Instance.imageObject.SetActive(true);
        modelma.Instance.imageObject.GetComponent<Image>().sprite = daiji;

    }

    void OnStartedRecordingVideo(VideoCapture.VideoCaptureResult result)
    {
        Debug.Log("开始录像!");
        modelma.Instance.SetTipText("Recording...");
        m_stopRecordingTimer = Time.time + MaxRecordingTime;
    }

    void OnStoppedRecordingVideo(VideoCapture.VideoCaptureResult result)
    {
        modelma.Instance.SetTipText("Stop recording");
        m_VideoCapture.StopVideoModeAsync(OnStoppedVideoCaptureMode);
    }

    public void PlayVideo(string filepath)
    {

        if (File.Exists(filepath))
        {
            quad.SetActive(true);
            videoplayer = quad.GetComponent<VideoPlayer>();
            print("播放视频...");
            modelma.Instance.SetTipText("Playing video");
            //videoplayer.url = "file:///" + Application.persistentDataPath + "/TestVideo.mp4";
            videoplayer.url = "file:///" + filepath;
            videoplayer.Play();
        }
        else
        {
            modelma.Instance.SetTipText("No video");
            print("视频不存在！");
        }
    }
}
