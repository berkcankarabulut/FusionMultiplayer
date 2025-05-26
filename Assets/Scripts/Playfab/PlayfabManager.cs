using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;

namespace FPSGame.PlayFab
{
    public class PlayfabManager : MonoBehaviour
    {
        public static PlayfabManager Instance { get; private set; }

        private LeaderboardService leaderboardService = new LeaderboardService();
        private AuthService authService = new AuthService();
        private string currentUsername;
        private string currentPlayFabId;

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
                {
                    currentPlayFabId = loginResult.PlayFabId;
                    tcs.SetResult(
                        new LoginUI.AuthResult { Success = true, Message = "Login başarılı!" }
                    );
                },
                error =>
                    tcs.SetResult(
                        new LoginUI.AuthResult { Success = false, Message = error.ErrorMessage }
                    )
            );
            return await tcs.Task;
        }

        public async Task<LoginUI.AuthResult> LoginWithUsernameAsync(
            string username,
            string password
        )
        {
            var tcs = new TaskCompletionSource<LoginUI.AuthResult>();
            authService.LoginWithUsername(
                username,
                password,
                loginResult =>
                {
                    currentPlayFabId = loginResult.PlayFabId;
                    tcs.SetResult(
                        new LoginUI.AuthResult { Success = true, Message = "Login başarılı!" }
                    );
                },
                error =>
                    tcs.SetResult(
                        new LoginUI.AuthResult { Success = false, Message = error.ErrorMessage }
                    )
            );
            return await tcs.Task;
        }

        public async Task<LoginUI.AuthResult> RegisterWithEmailAsync(
            string email,
            string password,
            string username
        )
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

        public async Task<string> GetUsernameAsync()
        {
            if (!string.IsNullOrEmpty(currentUsername))
                return currentUsername;

            Debug.Log("currentPlayFabId:" + currentPlayFabId);
            if (string.IsNullOrEmpty(currentPlayFabId))
            {
                Debug.LogWarning("No PlayFabId available. User might not be logged in.");
                return null;
            }
            var tcs = new TaskCompletionSource<string>();
            var request = new GetAccountInfoRequest { PlayFabId = currentPlayFabId };
            PlayFabClientAPI.GetAccountInfo(
                request,
                result =>
                {
                    currentUsername =
                        result.AccountInfo?.Username ?? result.AccountInfo?.TitleInfo?.DisplayName;
                    tcs.SetResult(currentUsername);
                },
                error =>
                {
                    Debug.LogError(error.GenerateErrorReport());
                    tcs.SetResult(null);
                }
            );

            return await tcs.Task;
        }
        #endregion
    }
}
