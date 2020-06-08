using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using HoloToolkit.Unity;
public class statusma  :Singleton <statusma>{

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
    public GameObject openquad;
    public GameObject quad;

    // Use this for initialization
    void Start()
    {
        vp = openquad.GetComponent<VideoPlayer>();
        audio1 = gameObject.GetComponent<AudioSource>();
        cameraIsColosed();
        currentstatus = CurrentStatus.Close;


    }

    // Update is called once per frame
    void Update()
    {

        if (!vp.isPlaying)
            quad.SetActive(false);
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
    void cameraIsColosed()
    {
        modelma.Instance.SetTipText(null);
        audio1.clip = closedAudioClip;
        audio1.Play();
        //imageObj.GetComponent<Renderer>().material.color = Color.black;
        quad.SetActive(false);
        modelma.Instance.imageObject.SetActive(false);
        modelma.Instance.imageObject.GetComponent<Image>().material.color = Color.black;
        photoBtn.GetComponent<picCaptureFInal>().enabled = false;
        videoBtn.GetComponent<picCaptureFInal>().enabled = false;
        playBtn.GetComponent<picCaptureFInal>().enabled = false;
        removeBtn.GetComponent<picCaptureFInal>().enabled = false;
        nextBtn.GetComponent<picCaptureFInal>().enabled = false;
        backBtn.GetComponent<picCaptureFInal>().enabled = false;
        preBtn.GetComponent<picCaptureFInal>().enabled = false;
        behindBtn.GetComponent<picCaptureFInal>().enabled = false;
        modelma.Instance.imageObject.GetComponent<Image>().sprite = null;
    }
    void OpenCamera()
    {
        quad.SetActive(true);
        vp.Play();
        modelma.Instance.SetTipText("Camera has opened!");
        audio1.clip = openedAudioClip;
        audio1.Play();

        modelma.Instance.imageObject.SetActive(true);
        modelma.Instance.imageObject.GetComponent<Image>().material.color = Color.white;
        //imageObj.GetComponent<Renderer>().material.color = Color.white;
        photoBtn.GetComponent<picCaptureFInal>().enabled = true;
        videoBtn.GetComponent<picCaptureFInal>().enabled = true;
        playBtn.GetComponent<picCaptureFInal>().enabled = true;
        removeBtn.GetComponent<picCaptureFInal>().enabled = true;
        nextBtn.GetComponent<picCaptureFInal>().enabled = true;
        backBtn.GetComponent<picCaptureFInal>().enabled = true;
        preBtn.GetComponent<picCaptureFInal>().enabled = true;
        behindBtn.GetComponent<picCaptureFInal>().enabled = true;
        modelma.Instance.imageObject.GetComponent<Image>().sprite = openimage;
    }
    public void SetStatus(CurrentStatus cstatus)
    {
        currentstatus = cstatus;
    }
    public CurrentStatus GetStatus()
    {
        return currentstatus;
    }
}
