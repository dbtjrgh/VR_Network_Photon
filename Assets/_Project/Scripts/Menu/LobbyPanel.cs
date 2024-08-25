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
        foreach(Transform child in roomListRect)            //var�� �� �� ���� transform���� ��ڽ��� ����� �� �� �ִ�.
        {
            Destroy(child.gameObject);
        }
    }

    public void UpdateRoomList(List<RoomInfo> roomList)
    {
        List<RoomInfo> destroyCandidate/*�ı� �� �ĺ�*/=
            currentRoomList.FindAll((x) => false == roomList.Contains(x));

        foreach (RoomInfo roomInfo in roomList)
        {
            if (currentRoomList.Contains(roomInfo))  continue;
            AddRoomButton(roomInfo);


        }

        foreach (Transform child in roomListRect)
        {
            if(destroyCandidate.Exists((x)=>x.Name==child.name))        //destroyCandidate�� �ִ� ���̸� �ı�
            {
               Destroy(child.gameObject);
            }

        }

        currentRoomList = roomList;
    }

    public void AddRoomButton(RoomInfo roominfo)
    {
        // RoomInfoList�� ���� ���������� �Ѱ��� �� ���� ��ư�� �����Ѵ�.
        Button joinButton = Instantiate(roomButtonPrefab, roomListRect, false);
        joinButton.gameObject.name = roominfo.Name;
        joinButton.onClick.AddListener(() => JoinButtonClick(roominfo.Name));
        //joinButton.onClick.AddListener(() => PhotonNetwork.JoinRoom(roominfo.Name));      //���� ������    
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
