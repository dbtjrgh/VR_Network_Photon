using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotonTest : MonoBehaviour
{
    // chche �뵵�� Ȱ���ϱ� ������ private���� ���Ƶд�.
    private ClientState photonState = 0;

    private void Start()
    {
// #if UNITY_EDITOR
        // PhotonNetwork.ConnectToMaster("",0,"");

// #elif MY_PUN_EDITOR

        // Photon ���� ������ ������ ���� �״�� ����� �� �ִ�.
        // PhotonNetwork.ConnectUsingSettings();

// #endif


    }

    private void Update()
    {
        // ��ü������ �̱��� �����Ǿ� ����.
        if(PhotonNetwork.NetworkClientState != photonState)
        {
            LogManager.Log($"state changed : {PhotonNetwork.NetworkClientState}");
            photonState = PhotonNetwork.NetworkClientState;
        }
    }

}
