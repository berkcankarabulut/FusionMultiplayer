using System;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

namespace FPSGame.PlayFab
{
    public class AuthService
    {
        public void LoginWithEmail(
            string email,
            string password,
            Action<LoginResult> onSuccess = null,
            Action<PlayFabError> onFail = null
        )
        {
            var request = new LoginWithEmailAddressRequest { Email = email, Password = password };
            PlayFabClientAPI.LoginWithEmailAddress(
                request,
                result =>
                {
                    Debug.Log(result.PlayFabId + " logged in");
                    onSuccess?.Invoke(result);
                },
                error =>
                {
                    Debug.Log(error.ErrorMessage);
                    onFail?.Invoke(error);
                }
            );
        }

        public void LoginWithUsername(
            string username,
            string password,
            Action<LoginResult> onSuccess = null,
            Action<PlayFabError> onFail = null
        )
        {
            var request = new LoginWithPlayFabRequest { Username = username, Password = password };
            PlayFabClientAPI.LoginWithPlayFab(
                request,
                result =>
                {
                    Debug.Log(result.PlayFabId + " logged in");
                    onSuccess?.Invoke(result);
                },
                error =>
                {
                    Debug.Log(error.ErrorMessage);
                    onFail?.Invoke(error);
                }
            );
        }

        public void RegisterWithEmail(
            string email,
            string password,
            string username,
            Action<RegisterPlayFabUserResult> onSuccess = null,
            Action<PlayFabError> onFail = null
        )
        {
            var request = new RegisterPlayFabUserRequest
            {
                Email = email,
                Password = password,
                Username = username,
                RequireBothUsernameAndEmail = true,
            };
            PlayFabClientAPI.RegisterPlayFabUser(
                request,
                result =>
                {
                    Debug.Log("Register Completed: " + result.PlayFabId);
                    onSuccess?.Invoke(result);
                },
                error =>
                {
                    Debug.Log(error.ErrorMessage);
                    onFail?.Invoke(error);
                }
            );
        }
    }
}
