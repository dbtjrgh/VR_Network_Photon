using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using System.Linq;
using System;


using Hashtable = ExitGames.Client.Photon.Hashtable;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;


public enum Difficulty
{
    Easy,
    Normal,
    Hard
}

public enum CharacterType
{
    Glasses,
    Eyes,
    Lens
}


public class RoomPanel : MonoBehaviourPunCallbacks
{
    public Text roomTitleText;
    public RectTransform playerList;
    public GameObject playerTextPrefab;

    public Button startButton;
    public Button cancelButton;
    public Dropdown diffDropdown;
    public Text diffText;
    public Toggle[] EyesToggle;


    // ������ ���, �÷��̾���� ready ���¸� ������ dictionary

    private Dictionary<int, bool> playersReady;
    // �濡 ���� ��� �÷��̵��� ���θ� �˰� �ֵ��� ����� dictionary

    public Dictionary<int, PlayerEntry> playerEntries = new();      // ���׸��� ���� ������ ����  �ؽ����̺��� object���·� ���⶧���� ��ڽ����� �� ����
    public PlayerController playercontroller;



    private void Awake()
    {
        startButton.onClick.AddListener(StartButtonClick);
        cancelButton.onClick.AddListener(CancelButtonClick);
        diffDropdown.ClearOptions();

        foreach (object diff in Enum.GetValues(typeof(Difficulty)))
        {
            Dropdown.OptionData option = new Dropdown.OptionData(diff.ToString());
            diffDropdown.options.Add(option);       
        }
        diffDropdown.onValueChanged.AddListener(DifficultyValueChange);
        playercontroller = GetComponent<PlayerController>();
    }


    public override void OnDisable()        // ���� ���� ��, ������ ������ ��, ���� �������� ��
    {
        base.OnDisable();
        foreach (Transform child in playerList) 
        {
            Destroy(child.gameObject);
        }
    }

    public override void OnEnable()     // �濡 �������� ��, ������ �Ǿ��� ��
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

        // ������ �ƴϸ� ���ӽ��۹�ư �� ���̵� ������ ��Ȱ��ȭ
        startButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        diffDropdown.gameObject.SetActive(PhotonNetwork.IsMasterClient);

        diffText.gameObject.SetActive(false == PhotonNetwork.IsMasterClient);

        foreach (Player player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            // �÷��̾� ��Ͽ� �÷��̾� �̸�ǥ �ϳ��� ����
            JoinPlayer(player);

            if (player.CustomProperties.ContainsKey("Ready"))   //Ready��� Ű���� �ִ��� Ȯ��
                                                                //Ű������� Ȯ��?  //�÷��̾��� Ŀ���� ������Ƽ�� �ִ��� Ȯ��
                                                                //Ŀ����������Ƽ�� ��� �ִ���? //�÷��̾ ����
                                                                
            {
                SetPlayerReady(player.ActorNumber, (bool)player.CustomProperties["Ready"]);
            }

        }

