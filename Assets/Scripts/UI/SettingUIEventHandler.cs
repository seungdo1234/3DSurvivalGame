using System;
using UnityEngine;

public class SettingUIEventHandler : MonoBehaviour
{
    [SerializeField]private GameObject settingUI;

    private void Awake()
    {
        settingUI = transform.GetChild(0).gameObject;
    }

    private void Start()
    {
        CharacterManager.Instance.Player.onSettingUI += SettingEvent;
    }

    private void SettingEvent()
    {
        settingUI.SetActive(!settingUI.activeSelf);
    }

    public void ExitSetting() // 버튼
    {
        settingUI.gameObject.SetActive(false);
        CharacterManager.Instance.Player.controller.ControlLocked();
    }
}