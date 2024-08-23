using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FBLoginPanel : MonoBehaviour
{
    public InputField idInput;
    public InputField pwInput;

    public Button createButton;
    public Button loginButton;

	private void Awake() {
        createButton.onClick.AddListener(CreateButtonClick);
        loginButton.onClick.AddListener(LoginButtonClick);
        
	}

	private void Start() {
		SetUIInteractable(FirebaseManager.Instance.IsInitialized);
		FirebaseManager.Instance.onInit += () => SetUIInteractable(true);
	}

	public void LoginButtonClick() {
        SetUIInteractable(false);
        FirebaseManager.Instance.Login(idInput.text, pwInput.text,
            (user) => {
                FBPanelManager.Instance.PanelOpen<FBUserInfoPanel>().SetUserInfo(user);
            }    
        );
    }

    public void CreateButtonClick() {
        _ = FBPanelManager.Instance.PanelOpen<FBCreatePanel>();
    }

	public void SetUIInteractable(bool isTrue) { //UI 활성화 비활성화
        idInput.interactable = isTrue;
        pwInput.interactable = isTrue;
        createButton.interactable = isTrue;
        loginButton.interactable = isTrue;
    }

}
