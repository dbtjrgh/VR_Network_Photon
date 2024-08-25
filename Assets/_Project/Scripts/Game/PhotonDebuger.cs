using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 일반적인 상태에서는 씬에 올라오지 않고, 디버깅을 통해 로비를 건너 뛰고 게임 씬에 들어왔을 경우
// 활성화.
public class PhotonDebuger : MonoBehaviourPunCallbacks
{
    private void Start()
    {
        PhotonNetwork.NickName = $"Tester {Random.Range(1, 10)}";
        PhotonNetwork.ConnectUsingSettings();

    }

    public override void OnConnectedToMaster()
    {
        // 테스트를 위해 접속이 되면 테스트용 룸을 생성
        RoomOptions testRoomOptions = new RoomOptions() {
            IsVisible = false,
            MaxPlayers = 10
        };
        PhotonNetwork.JoinOrCreateRoom("TestRoom", testRoomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        GameObject.Find("Canvas/DebugText").GetComponent<UnityEngine.UI.Text>().text=
            PhotonNetwork.CurrentRoom.Name;
        TestManager.debugReady = true;
    }



}


