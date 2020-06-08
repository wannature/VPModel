using HoloToolkit.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.WSA.WebCam;
using System.IO;


public class CapturePhotoManager : Singleton<CapturePhotoManager>
{
    Resolution cameraResolution;
    UnityEngine.XR.WSA.WebCam.PhotoCapture photoCaptureObject = null;
    UnityEngine.XR.WSA.WebCam.CameraParameters cameraParameters;

    private CurrentStatus currentStatus = CurrentStatus.Ready;

    public GameObject cropBoxObject;

    public AudioClip captureAudioClip;
    public AudioClip failedAudioClip;
    private AudioSource audioSource;
    private Texture2D targetTexture;
    private List<byte> imageBufferList = new List<byte>();

    

    // Use this for initialization
    void Start () {
        audioSource = gameObject.GetComponent<AudioSource>();
        cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
        
    }
    private void OnTap()
    {

        gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
    
        TakePhoto();
        Debug.Log("tap");

    }

    private void OnDoubleTap()
    {
        gameObject.GetComponent<MeshRenderer>().material.color = Color.yellow;
    }
    /// <summary>
    /// 开始拍照流程
    /// </summary>
    public void TakePhoto()
    {        
        PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);  //全息？
    }

    /// <summary>
    /// 获取当前状态
    /// </summary>
    /// <returns>当前状态值</returns>
    public CurrentStatus GetCurrentStatus()
    {
        return currentStatus;
    }

    /// <summary>
    /// 设置当前状态
    /// </summary>
    /// <param name="status">状态值</param>
    public void SetCurrentStatus(CurrentStatus status)
    {
        currentStatus = status;
    }

    /// <summary>
    /// 设置Camera参数，开始拍照
    /// </summary>
    /// <param name="captureObject"></param>
    void OnPhotoCaptureCreated(UnityEngine.XR.WSA.WebCam.PhotoCapture captureObject)
    {        
        ModelManager.Instance.SetTipText("taking a photo...");
        currentStatus = CurrentStatus.TakingPhoto;

        Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
        targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);
        
        photoCaptureObject = captureObject;
        cameraParameters = new UnityEngine.XR.WSA.WebCam.CameraParameters();
        cameraParameters.hologramOpacity = 0.0f;
        cameraParameters.cameraResolutionWidth = cameraResolution.width;
        cameraParameters.cameraResolutionHeight = cameraResolution.height;
        cameraParameters.pixelFormat = UnityEngine.XR.WSA.WebCam.CapturePixelFormat.BGRA32;

        captureObject.StartPhotoModeAsync(cameraParameters, OnPhotoModeStarted);
        
    }

    /// <summary>
    /// 开始拍照
    /// </summary>
    /// <param name="result">拍照结果</param>
    private void OnPhotoModeStarted(UnityEngine.XR.WSA.WebCam.PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            ModelManager.Instance.SetTipText("ready to memory");
            Debug.Log("ready to memory");
            audioSource.Play();
            photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
             Debug.Log("sprite");
            //SavePhoto();
           // ShowPhoto();
        }
        else
        {
            ModelManager.Instance.SetTipText("once again");
            currentStatus = CurrentStatus.Ready;
            ModelManager.Instance.SetPhotoImageActive(false);
        }
    }
   /* int count = 0;
    /// <summary>
    /// 照片拍摄完成，获取拍摄的照片，调用Custom Vision API,对图片进行分析
    /// </summary>
    /// <param name="result">拍照的结果</param>
    /// <param name="photoCaptureFrame">拍摄的图片</param>
    private void OnCapturedPhotoToMemory(UnityEngine.XR.WSA.WebCam.PhotoCapture.PhotoCaptureResult result, UnityEngine.XR.WSA.WebCam.PhotoCaptureFrame photoCaptureFrame)
    {
        if (result.success)
        {
            audioSource.Stop();
            audioSource.clip = captureAudioClip;
            audioSource.Play();

            ModelManager.Instance.SetPhotoImageActive(true);
            ModelManager.Instance.SetTipText("showing...");

        

            photoCaptureFrame.UploadImageDataToTexture(targetTexture);
            targetTexture.Apply();

            ModelManager.Instance.imageObject.GetComponent<Image>().sprite = Sprite.Create(targetTexture, new Rect(0, 0, targetTexture.width, targetTexture.height), new Vector2(0.5f, 0.5f));
            string path = Application.persistentDataPath + "/" + "holophoto" + count.ToString() + ".jpg";
            System.IO.File.WriteAllBytes(path, targetTexture.EncodeToJPG());
            Debug.Log(path);
            count++;
            ModelManager.Instance.SetTipText("success");
            currentStatus = CurrentStatus.Ready;
            // Deactivate our camera
            photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);

        }
        else
        {
            audioSource.Stop();
            audioSource.clip = failedAudioClip;
            audioSource.Play();

            currentStatus = CurrentStatus.Ready;
            ModelManager.Instance.SetTipText(" to memory failed");
        }
        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }*/
    
    void OnCapturedPhotoToMemory(UnityEngine.XR.WSA.WebCam.PhotoCapture.PhotoCaptureResult result, UnityEngine.XR.WSA.WebCam.PhotoCaptureFrame photoCaptureFrame)
    {
        if (result.success)
        {
            //照片显示
            
            Debug.Log("success");
            audioSource.Stop();
            audioSource.clip = captureAudioClip;
            audioSource.Play();

            ModelManager.Instance.SetPhotoImageActive(true);
           

            // ToolManager.Instance.ShowMenu();
            

            photoCaptureFrame.CopyRawImageDataIntoBuffer(imageBufferList);
            imageBufferList = FlipVertical(imageBufferList, cameraParameters.cameraResolutionWidth, cameraParameters.cameraResolutionHeight, 4);

            Texture2D targetTexture = CreateTexture(imageBufferList, cameraParameters.cameraResolutionWidth, cameraParameters.cameraResolutionHeight);
            Sprite sprite = Sprite.Create(targetTexture, new Rect(0, 0, targetTexture.width, targetTexture.height), new Vector2(0.5f, 0.5f));

            ModelManager.Instance.SetPhotoImage(sprite);
            // photoCaptureFrame.CopyRawImageDataIntoBuffer(imageBufferList);
            // imageBufferList = FlipVertical(imageBufferList, cameraParameters.cameraResolutionWidth, cameraParameters.cameraResolutionHeight, 4);
            // targetTexture = CreateTexture(imageBufferList, cameraParameters.cameraResolutionWidth, cameraParameters.cameraResolutionHeight);
            //Sprite sprite = Sprite.Create(targetTexture, new Rect(0, 0, targetTexture.width, targetTexture.height), new Vector2(0.5f, 0.5f));
            // ModelManager.Instance.SetPhotoImage(sprite);


            // photoCaptureFrame.UploadImageDataToTexture(targetTexture);
            // Sprite sprite = Sprite.Create(targetTexture, new Rect(0, 0, targetTexture.width, targetTexture.height), new Vector2(0.5f, 0.5f));
            // ModelManager.Instance.SetPhotoImage(sprite);
            // GameObject sceneObj = ModelManager.Instance.imageObject;
            // Renderer objRenderer = sceneObj.GetComponent<Renderer>() as Renderer;
            //objRenderer.material = new Material(Shader.Find("Unlit/Texture"));

            //// sceneObj.transform.parent = this.transform;
            //sceneObj.transform.localPosition = new Vector3(0.0f, 0.0f, 3.0f);
            // objRenderer.material.SetTexture("_MainTex", targetTexture);

        }
        else
        {
            currentStatus = CurrentStatus.Ready;
            ModelManager.Instance.SetTipText("failed");
            Debug.Log("failed");
        }
        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
        
    }
    



    /// <summary>
    /// 拍照结束，释放资源
    /// </summary>
    /// <param name="result">result</param>
    private void OnStoppedPhotoMode(UnityEngine.XR.WSA.WebCam.PhotoCapture.PhotoCaptureResult result)
    {
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }
    /*
    public void ShowEditedImage()
    {
        bool cropBoxActiveFlag = ModelManager.Instance.IsCropBoxActive();
        RectTransform rectTransform = cropBoxObject.GetComponent<CropBoxManager>().GetCropBoxRectTransform();
        Vector3 cropBoxLocalPosition = new Vector3(rectTransform.localPosition.x, rectTransform.localPosition.y, rectTransform.localPosition.z);
        Vector2 cropBoxSize = new Vector2(rectTransform.rect.width, rectTransform.rect.height);
        Vector2 parentSize = cropBoxObject.GetComponent<CropBoxManager>().GetParentSize();

        if (cropBoxActiveFlag)
        {
            int leftSide = (int)((cropBoxLocalPosition.x - cropBoxSize.x / 2 + parentSize.x / 2) / parentSize.x * cameraParameters.cameraResolutionWidth);
            int rightSide = (int)((parentSize.x / 2 + cropBoxLocalPosition.x + cropBoxSize.x / 2) / parentSize.x * cameraParameters.cameraResolutionWidth);
            int bottomSide = (int)((cropBoxLocalPosition.y - cropBoxSize.y / 2 + parentSize.y / 2) / parentSize.y * cameraParameters.cameraResolutionHeight);
            int topSide = (int)((parentSize.y / 2 + cropBoxLocalPosition.y + cropBoxSize.y / 2) / parentSize.y * cameraParameters.cameraResolutionHeight);
            //用于显示
            byte[] dst = new byte[imageBufferList.Count];
            //用于上传
            byte[] dstpost = new byte[(rightSide - leftSide + 1) * (topSide - bottomSide + 1) * 4];
            int count = 0;
            for (int y = 0; y < cameraParameters.cameraResolutionHeight; ++y)
            {
                for (int x = 0; x < cameraParameters.cameraResolutionWidth; ++x)
                {
                    int px = (y * cameraParameters.cameraResolutionWidth + x) * 4;
                    if (x >= leftSide && x <= rightSide && y >= bottomSide && y <= topSide)
                    {
                        int index = count * 4;
                        for (int i = 0; i < 4; ++i)
                        {
                            dst[px + i] = imageBufferList[px + i];
                            dstpost[index + i] = imageBufferList[px + i];
                        }
                        count++;
                    }
                    else
                    {
                    }
                }
            }

            Texture2D targetTexture = new Texture2D(cameraParameters.cameraResolutionWidth, cameraParameters.cameraResolutionHeight, TextureFormat.BGRA32, false);
            targetTexture.LoadRawTextureData(dst);
            targetTexture.Apply();
            Sprite sprite = Sprite.Create(targetTexture, new Rect(0, 0, targetTexture.width, targetTexture.height), new Vector2(0.5f, 0.5f));
            ModelManager.Instance.SetPhotoImage(sprite);
            ModelManager.Instance.SetCropBoxActive(false);

            Texture2D postTexture = new Texture2D(rightSide - leftSide + 1, topSide - bottomSide + 1, TextureFormat.BGRA32, false);
            postTexture.LoadRawTextureData(dstpost);
            postTexture.Apply();
        }
        ModelManager.Instance.SetTipText("air tap to take a photo");
        currentStatus = CurrentStatus.Ready;
    }*/

    /// <summary>
    /// 创建Texture2D
    /// </summary>
    /// <param name="rawData">图像数据</param>
    /// <param name="width">宽度</param>
    /// <param name="height">高度</param>
    /// <returns></returns>
    private Texture2D CreateTexture(List<byte> rawData, int width, int height)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.BGRA32, false);
        tex.LoadRawTextureData(rawData.ToArray());
        tex.Apply();
        return tex;
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

    public void SavePhoto()

    {
        SavenPic(targetTexture, "test");
        ModelManager.Instance.imageObject.SetActive(false);

    }

    public void SavenPic(Texture2D tex, string filename)
    {
        try
        {
            string path = Application.persistentDataPath + "/" + filename + ".jpg";
          // string path= "C:\\Data\\Users\\ediso\\Pictures\\"+filename + ".jpg";
            File.WriteAllBytes(path, tex.EncodeToJPG());
            
            ModelManager.Instance.imageObject.GetComponent<Image>().sprite = null;
            print("保存成功！" + path);
            ModelManager.Instance.SetTipText("save successfully");
            
        }
        catch (System.Exception e)
        {
            ModelManager.Instance.SetTipText("save failed");
            print("保存失败！" + e.Message);

        }
    }
    public void ShowPhoto()

    {
        ModelManager.Instance.imageObject.SetActive(true);
        StartCoroutine(LoadPic("test"));
        ModelManager.Instance.SetTipText("air tap to take a photo");
        currentStatus = CurrentStatus.Ready;
    }
    private IEnumerator LoadPic(string picname)
    {
        string path = Application.persistentDataPath + "/" + picname + ".jpg";
        //string path = "C:\\Data\\Users\\ediso\\Pictures\\" + picname + ".jpg";
        if (File.Exists(path))
        {
            WWW www = new WWW("file:///" + path);
            yield return www;
            Texture2D dynaPic = www.texture;
            ModelManager.Instance.imageObject.GetComponent<Image>().sprite = Sprite.Create(dynaPic, new Rect(0, 0, dynaPic.width, dynaPic.height), new Vector2(0.5f, 0.5f));
            print("读取成功");
            ModelManager.Instance.SetTipText("load picture successfully");
            currentStatus = CurrentStatus.Ready;
        }
        else
        {
            print("图片不存在！");
            ModelManager.Instance.SetTipText("none");
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
