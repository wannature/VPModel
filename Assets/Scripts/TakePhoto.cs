using HoloToolkit.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.WSA.WebCam;
using System.IO;


public class TakePhoto : Singleton<TakePhoto>
{

    Resolution cameraResolution;
    UnityEngine.XR.WSA.WebCam.PhotoCapture photoCaptureObject = null;
    UnityEngine.XR.WSA.WebCam.CameraParameters cameraParameters;
    private CurrentStatus currentStatus = CurrentStatus.Ready;

    public AudioClip captureAudioClip;
    public AudioClip failedAudioClip;
    private AudioSource audioSource;
    private Texture2D targetTexture;
    private List<byte> imageBufferList = new List<byte>();

    
    Camera secondCam;
    public GameObject quad;
    int count = 0;
    // Use this for initialization
    void Start()
    {
        cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
        secondCam = GameObject.FindWithTag("Camera").GetComponent<Camera>();
        audioSource = gameObject.GetComponent<AudioSource>();
        ModelManager.Instance.SetTipText("tap to take photos");
        currentStatus = CurrentStatus.Ready;
        
    }

    // Update is called once per frame
    void Update()
    {
       
    }
    private void OnTap()
    {

        gameObject.GetComponent<MeshRenderer>().material.color = Color.red;

        Capture(secondCam, "background");
        HololensPhoto();

    }

    private void OnDoubleTap()
    {
        gameObject.GetComponent<MeshRenderer>().material.color = Color.yellow;
    }
    void HololensPhoto()
    {
        Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
        targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);

        // Create a PhotoCapture object
            PhotoCapture.CreateAsync(false, delegate (PhotoCapture captureObject) {
            photoCaptureObject = captureObject;
            CameraParameters cameraParameters = new CameraParameters();
            cameraParameters.hologramOpacity = 0.0f;
            cameraParameters.cameraResolutionWidth = cameraResolution.width;
            cameraParameters.cameraResolutionHeight = cameraResolution.height;
            cameraParameters.pixelFormat = CapturePixelFormat.BGRA32;

            // Activate the camera
            photoCaptureObject.StartPhotoModeAsync(cameraParameters, delegate (PhotoCapture.PhotoCaptureResult result) {
                // Take a picture
                photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
            });
        });
    }

    void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        // Copy the raw image data into our target texture
        photoCaptureFrame.UploadImageDataToTexture(targetTexture);
        targetTexture.Apply();

        quad.GetComponent<Image>().sprite = Sprite.Create(targetTexture, new Rect(0, 0, targetTexture.width, targetTexture.height), new Vector2(0.5f, 0.5f));
        string path = Application.persistentDataPath + "/" +"holophoto" + count.ToString() + ".jpg";
        System.IO.File.WriteAllBytes(path, targetTexture.EncodeToJPG());
        Debug.Log(path);
        count++;
        ModelManager.Instance.SetTipText("success");
        currentStatus = CurrentStatus.Ready;
        // Deactivate our camera
        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }

    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        // Shutdown our photo capture resource
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }
    void Capture(Camera m_Camera, string filename)
    {
        ModelManager.Instance.SetTipText("taking a photo...");
        currentStatus = CurrentStatus.TakingPhoto;
        audioSource.clip = captureAudioClip;
        audioSource.Play();
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 16);
        m_Camera.targetTexture = rt;
        m_Camera.Render();

        RenderTexture.active = rt;
        Texture2D t = new Texture2D(Screen.width, Screen.height);
        t.ReadPixels(new Rect(0, 0, t.width, t.height), 0, 0);
        t.Apply();

        quad.GetComponent<Image>().sprite = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f));
        string path = Application.persistentDataPath + "/" + filename + count.ToString() + ".jpg";
        System.IO.File.WriteAllBytes(path, t.EncodeToJPG());
        Debug.Log(path);
        count++;
        ModelManager.Instance.SetTipText("tap to take photos");
        currentStatus = CurrentStatus.Ready;
    }

    
    
    public CurrentStatus GetCurrentStatus()
    {
        return currentStatus;
    }

  
    public void SetCurrentStatus(CurrentStatus status)
    {
        currentStatus = status;
    }

   

    /// <summary>
    /// 上下反转
    /// </summary>
    /// <param name="src">图像数据</param>
    /// <param name="width">宽度</param>
    /// <param name="height">高度</param>
    /// <param name="stride">每个像素的宽度</param>
    /// <returns></returns>
    private List<byte> FlipVertical(List<byte> src, int width, int height, int stride)
    {
        byte[] dst = new byte[src.Count];
        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x < width; ++x)
            {
                int invY = (height - 1) - y;
                int pxel = (y * width + x) * stride;
                int invPxel = (invY * width + x) * stride;
                for (int i = 0; i < stride; ++i)
                {
                    dst[invPxel + i] = src[pxel + i];
                }
            }
        }
        return new List<byte>(dst);
    }

    
    }

    


