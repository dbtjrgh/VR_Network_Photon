using Photon.Pun;
using Photon.Pun.UtilityScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestManager : MonoBehaviour {

	public Transform startPositions;

	private void Start() {

		if (PhotonNetwork.InRoom) {
			StartCoroutine(NormalStart());
		} else {
			//개발중 방 생성 및 참여 절차를 건너 뛰었으므로, 자동으로 디버그룸에 입장시킴
			StartCoroutine(DebugStart());
		}

	}

	private IEnumerator NormalStart() {
		//PhotonNetwork가 모든 플레이어의 로드 상태를 판단하여 넘버링을 해야 하는데, 현재 그런 모듈이 구현되어있지 않으므로, 1초 대기 후 게임 시작 절차 수행
		//yield return new WaitForSeconds(1f);
		yield return new WaitUntil(() =>
			PhotonNetwork.LocalPlayer.GetPlayerNumber() != -1);

		//GameObject playerPrefab = 
		//	Resources.Load<GameObject>("Players/Player");
		//Instantiate(playerPrefab, startPositions.GetChild(0).position, Quaternion.identity);

		//게임에 참여한 방에서 부여된 내 번호.
		//활용하기 위해서는 게임 씬에 PlayerNumbering 컴포넌트가 존재해야함.
		int playerNumber = PhotonNetwork.LocalPlayer.GetPlayerNumber();
		print($"player num : {playerNumber}");

		Transform playerPos = startPositions.GetChild(playerNumber);

		GameObject playerObj = PhotonNetwork.Instantiate("Players/Player", playerPos.position, playerPos.rotation);

		playerObj.name = $"Player {playerNumber}";
	}

	public static bool debugReady = false;

	private IEnumerator DebugStart() {
		//디버그 상태의 Start 절차
		gameObject.AddComponent<PhotonDebuger>();

		yield return new WaitUntil(()=>debugReady);

		StartCoroutine(NormalStart());

	}

}
