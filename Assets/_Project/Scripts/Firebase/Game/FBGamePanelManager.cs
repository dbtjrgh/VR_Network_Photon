using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FBGamePanelManager : MonoBehaviour
{
    public Text titleText;
    public Text nameText;
    public Text classText;
    public Text levelText;
    public Text addrText;

    public Dropdown classDropdown;
    public Button levelupButton;
    public InputField addrInput;
    public Button sendMessageButton;

    public FBReceiveMessage rPopup;
    public FBSendMessagePopup sPopup;

    private void Awake()
    {
        var options=new List<Dropdown.OptionData>();            // 옵션 리스트로 사용할 list 인스턴스
        // 생성
        
        foreach(string clNames in Enum.GetNames(typeof(UserData.Class)))        // UserData.Class의 이름들을 가져와서
        {
            options.Add(new Dropdown.OptionData(clNames));        // 각 이름을 Dropdown.OptionData로 만들어서 리스트에 추가
        }

        classDropdown.options=options;        // 드롭다운의 옵션을 설정

        classDropdown.onValueChanged.AddListener(ClassDropdownChange);        // 드롭다운이 변경될때마다 호출할 함수 등록
        levelupButton.onClick.AddListener(LevelUpButtonClick);                // 레벨업 버튼 클릭시 호출할 함수 등록
        //addrInput.onEndEdit.AddListener((string addr)=>{addrText.text=addr;});    // 주소 입력 후 엔터를 누를 경우 호출할 함수 등록         // onendeit : 입력이 끝날때 호출되는 이벤트 
        //addrInput.onSubmit.AddListener(AddressInputChange);                    // onsubmit : PC환경 - ENTER 키 누를 때 , 모바일 환경 - 가상 키보드의 완료 버튼을 누를때
        addrInput.onEndEdit.AddListener(AddressInputChange);                                                                          //addrInput.OnSubmit.AddListener(AddressInputChange);                    // onsubmit : PC환경 - ENTER 키 누를 때 , 모바일 환경 - 가상 키보드의 완료 버튼을 누를때
                                                                                  //onEndEdit : onSubmit을 포함하여, PC -다른 곳을 클릭하는 등 포커스를 잃을 때 모바일 다른곳을 터치하여 가상 키보드가 사라질때
        sendMessageButton.onClick.AddListener(SendMessageButtonClick);        // 메시지 보내기 버튼 클릭시 호출할 함수 등록
        // onsubmit : 엔터를 누를때 호출되는 이벤트

    }

    private void Start()
    {
        SetUserData(FirebaseManager.Instance.userData);
        FirebaseManager.Instance.onReceiveMessage += rPopup.OnReceiveMessage;
    }

    public void SetUserData(UserData data)
    {
        titleText.text=$"안녕하세요, {data.userName}";
        nameText.text = data.userName;
        classText.text = data.characterClass.ToString();
        levelText.text = data.level.ToString();
        addrText.text=data.address;
    }

    public void ClassDropdownChange(int value)
    {
        // 드롭다운으로 직업을 교체할 경우
        FirebaseManager.Instance.UpdateCharacterClass((UserData.Class)value,()=>
        {
            SetUserData(FirebaseManager.Instance.userData);
        });

    }
    public void LevelUpButtonClick()
    {
        //TODO : 레벨업 버튼을 클릭할 경우
        FirebaseManager.Instance.UpdateCharacterLevel(()=>
        {
            SetUserData(FirebaseManager.Instance.userData);
        });

    }
    public void AddressInputChange(string value)
    {
        //TODO : address에 값을 입력 후 enter 등을 누를 경우

        FirebaseManager.Instance.UpdateCharacterAddress(value, () => SetUserData(FirebaseManager.Instance.userData));

    }

    public void SendMessageButtonClick()
    {
        sPopup.gameObject.SetActive(true);

    }

}