        // ������ �ƴϸ� ���� ���� ��ư�� ��Ȱ��ȭ.
        startButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);




        PhotonNetwork.AutomaticallySyncScene = true;        // ������ ���� ���� ��ư�� ������ ��� Ŭ���̾�Ʈ���� ���� ������ �̵��ϰ� ����.
        // �濡 ���� ���� ��, ������ �� �ε� ���ο� ���� �Բ� �� �ε�


    }

    public void JoinPlayer(Player newPlayer)
    {
        /*GameObject playerName= Instantiate(playerTextPrefab, playerList, false);
        playerName.name=newPlayer.NickName;
        playerName.GetComponent<Text>().text = newPlayer.NickName;*/

        var playerEntry = Instantiate(playerTextPrefab, playerList, false).GetComponent<PlayerEntry>();
        playerEntry.player = newPlayer;

        string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("users").Child(userId).Child("userName");
        reference.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                string userName = snapshot.Value.ToString();
                playerEntry.playerNameText.text = userName;
            }
        });


        /*playerEntry.playerNameText.text = ;*/

        var toggle = playerEntry.readyToggle;
        var toggleGroup = toggle.group;         

        if (PhotonNetwork.LocalPlayer.ActorNumber == newPlayer.ActorNumber)
        {
            // TODO : �� ��Ʈ���� ��쿡�� ����� onValueChanged�� �̺�Ʈ �ڵ鸵
            

            toggle.onValueChanged.AddListener(ReadyToggleClick);
            toggle.onValueChanged.AddListener(EyeToggleClick);

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
        GameObject leaveTarget = playerEntries[gonePlayer.ActorNumber].gameObject;      //dictionary�� key������ ����
        playerEntries.Remove(gonePlayer.ActorNumber);                   // �����ֱ� �� dictionary���� ����
        Destroy(leaveTarget);
        if (PhotonNetwork.IsMasterClient)
        {
            playersReady.Remove(gonePlayer.ActorNumber);
            CheckReady();
        }
        SortPlayers();              // �߰� ��� ������ ��Ÿ���� �÷���
                                    // ���� ��
        /*Destroy(playerList.Find(gonePlayer.NickName));*/

    }

    public void SortPlayers()
    {
        foreach (int actorNumber in playerEntries.Keys)
        {
            playerEntries[actorNumber].transform.SetSiblingIndex(actorNumber);      // actorNumber�� ���� ����       
            // setasfirstsibling => �� ������ �̵�    // setaslastsibling => �� �ڷ� �̵�
            // SetSiblingIndex => Hierarchy �� �� �θ� �ȿ��� �ٸ� ��ü �� ������ �����ϰ� ���� ��

        }
    }

    private void StartButtonClick()
    {
        // ���� ���� ��ư
        // ������ �� �ε� ���
        //SceneManager.LoadScene("GameScene");    //Ŭ���̾�Ʈ�� �Ѿ

        if (PhotonNetwork.IsMasterClient)       // ��ȿ�� �˻�
        {
            // photon�� ���� �÷��̾��� ���� ����ȭ�Ͽ� �ε�
            PhotonNetwork.LoadLevel("GameScene");
        }
    }


    private void CancelButtonClick()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.AutomaticallySyncScene = false;       // ���� ���� ��, �� �ε带 ����
        // �ð� �������� ���� ���� �����Ͽ��µ�, ������ ���� �ݿ� ���� ���� �Ѿ�� ���� ����.
    }




    // �� ���� ���°� ����� �� Custom Properties ����
    public void ReadyToggleClick(bool isOn)         // ������ ����ȭ �ʿ�  bool �̶� �����ϴ�.
    {
        Player localPlayer = PhotonNetwork.LocalPlayer;

        // PhotonNetwork�� customProperties�� Hashtable ������ Ȱ��
        // �׷��� dotnet�� HashTable�� �ƴ� ����ȭ ������ Hashtable Ŭ������ ���� ����

        Hashtable customProps = localPlayer.CustomProperties;

        //localPlayer.CustomProperties["Ready"] = isOn;           // �ؿ��� �ѹ��� ȣ���ؾߵ�

        customProps["Ready"] = isOn;

        localPlayer.SetCustomProperties(customProps);          // localplayer.customProperties

    }

    public void EyeToggleClick(bool isOn)
    {
        Player localPlayer = PhotonNetwork.LocalPlayer;

        Hashtable customProps = localPlayer.CustomProperties;

        customProps["Eyes"] = isOn;

        localPlayer.SetCustomProperties(customProps);
    }

    

    // �ٸ� �÷��̾ ReadyToggle�� �������� ��� �� Ŭ���̾�Ʈ���� �ݿ�.
    public void SetPlayerReady(int actorNumber, bool isReady)
    {
        playerEntries[actorNumber].readyLabel.gameObject.SetActive(isReady);        // readyLabel�� Ȱ��ȭ
        if (PhotonNetwork.IsMasterClient)
        {
            playersReady[actorNumber] = isReady;
            CheckReady();
        }
    }

    

    // ������ ��쿡 �ٸ� �÷��̾���� ��� ready �������� Ȯ�� �Ͽ�
    // Start ��ư�� Ȱ��ȭ ���θ� ����
    private void CheckReady()
    {
        // ���� ��� �� �Ѱ��� false�̸� false���� �Ҷ�.
        // �� ��� ��Ұ� && ������ �ؾ� �Ҷ�.

        bool allReady = playersReady.Values.All(x => x);        // ��� �÷��̾ �غ� �Ǿ����� �� ��ư Ȱ��ȭ
        bool anyReady = playersReady.Values.Any(x => x);          // �Ѹ��̶� �غ� �Ǿ����� �� ��ư Ȱ��ȭ

        startButton.interactable = allReady;


        /*bool allReady = true;       // �ʱ���´� true*/

        // 5���� �÷��̾� �� 3��° ���̾��� isReady�� false , ������ true=> allReady�� false

        /*foreach(bool isReady in playersReady.Values)
        {
            if(isReady)
            {
                continue;
            }
            else
            {
                allReady = false;
                break;
            }
        }

        startButton.interactable=allReady;        // ��� �÷��̾ �غ� �Ǿ����� �� ��ư Ȱ��ȭ
    }*/
    }


    /*private void CheckNotReady()
    {
        bool anyReady = false;

        foreach (bool isReady in playersReady.Values)
        {
            if (isReady)
            {
                anyReady = true;
                break;
            }
            else
            {
                continue;
            }
        }
    }*/

    private void DifficultyValueChange(int value)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        var customProps = PhotonNetwork.CurrentRoom.CustomProperties;
        customProps["Diff"] = value;
        PhotonNetwork.CurrentRoom.SetCustomProperties(customProps);

    }

    private void CharacterChange(int value)
    {
        var customProps = PhotonNetwork.LocalPlayer.CustomProperties;
        customProps["Character"] = value;
        PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);
    }




    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)          // ���� ���߿� ���� �÷��̾�� �������� ����� ready���°� ������ �ȵǾ�����.
    {
        print($"Ŀ���� ������Ƽ ����ƽ��ϴ�. : {PhotonNetwork.Time}");

        if (changedProps.ContainsKey("Ready"))
        {
            SetPlayerReady(targetPlayer.ActorNumber, (bool)changedProps["Ready"]);
        }
    }


    public override void OnRoomPropertiesUpdate(Hashtable props)    
    {
        if (props.ContainsKey("Diff"))  // �۵�������.
        {
            print($"room difficulty changed : {props["Diff"]}");
            diffText.text = ((Difficulty)props["Diff"]).ToString();             // enum���·� ��ȯ
        }

        if(props.ContainsKey("Character"))
        {
            print($"room character changed : {props["Character"]}");

            
        }

    }

    public override void OnJoinedRoom() 
    {
        var props = PhotonNetwork.CurrentRoom.CustomProperties;

        if (props.ContainsKey("Diff"))
        {
            print($"room difficulty changed : {props["Diff"]}");
            diffText.text = ((Difficulty)props["Diff"]).ToString();             // enum���·� ��ȯ
        }

        if (props.ContainsKey("Character"))
        {
            print($"room difficulty changed : {props["Diff"]}");
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient) 
    {
        // ������ �������� ȣ��ǹǷ�, �濡 �����Ǿ��ִ� ���¿��� ������ ������ ������ �� �ֵ���
        // ��ȿ�� �˻縦 �� �ʿ䰡 ����.
        // ��) playersReady�� Dictionary��ü ���� ���
           
        Debug.Log($"new master client : {newMasterClient.NickName}");
        // newmasterClient= ismasterclient?
        
        if(PhotonNetwork.IsMasterClient)
            Debug.Log("I'm master client");
        if(playersReady==null)
        {
            playersReady = new Dictionary<int, bool>();
        }
        foreach(Player player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if(!playersReady.ContainsKey(player.ActorNumber))
            {
                playersReady.Add(player.ActorNumber, false);
            }
        }

        CheckReady();
        startButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        diffDropdown.gameObject.SetActive(PhotonNetwork.IsMasterClient);

        diffText.gameObject.SetActive(false == PhotonNetwork.IsMasterClient);
    }
}


