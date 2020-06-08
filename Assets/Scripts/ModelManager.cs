using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HoloToolkit.Unity;

public class ModelManager : Singleton<ModelManager> {

    public Text tipText = null;
    public GameObject imageObject;
   

    // Use this for initialization
    void Start () {
        imageObject.SetActive(false);
       // cropBox.SetActive(false);
       // tool.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
	}

    /// <summary>
    /// 显示Tip 信息
    /// </summary>
    /// <param name="text">Tip 信息</param>
    public void SetTipText(string text)
    {
        if (text != null) {
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

    /// <summary>
    /// 设置裁剪框active状态
    /// </summary>
    /// <param name="activeFlag"></param>
   /* public void SetCropBoxActive(bool activeFlag)
    {
        if (cropBox != null)
        {
            cropBox.SetActive(activeFlag);
        }
    }

    /// <summary>
    /// 获取裁剪框active状态
    /// </summary>
    /// <returns></returns>
    public bool IsCropBoxActive()
    {
        if (cropBox != null)
        {
            return cropBox.activeSelf;
        }
        return false;
    }

    /// <summary>
    /// 重置裁剪框的大小和位置
    /// </summary>
    public void ResetCropBoxTransform()
    {
        if (cropBox != null)
        {
            cropBox.GetComponent<CropBoxManager>().ResetCropBoxTransform();
        }
    }*/
}
