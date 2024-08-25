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
using Photon.Pun;

public class FirebaseManager : MonoBehaviour
{

    public static FirebaseManager Instance { get; private set; }
    public FirebaseApp App { get; private set; }        // 파이어 베이스 기본 앱(기본 기능들)
    public FirebaseAuth Auth { get;private set; }       // 인증기능 전용

    public FirebaseDatabase DB { get; private set; }     // 데이터베이스 전용

    public UserData userData;

    public DatabaseReference usersRef;

    

    // 파이어 베이스 앱이 초기화 되어 사용 가능한지 여부
    public bool IsInitialized { get; private set; } = false;

    public event Action onInit;                     // 파이어베이스가 초기화되면 호출

    public event Action<FirebaseUser> onLogin;       // 로그인 또는 회원가입 후에 호출

    public event Action<string> onReceiveMessage;    // 현재 유저가 메시지를 받으면 호출

    private void Awake()
    {
        Instance = this;
        onLogin += OnLogin;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitializeAsync();
    }

    // 로그인이 되었을 때 호출됐음 좋겠음
    private void OnLogin(FirebaseUser user)
    {
        var msgRef = DB.GetReference($"msg/{user.UserId}");
        msgRef.ChildAdded += RecvMsgEventHandler;
    }

    private void MsgRef_ChildAdded(object sender, ChildChangedEventArgs e)
    {
        throw new NotImplementedException();
    }

