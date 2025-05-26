using System;
using System.Threading.Tasks;
using SceneLoadSystem.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FPSGame.PlayFab
{
    public class LoginUI : MonoBehaviour
    {
        public TMP_InputField emailOrUsernameInput;
        public TMP_InputField passwordInput;
        public Button loginButton;
        public Button registerButton;
        public TextMeshProUGUI infoText;

        [SerializeField]
        private SceneLoader _sceneLoader;

        public struct AuthResult
        {
            public bool Success;
            public string Message;
        }

        private void OnEnable()
        {
            loginButton.onClick.AddListener(OnLoginClicked);
            registerButton.onClick.AddListener(OnRegisterClicked);
        }

        private void OnDisable()
        {
            loginButton.onClick.RemoveListener(OnLoginClicked);
            registerButton.onClick.RemoveListener(OnRegisterClicked);
        }

        private void OnRegisterResult(string message)
        {
            infoText.text = message;
        }

        private async void OnRegisterClicked()
        {
            string user = emailOrUsernameInput.text;
            string pass = passwordInput.text;

            if (!user.Contains("@"))
            {
                infoText.text = "Enter Email!";
                return;
            }

            string username = user.Split('@')[0];
            var registerResult = await PlayfabManager.Instance.RegisterWithEmailAsync(
                user,
                pass,
                username
            );

            if (!registerResult.Success)
            {
                infoText.text = registerResult.Message;
                return;
            }

            var loginResult = await PlayfabManager.Instance.LoginWithEmailAsync(user, pass);
            if (loginResult.Success)
                OnSuccessLogin();
            else
                SetInfo(loginResult.Message);
        }

        private async void OnLoginClicked()
        {
            string user = emailOrUsernameInput.text;
            string pass = passwordInput.text;

            AuthResult result;
            if (user.Contains("@"))
                result = await PlayfabManager.Instance.LoginWithEmailAsync(user, pass);
            else
                result = await PlayfabManager.Instance.LoginWithUsernameAsync(user, pass);

            SetInfo(result.Message);
            if (result.Success)
                OnSuccessLogin();
        }

        private void SetInfo(string info)
        {
            infoText.text = info.ToString();
        }

        private void OnSuccessLogin()
        {
            _sceneLoader.LoadScene();
        }
    }
}
