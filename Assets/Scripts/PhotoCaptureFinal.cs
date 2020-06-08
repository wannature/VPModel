
using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.XR.WSA.WebCam;
using System.IO;
using UnityEngine.UI;
using UnityEngine.Video;
using System;

public class PhotoCaptureFinal : MonoBehaviour
{
    PhotoCapture photoCaptureObject = null;
    public AudioClip captureAudioClip;
    public AudioClip failedAudioClip;
    private AudioSource audioSource;

    public Text showT = null;//显示图片个数

    static readonly int TotalImagesToCapture = 1;
    int capturedImageCount = 0;
    public GameObject photobtn;

    public static string openpicpath = null;
    public static int openpicnum=0;
    public static int sumpic = 0;
    public string openpicname = null;
    public static bool isPhotoExist = false;

    public static VideoCapture m_VideoCapture = null;
    static bool isRecording;
    static readonly float MaxRecordingTime = 5.0f;
    float m_stopRecordingTimer = float.MaxValue;

    private VideoPlayer videoplayer;
    public GameObject quad;
    int VideoCount = 0;
    public static int openVdieonum = 0;
    public static int sumVideo = 0;
    public Sprite daiji;
    public Sprite jujiao;
    

    //Community com = new Community();

    //修改时间用的变量
    public Text[] D = new Text[3];
    public int flag = 0;
    DateTime da = DateTime.Now;
    public int timeflag;

    // Use this for initialization]
    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        ModelManager.Instance.SetTipText("");
        StatusManager.Instance.currentstatus = CurrentStatus.Close;
        quad.SetActive(false);

