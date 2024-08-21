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
    #region 변수
    public Text roomTitleText;
    public RectTransform playerList;
    public GameObject playerTextPrefab;

    public Button startButton;
    public Button cancelButton;
    public Dropdown diffDropdown;
    public Text diffText;

    // 방장일 경우, 플레이어들의 ready 상태를 저장할 dictionary
    private Dictionary<int, bool> playersReady;

    // 방에 들어온 모든 플레이어들이 서로를 알고 있도록 사용할 dictionary
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
            // 방장인 상태
            playersReady = new Dictionary<int, bool>();

        }
        else
        {
            // 방장이 아닌 상태

        }

        // 방장이 아니면 게임 시작 버튼 및 난이도 조절을 비활성화
        startButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        diffDropdown.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        diffText.gameObject.SetActive(false == PhotonNetwork.IsMasterClient);

        foreach (Player player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            // 플레이어 목록에 플레이어 이름표 하나씩 생성
            JoinPlayer(player);
            if (player.CustomProperties.ContainsKey("Ready"))
            {
                SetPlayerReady(player.ActorNumber, (bool)player.CustomProperties["Ready"]);
            }
        }
        // 방에 입장 했을 때, 방장의 씬 로드 여부에 따라 함께 씬 로드
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
            // TODO : 내 엔트리일 경우엔 토글의 OnValueChanged에 이벤트 핸들링
            toggle.onValueChanged.AddListener(ReadyToggleClick);
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
            // SetSiblingIndex => Hierachy상 내 부모 안에서 다른 객체 중 순서를 지정하고 싶을 때
            playerEntries[actorNumber].transform.SetSiblingIndex(actorNumber);
        }
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

    // 내 Ready 상태가 변경 될 때 Custom Properties 변경
    public void ReadyToggleClick(bool isOn)
    {
        Player localPlayer = PhotonNetwork.LocalPlayer;

        // PhotonNetwork의 CustomPropertie는 Hashtable 구조를 활용
        // 그러나 dotnet의 Hashtable이 아닌 간소화 형태의 Hashtable 클래스를 직접 제공
        PhotonHashtable customProps = localPlayer.CustomProperties;

        customProps["Ready"] = isOn;

        localPlayer.SetCustomProperties(customProps);
    }

    // 다른 플레이어가 ReadyToggle을 변경했을 경우 내 클라이언트에도 반영
    public void SetPlayerReady(int actorNumber, bool isReady)
    {
        playerEntries[actorNumber].readyLabel.gameObject.SetActive(isReady);
        if(PhotonNetwork.IsMasterClient)
        {
            playersReady[actorNumber] = isReady;
            CheckReady();
        }
    }

    // 방장일 경우에 다른 플레이어들이 모두 ready 상태인지 확인
    // Start 버튼의 활성화 여부를 결정
    private void CheckReady()
    {
        // 여러 요소 중 한개라도 false이면 false여야 할 때
        // 즉 모든 요소가 && 연산을 해야 할 때

        // 초기 상태는 true;
        //bool allReady = true;

        //// Ex : 5명의 플레이어중 3번째 플레이어의 isReady가 false, 나머진 true여도 allReady는 false
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

        // 전부 true여야 true로 된다. (AND)
        bool allReady = playersReady.Values.All(x=>x);
        // 하나라도 true면 true로 된다. (OR)
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
        // print($"커스텀 프로퍼티 변경됐습니다. : {PhotonNetwork.Time}");
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
        // 방장이 나갔을 때 호출되는 함수로, 방에 참가되어있는 상태에서 방장의 역할을
        // 수행 할 수 있도록 유효성 검사 및 추가 조치를 할 필요가 있음
        // Ex : playerReady에 Dictionary객체 생성 등등
    }
}
