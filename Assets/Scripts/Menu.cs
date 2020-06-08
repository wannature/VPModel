using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour {

   
    public GameObject CameraModelA;
    public GameObject CameraModelB;
    public GameObject text;
    public GameObject image;
    public GameObject btn1;
    public GameObject btn2;
	// Use this for initialization
	void Start () {
        
        CameraModelA.SetActive(false);
        CameraModelB.SetActive(false);
        text.SetActive(true);
        image.SetActive(true);
        btn1.SetActive(true);
        btn2.SetActive(true);

            }
    private void Awake()
    {
        
        Button button1 = btn1.GetComponent<Button>() as Button;//获取Button组件
        button1.onClick.AddListener(myClick1);
        Button button2 = btn2.GetComponent<Button>() as Button;//获取Button组件
        button2.onClick.AddListener(myClick2);
    }
    // Update is called once per frame
    void Update () {
        

    }
    void myClick1()
    {
        CameraModelA.SetActive(true);
        CameraModelB.SetActive(false);
        text.SetActive(false);
        image.SetActive(false);
        btn1.SetActive(false);
        btn2.SetActive(false);
    }
    void myClick2()
    {
        CameraModelB.SetActive(true);
        CameraModelA.SetActive(false);
        text.SetActive(false);
        image.SetActive(false);
        btn1.SetActive(false);
        btn2.SetActive(false);
        
    }
   
    void OnTap()
    {
        if(gameObject==GameObject.FindWithTag("cameraA"))
        {
            myClick1();
        }
        else if (gameObject == GameObject.FindWithTag("cameraB"))
        {
            myClick2();
        }
    }
}
