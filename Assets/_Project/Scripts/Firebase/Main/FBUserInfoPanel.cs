using Firebase.Auth;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FBUserInfoPanel : MonoBehaviour
{
    public Text greetingText;
    public Text displayNameText;
    public Text EmailText;
    public Text phoneNumText;
    public Text uid;

    public Button logoutButton;
    public Button updateButton;
    public Button gameStartButton;



    private void Awake()
    {
        logoutButton.onClick.AddListener(LogoutButtonClick);
        updateButton.onClick.AddListener(UpdateButtonClick);
        gameStartButton.onClick.AddListener(GameStartButtonClick);
    }

    public void SetUserInfo(FirebaseUser user)
    {
        greetingText.text = $"æ»≥Á«œººø‰, {user.DisplayName}¥‘!";
        displayNameText.text = user.DisplayName;
        EmailText.text = user.Email;
        phoneNumText.text = user.PhoneNumber;
        uid.text = user.UserId;
    }

    public void LogoutButtonClick()
    {
        FirebaseManager.Instance.Logout();
        _=FBPanelManager.Instance.PanelOpen<FBLoginPanel>();

    }

    public void UpdateButtonClick()
    {
        FBPanelManager.Instance.PanelOpen<FBUserInfoUpdatePanel>().SetUserInfo(FirebaseManager.Instance.Auth.CurrentUser);
    }
    public void GameStartButtonClick()
    {
        SceneManager.LoadScene("GameScene");
    }
}
