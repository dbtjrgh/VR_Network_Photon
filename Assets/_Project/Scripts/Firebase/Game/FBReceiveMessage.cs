using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FBReceiveMessage : MonoBehaviour
{
    public Text msgText;


    /*private void Start()
    {
        FirebaseManager.Instance.onReceiveMessage += OnReceiveMessage;  // 이벤트 등록
        

    }*/

    // 나에게 누군가 메시지를 보냈을 때 호출
    public void OnReceiveMessage(string msg)
    {
        msgText.text=msg;
        gameObject.SetActive(true);
    }

    
}