    //Async를 안쓸경우
    private void Initialize()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().
            ContinueWithOnMainThread(
            (Task<DependencyStatus> task) =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogWarning($"파이어베이스 초기화 실패 : {task.Status}");
                }
                else if (task.IsCompleted)
                {
                    print($"파이어베이스 초기화 성공 : {task.Status}");

                    if(task.Result==DependencyStatus.Available)
                    {
                        App = FirebaseApp.DefaultInstance;
                        Auth = FirebaseAuth.DefaultInstance;            // async가 없을때는 이렇게 사용
                        IsInitialized = true;
                    }


                    App = FirebaseApp.DefaultInstance;
                    Auth = FirebaseAuth.DefaultInstance;
                }
            });     // 파이어베이스 앱 초기화

    }

    // async 키워드를 통해 비동기 프로그래밍
    private async void InitializeAsync()
    {
        DependencyStatus status = await FirebaseApp.CheckAndFixDependenciesAsync();

        if(status==DependencyStatus.Available)
        {
            // 파이어베이스 초기화 성공
            print("파이어 베이스 초기화 성공");
            App=FirebaseApp.DefaultInstance;
            Auth = FirebaseAuth.DefaultInstance;
            DB = FirebaseDatabase.DefaultInstance;
            IsInitialized = true;
            onInit?.Invoke();
        }
        else
        {
            // 파이어베이스 초기화 실패
            Debug.LogWarning($"파이어베이스 초기화 실패 : {status}");
        }
        
    }

    public async void Login(string email,string pw,Action<FirebaseUser> callback=null)
    {
        var result=await Auth.SignInWithEmailAndPasswordAsync(email, pw);
        /*
                print("로그인 성공");*/


        /*print($"로그인된 유저 : {CurrentUser.Email}");*/

        usersRef = DB.GetReference($"users/{result.User.UserId}");/*.Child("level"); */       //reference의 역할은 데이터베이스의 위치를 가리키는 역할

        DataSnapshot userDataValues = await usersRef.GetValueAsync();        //데이터베이스에서 데이터를 가져오는 함수

        if(userDataValues.Exists)
        {
            string json= userDataValues.GetRawJsonValue();      // 데이터 전체를 json으로 가져옴
            var address=userDataValues.Child("address");        // 데이터의 하위 레퍼런스(데이터스냅샷)을 가져옴.
            if (address.Exists) print($"주소 : {address.GetValue(false)}");
            userData = JsonConvert.DeserializeObject<UserData>(json);
            /*(userDataValues.GetRawJsonValue());*/
            print(json);
            
        }

        else
        {
            FBPanelManager.Instance.Dialog("로그인 정보에 문제가 생겼습니다.\n고객센터에 문의하세요.");
        }

        onLogin?.Invoke(result.User);

        callback?.Invoke(result.User);
    }

    public async void Create(string email,string pw,Action<FirebaseUser> callback=null)
    {
        try
        {
            var result = await Auth.CreateUserWithEmailAndPasswordAsync(email, pw);

            /*print("회원가입 성공");*/


            /*print($"회원가입된 유저 : {CurrentUser.Email}");*/
            

            usersRef = DB.GetReference($"users/{result.User.UserId}");        //reference의 역할은 데이터베이스의 위치를 가리키는 역할

            UserData userData = new UserData(result.User.UserId);
            string userdataJson = JsonConvert.SerializeObject(userData);

            await usersRef.SetRawJsonValueAsync(userdataJson);      //데이터베이스에 데이터를 저장하는 함수  //SetRawJsonValueAsync는 json형식으로 데이터를 저장하는 함수

            this.userData = userData;

            onLogin?.Invoke(result.User);
            callback?.Invoke(result.User);
        }
        catch (FirebaseException fe)
        {
            Debug.LogError(fe.Message);
        }


    }

    public async void UpdateUser(string name, string pw, Action callback= null)
    {
        var profile = new UserProfile
        {
            DisplayName = name
        };

        await Auth.CurrentUser.UpdateUserProfileAsync(profile);
        if (false == string.IsNullOrWhiteSpace(pw))
        {
            await Auth.CurrentUser.UpdatePasswordAsync(pw);
        }

        callback?.Invoke();
    }



    public void Logout()
    {
        Auth.SignOut();
        
    }

    // DB와 Transaction을 수행할 함수들
    public async void UpdateCharacterClass(UserData.Class cl,Action callback=null)
    {
        string refKey=//"characterClass";
            nameof(UserData.characterClass);    //nameof : 문자열로 바꿔주는 함수

        // 캐릭터 클래스 항목의 데이터 레퍼런스
        var classRef=usersRef.Child(refKey/*"characterClass"*/);                // 변수명 바꿔도 괜찮음

        await classRef.SetValueAsync((int)cl);        // 캐릭터 클래스를 데이터베이스에 저장
        userData.characterClass = cl;                // 유저 데이터에도 저장

        callback?.Invoke();

    }

    public async void UpdateCharacterType(UserData.CharacterType ct,Action callback=null)
    {
        string refKey =
            nameof(UserData.EyesType);

        var eyeRef=usersRef.Child(refKey);

        await eyeRef.SetValueAsync((int)ct);
        userData.EyesType = ct;

        callback?.Invoke();
    }

    public async void UpdateCharacterLevel(Action callback=null)
    {
        int level = userData.level + 1;
        string refkey = nameof(UserData.level);
        
        var levelRef = usersRef.Child(refkey);
        await levelRef.SetValueAsync(level);
        userData.level = level;

        callback?.Invoke();
    }

    public async void UpdateCharacterAddress(string address,Action callback=null)       //async : 비동기 함수
    {
        string refKey = nameof(UserData.address);

        var addressRef = usersRef.Child(refKey);
        await addressRef.SetValueAsync(address);    //await : 비동기 함수를 동기적으로 사용할 수 있게 해주는 키워드
        userData.address = address;

        callback?.Invoke();
    }

    public async void UpdateUserData(string childName,object value, Action<object> callback=null)
    {
        var targetRef=usersRef.Child(childName);
        
    }

    // TODO : DB를 통해 메시지를 주고 받는 함수

    // 메시지 보낼때
    public void SendMsg(string receiver,Message msg)
    {
        var msgRef = DB.GetReference($"msg/{receiver}");
        var msgJson = JsonConvert.SerializeObject(msg);
        msgRef.Child(msg.sender + msg.sendTime).SetRawJsonValueAsync(msgJson);
    }

    // 메시지 받을때
    public void RecvMsgEventHandler(object sender/*이벤트를 호출한 객체의 고유Key 구분을 위한
                                                  객체*/,ChildChangedEventArgs args)
    {
        if(args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError);
            return;
        }

        else
        {
            var rawJson = args.Snapshot.GetRawJsonValue();

            Message msg=JsonConvert.DeserializeObject<Message>(rawJson);

            string msgString=
                $"보낸이 : {msg.sender}\n 내용 : {msg.message}\n 보낸 시각 : {msg.GetSendTime()}";
            onReceiveMessage?.Invoke(msgString);
        }
    }

    

}
