using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Firebase.Database;

public class LoginPanel : MonoBehaviour
{
    public InputField idInput;
    public InputField pwInput;

    public Button createButton;
    public Button loginButton;
    public Button logoutButton;
    public Button PhotonLogin;

    


    private void Awake()
    {
        PhotonLogin.onClick.AddListener(PhotonConnectButtonClick);
        createButton.onClick.AddListener(CreateButtonClick);
        loginButton.onClick.AddListener(LoginButtonClick);
        logoutButton.onClick.AddListener(LogoutButtonClick);
    }

    private IEnumerator Start()
    {
        idInput.interactable = false;
        pwInput.interactable = false;
        createButton.interactable = false;
        loginButton.interactable = false;

        yield return new WaitUntil(() => FirebaseManager.Instance.IsInitialized);

        idInput.interactable = true;
        pwInput.interactable = true;
        createButton.interactable = true;
        loginButton.interactable = true;


        idInput.text = $"Player{PhotonNetwork.LocalPlayer.NickName}";
        /*idInput.text = $"PLAYER{Random.Range(100, 1000)}";*/

    }

    public void OnEnable()      //로그인 패널이 활성화 될 때마다 호출
    {
        idInput.interactable = true;
        loginButton.interactable = true;
    }

    public void CreateButtonClick()
    {
        createButton.interactable = false;
        FirebaseManager.Instance.Create(idInput.text, pwInput.text, (user) =>
        {
            print("회원가입성공");
            createButton.interactable = true;


        });
        
    }

    public void LoginButtonClick()
    {
        loginButton.interactable = false;

        FirebaseManager.Instance.Login(idInput.text, pwInput.text, (user) => { 
        loginButton.interactable = true;
        
        if(user!=null)
        {
            PhotonNetwork.LocalPlayer.NickName = idInput.text;  // 플레이어의 닉네임을 설정      ..로그인 버튼을 누르는 순간
            PhotonNetwork.ConnectUsingSettings();
        }
        });

    }

    public void LogoutButtonClick()
    {
        logoutButton.interactable = false;
        FirebaseManager.Instance.Logout();
    }

    public void PhotonConnectButtonClick()
    {
        PhotonNetwork.LocalPlayer.NickName = idInput.text;  // 플레이어의 닉네임을 설정      ..로그인 버튼을 누르는 순간
        PhotonNetwork.ConnectUsingSettings();               // 포톤 클라우드에 접속
        
        // 버튼과 인풋필드 비활성화 시키고 로그인 메시지 또는 아이콘 출력

        idInput.interactable = false;
        loginButton.interactable = false;       //interactable : 상호작용 가능 여부     // 여러번 접속을 막기위함.
    }



}
