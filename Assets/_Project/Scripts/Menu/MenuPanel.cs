using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuPanel : MonoBehaviour
{
    public Text playerName;

    public InputField playerNameInput;
    public Button playerNameChangeButton;

    // 방 메뉴와 방 생성 메뉴를 구분 해야 함.
    [Header("Main Menu")]
    public RectTransform mainMenuPanel;
    public Button createRoomButton;
    public Button findRoomButton;
    public Button randomRoomButton;
    public Button logoutButton;

    [Header("Create Room Menu")]
    public RectTransform createRoomMenuPanel;
    public InputField roomNameInput;
    public InputField playerNumInput;
    public Button createButton;
    public Button cancelButton;

    private void Awake()
    {
        createRoomButton.onClick.AddListener(CreateRoomButtonClick);
        findRoomButton.onClick.AddListener(FindRoomButtonClick);
        randomRoomButton.onClick.AddListener(RandomRoomButtonClick);
        logoutButton.onClick.AddListener(LogoutButtonClick);
        createButton.onClick.AddListener(CreateButtonClick);
        cancelButton.onClick.AddListener(CancelButtonClick);
        playerNameChangeButton.onClick.AddListener(ChangeNameButton);

    }

    private void OnEnable()
    {
        string userId=FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        DatabaseReference reference=FirebaseDatabase.DefaultInstance.GetReference("users").Child(userId).Child("userName");
        reference.GetValueAsync().ContinueWithOnMainThread(task =>      
        {
            if (task.IsCompleted)   
            {
                DataSnapshot snapshot = task.Result;
                string userName = snapshot.Value.ToString();
                playerName.text = $"환영합니다, {userName}";
            }
        });



        /*playerName.text = $"환영합니다, {FirebaseDatabase.DefaultInstance.GetReference("users").Child("userName")}";*/   // 파이어베이스에서 가져온 닉네임을 표시하려면?

        mainMenuPanel.gameObject.SetActive(true);
        createRoomMenuPanel.gameObject.SetActive(false);
    }

    private void CreateRoomButtonClick()    // 방 생성 버튼
    {
        mainMenuPanel.gameObject.SetActive(false);
        createRoomMenuPanel.gameObject.SetActive(true);
    }


    private void FindRoomButtonClick()      // 방 목록을 받아오기 위해 로비에 입장.
    {
        PhotonNetwork.JoinLobby();
    }

    private void RandomRoomButtonClick()    // 랜덤으로 방에 입장
    {
        RoomOptions option = new()
        {     // 방 생성 옵션
            MaxPlayers = 8          // 방 최대 인원
        };
        string roomName = $"Random Room{Random.Range(100, 1000)}";
        PhotonNetwork.JoinRandomOrCreateRoom(roomOptions: option,roomName: roomName);       // roomoption 파라미터만 디폴트로 넣고 나머지는 랜덤으로 생성


    }

    private void LogoutButtonClick()        // 로그아웃
    {
        mainMenuPanel.gameObject.SetActive(false);
        PhotonNetwork.Disconnect();
    }

    private void CreateButtonClick()
    {
        string roomName = roomNameInput.text;
        int maxPlayer = int.Parse(playerNumInput.text);       // int가 아닐시 오류가남
        /*if(int.TryParse(playerNumInput.text,out maxPlayer))                              // 반환형으로 체크
        {

        }*/
        if (string.IsNullOrEmpty(roomName))
        {
            // 같은 방 번호가 있을 수 있으므로 사실 좀 더 섬세한 유효성 검사가 필요합니다.
            roomName = $"Room{Random.Range(0, 1000)}";
        }

        if (maxPlayer <= 0)
        {
            maxPlayer = 8;
        }

        PhotonNetwork.CreateRoom(roomName, new RoomOptions()
        {
            MaxPlayers = maxPlayer,
        }
        );


    }

    private void ChangeNameButton()
    {
        
            

                PhotonNetwork.NickName = playerNameInput.GetComponentInChildren<InputField>().text;

            

        
    }


    private void CancelButtonClick()      // 방 생성 패널의 취소 버튼
    {
        mainMenuPanel.gameObject.SetActive(true);
        createRoomMenuPanel.gameObject.SetActive(false);
    }
}
