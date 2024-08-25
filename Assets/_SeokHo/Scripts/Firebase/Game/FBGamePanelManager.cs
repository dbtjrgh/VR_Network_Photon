using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FBGamePanelManager : MonoBehaviour {
	
	public Text titleText;
	public Text nameText;
	public Text classText;
	public Text levelText;
	public Text addrText;

	public Dropdown classDropdown;
	public Button levelupButton;
	public InputField addrInput;
	public Button sendMessageButton;

	public FBReceiveMessagePopup rPopup;
	public FBSendMessagePopup sPopup;

	private void Awake() {
		
		var options = new List<Dropdown.OptionData>(); //�ɼǸ���Ʈ�� ����� list �ν��Ͻ� ����

		foreach (string clNames in Enum.GetNames(typeof(UserData.Class))) {
			options.Add(new Dropdown.OptionData(clNames));
		}

		classDropdown.options = options;

		classDropdown.onValueChanged.AddListener(ClassDropdownChange);
		levelupButton.onClick.AddListener(LevelupButtonClick);
		addrInput.onSubmit.AddListener(AddressInputChange);
		//onSubmit : PCȯ�� - Enter Ű ������, �����ȯ�� - ���� Ű������ �Ϸ� ��ư�� ������
		//onEndEdit : onSubmit�� �����Ͽ�, PC - �ٸ����� Ŭ�� �ϴ� �� ��Ŀ���� ������
		//								����� : �ٸ����� ��ġ�Ͽ� ���� Ű���尡 �������
		sendMessageButton.onClick.AddListener(SendMessageButtonClick);
	}

	private void Start() {
		SetUserData(FirebaseManager.Instance.userData);
		FirebaseManager.Instance.onReceiveMessage += rPopup.OnReceiveMessage;
	}

	public void SetUserData(UserData data) {
		titleText.text = $"�ȳ��ϼ���, {data.userName}";
		nameText.text = data.userName;
		classText.text = data.characterClass.ToString();
		levelText.text = data.level.ToString();
		addrText.text = data.address;
	}

	public void ClassDropdownChange(int value) {
		//TODO:��Ӵٿ����� ������ ��ü�� ���

		FirebaseManager.Instance.UpdateCharacterClass((UserData.Class)value, () => {
			SetUserData(FirebaseManager.Instance.userData);
			}
		);

	}

	public void LevelupButtonClick() {
		//TODO:������ ��ư�� Ŭ���� ���
		FirebaseManager.Instance.UpdateCharacterLevel(
			()=>SetUserData(FirebaseManager.Instance.userData)	
		);
	}

	public void AddressInputChange(string value) {
		//TODO:address�� ���� �Է� �� enter���� ���� ���

		FirebaseManager.Instance.UpdateCharacterAddress(value,
			() => SetUserData(FirebaseManager.Instance.userData)
		);

	}

	public void SendMessageButtonClick() {
		sPopup.gameObject.SetActive(true);
	}

}
