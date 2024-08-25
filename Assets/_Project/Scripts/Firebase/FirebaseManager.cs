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
    public FirebaseApp App { get; private set; }        // ���̾� ���̽� �⺻ ��(�⺻ ��ɵ�)
    public FirebaseAuth Auth { get;private set; }       // ������� ����

    public FirebaseDatabase DB { get; private set; }     // �����ͺ��̽� ����

    public UserData userData;

    public DatabaseReference usersRef;

    

    // ���̾� ���̽� ���� �ʱ�ȭ �Ǿ� ��� �������� ����
    public bool IsInitialized { get; private set; } = false;

    public event Action onInit;                     // ���̾�̽��� �ʱ�ȭ�Ǹ� ȣ��

    public event Action<FirebaseUser> onLogin;       // �α��� �Ǵ� ȸ������ �Ŀ� ȣ��

    public event Action<string> onReceiveMessage;    // ���� ������ �޽����� ������ ȣ��

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

    // �α����� �Ǿ��� �� ȣ����� ������
    private void OnLogin(FirebaseUser user)
    {
        var msgRef = DB.GetReference($"msg/{user.UserId}");
        msgRef.ChildAdded += RecvMsgEventHandler;
    }

    private void MsgRef_ChildAdded(object sender, ChildChangedEventArgs e)
    {
        throw new NotImplementedException();
    }

    //Async�� �Ⱦ����
    private void Initialize()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().
            ContinueWithOnMainThread(
            (Task<DependencyStatus> task) =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogWarning($"���̾�̽� �ʱ�ȭ ���� : {task.Status}");
                }
                else if (task.IsCompleted)
                {
                    print($"���̾�̽� �ʱ�ȭ ���� : {task.Status}");

                    if(task.Result==DependencyStatus.Available)
                    {
                        App = FirebaseApp.DefaultInstance;
                        Auth = FirebaseAuth.DefaultInstance;            // async�� �������� �̷��� ���
                        IsInitialized = true;
                    }


                    App = FirebaseApp.DefaultInstance;
                    Auth = FirebaseAuth.DefaultInstance;
                }
            });     // ���̾�̽� �� �ʱ�ȭ

    }

    // async Ű���带 ���� �񵿱� ���α׷���
    private async void InitializeAsync()
    {
        DependencyStatus status = await FirebaseApp.CheckAndFixDependenciesAsync();

        if(status==DependencyStatus.Available)
        {
            // ���̾�̽� �ʱ�ȭ ����
            print("���̾� ���̽� �ʱ�ȭ ����");
            App=FirebaseApp.DefaultInstance;
            Auth = FirebaseAuth.DefaultInstance;
            DB = FirebaseDatabase.DefaultInstance;
            IsInitialized = true;
            onInit?.Invoke();
        }
        else
        {
            // ���̾�̽� �ʱ�ȭ ����
            Debug.LogWarning($"���̾�̽� �ʱ�ȭ ���� : {status}");
        }
        
    }

    public async void Login(string email,string pw,Action<FirebaseUser> callback=null)
    {
        var result=await Auth.SignInWithEmailAndPasswordAsync(email, pw);
        /*
                print("�α��� ����");*/


        /*print($"�α��ε� ���� : {CurrentUser.Email}");*/

        usersRef = DB.GetReference($"users/{result.User.UserId}");/*.Child("level"); */       //reference�� ������ �����ͺ��̽��� ��ġ�� ����Ű�� ����

        DataSnapshot userDataValues = await usersRef.GetValueAsync();        //�����ͺ��̽����� �����͸� �������� �Լ�

        if(userDataValues.Exists)
        {
            string json= userDataValues.GetRawJsonValue();      // ������ ��ü�� json���� ������
            var address=userDataValues.Child("address");        // �������� ���� ���۷���(�����ͽ�����)�� ������.
            if (address.Exists) print($"�ּ� : {address.GetValue(false)}");
            userData = JsonConvert.DeserializeObject<UserData>(json);
            /*(userDataValues.GetRawJsonValue());*/
            print(json);
            
        }

        else
        {
            FBPanelManager.Instance.Dialog("�α��� ������ ������ ������ϴ�.\n�����Ϳ� �����ϼ���.");
        }

        onLogin?.Invoke(result.User);

        callback?.Invoke(result.User);
    }

    public async void Create(string email,string pw,Action<FirebaseUser> callback=null)
    {
        try
        {
            var result = await Auth.CreateUserWithEmailAndPasswordAsync(email, pw);

            /*print("ȸ������ ����");*/


            /*print($"ȸ�����Ե� ���� : {CurrentUser.Email}");*/
            

            usersRef = DB.GetReference($"users/{result.User.UserId}");        //reference�� ������ �����ͺ��̽��� ��ġ�� ����Ű�� ����

            UserData userData = new UserData(result.User.UserId);
            string userdataJson = JsonConvert.SerializeObject(userData);

            await usersRef.SetRawJsonValueAsync(userdataJson);      //�����ͺ��̽��� �����͸� �����ϴ� �Լ�  //SetRawJsonValueAsync�� json�������� �����͸� �����ϴ� �Լ�

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

    // DB�� Transaction�� ������ �Լ���
    public async void UpdateCharacterClass(UserData.Class cl,Action callback=null)
    {
        string refKey=//"characterClass";
            nameof(UserData.characterClass);    //nameof : ���ڿ��� �ٲ��ִ� �Լ�

        // ĳ���� Ŭ���� �׸��� ������ ���۷���
        var classRef=usersRef.Child(refKey/*"characterClass"*/);                // ������ �ٲ㵵 ������

        await classRef.SetValueAsync((int)cl);        // ĳ���� Ŭ������ �����ͺ��̽��� ����
        userData.characterClass = cl;                // ���� �����Ϳ��� ����

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

    public async void UpdateCharacterAddress(string address,Action callback=null)       //async : �񵿱� �Լ�
    {
        string refKey = nameof(UserData.address);

        var addressRef = usersRef.Child(refKey);
        await addressRef.SetValueAsync(address);    //await : �񵿱� �Լ��� ���������� ����� �� �ְ� ���ִ� Ű����
        userData.address = address;

        callback?.Invoke();
    }

    public async void UpdateUserData(string childName,object value, Action<object> callback=null)
    {
        var targetRef=usersRef.Child(childName);
        
    }

    // TODO : DB�� ���� �޽����� �ְ� �޴� �Լ�

    // �޽��� ������
    public void SendMsg(string receiver,Message msg)
    {
        var msgRef = DB.GetReference($"msg/{receiver}");
        var msgJson = JsonConvert.SerializeObject(msg);
        msgRef.Child(msg.sender + msg.sendTime).SetRawJsonValueAsync(msgJson);
    }

    // �޽��� ������
    public void RecvMsgEventHandler(object sender/*�̺�Ʈ�� ȣ���� ��ü�� ����Key ������ ����
                                                  ��ü*/,ChildChangedEventArgs args)
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
                $"������ : {msg.sender}\n ���� : {msg.message}\n ���� �ð� : {msg.GetSendTime()}";
            onReceiveMessage?.Invoke(msgString);
        }
    }

    

}
