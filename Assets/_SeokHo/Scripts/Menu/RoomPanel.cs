using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using Hashtable = ExitGames.Client.Photon.Hashtable;

public enum Difficulty {
	Easy,
	Normal,
	Hard
}

public class RoomPanel : MonoBehaviourPunCallbacks {

	public Text roomTitleText;
	public RectTransform playerList;
	public GameObject playerTextPrefab;

	public Button startButton;
	public Button cancelButton;
	
	public Dropdown diffDropdown;
	public Text diffText;

	//������ ���, �÷��̾���� ready ���¸� ������ dictionary
	private Dictionary<int, bool> playersReady;
	//�濡 ���� ��� �÷��̾���� ���θ� �˰� �ֵ��� ����� dictionary
	public Dictionary<int, PlayerEntry> playerEntries = new();

	private void Awake() {
		startButton.onClick.AddListener(StartButtonClick);
		cancelButton.onClick.AddListener(CancelButtonClick);
		
		diffDropdown.ClearOptions();

		foreach (object diff in Enum.GetValues(typeof(Difficulty))) {
			Dropdown.OptionData option = new Dropdown.OptionData(diff.ToString());
			diffDropdown.options.Add(option);
		}
		diffDropdown.onValueChanged.AddListener(DifficultyValueChange);
	}

	public override void OnDisable() {
		base.OnDisable();

		foreach (Transform child in playerList) {
			Destroy(child.gameObject);
		}
	}

	public override void OnEnable() {
		base.OnEnable();

		roomTitleText.text = PhotonNetwork.CurrentRoom.Name;

		if (PhotonNetwork.IsMasterClient) {
			//������ ����
			playersReady = new Dictionary<int, bool>();
		} else {
			//������ �ƴ� ����
		}

		//������ �ƴϸ� ���� ���� ��ư �� ���̵� ������ ��Ȱ��ȭ
		startButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
		diffDropdown.gameObject.SetActive(PhotonNetwork.IsMasterClient);
		diffText.gameObject.SetActive(false == PhotonNetwork.IsMasterClient);

		foreach (Player player in PhotonNetwork.CurrentRoom.Players.Values) {
			//�÷��̾� ��Ͽ� �÷��̾� �̸�ǥ �ϳ��� ����
			JoinPlayer(player);

			if (player.CustomProperties.ContainsKey("Ready")) {
				SetPlayerReady(player.ActorNumber, (bool)player.CustomProperties["Ready"]);
			}
		}

	

		
		//�濡 ���� ������, ������ �� �ε� ���ο� ���� �Բ� �� �ε�
		PhotonNetwork.AutomaticallySyncScene = true;


	}

	public void JoinPlayer(Player newPlayer) {

		var playerEntry = Instantiate(playerTextPrefab, playerList, false).GetComponent<PlayerEntry>();

		playerEntry.player = newPlayer;
		playerEntry.playerNameText.text = newPlayer.NickName;

		var toggle = playerEntry.readyToggle;

		if(PhotonNetwork.LocalPlayer.ActorNumber == newPlayer.ActorNumber) {
			//TODO:�� ��Ʈ���� ��쿡�� ����� onValueChanged�� �̺�Ʈ �ڵ鸵.
			toggle.onValueChanged.AddListener(ReadyToggleClick);
		} else {
			//���� �ƴ� �ٸ� �÷��̾��� ��Ʈ��
			toggle.gameObject.SetActive(false);
		}

		playerEntries[newPlayer.ActorNumber] = playerEntry;

		if (PhotonNetwork.IsMasterClient) {
			playersReady[newPlayer.ActorNumber] = false;
			CheckReady();
		}

		SortPlayers();
	}

	public void LeavePlayer(Player gonePlayer) {
		GameObject leaveTarget = playerEntries[gonePlayer.ActorNumber].gameObject;
		playerEntries.Remove(gonePlayer.ActorNumber);
		Destroy(leaveTarget);
		if (PhotonNetwork.IsMasterClient) {
			playersReady.Remove(gonePlayer.ActorNumber);
			CheckReady();
		}
		
		SortPlayers();
	}

	public void SortPlayers() {
		foreach (int actorNumber in playerEntries.Keys) {

			playerEntries[actorNumber].transform.SetSiblingIndex(actorNumber);
			//SetSiblingIndex => Hierachy�� �� �θ� �ȿ��� �ٸ� ��ü �� ������ �����ϰ� ������
		}
	}

