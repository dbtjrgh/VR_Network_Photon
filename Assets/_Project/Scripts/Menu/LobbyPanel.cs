using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPanel : MonoBehaviour
{
    public RectTransform roomListRect;
    private List<RoomInfo> currentRoomList = new List<RoomInfo>();
    public Button roomButtonPrefab;
    public Button backButton;
    

    private void Awake()
    {
        backButton.onClick.AddListener(/*()=>PhotonNetwork.LeaveLobby()*/BackButtonClick);
        

    }

    private void OnDisable()
    {
        foreach(Transform child in roomListRect)            //var를 쓸 수 없고 transform으로 언박싱을 해줘야 쓸 수 있다.
        {
            Destroy(child.gameObject);
        }
    }

    public void UpdateRoomList(List<RoomInfo> roomList)
    {
        List<RoomInfo> destroyCandidate/*파괴 될 후보*/=
            currentRoomList.FindAll((x) => false == roomList.Contains(x));

        foreach (RoomInfo roomInfo in roomList)
        {
            if (currentRoomList.Contains(roomInfo))  continue;
            AddRoomButton(roomInfo);


        }

        foreach (Transform child in roomListRect)
        {
            if(destroyCandidate.Exists((x)=>x.Name==child.name))        //destroyCandidate에 있는 방이면 파괴
            {
               Destroy(child.gameObject);
            }

        }

        currentRoomList = roomList;
    }

    public void AddRoomButton(RoomInfo roominfo)
    {
        // RoomInfoList를 통해 순차적으로 한개씩 방 입장 버튼을 생성한다.
        Button joinButton = Instantiate(roomButtonPrefab, roomListRect, false);
        joinButton.gameObject.name = roominfo.Name;
        joinButton.onClick.AddListener(() => JoinButtonClick(roominfo.Name));
        //joinButton.onClick.AddListener(() => PhotonNetwork.JoinRoom(roominfo.Name));      //위랑 같은뜻    
        joinButton.GetComponentInChildren<Text>().text = roominfo.Name;
    }

    private void JoinButtonClick(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    

    private void BackButtonClick()
    {
        PhotonNetwork.LeaveLobby();
    }

    

    

    private void Reset()
    {
        roomListRect=transform.Find("RoomListRect").GetComponent<RectTransform>();
        backButton=transform.Find("BackButton").GetComponent<Button>();
    }
}
