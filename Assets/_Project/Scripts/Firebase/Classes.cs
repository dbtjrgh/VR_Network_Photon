using System;

[Serializable]
public class UserData
{
    public enum Class  // ����
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
    // json�� ���� �����͸� �ְ� �������� intager�� ĳ������ �ؼ� Ȱ���ϰڴ�.
    public string address;

    public UserData()
    {
        // Json�� ������ȭ �Ͽ� ��ü�� �����ϱ� ���� �ʿ��� �⺻������.

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
        userName="������ ����";
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