	private void StartButtonClick() {
		//���� ���� ��ư
		//������ �� �ε� ���
		//SceneManager.LoadScene("GameScene");

		if (PhotonNetwork.IsMasterClient) {
			//Photon�� ���� �÷��̾��� ���� ����ȭ �Ͽ� �ε�
			PhotonNetwork.LoadLevel("GameScene");
		}
	}

	private void CancelButtonClick() {
		PhotonNetwork.LeaveRoom();
		//�ð� �������� ���� ���� �����Ͽ��µ� ������ ���� �ݿ� ���� ���� �Ѿ���°��� ����
		PhotonNetwork.AutomaticallySyncScene = false;
	}

	//�� Ready ���°� ����� �� Custom Properties ����
	public void ReadyToggleClick(bool isOn) {

		Player localPlayer = PhotonNetwork.LocalPlayer;

		//Photonnetwork�� customPropertie�� Hashtable ������ Ȱ��
		//�׷��� dotnet�� Hashtable�� �ƴ� ����ȭ ������ Hashtable Ŭ������ ���� ����
		Hashtable customProps = localPlayer.CustomProperties;

		customProps["Ready"] = isOn;

		localPlayer.SetCustomProperties(customProps);

	}

	//�ٸ� �÷��̾ ReadyToggle�� �������� ��� �� Ŭ���̾�Ʈ���� �ݿ�
	public void SetPlayerReady(int actorNumber, bool isReady) {
		playerEntries[actorNumber].readyLabel.gameObject.SetActive(isReady);
		if (PhotonNetwork.IsMasterClient) {
			playersReady[actorNumber] = isReady;
			CheckReady();
		}

	}

	//������ ��쿡 �ٸ� �÷��̾���� ��� ready �������� Ȯ�� �Ͽ� 
	//Start ��ư�� Ȱ��ȭ ���θ� ����
	private void CheckReady() {
		//���� ����� �Ѱ��� false�̸� false���� �Ҷ�.
		//�� ��� ��Ұ� && ������ �ؾ� �� ��.

		bool allReady = playersReady.Values.All(x=>x);
		bool anyReady = playersReady.Values.Any(x=>x);

		//bool allReady = true;//�ʱ� ���´� true;

		////5���� �÷��̾��� 3��° �÷��̾��� isReady �� false, ������ true => false

		//foreach(bool isReady in playersReady.Values) {
		//	if (isReady) {
		//		continue;
		//	} else {
		//		allReady = false;
		//		break;
		//	}
		//}

		startButton.interactable = allReady;

	}

	private void CheckNotReady() {
		bool anyReady = false;
		foreach (bool isReady in playersReady.Values) {
			if (isReady) {
				anyReady = true; break;
			}
		}
	}

	private void DifficultyValueChange(int value) {
		if (false == PhotonNetwork.IsMasterClient) return;

		var customProps = PhotonNetwork.CurrentRoom.CustomProperties;
		customProps["Diff"] = value;
		PhotonNetwork.CurrentRoom.SetCustomProperties(customProps);
	}

	public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps) {

		//print($"Ŀ���� ������Ƽ ����ƽ��ϴ� : {PhotonNetwork.Time}");

		if (changedProps.ContainsKey("Ready")) {
			SetPlayerReady(targetPlayer.ActorNumber, (bool)changedProps["Ready"]);
		} 

	}

	public override void OnRoomPropertiesUpdate(Hashtable props) {

		if (props.ContainsKey("Diff")) {
			print($"room difficulty changed : {props["Diff"]}");
			diffText.text = ((Difficulty)props["Diff"]).ToString();
		}
	}

	public override void OnJoinedRoom() {

		var props = PhotonNetwork.CurrentRoom.CustomProperties;

		if (props.ContainsKey("Diff")) {
			print($"room difficulty changed : {props["Diff"]}");
			diffText.text = ((Difficulty)props["Diff"]).ToString();
		}
	}

	public override void OnMasterClientSwitched(Player newMasterClient) {
		//������ �������� ȣ�� �ǹǷ�, �濡 �����Ǿ��ִ� ���¿��� ������ ������ ������ �� �ֵ���
		//��ȿ�� �˻� �� �߰� ��ġ�� �� �ʿ䰡 ����
		//��) playersReady�� Dictionary��ü ���� ���


	}
}
