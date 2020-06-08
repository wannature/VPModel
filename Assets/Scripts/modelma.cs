using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HoloToolkit.Unity;

public class modelma : Singleton<modelma>
{

    public Text tipText = null;
    public GameObject imageObject;


    // Use this for initialization
    void Start()
    {
        imageObject.SetActive(false);
        // cropBox.SetActive(false);
        // tool.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
    }

    /// <summary>
    /// 显示Tip 信息
    /// </summary>
    /// <param name="text">Tip 信息</param>
    public void SetTipText(string text)
    {
        if (tipText != null)
        {
            tipText.text = text;
        }
    }

    /// <summary>
    /// 设置图片
    /// </summary>
    /// <param name="sprite"></param>
    public void SetPhotoImage(Sprite sprite)
    {
        if (imageObject != null)
        {
            imageObject.SetActive(true);
            imageObject.GetComponent<Image>().sprite = sprite;
            ModelManager.Instance.SetTipText("sprite ok");
            Debug.Log("sprite ok");
        }
    }

    /// <summary>
    /// 设置Image active状态
    /// </summary>
    /// <param name="activeFlag"></param>
    public void SetPhotoImageActive(bool activeFlag)
    {
        if (imageObject != null)
        {
            imageObject.SetActive(activeFlag);
        }
    }
}
