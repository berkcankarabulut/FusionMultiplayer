using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;

public class PlayfabManager : MonoBehaviour
{
    public static PlayfabManager Instance { get; private set; }

    private LeaderboardService leaderboardService = new LeaderboardService();
    private AuthService authService = new AuthService();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    #region Result Functions
    private void OnSuccess(LoginResult obj)
    {
        Debug.Log(obj.PlayFabId + " logged in");
    }

    private void OnFail(PlayFabError obj)
    {
        Debug.Log(obj.ErrorMessage);
    }
    #endregion

    #region Leaderboard
    public void SendLeaderboard(int score)
    {
        leaderboardService.SendLeaderboard(score);
    }

    public void GetLeaderboard()
    {
        leaderboardService.GetLeaderboard();
    }
    #endregion


    #region Auth

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log("Register Success: " + result.PlayFabId);
    }

    public async Task<LoginUI.AuthResult> LoginWithEmailAsync(string email, string password)
    {
        var tcs = new TaskCompletionSource<LoginUI.AuthResult>();
        authService.LoginWithEmail(
            email,
            password,
            loginResult =>
                tcs.SetResult(
                    new LoginUI.AuthResult { Success = true, Message = "Login başarılı!" }
                ),
            error =>
                tcs.SetResult(
                    new LoginUI.AuthResult { Success = false, Message = error.ErrorMessage }
                )
        );
        return await tcs.Task;
    }

    public async Task<LoginUI.AuthResult> LoginWithUsernameAsync(string username, string password)
    {
        var tcs = new TaskCompletionSource<LoginUI.AuthResult>();
        authService.LoginWithUsername(
            username,
            password,
            loginResult =>
                tcs.SetResult(
                    new LoginUI.AuthResult { Success = true, Message = "Login başarılı!" }
                ),
            error =>
                tcs.SetResult(
                    new LoginUI.AuthResult { Success = false, Message = error.ErrorMessage }
                )
        );
        return await tcs.Task;
    }

    public async Task<LoginUI.AuthResult> RegisterWithEmailAsync(string email, string password, string username)
    {
        var tcs = new TaskCompletionSource<LoginUI.AuthResult>();
        authService.RegisterWithEmail(
            email,
            password,
            username,
            registerResult =>
                tcs.SetResult(
                    new LoginUI.AuthResult { Success = true, Message = "Kayıt başarılı!" }
                ),
            error =>
                tcs.SetResult(
                    new LoginUI.AuthResult { Success = false, Message = error.ErrorMessage }
                )
        );
        return await tcs.Task;
    }
    #endregion
}
