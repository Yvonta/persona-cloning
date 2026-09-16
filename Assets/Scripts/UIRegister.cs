using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using Yvonta;


    public class UIRegister : MonoBehaviour
    {
        private TMP_InputField emailInputField;
        private TMP_InputField passwordInputField;
        private TMP_InputField nameInputField;
        private TMP_Dropdown genderDropdown;
        private TMP_InputField birthdateInputField;
        private Button registerButton;
        private Button backButton;
        private TextMeshProUGUI statusText;
        private GameObject registerPanelObj;

        public event Action<string, string, string, string, string> OnRegisterSubmitted;
        public event Action OnBackClicked;

        public void BuildUI(Transform parentCanvasTransform)
        {
            if (registerPanelObj != null) return;

            // 1. Panel Base Construction
registerPanelObj = UIHelper.CreatePanel(
    parentCanvasTransform,
    "RegisterPanel",
    new Vector2(440, 520),                           // Height adjusted to fit all elements cleanly
    new RectOffset(35, 35, 20, 20),                  // Reduced top/bottom padding to 20
    8f                                               // Spacing between elements set to 8
);

            // 2. Header Section
            UIHelper.CreateHeader(registerPanelObj.transform, "Join Us", "Create your personal account profile");

            // 3. Input Fields
            nameInputField = UIHelper.CreateInputField(registerPanelObj.transform, "Full Name", TMP_InputField.ContentType.Standard);
            emailInputField = UIHelper.CreateInputField(registerPanelObj.transform, "Email Address", TMP_InputField.ContentType.EmailAddress);
            passwordInputField = UIHelper.CreateInputField(registerPanelObj.transform, "Password", TMP_InputField.ContentType.Password);

            // 4. Gender Dropdown
            List<string> genderOptions = new List<string>
            {
                "Select Gender...",
                "Male",
                "Female",
                "Non-binary"
            };
            genderDropdown = UIHelper.CreateDropdown(registerPanelObj.transform, "Gender", genderOptions);

            // 5. Birthdate Field
            birthdateInputField = UIHelper.CreateInputField(registerPanelObj.transform, "Birthdate (YYYY-MM-DD)", TMP_InputField.ContentType.Standard);

            // 6. Action Buttons
            registerButton = UIHelper.CreateButton(registerPanelObj.transform, "Register Account", UIHelper.GetPrimaryButtonColors(), UIHelper.TextLight, 44f);
            backButton = UIHelper.CreateButton(registerPanelObj.transform, "Back to Login", UIHelper.GetSecondaryButtonColors(), UIHelper.SubtitleColor, 36f);

            // 7. Status Text
            statusText = UIHelper.CreateStatusText(registerPanelObj.transform);

            // Listeners
            if (registerButton != null) registerButton.onClick.AddListener(HandleRegisterClicked);
            if (backButton != null) backButton.onClick.AddListener(HandleBackClicked);

            if (emailInputField != null)
            {
                emailInputField.Select();
                emailInputField.ActivateInputField();
            }

            SetVisible(false);
        }

        public void SetVisible(bool isVisible)
        {
            if (registerPanelObj != null)
            {
                registerPanelObj.SetActive(isVisible);
            }
        }

        private void HandleRegisterClicked()
        {
            string email = emailInputField != null ? emailInputField.text.Trim() : string.Empty;
            string password = passwordInputField != null ? passwordInputField.text : string.Empty;
            string name = nameInputField != null ? nameInputField.text.Trim() : string.Empty;
            string gender = (genderDropdown != null && genderDropdown.value > 0) ? genderDropdown.options[genderDropdown.value].text : string.Empty;
            string birthdate = birthdateInputField != null ? birthdateInputField.text.Trim() : string.Empty;

            if (ValidateInputs(email, password, name, gender, birthdate))
            {
                SetStatusMessage(string.Empty);
                OnRegisterSubmitted?.Invoke(email, password, name, gender, birthdate);
            }
        }

        private void HandleBackClicked()
        {
            OnBackClicked?.Invoke();
        }

        private bool ValidateInputs(string email, string password, string name, string gender, string birthdate)
        {
            if (string.IsNullOrEmpty(name))
            {
                SetStatusMessage("Please enter your name.");
                return false;
            }

            if (string.IsNullOrEmpty(email) || !email.Contains("@") || !email.Contains("."))
            {
                SetStatusMessage("Please enter a valid email address.");
                return false;
            }

            if (string.IsNullOrEmpty(password))
            {
                SetStatusMessage("Please enter your password.");
                return false;
            }

            if (string.IsNullOrEmpty(gender))
            {
                SetStatusMessage("Please select your gender.");
                return false;
            }

            if (string.IsNullOrEmpty(birthdate))
            {
                SetStatusMessage("Please enter your birthdate.");
                return false;
            }

            return true;
        }

        public void SetStatusMessage(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }

        public void SetInteractable(bool state)
        {
            if (emailInputField != null) emailInputField.interactable = state;
            if (passwordInputField != null) passwordInputField.interactable = state;
            if (nameInputField != null) nameInputField.interactable = state;
            if (genderDropdown != null) genderDropdown.interactable = state;
            if (birthdateInputField != null) birthdateInputField.interactable = state;
            if (registerButton != null) registerButton.interactable = state;
            if (backButton != null) backButton.interactable = state;
        }
    }
