using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;

using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public enum Difficulty
{
    Easy,
    Normal,
    Hard
}


public class RoomPanel : MonoBehaviourPunCallbacks
{
    #region ����
    public Text roomTitleText;
    public RectTransform playerList;
    public GameObject playerTextPrefab;

    public Button startButton;
    public Button cancelButton;
    public Dropdown diffDropdown;
    public Text diffText;

    // ������ ���, �÷��̾���� ready ���¸� ������ dictionary
    private Dictionary<int, bool> playersReady;

    // �濡 ���� ��� �÷��̾���� ���θ� �˰� �ֵ��� ����� dictionary
    public Dictionary<int, PlayerEntry> playerEntries = new();
    #endregion


    private void Awake()
    {
        startButton.onClick.AddListener(StartButtonClick);
        cancelButton.onClick.AddListener(CancelButtonClick);
        diffDropdown.ClearOptions();

        foreach(object diff in Enum.GetValues(typeof(Difficulty)))
        {
            Dropdown.OptionData option = new Dropdown.OptionData(diff.ToString());
            diffDropdown.options.Add(option);
        }

        diffDropdown.onValueChanged.AddListener(DifficultyValueChange);
    }
    public override void OnDisable()
    {
        base.OnDisable();

        foreach (Transform child in playerList)
        {
            Destroy(child.gameObject);
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        roomTitleText.text = PhotonNetwork.CurrentRoom.Name;

        if (PhotonNetwork.IsMasterClient)
        {
            // ������ ����
            playersReady = new Dictionary<int, bool>();

        }
        else
        {
            // ������ �ƴ� ����

        }

        // ������ �ƴϸ� ���� ���� ��ư �� ���̵� ������ ��Ȱ��ȭ
        startButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        diffDropdown.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        diffText.gameObject.SetActive(false == PhotonNetwork.IsMasterClient);

        foreach (Player player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            // �÷��̾� ��Ͽ� �÷��̾� �̸�ǥ �ϳ��� ����
            JoinPlayer(player);
            if (player.CustomProperties.ContainsKey("Ready"))
            {
                SetPlayerReady(player.ActorNumber, (bool)player.CustomProperties["Ready"]);
            }
        }
        // �濡 ���� ���� ��, ������ �� �ε� ���ο� ���� �Բ� �� �ε�
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    
    public void JoinPlayer(Player newPlayer)
    {
        //GameObject playername = Instantiate(playerTextPrefab, playerList, false);
        //playername.name = newPlayer.NickName;
        //playername.GetComponent<Text>().text = newPlayer.NickName;

        var playerEntry = Instantiate(playerTextPrefab, playerList, false).GetComponent<PlayerEntry>();
        playerEntry.player = newPlayer;
        playerEntry.playerNameText.text = newPlayer.NickName;

        var toggle = playerEntry.readyToggle;

        if (PhotonNetwork.LocalPlayer.ActorNumber == newPlayer.ActorNumber)
        {
            // TODO : �� ��Ʈ���� ��쿣 ����� OnValueChanged�� �̺�Ʈ �ڵ鸵
            toggle.onValueChanged.AddListener(ReadyToggleClick);
        }
        else
        {
            // ���� �ƴ� �ٸ� �÷��̾��� ��Ʈ��
            toggle.gameObject.SetActive(false);
        }

        playerEntries[newPlayer.ActorNumber] = playerEntry;

        if (PhotonNetwork.IsMasterClient)
        {
            playersReady[newPlayer.ActorNumber] = false;
            CheckReady();
        }

        SortPlayers();
    }

    public void LeavePlayer(Player gonePlayer)
    {
        GameObject leaveTarget = playerEntries[gonePlayer.ActorNumber].gameObject;
        playerEntries.Remove(gonePlayer.ActorNumber);
        Destroy(leaveTarget);

        if(PhotonNetwork.IsMasterClient)
        {
            playersReady.Remove(gonePlayer.ActorNumber);
            CheckReady();
        }

        SortPlayers();
    }

    public void SortPlayers()
    {
        foreach(int actorNumber in playerEntries.Keys)
        {
            // SetSiblingIndex => Hierachy�� �� �θ� �ȿ��� �ٸ� ��ü �� ������ �����ϰ� ���� ��
            playerEntries[actorNumber].transform.SetSiblingIndex(actorNumber);
        }
    }

    private void StartButtonClick()
    {
        // ���� ���� ��ư
        // ������ �� �ε� ���
        // SceneManager.LoadScene("GameScene");

        // Photon�� ���� �÷��̾��� ���� ����ȭ �Ͽ� �ε�
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("GameScene");
        }
    }

    private void CancelButtonClick()
    {
        // ���� ������ ��ư
        PhotonNetwork.LeaveRoom();
        // �ð� �������� ���� ���� �����Ͽ��µ� ������ ���� �ݿ� ���� ���� �Ѿ���� ���� ����
        PhotonNetwork.AutomaticallySyncScene = false;
        
        Debug.Log("������");
    }

    // �� Ready ���°� ���� �� �� Custom Properties ����
    public void ReadyToggleClick(bool isOn)
    {
        Player localPlayer = PhotonNetwork.LocalPlayer;

        // PhotonNetwork�� CustomPropertie�� Hashtable ������ Ȱ��
        // �׷��� dotnet�� Hashtable�� �ƴ� ����ȭ ������ Hashtable Ŭ������ ���� ����
        PhotonHashtable customProps = localPlayer.CustomProperties;

        customProps["Ready"] = isOn;

        localPlayer.SetCustomProperties(customProps);
    }

    // �ٸ� �÷��̾ ReadyToggle�� �������� ��� �� Ŭ���̾�Ʈ���� �ݿ�
    public void SetPlayerReady(int actorNumber, bool isReady)
    {
        playerEntries[actorNumber].readyLabel.gameObject.SetActive(isReady);
        if(PhotonNetwork.IsMasterClient)
        {
            playersReady[actorNumber] = isReady;
            CheckReady();
        }
    }

    // ������ ��쿡 �ٸ� �÷��̾���� ��� ready �������� Ȯ��
    // Start ��ư�� Ȱ��ȭ ���θ� ����
    private void CheckReady()
    {
        // ���� ��� �� �Ѱ��� false�̸� false���� �� ��
        // �� ��� ��Ұ� && ������ �ؾ� �� ��

        // �ʱ� ���´� true;
        //bool allReady = true;

        //// Ex : 5���� �÷��̾��� 3��° �÷��̾��� isReady�� false, ������ true���� allReady�� false
        //foreach(bool isReady in playersReady.Values)
        //{
        //    if(isReady)
        //    {
        //        continue;
        //    }
        //    else
        //    {
        //        allReady = false;
        //        break;
        //    }
        //}

        // ���� true���� true�� �ȴ�. (AND)
        bool allReady = playersReady.Values.All(x=>x);
        // �ϳ��� true�� true�� �ȴ�. (OR)
        bool anyReady = playersReady.Values.Any(x=>x);

        startButton.interactable = allReady;
    }

    private void DifficultyValueChange(int value)
    {
        if(!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        var customProps = PhotonNetwork.CurrentRoom.CustomProperties;
        customProps["Diff"] = value;
        PhotonNetwork.CurrentRoom.SetCustomProperties(customProps);
    }


    public override void OnPlayerPropertiesUpdate(Player targetPlayer, PhotonHashtable changedProps)
    {
        // print($"Ŀ���� ������Ƽ ����ƽ��ϴ�. : {PhotonNetwork.Time}");
        if (changedProps.ContainsKey("Ready"))
        {
            SetPlayerReady(targetPlayer.ActorNumber, (bool)changedProps["Ready"]);
        }
    }

    public override void OnRoomPropertiesUpdate(PhotonHashtable props)
    {
        if(props.ContainsKey("Diff"))
        {
            print($"room difficulty changed : {props["Diff"]}");
            diffText.text = ((Difficulty)props["Diff"]).ToString();
        }
    }

    public override void OnJoinedRoom()
    {
        var props = PhotonNetwork.CurrentRoom.CustomProperties;

        if (props.ContainsKey("Diff"))
        {
            print($"room difficulty changed : {props["Diff"]}");
            diffText.text = ((Difficulty)props["Diff"]).ToString();
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        // ������ ������ �� ȣ��Ǵ� �Լ���, �濡 �����Ǿ��ִ� ���¿��� ������ ������
        // ���� �� �� �ֵ��� ��ȿ�� �˻� �� �߰� ��ġ�� �� �ʿ䰡 ����
        // Ex : playerReady�� Dictionary��ü ���� ���
    }
}
