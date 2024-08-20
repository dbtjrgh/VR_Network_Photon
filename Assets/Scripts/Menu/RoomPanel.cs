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
            // 플레이어 목록에 플레이어 이름표 하나씩 생성
            JoinPlayer(player);

        }

        // 방장이 아니면 게임 시작 버튼을 비활성화
        startButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);

        //if(PhotonNetwork.IsMasterClient)
        //{
        //    // 방장인 상태

        //}
        //else
        //{
        //    // 방장이 아닌 상태

        //}

        // 방에 입장 했을 때, 방장의 씬 로드 여부에 따라 함께 씬 로드
        PhotonNetwork.AutomaticallySyncScene = true;

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
        // 게임 시작 버튼
        // 기존의 씬 로드 방식
        // SceneManager.LoadScene("GameScene");

        // Photon을 통해 플레이어들과 씬을 동기화 하여 로드
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("GameScene");
        }
    }

    private void CancelButtonClick()
    {
        // 게임 나가기 버튼
        PhotonNetwork.LeaveRoom();
        // 시간 지연으로 인해 방을 퇴장하였는데 방장의 시작 콜에 의해 씬이 넘어가지는 것을 방지
        PhotonNetwork.AutomaticallySyncScene = false;
        
        Debug.Log("나가기");
    }

}