        D[0] = GameObject.Find("Canvas/year").GetComponent<Text>();
        D[1] = GameObject.Find("Canvas/month").GetComponent<Text>();
        D[2] = GameObject.Find("Canvas/day").GetComponent<Text>();
        D[0].gameObject.SetActive(false);
        D[1].gameObject.SetActive(false);
        D[2].gameObject.SetActive(false);
    }

    void Update()
    {
        StatusManager.Instance.currentstatus = StatusManager.Instance.GetStatus();
        if (m_VideoCapture == null || !m_VideoCapture.IsRecording)
        {
            return;
        }



        if (StatusManager.Instance.currentstatus == CurrentStatus.Ready)
            ModelManager.Instance.imageObject.GetComponent<Image>().sprite = daiji;
    }
    private void OnTap()
    {
        if (StatusManager.Instance.currentstatus != CurrentStatus.Close)
        {
            if (gameObject == GameObject.FindGameObjectWithTag("shutter"))
            {
                StatusManager.Instance.SetStatus(CurrentStatus.Ready);
                // photobtn.GetComponent<MeshRenderer>().material.color = Color.red;
                // gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
                // ReadyToPhoto();
                quad.SetActive(false);
                ModelManager.Instance.imageObject.SetActive(true);
                ModelManager.Instance.imageObject.GetComponent<Image>().sprite = jujiao;
                Debug.Log("tap");
            }
            else if (gameObject == GameObject.FindGameObjectWithTag("play"))
            {
                Debug.Log("play");
                if(timeflag==0)
                {
                    StatusManager.Instance.SetStatus(CurrentStatus.ChangeTime);
                    ModelManager.Instance.SetTipText("Change Time");
                    showtime();
                    timeflag = 1;
                }
                else if (StatusManager.Instance.currentstatus != CurrentStatus.ReadPhoto)
                {
                    StatusManager.Instance.SetStatus(CurrentStatus.ReadPhoto);
                    ModelManager.Instance.imageObject.SetActive(true);
                    quad.SetActive(false);
                    openpicname = string.Format(@"CapturedImage{0}.jpg", openpicnum);
                    openpicpath = Path.Combine(Application.persistentDataPath, openpicname);
                    ShowPhoto(openpicname, openpicpath);
                    timeflag = 0;
                }
                else
                {
                    
                    StatusManager.Instance.SetStatus(CurrentStatus.Ready);
                    ModelManager.Instance.imageObject.SetActive(true);
                    ModelManager.Instance.imageObject.GetComponent<Image>().sprite = daiji;
                    timeflag = 0;
                }
            }
            else if (gameObject == GameObject.FindGameObjectWithTag("remove"))
            {
                Debug.Log("remove");
                if (StatusManager.Instance.currentstatus == CurrentStatus.ReadPhoto)
                {
                    openpicname = string.Format(@"CapturedImage{0}.jpg", openpicnum);
                    openpicpath = Path.Combine(Application.persistentDataPath, openpicname);
                    File.Delete(openpicpath);
                    if (openpicnum > 1)
                    {
                        
                        BackPhoto();
                    }
                    else if(openpicnum==1)
                    {
                        ModelManager.Instance.imageObject.GetComponent<Image>().sprite = null;
                        ModelManager.Instance.SetTipText("Nothing.Please air tap to take photos");
                    }
                    sumpic--;
                }
                else if (StatusManager.Instance.currentstatus == CurrentStatus.WatchVideo)
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
                        ModelManager.Instance.SetTipText("Nothing.Please air tap to record videos");
                    }
                }
            }
            else if (gameObject == GameObject.FindGameObjectWithTag("back"))
            {
                Debug.Log("back");
                if (StatusManager.Instance.currentstatus == CurrentStatus.ReadPhoto)
                {
                    BackPhoto();
                }else if(StatusManager.Instance.currentstatus==CurrentStatus.ChangeTime)
                {
                    timedecrease();
                }

            }
            else if (gameObject == GameObject.FindGameObjectWithTag("next"))
            {
                Debug.Log("next");
                if (StatusManager.Instance.currentstatus == CurrentStatus.ReadPhoto)
                {
                    NextPhoto();
                }
                else if (StatusManager.Instance.currentstatus == CurrentStatus.ChangeTime)
                {
                    timeadd();
                }
            }
            else if (gameObject == GameObject.FindGameObjectWithTag("record"))
            {
                if (StatusManager.Instance.currentstatus != CurrentStatus.CaptureVideo)
                {
                    
                    StatusManager.Instance.SetStatus(CurrentStatus.CaptureVideo);
                    quad.SetActive(false);
                    ModelManager.Instance.imageObject.SetActive(true);
                    ModelManager.Instance.imageObject.GetComponent<Image>().sprite = jujiao;
                    TakeVideo();
                }
                else
                {
                    m_VideoCapture.StopRecordingAsync(OnStoppedRecordingVideo);
                    quad.SetActive(false);
                    ModelManager.Instance.imageObject.SetActive(true);
                    ModelManager.Instance.imageObject.GetComponent<Image>().sprite = daiji;
                    StatusManager.Instance.SetStatus(CurrentStatus.Ready);
                }
            }
            else if (gameObject == GameObject.FindGameObjectWithTag("video"))
            {
                if(StatusManager.Instance.GetStatus()!=CurrentStatus.WatchVideo)
                {
                    quad.SetActive(true);
                    ModelManager.Instance.imageObject.SetActive(false);
                    StatusManager.Instance.currentstatus = CurrentStatus.WatchVideo;
                    string filename = string.Format("TestVideo_{0}.mp4", openVdieonum);
                    string filepath = Path.Combine(Application.persistentDataPath, filename);
                    filepath = filepath.Replace("/", @"\");
                    PlayVideo(filepath);
                    
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
            else if(gameObject == GameObject.FindGameObjectWithTag("previous"))
            {
                Debug.Log("previous");
                if (StatusManager.Instance.currentstatus == CurrentStatus.WatchVideo)
                {
                    PreviousVideo();
                }
                else if (StatusManager.Instance.currentstatus == CurrentStatus.ChangeTime)
                {
                    datedecrease();
                }
            }
            else if (gameObject == GameObject.FindGameObjectWithTag("behind"))
            {
                Debug.Log("behind");
                if (StatusManager.Instance.currentstatus == CurrentStatus.WatchVideo)
                {
                    NextVideo();
                }
                else if (StatusManager.Instance.currentstatus == CurrentStatus.ChangeTime)
                {
                    dateadd();
                }
            }
        }
    }

    private void OnDoubleTap()
    {
        if (gameObject == GameObject.FindGameObjectWithTag("shutter"))
        {
            StatusManager.Instance.SetStatus(CurrentStatus.Ready);
            photobtn.GetComponent<MeshRenderer>().material.color = Color.yellow;
            ReadyToPhoto();
            Debug.Log("tap");
        }
        //gameObject.GetComponent<MeshRenderer>().material.color = Color.yellow;
         
    }
    public void showtime()
    {
        D[0].gameObject.SetActive(true);
        D[1].gameObject.SetActive(true);
        D[2].gameObject.SetActive(true);
        D[0].text = DateTime.Now.Year.ToString();
        D[1].text = DateTime.Now.Month.ToString();
        D[2].text = DateTime.Now.Day.ToString();

        D[0].color = Color.blue;
        flag = 0;
    }
    public void datedecrease()
    {
        if (StatusManager.Instance.currentstatus == CurrentStatus.ChangeTime) ;
        {
            switch (flag)
            {
                case 0:
                    da.AddYears(-1);
                    D[flag].text = da.Year.ToString();
                    break;
                case 1:
                    da.AddMonths(-1);
                    D[flag].text = da.Month.ToString();
                    break;
                case 2:
                    da.AddDays(-1);
                    D[flag].text = da.Day.ToString();
                    break;
            }
        }
    }

    public void dateadd()
    {
        if (StatusManager.Instance.currentstatus == CurrentStatus.ChangeTime) ;
        {
            switch (flag)
            {
                case 0:
                    da.AddYears(1);
                    D[flag].text = da.Year.ToString();
                    break;
                case 1:
                    da.AddMonths(1);
                    D[flag].text = da.Month.ToString();
                    break;
                case 2:
                    da.AddDays(1);
                    D[flag].text = da.Day.ToString();
                    break;
            }
        }
    }

    public void timedecrease()
    {
        if (StatusManager.Instance.currentstatus == CurrentStatus.ChangeTime) ;
        {
            flag = (flag - 1) % 3;
            D[flag].color = Color.blue;
        }
    }

    public void timeadd()
    {
        if (StatusManager.Instance.currentstatus == CurrentStatus.ChangeTime) ;
        {
            flag = (flag + 1) % 3;
            D[flag].color = Color.blue;
        }
    }

    public void ReadyToPhoto()
    {
        StatusManager.Instance.currentstatus = CurrentStatus.TakingPhoto;
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


        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }

    void TakePicture()
    {
        capturedImageCount++;
        getPicNum();
        Debug.Log(string.Format("Taking Picture ({0}/{1})...", capturedImageCount, TotalImagesToCapture));
        string filename = string.Format(@"CapturedImage{0}.jpg", capturedImageCount);
        //string filePath = System.IO.Path.Combine(Application.persistentDataPath, filename);
        string filePath = Path.Combine(Application.persistentDataPath, filename);
        audioSource.clip = captureAudioClip;
        audioSource.Play();
        Debug.Log("take picture "+filePath);
        ModelManager.Instance.SetTipText("take picture " + filePath);
        photoCaptureObject.TakePhotoAsync(filePath, PhotoCaptureFileOutputFormat.JPG, OnCapturedPhotoToDisk);
        ShowPhoto(filename,filePath);
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
        StatusManager.Instance.currentstatus = CurrentStatus.Ready;
    }

    public void ShowPhoto(string file, string path)

    {
        Debug.Log("opencount"+openpicnum.ToString()+"sumpic"+sumpic.ToString());
        
        ModelManager.Instance.imageObject.SetActive(true);
        StartCoroutine(LoadPic(file,path));
        //ModelManager.Instance.SetTipText("air tap to take a photo");
        ModelManager.Instance.SetTipText("" + openpicnum.ToString() + "/" + sumpic.ToString());
        showT.text = openpicnum.ToString() + "/" + sumpic.ToString();

       
    }
    private IEnumerator LoadPic(string picname,string path)
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
            ModelManager.Instance.imageObject.GetComponent<Image>().sprite = Sprite.Create(dynaPic, new Rect(0, 0, dynaPic.width, dynaPic.height), new Vector2(0.5f, 0.5f));
            print("读取成功");
            ModelManager.Instance.SetTipText("load picture successfully");
        }
        else
        {
            isPhotoExist = false;
            print("图片不存在！");
            ModelManager.Instance.SetTipText("no picture");
        }
        if (StatusManager.Instance.currentstatus != CurrentStatus.ReadPhoto)
        {
            yield return new WaitForSeconds(5.0f);
            ModelManager.Instance.imageObject.SetActive(true);
            ModelManager.Instance.imageObject.GetComponent<Image>().sprite = daiji;

        }
    }
    public void BackPhoto()
    {
        if (openpicnum > 1)
        {
            openpicnum--;
            Debug.Log("back" + openpicnum.ToString());
            openpicname = string.Format(@"CapturedImage{0}.jpg", openpicnum);
            //openpicpath = System.IO.Path.Combine(Application.persistentDataPath, openpicname);
            openpicpath = Path.Combine(Application.persistentDataPath, openpicname);
            if (File.Exists(openpicpath))
                ShowPhoto(openpicname, openpicpath);
            else
                BackPhoto();
        }
        else ModelManager.Instance.SetTipText("It's has been the first picture.");
    }
    public void NextPhoto()
    {
        if (openpicnum < sumpic)
        {
            openpicnum++;
            Debug.Log("next" + openpicnum.ToString());
            openpicname = string.Format(@"CapturedImage{0}.jpg", openpicnum);
            //openpicpath = System.IO.Path.Combine(Application.persistentDataPath, openpicname);
            openpicpath = Path.Combine(Application.persistentDataPath, openpicname);
            if (File.Exists(openpicpath))
                ShowPhoto(openpicname, openpicpath);
            else
                NextPhoto();
        }
        else ModelManager.Instance.SetTipText("It's has been the last picture.");
    }
    public void NextVideo()
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
        else ModelManager.Instance.SetTipText("It's has been the last video.");

    }
    public void PreviousVideo()
    {
        if (openVdieonum >1)
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
        else ModelManager.Instance.SetTipText("It's has been the first video.");
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
        
        string filename = string.Format("TestVideo_{0}.mp4",VideoCount);
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
        StatusManager.Instance.currentstatus = CurrentStatus.Ready;
        ModelManager.Instance.imageObject.SetActive(true);
        ModelManager.Instance.imageObject.GetComponent<Image>().sprite = daiji;

    }

    void OnStartedRecordingVideo(VideoCapture.VideoCaptureResult result)
    {
        Debug.Log("开始录像!");
        ModelManager.Instance.SetTipText("Recording...");
        m_stopRecordingTimer = Time.time + MaxRecordingTime;
    }

    public void OnStoppedRecordingVideo(VideoCapture.VideoCaptureResult result)
    {
        ModelManager.Instance.SetTipText("Stop recording");
        m_VideoCapture.StopVideoModeAsync(OnStoppedVideoCaptureMode);
    }

    public void PlayVideo(string filepath)
    {

        if (File.Exists(filepath))
        {
            quad.SetActive(true);
            videoplayer = quad.GetComponent<VideoPlayer>();
            print("播放视频...");
            ModelManager.Instance.SetTipText("Playing video");
            showT.text = openVdieonum.ToString() + "/" + sumVideo.ToString();
            //videoplayer.url = "file:///" + Application.persistentDataPath + "/TestVideo.mp4";
            videoplayer.url = "file:///" + filepath;
            videoplayer.Play();
        }
        else
        {
            ModelManager.Instance.SetTipText("No video");
            print("视频不存在！");
        }
    }


}

