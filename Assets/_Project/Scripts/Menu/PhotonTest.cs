using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotonTest : MonoBehaviour
{
    private ClientState photonState=0;  // cache 용도로 활용하기 때문에 private 으로 막아둔다.

    /*private void Start()
    {
*//*#if UNITY_EDITOR
        PhotonNetwork.ConnectToMaster("",0,"");                    // 직접 파라미터를 넣어줘야함..
#elif MY_PUN_EDITOR*//*
        PhotonNetwork.ConnectUsingSettings();           // 포톤에 설정해놓은대로 접속을 하겠다.
*//*#endif*//*
    }*/

    private void Update()
    {
        if(PhotonNetwork.NetworkClientState!=photonState)           //udp 프로토콜을 통해 접속을 한다.  // 상태가 바뀌었을 때만 로그를 찍어준다.
        {
            LogManager.Log($"state Changed : {PhotonNetwork.NetworkClientState}");
            photonState = PhotonNetwork.NetworkClientState;
        }

    }
}
