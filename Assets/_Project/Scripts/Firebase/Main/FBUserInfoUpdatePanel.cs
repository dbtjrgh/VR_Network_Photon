using Firebase.Auth;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FBUserInfoUpdatePanel : MonoBehaviour
{
    public InputField displayNameInput;
    public Text email;
    public InputField pwInput;
    public Text uidText;

    public Button updateButton;
    public Button cancelButton;


    private void Awake()
    {
        updateButton.onClick.AddListener(UpdateButtonClick);
        cancelButton.onClick.AddListener(CancelButtonClick);
    }


    public void SetUserInfo(FirebaseUser user)
    {
        displayNameInput.text = user.DisplayName;
        email.text = user.Email;
        pwInput.text = "";
        uidText.text = user.UserId;

    }

    public void UpdateButtonClick()
    {
        FirebaseManager.Instance.UpdateUser(displayNameInput.text, pwInput.text, () =>
        {
            FBPanelManager.Instance.Dialog("정보가 수정되었습니다");
            FBPanelManager.Instance.PanelOpen<FBUserInfoPanel>().SetUserInfo
            (FirebaseManager.Instance.Auth.CurrentUser);
        }
        );

    }

    public void CancelButtonClick()
    {
        _ = FBPanelManager.Instance.PanelOpen<FBUserInfoPanel>();
    }
}