/*
using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.XR.WSA.WebCam;
using System.IO;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections.Generic;

public class PhotoCaptureFinal : MonoBehaviour
{
    PhotoCapture photoCaptureObject = null;
    public AudioClip captureAudioClip;
    public AudioClip failedAudioClip;
    private AudioSource audioSource;

    public Text showT = null;//显示图片个数

    static readonly int TotalImagesToCapture = 1;
    static int capturedImageCount = 0;
    public GameObject photobtn;

    public static string openpicpath = null;
    public static int openpicnum = 0;
    public static int sumpic = 0;
    public string openpicname = null;
    public static bool isPhotoExist = false;

    public static VideoCapture m_VideoCapture = null;
    static bool isRecording;
    static readonly float MaxRecordingTime = 5.0f;
    float m_stopRecordingTimer = float.MaxValue;

    private VideoPlayer videoplayer;
    public GameObject quad;
    int VideoCount = 0;
    public static int openVdieonum = 0;
    public static int sumVideo = 0;

    public static FileInfo[] files;
    public static DirectoryInfo root;
    public static List<FileInfo> picList = new List<FileInfo>();
    public static List<FileInfo> videoList = new List<FileInfo>();

    public Sprite daiji;
    public Sprite jujiao;
    // Use this for initialization]
    void Start()
    {
        root = new DirectoryInfo(Application.persistentDataPath);
        files = root.GetFiles();
        foreach (FileInfo f in files)
        {
            if (f.Name.Contains("Image") || f.Name.Contains("jpg"))
            {
                sumpic++;
                picList.Add(f);
            }
            if (f.Name.Contains("Video") || f.Name.Contains("mp4"))
            {
                sumVideo++;
                videoList.Add(f);
            }
        }

        audioSource = gameObject.GetComponent<AudioSource>();
        ModelManager.Instance.SetTipText("");
        StatusManager.Instance.currentstatus = CurrentStatus.Close;
        quad.SetActive(false);

    }

    void Update()
    {
        StatusManager.Instance.currentstatus = StatusManager.Instance.GetStatus();
        if (m_VideoCapture == null || !m_VideoCapture.IsRecording)
        {
            return;
        }



        if (StatusManager.Instance.currentstatus == CurrentStatus.Ready)
            ModelManager.Instance.imageObject.GetComponent<Image>().sprite = daiji;
    }
    private void OnTap()
    {
        if (StatusManager.Instance.currentstatus != CurrentStatus.Close)
        {
            if (gameObject == GameObject.FindGameObjectWithTag("shutter"))
            {
                StatusManager.Instance.SetStatus(CurrentStatus.Ready);
                // photobtn.GetComponent<MeshRenderer>().material.color = Color.red;
                // gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
                // ReadyToPhoto();
                quad.SetActive(false);
                ModelManager.Instance.imageObject.SetActive(true);
                ModelManager.Instance.imageObject.GetComponent<Image>().sprite = jujiao;
                Debug.Log("tap");
            }
            else if (gameObject == GameObject.FindGameObjectWithTag("play"))
            {
                Debug.Log("play");
                if (StatusManager.Instance.currentstatus != CurrentStatus.ReadPhoto)
                {
                    StatusManager.Instance.SetStatus(CurrentStatus.ReadPhoto);
                    ModelManager.Instance.imageObject.SetActive(true);
                    quad.SetActive(false);
                    openpicname = string.Format(@"CapturedImage{0}.jpg", openpicnum);
                    openpicpath = Path.Combine(Application.persistentDataPath, openpicname);
                    //ShowPhoto(openpicname, openpicpath);
                }
                else
                {
                    StatusManager.Instance.SetStatus(CurrentStatus.Ready);
                    ModelManager.Instance.imageObject.SetActive(true);
                    ModelManager.Instance.imageObject.GetComponent<Image>().sprite = daiji;
                }
            }
            else if (gameObject == GameObject.FindGameObjectWithTag("remove"))
            {
                Debug.Log("remove");
                if (StatusManager.Instance.currentstatus == CurrentStatus.ReadPhoto)
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
                        ModelManager.Instance.imageObject.GetComponent<Image>().sprite = null;
                        ModelManager.Instance.SetTipText("Nothing.Please air tap to take photos");
                    }
                    sumpic--;
                }
                else if (StatusManager.Instance.currentstatus == CurrentStatus.WatchVideo)
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
                        ModelManager.Instance.SetTipText("Nothing.Please air tap to record videos");
                    }
                }

            }
            else if (gameObject == GameObject.FindGameObjectWithTag("back"))
            {
                Debug.Log("back");
                if (StatusManager.Instance.currentstatus == CurrentStatus.ReadPhoto)
                {
                    BackPhoto();
                }

            }
            else if (gameObject == GameObject.FindGameObjectWithTag("next"))
            {
                Debug.Log("next");
                if (StatusManager.Instance.currentstatus == CurrentStatus.ReadPhoto)
                {
                    NextPhoto();
                }
            }
            else if (gameObject == GameObject.FindGameObjectWithTag("record"))
            {
                if (StatusManager.Instance.currentstatus != CurrentStatus.CaptureVideo)
                {

                    StatusManager.Instance.SetStatus(CurrentStatus.CaptureVideo);
                    quad.SetActive(false);
                    ModelManager.Instance.imageObject.SetActive(true);
                    ModelManager.Instance.imageObject.GetComponent<Image>().sprite = jujiao;
                    TakeVideo();
                }
                else
                {
                    m_VideoCapture.StopRecordingAsync(OnStoppedRecordingVideo);
                    quad.SetActive(false);
                    ModelManager.Instance.imageObject.SetActive(true);
                    ModelManager.Instance.imageObject.GetComponent<Image>().sprite = daiji;
                    StatusManager.Instance.SetStatus(CurrentStatus.Ready);
                }
            }
            else if (gameObject == GameObject.FindGameObjectWithTag("video"))
            {
                if (StatusManager.Instance.GetStatus() != CurrentStatus.WatchVideo)
                {
                    quad.SetActive(true);
                    ModelManager.Instance.imageObject.SetActive(false);
                    StatusManager.Instance.currentstatus = CurrentStatus.WatchVideo;
                    string filename = string.Format("TestVideo_{0}.mp4", openVdieonum);
                    string filepath = Path.Combine(Application.persistentDataPath, filename);
                    filepath = filepath.Replace("/", @"\");
                    //PlayVideo(filepath);

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
            else if (gameObject == GameObject.FindGameObjectWithTag("previous"))
            {
                Debug.Log("previous");
                if (StatusManager.Instance.currentstatus == CurrentStatus.WatchVideo)
                {
                    PreviousVideo();
                }
            }
            else if (gameObject == GameObject.FindGameObjectWithTag("behind"))
            {
                Debug.Log("behind");
                if (StatusManager.Instance.currentstatus == CurrentStatus.WatchVideo)
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
            StatusManager.Instance.SetStatus(CurrentStatus.Ready);
            photobtn.GetComponent<MeshRenderer>().material.color = Color.yellow;
            ReadyToPhoto();
            Debug.Log("tap");
        }
        //gameObject.GetComponent<MeshRenderer>().material.color = Color.yellow;

    }

    public void ReadyToPhoto()
    {
        StatusManager.Instance.currentstatus = CurrentStatus.TakingPhoto;
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


        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }

    void TakePicture()
    {
        sumpic++;
        int picnum = sumpic;
        //Debug.Log(string.Format("Taking Picture ({0}/{1})...", capturedImageCount, TotalImagesToCapture));
        string filename = string.Format(@"CapturedImage{0}.jpg", picnum);
        //string filePath = System.IO.Path.Combine(Application.persistentDataPath, filename);
        string filePath = null;
        filePath = Path.Combine(Application.persistentDataPath, filename);
        while (File.Exists(filePath))
        {
            picnum++;
            filename = string.Format(@"CapturedImage{0}.jpg", picnum);
            filePath = Path.Combine(Application.persistentDataPath, filename);
        }

        audioSource.clip = captureAudioClip;
        audioSource.Play();
        Debug.Log("take picture " + filePath);
        ModelManager.Instance.SetTipText("take picture " + filePath);
        photoCaptureObject.TakePhotoAsync(filePath, PhotoCaptureFileOutputFormat.JPG, OnCapturedPhotoToDisk);
        ShowPhoto(filename, filePath, sumpic);

        files = root.GetFiles();
        picList.Add(files[files.Length - 1]);
    }



    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
        photobtn.GetComponent<MeshRenderer>().material.color = Color.gray;
        Debug.Log("Captured images have been saved at the following path.");
        Debug.Log(Application.persistentDataPath);
        StatusManager.Instance.currentstatus = CurrentStatus.Ready;
    }

    public void ShowPhoto(string file, string path, int position)
    {
        Debug.Log("opencount" + openpicnum.ToString() + "sumpic" + sumpic.ToString());

        openpicnum = position;
        ModelManager.Instance.imageObject.SetActive(true);
        StartCoroutine(LoadPic(file, path));
        //ModelManager.Instance.SetTipText("air tap to take a photo");
        ModelManager.Instance.SetTipText("" + openpicnum.ToString() + "/" + sumpic.ToString());
        showT.text = openpicnum.ToString() + "/" + sumpic.ToString();


    }

    private IEnumerator LoadPic(string picname, string path)
    {
        yield return new WaitForSeconds(2.0f);
        // string path = Application.persistentDataPath + "/" + picname + ".jpg";
        //string path = "C:\\Data\\Users\\ediso\\Pictures\\" + picname + ".jpg";
        string path1 = Path.Combine(Application.persistentDataPath, picname);
        if (File.Exists(path1))
        {
            isPhotoExist = true;
            WWW www = new WWW("file:///" + path1);
            yield return www;
            Texture2D dynaPic = www.texture;
            ModelManager.Instance.imageObject.GetComponent<Image>().sprite = Sprite.Create(dynaPic, new Rect(0, 0, dynaPic.width, dynaPic.height), new Vector2(0.5f, 0.5f));
            print("读取成功");
            ModelManager.Instance.SetTipText("load picture successfully");
        }
        else
        {
            isPhotoExist = false;
            print("图片不存在！");
            ModelManager.Instance.SetTipText("none:" + picList.Count.ToString() + "file" + files.Length.ToString());
        }
        if (StatusManager.Instance.currentstatus != CurrentStatus.ReadPhoto)
        {
            yield return new WaitForSeconds(5.0f);
            ModelManager.Instance.imageObject.SetActive(true);
            ModelManager.Instance.imageObject.GetComponent<Image>().sprite = daiji;

        }
    }
    public void BackPhoto()
    {
        if (sumpic > 0)
        {
            if (openpicnum > 0)
            {
                openpicnum--;
                Debug.Log("back" + openpicnum.ToString());
                //openpicname = string.Format(@"CapturedImage{0}.jpg", openpicnum);
                openpicname = picList[openpicnum].Name+".jpg";
                //openpicpath = System.IO.Path.Combine(Application.persistentDataPath, openpicname);
                openpicpath = Path.Combine(Application.persistentDataPath, openpicname);
                if (File.Exists(openpicpath))
                    ShowPhoto(openpicname, openpicpath, openpicnum);
                // else BackPhoto();
            }
            else ModelManager.Instance.SetTipText("It's has been the first picture.");
        }

    }
    public void NextPhoto()
    {
        if (sumpic > 0)
        {
            if (openpicnum < sumpic - 1)
            {
                openpicnum++;
                Debug.Log("next" + openpicnum.ToString());
                //openpicname = string.Format(@"CapturedImage{0}.jpg", openpicnum);
                openpicname = picList[openpicnum].Name+".jpg";
                //openpicpath = System.IO.Path.Combine(Application.persistentDataPath, openpicname);
                openpicpath = Path.Combine(Application.persistentDataPath, openpicname);
                if (File.Exists(openpicpath))
                    ShowPhoto(openpicname, openpicpath, openpicnum);
                //else NextPhoto();
            }
            else ModelManager.Instance.SetTipText("It's has been the last picture.");
        }

    }
    //************************************Video****************************************************

    public void NextVideo()
    {
        if (sumVideo > 0)
        {
            if (openVdieonum < sumVideo - 1)
            {
                openVdieonum++;
                Debug.Log("next" + openVdieonum.ToString());
                //string filename = string.Format("TestVideo_{0}.mp4", openVdieonum);
                string filename = videoList[openVdieonum].Name+".mp4";
                string filepath = Path.Combine(Application.persistentDataPath, filename);
                filepath = filepath.Replace("/", @"\");
                if (File.Exists(filepath))
                    PlayVideo(filepath, openVdieonum);
                //else NextVideo();
            }
            else ModelManager.Instance.SetTipText("It's has been the last video.");
        }
    }
    public void PreviousVideo()
    {
        if (sumVideo > 0)
        {
            if (openVdieonum > 0)
            {
                openVdieonum--;
                Debug.Log("back" + openVdieonum.ToString());
                //string filename = string.Format("TestVideo_{0}.mp4", openVdieonum);
                string filename = videoList[openVdieonum].Name+".mp4";
                string filepath = Path.Combine(Application.persistentDataPath, filename);
                filepath = filepath.Replace("/", @"\");
                if (File.Exists(filepath))
                    PlayVideo(filepath, openVdieonum);
                //else PreviousVideo();
            }
            else ModelManager.Instance.SetTipText("It's has been the first video.");
        }


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

        files = root.GetFiles();
        //  }

    }

    void OnStartedVideoCaptureMode(VideoCapture.VideoCaptureResult result)
    {
        Debug.Log("开始录像模式!");
        sumVideo++;
        //getPicNum();
        //timeStamp = Time.time.ToString().Replace(".", "").Replace(":", "");

        string filename = string.Format("TestVideo_{0}.mp4", sumVideo);
        //string filename = "TestVideo.mp4";
        string filepath = Path.Combine(Application.persistentDataPath, filename);
        filepath = filepath.Replace("/", @"\");

        m_VideoCapture.StartRecordingAsync(filepath, OnStartedRecordingVideo);
        print("videopath:" + filepath);

        files = root.GetFiles();
    }

    void OnStoppedVideoCaptureMode(VideoCapture.VideoCaptureResult result)
    {
        m_VideoCapture.Dispose();
        m_VideoCapture = null;
        Debug.Log("停止录像模式!");
        StatusManager.Instance.currentstatus = CurrentStatus.Ready;
        ModelManager.Instance.imageObject.SetActive(true);
        ModelManager.Instance.imageObject.GetComponent<Image>().sprite = daiji;

    }

    void OnStartedRecordingVideo(VideoCapture.VideoCaptureResult result)
    {
        Debug.Log("开始录像!");
        ModelManager.Instance.SetTipText("Recording...");
        m_stopRecordingTimer = Time.time + MaxRecordingTime;
    }

    public void OnStoppedRecordingVideo(VideoCapture.VideoCaptureResult result)
    {
        ModelManager.Instance.SetTipText("Stop recording");
        m_VideoCapture.StopVideoModeAsync(OnStoppedVideoCaptureMode);
    }

    public void PlayVideo(string filepath, int position)
    {
        if (File.Exists(filepath))
        {
            openVdieonum = position;
            quad.SetActive(true);
            videoplayer = quad.GetComponent<VideoPlayer>();
            print("播放视频...");
            ModelManager.Instance.SetTipText("Playing video");
            showT.text = openVdieonum.ToString() + "/" + sumVideo.ToString();
            //videoplayer.url = "file:///" + Application.persistentDataPath + "/TestVideo.mp4";
            videoplayer.url = "file:///" + filepath;
            videoplayer.Play();
        }
        else
        {
            ModelManager.Instance.SetTipText("No video");
            print("视频不存在！");
        }
    }


}
*/