using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomPanel : MonoBehaviour
{
    public Text roomTitleText;
    public RectTransform playerList;
    public GameObject playerTextPrefab;

    public Button startButton;
    public Button cancelButton;

    private void Awake()
    {
        startButton.onClick.AddListener(StartButtonClick);
        cancelButton.onClick.AddListener(CancelButtonClick);
    }
    private void OnDisable()
    {
        foreach (Transform child in playerList)
        {
            Destroy(child.gameObject);
        }
    }

    private void OnEnable()
    {
        roomTitleText.text = PhotonNetwork.CurrentRoom.Name;
        foreach(Player player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            // �÷��̾� ��Ͽ� �÷��̾� �̸�ǥ �ϳ��� ����
            JoinPlayer(player);

        }
    }
    
    public void JoinPlayer(Player newPlayer)
    {
        GameObject playername = Instantiate(playerTextPrefab, playerList, false);
        playername.name = newPlayer.NickName;
        playername.GetComponent<Text>().text = newPlayer.NickName;
    }

    public void LeavePlayer(Player gonePlayer)
    {
        GameObject leaveTarget = playerList.Find(gonePlayer.NickName).gameObject;

        Destroy(leaveTarget);
    }

    private void StartButtonClick()
    {
        // ���� ���� ��ư

    }

    private void CancelButtonClick()
    {
        // ���� ������ ��ư
        PhotonNetwork.LeaveLobby();
        Debug.Log("������");
    }

}
