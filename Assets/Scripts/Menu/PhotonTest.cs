using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotonTest : MonoBehaviour
{
    // chche 용도로 활용하기 때문에 private으로 막아둔다.
    private ClientState photonState = 0;

    private void Start()
    {
// #if UNITY_EDITOR
        // PhotonNetwork.ConnectToMaster("",0,"");

// #elif MY_PUN_EDITOR

        // Photon 서버 세팅이 설정된 것을 그대로 사용할 수 있다.
        // PhotonNetwork.ConnectUsingSettings();

// #endif


    }

    private void Update()
    {
        // 자체적으로 싱글톤 구현되어 있음.
        if(PhotonNetwork.NetworkClientState != photonState)
        {
            LogManager.Log($"state changed : {PhotonNetwork.NetworkClientState}");
            photonState = PhotonNetwork.NetworkClientState;
        }
    }

}
