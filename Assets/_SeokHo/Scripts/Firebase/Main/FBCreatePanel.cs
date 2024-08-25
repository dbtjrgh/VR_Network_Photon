using Firebase.Auth;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FBCreatePanel : MonoBehaviour
{
    public InputField idInput;
    public InputField pwInput;
    public InputField pwReInput;

    public Button createButton;
    public Button cancelButton;

	private void Awake() {
        createButton.onClick.AddListener(CreateButtonClick);
        cancelButton.onClick.AddListener(() => { FBPanelManager.Instance.PanelOpen<FBLoginPanel>(); });
	}

	public void CreateButtonClick() {
        //TODO:비밀번호 확인하기
        FirebaseManager.Instance.Create(idInput.text, pwInput.text, SetUser);
    }

    private void SetUser(FirebaseUser user) {
        FBPanelManager.Instance.Dialog("회원가입 완료");
        FBPanelManager.Instance.PanelOpen<FBUserInfoPanel>().SetUserInfo(user);
    }

}
