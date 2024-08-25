using Photon.Pun;
using Photon.Pun.UtilityScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestManager : MonoBehaviour
{

    public Transform startPositions;


    private void Start()
    {
        if(PhotonNetwork.InRoom)
        {
            StartCoroutine(NormalStart());
        }
        else
        {
            // 개발중 방 생성 및 참여 절차를 건너 뛰었으므로, 자동으로 디버그룸에 입장시킴
            StartCoroutine(DebugStart());
        }
    }

    private IEnumerator NormalStart()
    {
        // PhotonNetwork 가 모든 플레이어의 로드 상태를 판단하여 넘버링을 해
        // 야 하는데, 현재 그런 모듈이 구현되어 있지않으므로, 1초 대기 후 게임 시작 수행 절차를 진행함.
        //yield return new WaitForSeconds(1f);

        yield return new WaitUntil(() =>
        PhotonNetwork.LocalPlayer.GetPlayerNumber() != -1);

        /*GameObject PlayerPrefab=Resources.Load<GameObject>("Player");   

        Instantiate(PlayerPrefab, startPositions.GetChild(0).position, Quaternion.identity);*/


        // 게임에 참여한 방에서 부여된 내 번호.
        // 활용하기 위해서는 게임 씬에 PlayerNumbering 컴포넌트를 추가해야함.
        int playerNumber = PhotonNetwork.LocalPlayer.GetPlayerNumber();

        Transform playerPos = startPositions.GetChild(playerNumber);


        GameObject playerObj = PhotonNetwork.Instantiate("Player", playerPos.position, playerPos.rotation);        // 네트워크상에 생성된 오브젝트를 반환함.

        playerObj.name = $"Player{playerNumber}";     // 이름을 설정함.
    }

    public static bool debugReady;


    private IEnumerator DebugStart()
    {
        // 디버그 상태의 Start 절차
        gameObject.AddComponent<PhotonDebuger>();

        yield return new WaitUntil(() => debugReady);

        yield return new WaitUntil(()=>
        PhotonNetwork.LocalPlayer.GetPlayerNumber()!=-1);

        StartCoroutine(NormalStart());
    }
}
