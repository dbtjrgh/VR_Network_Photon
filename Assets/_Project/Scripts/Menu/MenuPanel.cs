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

    // �� �޴��� �� ���� �޴��� ���� �ؾ� ��.
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
                playerName.text = $"ȯ���մϴ�, {userName}";
            }
        });



        /*playerName.text = $"ȯ���մϴ�, {FirebaseDatabase.DefaultInstance.GetReference("users").Child("userName")}";*/   // ���̾�̽����� ������ �г����� ǥ���Ϸ���?

        mainMenuPanel.gameObject.SetActive(true);
        createRoomMenuPanel.gameObject.SetActive(false);
    }

    private void CreateRoomButtonClick()    // �� ���� ��ư
    {
        mainMenuPanel.gameObject.SetActive(false);
        createRoomMenuPanel.gameObject.SetActive(true);
    }


    private void FindRoomButtonClick()      // �� ����� �޾ƿ��� ���� �κ� ����.
    {
        PhotonNetwork.JoinLobby();
    }

    private void RandomRoomButtonClick()    // �������� �濡 ����
    {
        RoomOptions option = new()
        {     // �� ���� �ɼ�
            MaxPlayers = 8          // �� �ִ� �ο�
        };
        string roomName = $"Random Room{Random.Range(100, 1000)}";
        PhotonNetwork.JoinRandomOrCreateRoom(roomOptions: option,roomName: roomName);       // roomoption �Ķ���͸� ����Ʈ�� �ְ� �������� �������� ����


    }

    private void LogoutButtonClick()        // �α׾ƿ�
    {
        mainMenuPanel.gameObject.SetActive(false);
        PhotonNetwork.Disconnect();
    }

    private void CreateButtonClick()
    {
        string roomName = roomNameInput.text;
        int maxPlayer = int.Parse(playerNumInput.text);       // int�� �ƴҽ� ��������
        /*if(int.TryParse(playerNumInput.text,out maxPlayer))                              // ��ȯ������ üũ
        {

        }*/
        if (string.IsNullOrEmpty(roomName))
        {
            // ���� �� ��ȣ�� ���� �� �����Ƿ� ��� �� �� ������ ��ȿ�� �˻簡 �ʿ��մϴ�.
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


    private void CancelButtonClick()      // �� ���� �г��� ��� ��ư
    {
        mainMenuPanel.gameObject.SetActive(true);
        createRoomMenuPanel.gameObject.SetActive(false);
    }
}
