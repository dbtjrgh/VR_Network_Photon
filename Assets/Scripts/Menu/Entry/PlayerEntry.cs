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

    public Player player;
    public bool IsMine => player == PhotonNetwork.LocalPlayer;

    private void Awake()
    {
        //readyToggle.onValueChanged.AddListener(ReadyToggleClick);
        // readyToggle.isOn = false; => onValueChanged가 호출
        readyToggle.SetIsOnWithoutNotify(false); // => isOn 값을 수정하지만 onValueChange가 호출되지 않음
    }

    private void ReadyToggleClick(bool isOn)
    {
        // 커스텀 프로퍼티에 isOn을 추가하는 로직을 작성했을 경우
    }



}
