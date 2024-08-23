using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : MonoBehaviour {

	public InputField idInput;
	public InputField pwInput;

	public Button createButton;
	public Button loginButton;

	private void Awake() {
		createButton.onClick.AddListener(CreateButtonClick);
		loginButton.onClick.AddListener(LoginButtonClick);
	}

	private IEnumerator Start() {

		idInput.interactable = false;
		pwInput.interactable = false;
		createButton.interactable = false;
		loginButton.interactable = false;

		yield return new WaitUntil(() => FirebaseManager.Instance.IsInitialized);

		idInput.interactable = true;
		pwInput.interactable = true;
		createButton.interactable = true;
		loginButton.interactable = true;

		//idInput.text = $"PLAYER {Random.Range(100, 1000)}";
	}

	private void OnEnable() {
		idInput.interactable = true;
		loginButton.interactable = true;
	}

	public void CreateButtonClick() {
		createButton.interactable = false;
		FirebaseManager.Instance.Create(idInput.text, pwInput.text, (user) => {
			print("회원가입 성공!");
			createButton.interactable = true;
		});
	}

	public void LoginButtonClick() {

		loginButton.interactable = false;

		FirebaseManager.Instance.Login(idInput.text, pwInput.text, (user) => {
			loginButton.interactable = true;
		});

	}

	public void PhotonConnectButtonClick() {

		PhotonNetwork.LocalPlayer.NickName = idInput.text;
		PhotonNetwork.ConnectUsingSettings();

		//버튼과 인풋필드 비활성화 시키고 로그인 메시지 또는 아이콘 출력

		idInput.interactable = false;
		loginButton.interactable = false;

	}

}
