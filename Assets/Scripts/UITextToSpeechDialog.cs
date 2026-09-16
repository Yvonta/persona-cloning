using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
using System.Collections.Generic;
using Yvonta;

    public class UITextToSpeechDialog : MonoBehaviour
    {
        private TMP_InputField messageInputField;
        private Button sendButton;
        private Button voiceInputButton;
        private Button settingsButton;
        private TextMeshProUGUI statusText;
        private GameObject dialogPanelObj;

        private UISettingsDialog settingsDialog;

        // Events
        public event Action<string> OnTextSubmitted;
        public event Action OnVoiceInputStarted;
        public event Action OnVoiceInputStopped;

        private List<UISettingsDialog.SettingsOption> options; 

        public void BuildUI(Transform parentCanvasTransform, List<UISettingsDialog.SettingsOption> options)
        {
            if (dialogPanelObj != null) return;

            this.options = options;

            // 1. Base Floating Bar Panel (Increased width to fit the Settings button)
            dialogPanelObj = UIHelper.CreatePanel(
                parentCanvasTransform,
                "TextToSpeechDialogBar",
                new Vector2(850, 65),
                new RectOffset(10, 10, 10, 10),
                12f
            );

            // Anchor panel to Bottom-Center
            RectTransform panelRect = dialogPanelObj.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                panelRect.anchorMin = new Vector2(0.5f, 0f);
                panelRect.anchorMax = new Vector2(0.5f, 0f);
                panelRect.pivot = new Vector2(0.5f, 0f);
                panelRect.anchoredPosition = new Vector2(0f, 25f);
            }

            // Remove existing layout components synchronously
            LayoutGroup existingLayout = dialogPanelObj.GetComponent<LayoutGroup>();
            if (existingLayout != null)
            {
                DestroyImmediate(existingLayout);
            }

            // Horizontal Layout Setup
            HorizontalLayoutGroup horizontalGroup = dialogPanelObj.AddComponent<HorizontalLayoutGroup>();
            horizontalGroup.spacing = 10;
            horizontalGroup.padding = new RectOffset(10, 10, 10, 10);
            horizontalGroup.childControlWidth = true;
            horizontalGroup.childControlHeight = true;
            horizontalGroup.childForceExpandWidth = false;
            horizontalGroup.childForceExpandHeight = false;
            horizontalGroup.childAlignment = TextAnchor.MiddleCenter;

            // 2. Input Field Setup
            messageInputField = UIHelper.CreateInputField(
                dialogPanelObj.transform,
                "Type a message to speak...",
                TMP_InputField.ContentType.Standard,
                45f
            );

            if (messageInputField != null)
            {
                GameObject inputObj = messageInputField.transform.parent != null && messageInputField.transform.parent != dialogPanelObj.transform
                    ? messageInputField.transform.parent.gameObject
                    : messageInputField.gameObject;

                ResetTransform(inputObj.GetComponent<RectTransform>());

                LayoutElement inputLayout = inputObj.GetComponent<LayoutElement>();
                if (inputLayout == null) inputLayout = inputObj.AddComponent<LayoutElement>();
                
                inputLayout.preferredWidth = 380f;
                inputLayout.flexibleWidth = 1f;
                inputLayout.minWidth = 200f;
                inputLayout.preferredHeight = 45f;

                messageInputField.lineType = TMP_InputField.LineType.SingleLine;

                Image inputBg = messageInputField.GetComponent<Image>();
                if (inputBg == null) inputBg = messageInputField.gameObject.AddComponent<Image>();
                inputBg.color = new Color(0.12f, 0.12f, 0.14f, 0.95f);

                if (messageInputField.textComponent != null)
                {
                    messageInputField.textComponent.alignment = TextAlignmentOptions.MidlineLeft;
                    messageInputField.textComponent.textWrappingMode = TextWrappingModes.NoWrap;
                    messageInputField.textComponent.overflowMode = TextOverflowModes.Ellipsis;
                    messageInputField.textComponent.raycastTarget = false;

                    RectTransform textRect = messageInputField.textComponent.rectTransform;
                    textRect.anchorMin = Vector2.zero;
                    textRect.anchorMax = Vector2.one;
                    textRect.offsetMin = new Vector2(12, 0);
                    textRect.offsetMax = new Vector2(-12, 0);
                }

                if (messageInputField.placeholder != null && messageInputField.placeholder is TextMeshProUGUI placeholderTmp)
                {
                    placeholderTmp.alignment = TextAlignmentOptions.MidlineLeft;
                    placeholderTmp.textWrappingMode = TextWrappingModes.NoWrap;
                    placeholderTmp.overflowMode = TextOverflowModes.Ellipsis;
                    placeholderTmp.raycastTarget = false;

                    RectTransform placeholderRect = placeholderTmp.rectTransform;
                    placeholderRect.anchorMin = Vector2.zero;
                    placeholderRect.anchorMax = Vector2.one;
                    placeholderRect.offsetMin = new Vector2(12, 0);
                    placeholderRect.offsetMax = new Vector2(-12, 0);
                }
            }

            // 3. Send Button
            sendButton = UIHelper.CreateButton(
                dialogPanelObj.transform,
                "Send",
                UIHelper.GetPrimaryButtonColors(),
                UIHelper.TextLight,
                45f
            );

            if (sendButton != null)
            {
                SetupButtonHitbox(sendButton.gameObject, 80f, 45f);
                sendButton.onClick.AddListener(HandleSendClicked);
            }

            // 4. Voice Hold-to-Talk Button
            voiceInputButton = UIHelper.CreateButton(
                dialogPanelObj.transform,
                "Hold to Speak",
                UIHelper.GetSecondaryButtonColors(),
                UIHelper.SubtitleColor,
                45f
            );

            if (voiceInputButton != null)
            {
                SetupButtonHitbox(voiceInputButton.gameObject, 130f, 45f);

                EventTrigger trigger = voiceInputButton.gameObject.GetComponent<EventTrigger>();
                if (trigger == null) trigger = voiceInputButton.gameObject.AddComponent<EventTrigger>();

                EventTrigger.Entry entryDown = new EventTrigger.Entry();
                entryDown.eventID = EventTriggerType.PointerDown;
                entryDown.callback.AddListener((data) => { HandleVoicePointerDown(); });
                trigger.triggers.Add(entryDown);

                EventTrigger.Entry entryUp = new EventTrigger.Entry();
                entryUp.eventID = EventTriggerType.PointerUp;
                entryUp.callback.AddListener((data) => { HandleVoicePointerUp(); });
                trigger.triggers.Add(entryUp);
            }

            // 5. Settings Button
            settingsButton = UIHelper.CreateButton(
                dialogPanelObj.transform,
                "Settings",
                UIHelper.GetSecondaryButtonColors(),
                UIHelper.TextLight,
                45f
            );

            if (settingsButton != null)
            {
                SetupButtonHitbox(settingsButton.gameObject, 90f, 45f);
                settingsButton.onClick.AddListener(() => OpenSettingsMenu(parentCanvasTransform));
            }

            // 6. Status Text overlay ABOVE bottom bar
            statusText = UIHelper.CreateStatusText(parentCanvasTransform, 20f);
            if (statusText != null)
            {
                statusText.raycastTarget = false;
                statusText.alignment = TextAlignmentOptions.Center;
                statusText.textWrappingMode = TextWrappingModes.NoWrap;

                RectTransform statusRect = statusText.GetComponent<RectTransform>();
                if (statusRect != null)
                {
                    statusRect.anchorMin = new Vector2(0.5f, 0f);
                    statusRect.anchorMax = new Vector2(0.5f, 0f);
                    statusRect.pivot = new Vector2(0.5f, 0f);
                    statusRect.sizeDelta = new Vector2(600f, 30f);
                    statusRect.anchoredPosition = new Vector2(0f, 95f);
                }
            }

            SetVisible(true);
        }

        private void OpenSettingsMenu(Transform parentCanvasTransform)
        {
            if (settingsDialog == null)
            {
                GameObject settingsObj = new GameObject("SettingsDialogController");
                settingsObj.transform.SetParent(transform);
                settingsDialog = settingsObj.AddComponent<UISettingsDialog>();
            }

            settingsDialog.BuildUI(parentCanvasTransform, this.options);
        }

        private void SetupButtonHitbox(GameObject buttonObj, float width, float height)
        {
            RectTransform rect = buttonObj.GetComponent<RectTransform>();
            ResetTransform(rect);

            LayoutElement layout = buttonObj.GetComponent<LayoutElement>();
            if (layout == null) layout = buttonObj.AddComponent<LayoutElement>();

            layout.preferredWidth = width;
            layout.minWidth = width;
            layout.preferredHeight = height;
            layout.minHeight = height;
            layout.flexibleWidth = 0f;

            Image img = buttonObj.GetComponent<Image>();
            if (img != null)
            {
                img.raycastTarget = true;
            }

            TMP_Text label = buttonObj.GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                label.raycastTarget = false;
                RectTransform labelRect = label.rectTransform;
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = Vector2.zero;
                labelRect.offsetMax = Vector2.zero;
            }
        }

        private void ResetTransform(RectTransform rect)
        {
            if (rect == null) return;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.localPosition = Vector3.zero;
            rect.localScale = Vector3.one;
        }

        public void SetVisible(bool isVisible)
        {
            if (dialogPanelObj != null)
            {
                dialogPanelObj.SetActive(isVisible);
            }
            if (statusText != null)
            {
                statusText.gameObject.SetActive(isVisible);
            }
        }

        public void SetInteractable(bool state)
        {
            if (messageInputField != null) messageInputField.interactable = state;
            if (sendButton != null) sendButton.interactable = state;
            if (voiceInputButton != null) voiceInputButton.interactable = state;
            if (settingsButton != null) settingsButton.interactable = state;
        }

        public void SetStatusMessage(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }

        public void ClearInput()
        {
            if (messageInputField != null)
            {
                messageInputField.text = string.Empty;
            }
        }

        private void HandleSendClicked()
        {
            string message = messageInputField != null ? messageInputField.text.Trim() : string.Empty;

            if (string.IsNullOrEmpty(message))
            {
                SetStatusMessage("Please type a message first.");
                return;
            }

            SetStatusMessage(string.Empty);
            OnTextSubmitted?.Invoke(message);
        }

        private void HandleVoicePointerDown()
        {
            SetStatusMessage("Recording... Release to send.");
            OnVoiceInputStarted?.Invoke();
        }

        private void HandleVoicePointerUp()
        {
            SetStatusMessage("Processing recording...");
            OnVoiceInputStopped?.Invoke();
        }
    }
