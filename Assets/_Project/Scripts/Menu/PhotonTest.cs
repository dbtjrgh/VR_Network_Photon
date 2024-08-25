using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotonTest : MonoBehaviour
{
    private ClientState photonState=0;  // cache �뵵�� Ȱ���ϱ� ������ private ���� ���Ƶд�.

    /*private void Start()
    {
*//*#if UNITY_EDITOR
        PhotonNetwork.ConnectToMaster("",0,"");                    // ���� �Ķ���͸� �־������..
#elif MY_PUN_EDITOR*//*
        PhotonNetwork.ConnectUsingSettings();           // ���濡 �����س������ ������ �ϰڴ�.
*//*#endif*//*
    }*/

    private void Update()
    {
        if(PhotonNetwork.NetworkClientState!=photonState)           //udp ���������� ���� ������ �Ѵ�.  // ���°� �ٲ���� ���� �α׸� ����ش�.
        {
            LogManager.Log($"state Changed : {PhotonNetwork.NetworkClientState}");
            photonState = PhotonNetwork.NetworkClientState;
        }

    }
}
