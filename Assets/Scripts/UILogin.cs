using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Events;
using Yvonta;

    public class UILogin : MonoBehaviour
    {
        private TMP_InputField emailInputField;
        private TMP_InputField passwordInputField;
        private Button loginButton;
        private Button registerButton;
        private TextMeshProUGUI statusText;
        private GameObject dialogPanelObj;

        // Events
        public UnityEvent<string, string> OnLoginSubmitted = new UnityEvent<string, string>();
        public UnityEvent OnRegisterClicked = new UnityEvent();

        private const string userEmailKey = "SavedUserEmail";

        public void BuildUI(Transform parentCanvasTransform)
        {
            if (dialogPanelObj != null) return;

            dialogPanelObj = UIHelper.CreatePanel(
                parentCanvasTransform,
                "LoginPanel",
                new Vector2(440, 400),                           // Height adjusted to hold status text
                new RectOffset(35, 35, 20, 20),                  // Reduced top/bottom padding to 20
                10f                                              // Spacing between elements set to 10
            );

            UIHelper.CreateHeader(
                dialogPanelObj.transform,
                "Welcome",
                "Log in to your account",
                50f,
                22f,
                12f
            );

            emailInputField = UIHelper.CreateInputField(
                dialogPanelObj.transform,
                "Email address...",
                TMP_InputField.ContentType.EmailAddress,
                40f
            );

            passwordInputField = UIHelper.CreateInputField(
                dialogPanelObj.transform,
                "Password...",
                TMP_InputField.ContentType.Password,
                40f
            );

            loginButton = UIHelper.CreateButton(
                dialogPanelObj.transform,
                "Log In",
                UIHelper.GetPrimaryButtonColors(),
                UIHelper.TextLight,
                44f
            );

            UIHelper.CreateDivider(dialogPanelObj.transform);

            registerButton = UIHelper.CreateButton(
                dialogPanelObj.transform,
                "Create Account",
                UIHelper.GetSecondaryButtonColors(),
                UIHelper.SubtitleColor,
                38f
            );

            statusText = UIHelper.CreateStatusText(dialogPanelObj.transform, 20f);

            if (loginButton != null) loginButton.onClick.AddListener(HandleLoginClicked);
            if (registerButton != null) registerButton.onClick.AddListener(HandleRegisterClicked);

            if (PlayerPrefs.HasKey(userEmailKey))
            {
                emailInputField.text = PlayerPrefs.GetString(userEmailKey);
            }


            SetVisible(true);
        }

        public void SetVisible(bool isVisible)
        {
            if (dialogPanelObj != null)
            {
                dialogPanelObj.SetActive(isVisible);
            }
        }

        // --- ADDED METHODS TO FIX CS1061 ---
        public void SetInteractable(bool state)
        {
            if (emailInputField != null) emailInputField.interactable = state;
            if (passwordInputField != null) passwordInputField.interactable = state;
            if (loginButton != null) loginButton.interactable = state;
            if (registerButton != null) registerButton.interactable = state;
        }

        public void SetStatusMessage(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }
        // -----------------------------------

        private void HandleLoginClicked()
        {
            string email = emailInputField != null ? emailInputField.text.Trim() : string.Empty;
            string password = passwordInputField != null ? passwordInputField.text : string.Empty;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                SetStatusMessage("Please fill in both fields.");
                return;
            }


            PlayerPrefs.SetString(userEmailKey, email);
            PlayerPrefs.Save(); // Force write to disk

            SetStatusMessage(string.Empty);
            OnLoginSubmitted?.Invoke(email, password);
        }

        private void HandleRegisterClicked()
        {
            SetStatusMessage(string.Empty);
            OnRegisterClicked?.Invoke();
        }
    }
