using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : MonoBehaviour
{
    public InputField idInput;
    public Button loginButton;
    private void Awake()
    {
        loginButton.onClick.AddListener(OnLoginButtonClick);
    }

    private void Start()
    {
        idInput.text = $"PLAYER {Random.Range(100, 1000)}";
    }

    private void OnEnable()
    {
        idInput.interactable = true;
        loginButton.interactable = true;
    }
    public void OnLoginButtonClick()
    {
        PhotonNetwork.LocalPlayer.NickName = idInput.text;
        PhotonNetwork.ConnectUsingSettings();

        // 버튼과 인풋필드 비활성화 시키고 로그인 메세지 또는 아이콘 출력
        idInput.interactable = false;
        loginButton.interactable = false;
    }
}
