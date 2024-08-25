using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerEntry : MonoBehaviour
{
    public Text playerNameText;
    public Toggle readyToggle;
    public GameObject readyLabel;
    public Toggle[] character;

    public Player player;
    public bool IsMine => player == PhotonNetwork.LocalPlayer;
    PlayerController playercontroller;

    private void Awake()                        // 문제 다른캐릭터들의 이벤트에도 영향을 줄 수 있음
    {
        playercontroller= GetComponent<PlayerController>();
        /*readyToggle.onValueChanged.AddListener(ReadyToggleClick);*/
        //readyToggle.isOn = false; =>onValueChangeed가 호출
        readyToggle.SetIsOnWithoutNotify(false);    // 알림은 호출하지말고 ison만 체크      -> isOn값을 수정하지만 onValueChanged 이벤트는 발생하지 않음
        character[0].SetIsOnWithoutNotify(false);
        character[1].SetIsOnWithoutNotify(false);
        character[2].SetIsOnWithoutNotify(false);
    }



    private void ReadyToggleClick(bool isOn)
    {
        // 커스텀 프로퍼티에 isOn을 추가하는 로직을 작성했을 경우
    }

    public void EyeToggleClick(bool isOn)
    {
        

        // 커스텀 프로퍼티에 isOn을 추가하는 로직을 작성했을 경우
        
    }
}
