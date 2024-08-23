using Firebase;
using Firebase.Extensions;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using System;
using Firebase.Database;
using Newtonsoft.Json;
using System.Data;

public class FirebaseManager : MonoBehaviour {
	public static FirebaseManager Instance { get; private set; }

	public FirebaseApp App { get; private set; } //파이어베이스 기본 앱(기본 기능들)
	public FirebaseAuth Auth { get; private set; } //인증(로그인)기능 전용
	public FirebaseDatabase DB { get; private set; } //데이터베이스 기능 전용

	//파이어베이스 앱이 초기화 되어 사용 가능한지 여부
	public bool IsInitialized { get; private set; } = false;

	public event Action onInit; //파이어베이스가 초기화되면 호출

	public event Action<FirebaseUser> onLogin; //로그인 또는 회원가입 후에 호출

	public event Action<string> onReceiveMessage; //현재 유저가 메시지를 받으면 호출

	internal UserData userData;

	public DatabaseReference usersRef;

	private void Awake() {
		Instance = this;
		onLogin += OnLogin;
		DontDestroyOnLoad(gameObject);
	}

	private void Start() {
		InitializeAsync();
	}

	//로그인이 되었을때 호출됐음 좋겠음
	private void OnLogin(FirebaseUser user) {
		var msgRef = DB.GetReference($"msg/{user.UserId}");
		msgRef.ChildAdded += RecvMsgEventHandler;
	}

	//async를 안 쓸 경우
	private void Initialize() {
		FirebaseApp.
			CheckAndFixDependenciesAsync().
			ContinueWithOnMainThread(
				(Task<DependencyStatus> task) => {
					if (task.IsFaulted || task.IsCanceled) {
						Debug.LogWarning($"파이어베이스 초기화 실패 : {task.Status}");
					} else if (task.IsCompleted) {
						print($"파이어베이스 초기화 성공 : {task.Status}");

						if (task.Result == DependencyStatus.Available) {
							App = FirebaseApp.DefaultInstance;
							Auth = FirebaseAuth.DefaultInstance;
							IsInitialized = true;
						}
					}
				}); //파이버베이스 앱 초기화
	}

	//async 키워드를 통해 비동기 프로그래밍
	private async void InitializeAsync() {
		DependencyStatus status = await FirebaseApp.CheckAndFixDependenciesAsync();

		if (status == DependencyStatus.Available) {
			//파이어베이스 초기화 성공
			print("파이어베이스 초기화 성공!");
			App = FirebaseApp.DefaultInstance;
			Auth = FirebaseAuth.DefaultInstance;
			DB = FirebaseDatabase.DefaultInstance;
			IsInitialized = true;
			onInit?.Invoke();
		} else {
			//파이어베이스 초기화 실패
			Debug.LogWarning($"파이어베이스 초기화 실패 : {status}");
		}
	}

	public async void Login(string email, string pw, Action<FirebaseUser> callback = null) {
		var result = await Auth.SignInWithEmailAndPasswordAsync(email, pw);

		usersRef = DB.GetReference($"users/{result.User.UserId}");

		DataSnapshot userDataValues = await usersRef.GetValueAsync();

		if (userDataValues.Exists) {
			string json = userDataValues.GetRawJsonValue();//데이터 전체를 json으로 가져옴
			var address = userDataValues.Child("address");//데이터의 하위 레퍼런스(데이터스냅샷)을 가져옴.
			if (address.Exists) print($"주소 : {address.GetValue(false)}");
			userData = JsonConvert.DeserializeObject<UserData>(json);
			print(json);
		} else {
			FBPanelManager.Instance.Dialog("로그인 정보에 문제가 있습니다.\n고객센터에 문의하세요.");
		}

		onLogin?.Invoke(result.User);
		callback?.Invoke(result.User);
	}

	public async void Create(string email, string pw, Action<FirebaseUser> callback = null) {

		try {
			var result = await Auth.CreateUserWithEmailAndPasswordAsync(email, pw);

			usersRef = DB.GetReference($"users/{result.User.UserId}");

			UserData userData = new UserData(result.User.UserId);

			string userDataJson = JsonConvert.SerializeObject(userData);

			await usersRef.SetRawJsonValueAsync(userDataJson);

			this.userData = userData;

			onLogin?.Invoke(result.User);
			callback?.Invoke(result.User);

		} catch (FirebaseException fe) {

			Debug.LogError(fe.Message);

		}
	}

	public async void UpdateUser(string name, string pw, Action callback = null) {

		var profile = new UserProfile() { DisplayName = name };

		await Auth.CurrentUser.UpdateUserProfileAsync(profile);

		if (false == string.IsNullOrWhiteSpace(pw)) {
			await Auth.CurrentUser.UpdatePasswordAsync(pw);
		}

		callback?.Invoke();

	}

	public void Logout() {
		Auth.SignOut();
	}

	//DB와 Transaction을 수행할 함수들
	public async void UpdateCharacterClass(UserData.Class cl, Action callback = null) {

		string refKey = //"characterClass";
			nameof(UserData.characterClass);

		//캐릭터 클래스 항목의 데이터 레퍼런스
		var classRef = usersRef.Child(refKey);
		await classRef.SetValueAsync((int)cl);
		userData.characterClass = cl;

		callback?.Invoke();
	}

	public async void UpdateCharacterLevel(Action callback = null) {
		
		int level = userData.level + 1;
		string refKey = nameof(UserData.level);
		var levelRef = usersRef.Child(refKey);
		await levelRef.SetValueAsync(level);
		userData.level = level;

		callback?.Invoke();

	}

	public async void UpdateCharacterAddress(string address, Action callback = null) {

		string refKey = nameof(UserData.address);
		var addrRef = usersRef.Child(refKey);
		await addrRef.SetValueAsync(address);
		userData.address = address;

		callback?.Invoke();

	}

	public async void UpdateUserData(string childName, object value, Action<object> callback = null) {

		var targetRef = usersRef.Child(childName);
		//.......

	}

	//TODO:DB를 통해 메시지를 주고 받는 함수

	//메시지 보낼때
	public void SendMsg(string receiver, Message msg) {
		var msgRef = DB.GetReference($"msg/{receiver}");
		var msgJson = JsonConvert.SerializeObject(msg);
		msgRef.Child(msg.sender + msg.sendTime).SetRawJsonValueAsync(msgJson);
	}

	//메시지 받을때

	public void RecvMsgEventHandler(object sender/*이벤트를 호출한 객체의 고유Key 구분을 위한 객체*/, ChildChangedEventArgs args) {
		if(args.DatabaseError != null) {
			Debug.LogError(args.DatabaseError);
			return;
		} else {
			var rawJson = args.Snapshot.GetRawJsonValue();

			Message msg = JsonConvert.DeserializeObject<Message>(rawJson);

			string msgString =
		$"보낸이 : {msg.sender}\n내용 : {msg.message}\n보낸 시각 : {msg.GetSendTime()}";
			onReceiveMessage?.Invoke(msgString);

		}

	}

}
