using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FBDialog : MonoBehaviour
{
    public Text text;
    public Button closeButton;

    private void Awake()
    {
        closeButton.onClick.AddListener(()=>gameObject.SetActive(false));
    }
}
