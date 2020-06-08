using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;
using System;
using System.Text;

public class DictionManager : MonoBehaviour
{

    [Tooltip("A text area for the recognizer to display the recognized strings.")]
    public Text DictationDisplay;
    private DictationRecognizer dictationRecognizer;

    // Use this for initialization
    void Start()
    {
        dictationRecognizer = new DictationRecognizer();
        //订阅事件
        dictationRecognizer.DictationHypothesis += DictationRecognizer_DictationHypothesis;
        dictationRecognizer.DictationResult += DictationRecognizer_DictationResult;
        dictationRecognizer.DictationComplete += DictationRecognizer_DictationComplete;
        dictationRecognizer.DictationError += DictationRecognizer_DictationError;

        dictationRecognizer.Start();
    }

    private void DictationRecognizer_DictationError(string error, int hresult)
    {
        DictationDisplay.text = "error";
    }

    private void DictationRecognizer_DictationComplete(DictationCompletionCause cause)
    {
        DictationDisplay.text = "complete:";
        //如果在听写开始后第一个5秒内没听到任何声音，将会超时
        //如果识别到了一个结果但是之后20秒没听到任何声音，也会超时
        if (cause == DictationCompletionCause.TimeoutExceeded)
        {
            //超时后本例重新启动听写识别器
            DictationDisplay.text += "Dictation has timed out.";
            dictationRecognizer.Stop();
            DictationDisplay.text += "Dictation restart.";
            dictationRecognizer.Start();
        }
    }

    private void DictationRecognizer_DictationResult(string text, ConfidenceLevel confidence)
    {
        DictationDisplay.text = "result:";
        DictationDisplay.text += text;
    }

    private void DictationRecognizer_DictationHypothesis(string text)
    {
        DictationDisplay.text = "Hypothesis:";
        DictationDisplay.text += text;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnDestroy()
    {
        dictationRecognizer.Stop();
        dictationRecognizer.DictationHypothesis -= DictationRecognizer_DictationHypothesis;
        dictationRecognizer.DictationResult -= DictationRecognizer_DictationResult;
        dictationRecognizer.DictationComplete -= DictationRecognizer_DictationComplete;
        dictationRecognizer.DictationError -= DictationRecognizer_DictationError;
        dictationRecognizer.Dispose();
    }
}
