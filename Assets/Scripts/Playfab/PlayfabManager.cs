using System;
using PlayFab.ClientModels;
using UnityEngine;
using PlayFab;
public class PlayfabManager : MonoBehaviour
{
    private void Start()
    {
        Login();
    }

    private void Login()
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true,
        };
        PlayFabClientAPI.LoginWithCustomID(request, OnSuccess, OnFail);
    }

    private void OnSuccess(LoginResult obj)
    {
        Debug.Log(obj.PlayFabId+ " logged in");
    }

    private void OnFail(PlayFabError obj)
    {
        Debug.Log(obj.ErrorMessage);
    }
}
