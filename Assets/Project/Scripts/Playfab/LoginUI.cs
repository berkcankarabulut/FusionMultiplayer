using SceneLoadSystem.Runtime;
using TMPro;
using UnityEngine; 
using UnityEngine.UI;

namespace FPSGame.PlayFab
{
    public class LoginUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _emailOrUsernameInput;
        [SerializeField] private TMP_InputField _passwordInput;
        [SerializeField] private Button _loginButton;
        [SerializeField] private Button _registerButton;
        [SerializeField] private TextMeshProUGUI _infoText; 
        [SerializeField] private SceneLoader _sceneLoader;

        public struct AuthResult
        {
            public bool Success;
            public string Message;
        }

        private void OnEnable()
        {
            _loginButton.onClick.AddListener(OnLoginClicked);
            _registerButton.onClick.AddListener(OnRegisterClicked);
        }

        private void OnDisable()
        {
            _loginButton.onClick.RemoveListener(OnLoginClicked);
            _registerButton.onClick.RemoveListener(OnRegisterClicked);
        } 

        private async void OnRegisterClicked()
        {
            string user = _emailOrUsernameInput.text;
            string pass = _passwordInput.text;

            if (!user.Contains("@"))
            {
                _infoText.text = "Enter Email!";
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
                _infoText.text = registerResult.Message;
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
            string user = _emailOrUsernameInput.text;
            string pass = _passwordInput.text;

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
            _infoText.text = info.ToString();
        }

        private void OnSuccessLogin()
        {
            _sceneLoader.LoadScene();
        }
    }
}