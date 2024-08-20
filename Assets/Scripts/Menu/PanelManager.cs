using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelManager : MonoBehaviourPunCallbacks
{
    public static PanelManager Instance { get; private set; }

    public LoginPanel login;
    public MenuPanel menu;
    public LobbyPanel lobby;
    public RoomPanel room;

    private Dictionary<string, GameObject> panels;

    #region Unity 메세지
    private void Awake()
    {
        Instance = this;
        panels = new Dictionary<string, GameObject>
        {
            { "Login", login.gameObject },
            { "Menu", menu.gameObject },
            { "Lobby", lobby.gameObject },
            { "Room", room.gameObject }
        };
        PanelOpen("Login");
        PhotonNetwork.AddCallbackTarget(this);
    }
    private void OnDestroy()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
    public override void OnEnable()
    {
        // base.OnEnable();
    }
    public override void OnDisable()
    {
        // base.OnDisable();
    }
    #endregion

    public void PanelOpen(string panelName)
    {
        foreach (var row in panels)
        {

            //if(row.Key == panelName)
            //{
            //    // 활성화
            //    row.Value.SetActive(true);
            //}
            //else
            //{
            //    // 비활성화
            //    row.Value.SetActive(false);
            //}

            // 효율
            row.Value.SetActive(row.Key.Equals(panelName));
        }
    }

    public override void OnConnected()
    {
        print("메인메뉴에 입장함.");
        PanelOpen("Menu");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        LogManager.Log($"disconnected cause : {cause}");
        PanelOpen("Login");
    }

    public override void OnJoinedLobby()
    {
        print("로비에 입장함.");
        PanelOpen("Lobby");
    }

    public override void OnLeftLobby()
    {
        print("메인메뉴에 입장함.");
        PanelOpen("Menu");
    }

    public override void OnJoinedRoom()
    {
        print("룸에 입장함.");
        PanelOpen("Room");
    }

    public override void OnCreatedRoom()
    {
        print("룸에 입장함.");
        PanelOpen("Room");
    }

    public override void OnLeftRoom()
    {
        print("메인메뉴에 입장함.");
        PanelOpen("Menu");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        room.JoinPlayer(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        room.LeavePlayer(otherPlayer);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        lobby.UpdateRoomList(roomList);
    }

}