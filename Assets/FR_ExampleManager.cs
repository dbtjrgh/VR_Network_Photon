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
        // 메시지 전달
        dialog.text.text = message;
    }

    public GameObject PanelOpen(PanelType type)
    {
        GameObject returnPanel = null;

        foreach (var row in panels)
        {
            bool isMatch = type == row.Key;         // 딕셔너리에서 파라미터와 키가 일치하는지 여부

            if (isMatch)
            {
                returnPanel = row.Value.gameObject;      // 일치하면 return을 함
            }

            row.Value.gameObject.SetActive(isMatch);   // 일치하는 패널만 활성화
        }

        return returnPanel;
    }

    public T PanelOpen<T>() where T : MonoBehaviour
    {

        T returnPanel = null; ;
        foreach (var row in panels)
        {
            bool isMatch = typeof(T) == row.Value.GetType(); //딕셔너리에서 파라미터와 값이 일치하는지 여부
            if (isMatch)
            {
                returnPanel = (T)row.Value;
            }

            row.Value.gameObject.SetActive(isMatch); // 일치하는 패널만 활성화
        }
        return returnPanel;
    }

}
