using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FBReceiveMessagePopup : MonoBehaviour {
	
	public Text msgText;

	//private void Start() {
	//	FirebaseManager.Instance.onReceiveMessage += OnReceiveMessage;
	//}

	//나에게 누군가 메시지를 보냈을 때 호출
	public void OnReceiveMessage(string msg) {
		msgText.text = msg;
		gameObject.SetActive(true);
	}
}
