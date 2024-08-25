using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FR_ExampleManager : MonoBehaviour
{
    public enum PanelType
    {
        Login,
        Create,
        Info,
        Update,
    }



    public static FR_ExampleManager Instance { get; private set; }

    public FBLoginPanel login;
    public FBCreatePanel create;
    public FBUserInfoPanel userInfo;
    public FBUserInfoUpdatePanel infoUpdate;

    public FBDialog dialog;

    private Dictionary<PanelType, MonoBehaviour> panels;



    private void Awake()
    {
        Instance = this;
        panels = new() {
        { PanelType.Login,login },
        { PanelType.Create, create },
        { PanelType.Info, userInfo },
        { PanelType.Update, infoUpdate }
            };



    }

    private void Start()
    {
        _ = PanelOpen(PanelType.Login);
        dialog.gameObject.SetActive(false);
    }

    public void Dialog(string message)
    {
        dialog.gameObject.SetActive(true);
        // �޽��� ����
        dialog.text.text = message;
    }

    public GameObject PanelOpen(PanelType type)
    {
        GameObject returnPanel = null;

        foreach (var row in panels)
        {
            bool isMatch = type == row.Key;         // ��ųʸ����� �Ķ���Ϳ� Ű�� ��ġ�ϴ��� ����

            if (isMatch)
            {
                returnPanel = row.Value.gameObject;      // ��ġ�ϸ� return�� ��
            }

            row.Value.gameObject.SetActive(isMatch);   // ��ġ�ϴ� �гθ� Ȱ��ȭ
        }

        return returnPanel;
    }

    public T PanelOpen<T>() where T : MonoBehaviour
    {

        T returnPanel = null; ;
        foreach (var row in panels)
        {
            bool isMatch = typeof(T) == row.Value.GetType(); //��ųʸ����� �Ķ���Ϳ� ���� ��ġ�ϴ��� ����
            if (isMatch)
            {
                returnPanel = (T)row.Value;
            }

            row.Value.gameObject.SetActive(isMatch); // ��ġ�ϴ� �гθ� Ȱ��ȭ
        }
        return returnPanel;
    }

}
