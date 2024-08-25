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

    private void Awake()                        // ���� �ٸ�ĳ���͵��� �̺�Ʈ���� ������ �� �� ����
    {
        playercontroller= GetComponent<PlayerController>();
        /*readyToggle.onValueChanged.AddListener(ReadyToggleClick);*/
        //readyToggle.isOn = false; =>onValueChangeed�� ȣ��
        readyToggle.SetIsOnWithoutNotify(false);    // �˸��� ȣ���������� ison�� üũ      -> isOn���� ���������� onValueChanged �̺�Ʈ�� �߻����� ����
        character[0].SetIsOnWithoutNotify(false);
        character[1].SetIsOnWithoutNotify(false);
        character[2].SetIsOnWithoutNotify(false);
    }



    private void ReadyToggleClick(bool isOn)
    {
        // Ŀ���� ������Ƽ�� isOn�� �߰��ϴ� ������ �ۼ����� ���
    }

    public void EyeToggleClick(bool isOn)
    {
        

        // Ŀ���� ������Ƽ�� isOn�� �߰��ϴ� ������ �ۼ����� ���
        
    }
}
