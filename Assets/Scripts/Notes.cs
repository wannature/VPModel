using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HoloToolkit.Unity;
using System.Threading;

public class Notes : Singleton<ModelManager>
{

    public  Text tipText = null;

    // Use this for initialization
    void Start()
    {


        StartCoroutine(SetTipText());
    }

    void Update()
    {
    }

    IEnumerator  SetTipText()
    {
        yield return new WaitForSeconds(3f);
        tipText.text = "The camera shutter ";
        yield return new WaitForSeconds(1.2f);
        tipText.text += "position needs ";
        yield return new WaitForSeconds(1.5f);
        tipText.text += "to be adjusted";
        yield return new WaitForSeconds(2.5f);
        tipText.text += " which is too close to the switch";
    }
}
