using System;

[Serializable]
public class UserData
{
    public enum Class  // 직업
    {
        Warrior,
        Wizard,
        Rogue,
        Archer,
    }
    
    public enum CharacterType
    {
        Cube,
        Twoeyes,
        Lens
    }

    public string userId;
    public string userName;
    public int level;
    public Class characterClass;
    public CharacterType EyesType;
    // json을 통해 데이터를 주고 받을때는 intager로 캐스팅을 해서 활용하겠다.
    public string address;

    public UserData()
    {
        // Json을 역직렬화 하여 객체를 생성하기 위해 필요한 기본생성자.

    }

    public UserData(string userId, string userName, int level, Class characterClass,CharacterType EyesType, string address)
    {
        this.userId = userId;
        this.userName = userName;
        this.level = level;
        this.characterClass = characterClass;
        this.EyesType = EyesType;
        this.address = address;
        
    }

    public UserData(string userId)
    {
        this.userId = userId;
        userName="무명의 전사";
        level=1;
        characterClass=Class.Warrior;
        EyesType=CharacterType.Cube;
        address="none";
        
    }

    
}

[Serializable]
public class Message
{
    public string sender;
    public string message;
    public long sendTime;

    public DateTime GetSendTime() { return new DateTime(sendTime); }

}
