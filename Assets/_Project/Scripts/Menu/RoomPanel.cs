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


    // 방장일 경우, 플레이어들의 ready 상태를 저장할 dictionary

    private Dictionary<int, bool> playersReady;
    // 방에 들어온 모든 플레이들이 서로를 알고 있도록 사용할 dictionary

    public Dictionary<int, PlayerEntry> playerEntries = new();      // 제네릭을 통해 엄격히 제어  해쉬테이블은 object형태로 오기때문에 언박싱해줄 수 있음
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


    public override void OnDisable()        // 방을 나갈 때, 방장이 나갔을 때, 방이 없어졌을 때
    {
        base.OnDisable();
        foreach (Transform child in playerList) 
        {
            Destroy(child.gameObject);
        }
    }

    public override void OnEnable()     // 방에 입장했을 때, 방장이 되었을 때
    {
        base.OnEnable();
        roomTitleText.text = PhotonNetwork.CurrentRoom.Name;

        if (PhotonNetwork.IsMasterClient)
        {
            // 방장인 상태
            playersReady = new Dictionary<int, bool>();

        }
        else
        {
            // 방장이 아닌 상태
        }

        // 방장이 아니면 게임시작버튼 및 난이도 조절을 비활성화
        startButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        diffDropdown.gameObject.SetActive(PhotonNetwork.IsMasterClient);

        diffText.gameObject.SetActive(false == PhotonNetwork.IsMasterClient);

        foreach (Player player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            // 플레이어 목록에 플레이어 이름표 하나씩 생성
            JoinPlayer(player);

            if (player.CustomProperties.ContainsKey("Ready"))   //Ready라는 키값이 있는지 확인
                                                                //키값은어디서 확인?  //플레이어의 커스텀 프로퍼티에 있는지 확인
                                                                //커스텀프로퍼티는 어디에 있는지? //플레이어에 있음
                                                                
            {
                SetPlayerReady(player.ActorNumber, (bool)player.CustomProperties["Ready"]);
            }

        }

        // 방장이 아니면 게임 시작 버튼을 비활성화.
        startButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);




        PhotonNetwork.AutomaticallySyncScene = true;        // 방장이 게임 시작 버튼을 누르면 모든 클라이언트들이 같은 씬으로 이동하게 해줌.
        // 방에 입장 했을 때, 방장의 씬 로드 여부에 따라 함께 씬 로드


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
            // TODO : 내 엔트리일 경우에만 토글의 onValueChanged에 이벤트 핸들링
            

            toggle.onValueChanged.AddListener(ReadyToggleClick);
            toggle.onValueChanged.AddListener(EyeToggleClick);

        }
        else
        {
            // 내가 아닌 다른 플레이어의 엔트리
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
        GameObject leaveTarget = playerEntries[gonePlayer.ActorNumber].gameObject;      //dictionary의 key값으로 접근
        playerEntries.Remove(gonePlayer.ActorNumber);                   // 지워주기 전 dictionary에서 제거
        Destroy(leaveTarget);
        if (PhotonNetwork.IsMasterClient)
        {
            playersReady.Remove(gonePlayer.ActorNumber);
            CheckReady();
        }
        SortPlayers();              // 중간 비는 현상이 나타날때 올려줌
                                    // 같은 뜻
        /*Destroy(playerList.Find(gonePlayer.NickName));*/

    }

    public void SortPlayers()
    {
        foreach (int actorNumber in playerEntries.Keys)
        {
            playerEntries[actorNumber].transform.SetSiblingIndex(actorNumber);      // actorNumber에 따라 정렬       
            // setasfirstsibling => 맨 앞으로 이동    // setaslastsibling => 맨 뒤로 이동
            // SetSiblingIndex => Hierarchy 상 내 부모 안에서 다른 객체 중 순서를 지정하고 싶을 때

        }
    }

    private void StartButtonClick()
    {
        // 게임 시작 버튼
        // 기존의 씬 로드 방식
        //SceneManager.LoadScene("GameScene");    //클라이언트만 넘어감

        if (PhotonNetwork.IsMasterClient)       // 유효성 검사
        {
            // photon을 통해 플레이어들과 씬을 동기화하여 로드
            PhotonNetwork.LoadLevel("GameScene");
        }
    }


    private void CancelButtonClick()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.AutomaticallySyncScene = false;       // 방을 나갈 때, 씬 로드를 해제
        // 시간 지연으로 인해 방을 퇴장하였는데, 방장의 시작 콜에 의해 씬이 넘어가는 것을 방지.
    }




    // 내 레디 상태가 변경될 때 Custom Properties 변경
    public void ReadyToggleClick(bool isOn)         // 데이터 직렬화 필요  bool 이라 가능하다.
    {
        Player localPlayer = PhotonNetwork.LocalPlayer;

        // PhotonNetwork의 customProperties는 Hashtable 구조를 활용
        // 그러나 dotnet의 HashTable이 아닌 간소화 형태의 Hashtable 클래스를 직접 제공

        Hashtable customProps = localPlayer.CustomProperties;

        //localPlayer.CustomProperties["Ready"] = isOn;           // 밑에서 한번더 호출해야됨

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

    

    // 다른 플레이어가 ReadyToggle을 변경했을 경우 내 클라이언트에도 반영.
    public void SetPlayerReady(int actorNumber, bool isReady)
    {
        playerEntries[actorNumber].readyLabel.gameObject.SetActive(isReady);        // readyLabel을 활성화
        if (PhotonNetwork.IsMasterClient)
        {
            playersReady[actorNumber] = isReady;
            CheckReady();
        }
    }

    

    // 방장일 경우에 다른 플레이어들이 모두 ready 상태인지 확인 하여
    // Start 버튼의 활성화 여부를 결정
    private void CheckReady()
    {
        // 여러 요소 중 한개라도 false이면 false여야 할때.
        // 즉 모든 요소가 && 연산을 해야 할때.

        bool allReady = playersReady.Values.All(x => x);        // 모든 플레이어가 준비가 되어있을 때 버튼 활성화
        bool anyReady = playersReady.Values.Any(x => x);          // 한명이라도 준비가 되어있을 때 버튼 활성화

        startButton.interactable = allReady;


        /*bool allReady = true;       // 초기상태는 true*/

        // 5명의 플레이어 중 3번째 블레이어의 isReady가 false , 나머진 true=> allReady는 false

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

        startButton.interactable=allReady;        // 모든 플레이어가 준비가 되어있을 때 버튼 활성화
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




    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)          // 맹점 나중에 들어온 플레이어는 먼저들어온 사람의 ready상태가 갱신이 안되어있음.
    {
        print($"커스텀 프로퍼티 변경됐습니다. : {PhotonNetwork.Time}");

        if (changedProps.ContainsKey("Ready"))
        {
            SetPlayerReady(targetPlayer.ActorNumber, (bool)changedProps["Ready"]);
        }
    }


    public override void OnRoomPropertiesUpdate(Hashtable props)    
    {
        if (props.ContainsKey("Diff"))  // 작동을안해.
        {
            print($"room difficulty changed : {props["Diff"]}");
            diffText.text = ((Difficulty)props["Diff"]).ToString();             // enum형태로 변환
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
            diffText.text = ((Difficulty)props["Diff"]).ToString();             // enum형태로 변환
        }

        if (props.ContainsKey("Character"))
        {
            print($"room difficulty changed : {props["Diff"]}");
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient) 
    {
        // 방장이 나갔을때 호출되므로, 방에 참가되어있는 상태에서 방장의 역할을 수행할 수 있도록
        // 유효성 검사를 할 필요가 있음.
        // 예) playersReady에 Dictionary객체 생성 등등
           
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


