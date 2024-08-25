using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogManager : MonoBehaviour
{
    public static LogManager Instance { get; private set; }

    public RectTransform logContent;
    public Text logText;

    private void Awake()
    {
        Instance = this;
    }

    public static void Log(string message)
    {
        if(Instance!=null)
        {
            Text logText = Instantiate(Instance.logText, Instance.logContent, false);   // instantiate prefab
            logText.text = message;
        }
        else
        {
            print(message);
        }
    }
}
