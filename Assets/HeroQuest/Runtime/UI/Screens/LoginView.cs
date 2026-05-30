using System.Threading;
using HeroQuest.Net.Auth;
using HeroQuest.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HeroQuest.UI.Screens
{
    public sealed class LoginView : UIView
    {
        [SerializeField] private TMP_InputField accountInput;
        [SerializeField] private TMP_InputField passwordInput;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button loginButton;
        [SerializeField] private string nextSceneName = "CharacterSelectScene";

        private IAuthService authService;

        private void Awake()
        {
            authService = new LocalTestAuthService();
        }

        private void OnEnable()
        {
            loginButton?.onClick.AddListener(Login);
            if (accountInput != null && string.IsNullOrWhiteSpace(accountInput.text))
            {
                accountInput.text = LocalTestAuthService.TestAccount;
            }

            if (passwordInput != null && string.IsNullOrWhiteSpace(passwordInput.text))
            {
                passwordInput.text = LocalTestAuthService.TestPassword;
            }
        }

        private void OnDisable()
        {
            loginButton?.onClick.RemoveListener(Login);
        }

        private async void Login()
        {
            SetMessage("Logging in...");
            SetInteractable(false);

            var account = accountInput != null ? accountInput.text.Trim() : string.Empty;
            var password = passwordInput != null ? passwordInput.text : string.Empty;
            var result = await authService.LoginAsync(account, password, CancellationToken.None);

            SetInteractable(true);
            SetMessage(result.Message);

            if (result.Success)
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }

        private void SetInteractable(bool interactable)
        {
            if (loginButton != null)
            {
                loginButton.interactable = interactable;
            }
        }

        private void SetMessage(string message)
        {
            if (messageText != null)
            {
                messageText.text = message;
            }
        }
    }
}
