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

    public void OnEnable()      //�α��� �г��� Ȱ��ȭ �� ������ ȣ��
    {
        idInput.interactable = true;
        loginButton.interactable = true;
    }

    public void CreateButtonClick()
    {
        createButton.interactable = false;
        FirebaseManager.Instance.Create(idInput.text, pwInput.text, (user) =>
        {
            print("ȸ�����Լ���");
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
            PhotonNetwork.LocalPlayer.NickName = idInput.text;  // �÷��̾��� �г����� ����      ..�α��� ��ư�� ������ ����
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
        PhotonNetwork.LocalPlayer.NickName = idInput.text;  // �÷��̾��� �г����� ����      ..�α��� ��ư�� ������ ����
        PhotonNetwork.ConnectUsingSettings();               // ���� Ŭ���忡 ����
        
        // ��ư�� ��ǲ�ʵ� ��Ȱ��ȭ ��Ű�� �α��� �޽��� �Ǵ� ������ ���

        idInput.interactable = false;
        loginButton.interactable = false;       //interactable : ��ȣ�ۿ� ���� ����     // ������ ������ ��������.
    }



}
