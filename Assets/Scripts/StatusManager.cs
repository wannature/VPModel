using HoloToolkit.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;


public class StatusManager : Singleton<StatusManager>
{

    public CurrentStatus currentstatus;
    public GameObject photoBtn;
    public GameObject videoBtn;
    public GameObject playBtn;
    public GameObject removeBtn;
    public GameObject nextBtn;
    public GameObject backBtn;
    public GameObject preBtn;
    public GameObject behindBtn;

    //public GameObject imageObj;
    private AudioSource audio1;
    public AudioClip openedAudioClip;
    public AudioClip closedAudioClip;

    public Material openmat;
    public Material closemat;
    public Sprite openimage;
    private VideoPlayer vp;
    private VideoPlayer vpe;
    public GameObject openquad;
    public GameObject closequad;
    public GameObject quad;
    public GameObject quad1;

    // Use this for initialization
    void Start () {
        openquad.SetActive(true);
        closequad.SetActive(false);
        vp = openquad.GetComponent<VideoPlayer>();
        
        audio1 = gameObject.GetComponent<AudioSource>();
        //cameraIsColosed();
        IsColosed();
        currentstatus = CurrentStatus.Close;
     
    }
	
	// Update is called once per frame
	void Update () {
        if (!vp.isPlaying)
            quad.SetActive(false);
        if (vpe.isPlaying)
            quad.SetActive(true);
        //if (vp.isPlaying)//zhe
        //    quad1.SetActive(false);
    }
    void OnTap()
    {
       if (gameObject == GameObject.FindGameObjectWithTag("open"))
       {
            if (currentstatus == CurrentStatus.Close)
            {
                currentstatus = CurrentStatus.Ready;
                OpenCamera();
            }
            else
            {
                currentstatus = CurrentStatus.Close;
                cameraIsColosed();
            }
       }      
    }
    public void cameraIsColosed()
    {
        ModelManager.Instance.SetTipText(null);

        quad.SetActive(true);
        openquad.SetActive(false);
        closequad.SetActive(true);
        vpe = closequad.GetComponent<VideoPlayer>();
        vpe.Play();

        audio1.clip = closedAudioClip;
        audio1.Play();
        //延迟
        //yield return new WaitForSeconds(2.0f);

        //imageObj.GetComponent<Renderer>().material.color = Color.black;
        //quad.SetActive(false);
        //vpe.Play();//位置
        ModelManager.Instance.SetTipText("Camera has closed!");
        //禁用组件
        ModelManager.Instance.imageObject.SetActive(false);
        ModelManager.Instance.imageObject.GetComponent<Image>().material.color = Color.black;
        photoBtn.GetComponent<PhotoCaptureFinal>().enabled = false;
        videoBtn.GetComponent<PhotoCaptureFinal>().enabled = false;
        playBtn.GetComponent<PhotoCaptureFinal>().enabled = false;
        removeBtn.GetComponent<PhotoCaptureFinal>().enabled = false;
        nextBtn.GetComponent<PhotoCaptureFinal>().enabled = false;
        backBtn.GetComponent<PhotoCaptureFinal>().enabled = false;
        preBtn.GetComponent<PhotoCaptureFinal>().enabled = false;
        behindBtn.GetComponent<PhotoCaptureFinal>().enabled = false;
        ModelManager.Instance.imageObject.GetComponent<Image>().sprite = null;
    }
    public void IsColosed()//start调用的
    {
        ModelManager.Instance.SetTipText(null);
        /*
        openquad.SetActive(false);
        closequad.SetActive(true);
        vpe = closequad.GetComponent<VideoPlayer>();
        vpe.Play();
        */
        audio1.clip = closedAudioClip;
        audio1.Play();
        //延迟
        //yield return new WaitForSeconds(2.0f);

        //imageObj.GetComponent<Renderer>().material.color = Color.black;
        quad.SetActive(false);
        //vpe.Play();//位置
        ModelManager.Instance.SetTipText("Camera is closed!");
        //禁用组件
        ModelManager.Instance.imageObject.SetActive(false);
        ModelManager.Instance.imageObject.GetComponent<Image>().material.color = Color.black;
        photoBtn.GetComponent<PhotoCaptureFinal>().enabled = false;
        videoBtn.GetComponent<PhotoCaptureFinal>().enabled = false;
        playBtn.GetComponent<PhotoCaptureFinal>().enabled = false;
        removeBtn.GetComponent<PhotoCaptureFinal>().enabled = false;
        nextBtn.GetComponent<PhotoCaptureFinal>().enabled = false;
        backBtn.GetComponent<PhotoCaptureFinal>().enabled = false;
        preBtn.GetComponent<PhotoCaptureFinal>().enabled = false;
        behindBtn.GetComponent<PhotoCaptureFinal>().enabled = false;
        ModelManager.Instance.imageObject.GetComponent<Image>().sprite = null;
    }
    public void OpenCamera()
    {
        quad.SetActive(true);
        
        openquad.SetActive(true);
        closequad.SetActive(false);

        vp.Play();
        ModelManager.Instance.SetTipText("Camera has opened!");
        audio1.clip = openedAudioClip;
        audio1.Play();
        
        ModelManager.Instance.imageObject.SetActive(true);
        ModelManager.Instance.imageObject.GetComponent<Image>().material.color = Color.white;
        //imageObj.GetComponent<Renderer>().material.color = Color.white;
        photoBtn.GetComponent<PhotoCaptureFinal>().enabled = true;
        videoBtn.GetComponent<PhotoCaptureFinal>().enabled = true;
        playBtn.GetComponent<PhotoCaptureFinal>().enabled = true;
        removeBtn.GetComponent<PhotoCaptureFinal>().enabled = true;
        nextBtn.GetComponent<PhotoCaptureFinal>().enabled = true;
        backBtn.GetComponent<PhotoCaptureFinal>().enabled = true;
        preBtn.GetComponent<PhotoCaptureFinal>().enabled = true;
        behindBtn.GetComponent<PhotoCaptureFinal>().enabled = true;
        ModelManager.Instance.imageObject.GetComponent<Image>().sprite = openimage;
    }
   public void SetStatus(CurrentStatus cstatus)
    {
        currentstatus = cstatus;
    }
   public  CurrentStatus GetStatus()
    {
        return currentstatus;
    }
    
}
