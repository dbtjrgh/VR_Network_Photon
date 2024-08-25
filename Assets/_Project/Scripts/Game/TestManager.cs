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
            // ������ �� ���� �� ���� ������ �ǳ� �پ����Ƿ�, �ڵ����� ����׷뿡 �����Ŵ
            StartCoroutine(DebugStart());
        }
    }

    private IEnumerator NormalStart()
    {
        // PhotonNetwork �� ��� �÷��̾��� �ε� ���¸� �Ǵ��Ͽ� �ѹ����� ��
        // �� �ϴµ�, ���� �׷� ����� �����Ǿ� ���������Ƿ�, 1�� ��� �� ���� ���� ���� ������ ������.
        //yield return new WaitForSeconds(1f);

        yield return new WaitUntil(() =>
        PhotonNetwork.LocalPlayer.GetPlayerNumber() != -1);

        /*GameObject PlayerPrefab=Resources.Load<GameObject>("Player");   

        Instantiate(PlayerPrefab, startPositions.GetChild(0).position, Quaternion.identity);*/


        // ���ӿ� ������ �濡�� �ο��� �� ��ȣ.
        // Ȱ���ϱ� ���ؼ��� ���� ���� PlayerNumbering ������Ʈ�� �߰��ؾ���.
        int playerNumber = PhotonNetwork.LocalPlayer.GetPlayerNumber();

        Transform playerPos = startPositions.GetChild(playerNumber);


        GameObject playerObj = PhotonNetwork.Instantiate("Player", playerPos.position, playerPos.rotation);        // ��Ʈ��ũ�� ������ ������Ʈ�� ��ȯ��.

        playerObj.name = $"Player{playerNumber}";     // �̸��� ������.
    }

    public static bool debugReady;


    private IEnumerator DebugStart()
    {
        // ����� ������ Start ����
        gameObject.AddComponent<PhotonDebuger>();

        yield return new WaitUntil(() => debugReady);

        yield return new WaitUntil(()=>
        PhotonNetwork.LocalPlayer.GetPlayerNumber()!=-1);

        StartCoroutine(NormalStart());
    }
}
