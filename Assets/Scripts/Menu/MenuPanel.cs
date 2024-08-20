using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

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
        playerNameChangeButton.onClick.AddListener(PlayerNameChangeButtonClick);
    }

    private void OnEnable()
    {
        playerName.text = $"�ȳ��ϼ���, {PhotonNetwork.LocalPlayer.NickName}";
        mainMenuPanel.gameObject.SetActive(true);
        createRoomMenuPanel.gameObject.SetActive(false);
    }

    private void CreateRoomButtonClick()
    {
        // �� ���� ��ư
        mainMenuPanel.gameObject.SetActive(false);
        createRoomMenuPanel.gameObject.SetActive(true);
    }

    private void FindRoomButtonClick()
    {
        // �� ����� �޾ƿ��� ���� �κ� ���� ��ư
        PhotonNetwork.JoinLobby();
    }

    private void RandomRoomButtonClick()
    {
        RoomOptions option = new()
        {
            MaxPlayers = 10,
        };
        PhotonNetwork.JoinRandomOrCreateRoom(roomOptions: option);
    }

    private void LogoutButtonClick()
    {
        // �α׾ƿ� ��ư
        mainMenuPanel.gameObject.SetActive(false);
        PhotonNetwork.Disconnect();
    }

    private void CreateButtonClick()
    {
        string roomName = roomNameInput.text;
        int maxPlayer = int.Parse(playerNumInput.text);
        if (string.IsNullOrEmpty(roomName))
        {
            // ���� �� ��ȣ�� ���� �� �����Ƿ� �� �� ������ ��ȿ�� �˻簡 �ʿ���.
            roomName = $"Room {Random.Range(0, 1000)}";
        }

        if (maxPlayer <= 0)
        {
            maxPlayer = 10;
        }

        PhotonNetwork.CreateRoom(roomName, new RoomOptions() { MaxPlayers = maxPlayer });
        {
            
        }
        //if (int.TryParse(playerNumInput.text, out maxPlayer))
        //{

        //}
    }

    private void CancelButtonClick()
    {
        // �� ���� �г��� ��� ��ư
        mainMenuPanel.gameObject.SetActive(true);
        createRoomMenuPanel.gameObject.SetActive(false);
    }

    private void PlayerNameChangeButtonClick()
    {
        PhotonNetwork.LocalPlayer.NickName = playerNameInput.text;
        PhotonNetwork.ConnectUsingSettings();
        playerName.text = $"�ȳ��ϼ���, {PhotonNetwork.LocalPlayer.NickName}";

    }

}
