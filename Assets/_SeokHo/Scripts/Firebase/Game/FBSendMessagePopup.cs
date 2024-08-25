using Firebase.Auth;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FBSendMessagePopup : MonoBehaviour {

	public InputField toInput;
	public InputField msgInput;
	public Button sendButton;

	private void Awake() {
		sendButton.onClick.AddListener(SendButtonClick);
	}

	public void SendButtonClick() {
		Message msg = new Message() {
			sender = FirebaseManager.Instance.Auth.CurrentUser.UserId,//보낸이: 나
			message = msgInput.text, //메시지 내용
			sendTime = DateTime.Now.Ticks //보낸 시각
		};

		//TODO:메시지 보냄
		FirebaseManager.Instance.SendMsg(toInput.text, msg);
	}

}
