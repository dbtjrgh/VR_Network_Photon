using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPanel : MonoBehaviour {
	public RectTransform roomListRect;
	private List<RoomInfo> currentRoomList = new List<RoomInfo> ();
	public Button roomButtonPrefab;
	public Button backButton;

	private void Awake() {
		backButton.onClick.AddListener(BackButtonClick);
	}

	private void OnDisable() {
		foreach (Transform child in roomListRect) {
			Destroy (child.gameObject);
		}
	}

	public void UpdateRoomList(List<RoomInfo> roomList) {
		List<RoomInfo> destroyCandidate /*파괴될 후보*/ = 
			currentRoomList.FindAll((x)=>false == roomList.Contains(x));

		foreach (RoomInfo roomInfo in roomList) {
			if(currentRoomList.Contains(roomInfo)) continue;
			AddRoomButton(roomInfo);
		}

		foreach (Transform child in roomListRect) {
			if (destroyCandidate.Exists((x) => x.Name == child.name)) 
				Destroy(child.gameObject);
		}

		currentRoomList = roomList;

	}

	public void AddRoomButton(RoomInfo roomInfo) { 
		//RoomInfoList를 통해 순차적으로 한개씩 방 입장 버튼을 생성
		Button joinButton = Instantiate(roomButtonPrefab, roomListRect, false);
		joinButton.gameObject.name = roomInfo.Name;
		joinButton.onClick.AddListener(() => PhotonNetwork.JoinRoom(roomInfo.Name));
		joinButton.GetComponentInChildren<Text>().text = roomInfo.Name;
	}

	private void BackButtonClick() { 
		PhotonNetwork.LeaveLobby();
	}

	private void Reset() {
		roomListRect = transform.Find("RoomListRect").GetComponent<RectTransform>();
		backButton = transform.Find("BackButton").GetComponent<Button>();
	}

}
