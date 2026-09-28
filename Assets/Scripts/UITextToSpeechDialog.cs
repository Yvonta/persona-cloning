using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
using System.Collections;
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

        // 1. Base Floating Bar Panel
        dialogPanelObj = UIHelper.CreatePanel(
            parentCanvasTransform,
            "TextToSpeechDialogBar",
            new Vector2(920, 95),
            new RectOffset(12, 12, 10, 10),
            12f
        );

        // Position Panel at 20% screen height from bottom (Anchor Y = 0.2)
        RectTransform panelRect = dialogPanelObj.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            panelRect.anchorMin = new Vector2(0.5f, 0.2f);
            panelRect.anchorMax = new Vector2(0.5f, 0.2f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
        }

        // Remove existing layout components
        LayoutGroup existingLayout = dialogPanelObj.GetComponent<LayoutGroup>();
        if (existingLayout != null)
        {
            DestroyImmediate(existingLayout);
        }

        // Horizontal Layout Setup
        HorizontalLayoutGroup horizontalGroup = dialogPanelObj.AddComponent<HorizontalLayoutGroup>();
        horizontalGroup.spacing = 14;
        horizontalGroup.padding = new RectOffset(12, 12, 10, 10);
        horizontalGroup.childControlWidth = true;
        horizontalGroup.childControlHeight = true;
        horizontalGroup.childForceExpandWidth = false;
        horizontalGroup.childForceExpandHeight = false;
        horizontalGroup.childAlignment = TextAnchor.MiddleCenter;

        // 2. Settings Button - Small Gear Button (44x44)
        settingsButton = UIHelper.CreateButton(
            dialogPanelObj.transform,
            "⚙",
            UIHelper.GetSecondaryButtonColors(),
            UIHelper.TextLight,
            44f
        );

        if (settingsButton != null)
        {
            SetupButtonHitbox(settingsButton.gameObject, 44f, 44f);
            settingsButton.onClick.AddListener(() => OpenSettingsMenu(parentCanvasTransform));

            TMP_Text gearText = settingsButton.GetComponentInChildren<TMP_Text>();
            if (gearText != null)
            {
                gearText.fontSize = 22;
            }
        }

        // 3. Input Field (Height 60, Width 280, Font 18pt)
        messageInputField = UIHelper.CreateInputField(
            dialogPanelObj.transform,
            "Type a message...",
            TMP_InputField.ContentType.Standard,
            60f
        );

        if (messageInputField != null)
        {
            GameObject inputObj = messageInputField.transform.parent != null && messageInputField.transform.parent != dialogPanelObj.transform
                ? messageInputField.transform.parent.gameObject
                : messageInputField.gameObject;

            ResetTransform(inputObj.GetComponent<RectTransform>());

            LayoutElement inputLayout = inputObj.GetComponent<LayoutElement>();
            if (inputLayout == null) inputLayout = inputObj.AddComponent<LayoutElement>();
            
            inputLayout.preferredWidth = 280f;
            inputLayout.flexibleWidth = 1f;
            inputLayout.minWidth = 160f;
            inputLayout.preferredHeight = 60f;

            messageInputField.lineType = TMP_InputField.LineType.SingleLine;

            Image inputBg = messageInputField.GetComponent<Image>();
            if (inputBg == null) inputBg = messageInputField.gameObject.AddComponent<Image>();
            inputBg.color = new Color(0.12f, 0.12f, 0.14f, 0.95f);

            if (messageInputField.textComponent != null)
            {
                messageInputField.textComponent.fontSize = 18f;
                messageInputField.textComponent.alignment = TextAlignmentOptions.MidlineLeft;
                messageInputField.textComponent.textWrappingMode = TextWrappingModes.NoWrap;
                messageInputField.textComponent.overflowMode = TextOverflowModes.Ellipsis;
                messageInputField.textComponent.raycastTarget = false;

                RectTransform textRect = messageInputField.textComponent.rectTransform;
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = new Vector2(16, 0);
                textRect.offsetMax = new Vector2(-16, 0);
            }

            if (messageInputField.placeholder != null && messageInputField.placeholder is TextMeshProUGUI placeholderTmp)
            {
                placeholderTmp.fontSize = 18f;
                placeholderTmp.alignment = TextAlignmentOptions.MidlineLeft;
                placeholderTmp.textWrappingMode = TextWrappingModes.NoWrap;
                placeholderTmp.overflowMode = TextOverflowModes.Ellipsis;
                placeholderTmp.raycastTarget = false;

                RectTransform placeholderRect = placeholderTmp.rectTransform;
                placeholderRect.anchorMin = Vector2.zero;
                placeholderRect.anchorMax = Vector2.one;
                placeholderRect.offsetMin = new Vector2(16, 0);
                placeholderRect.offsetMax = new Vector2(-16, 0);
            }
        }

        // 4. Send Button
        sendButton = UIHelper.CreateButton(
            dialogPanelObj.transform,
            "Send",
            UIHelper.GetPrimaryButtonColors(),
            UIHelper.TextLight,
            60f
        );

        if (sendButton != null)
        {
            SetupButtonHitbox(sendButton.gameObject, 80f, 60f);
            sendButton.onClick.AddListener(HandleSendClicked);
            
            TMP_Text sendText = sendButton.GetComponentInChildren<TMP_Text>();
            if (sendText != null)
            {
                sendText.fontSize = 16;
            }
        }

        // 5. "Hold to Speak" Big Red Circular Button
        voiceInputButton = UIHelper.CreateButton(
            dialogPanelObj.transform,
            "🎙",
            UIHelper.GetRedButtonColors(),
            UIHelper.TextLight,
            75f
        );

        if (voiceInputButton != null)
        {
            SetupButtonHitbox(voiceInputButton.gameObject, 75f, 75f);

            Image btnImg = voiceInputButton.GetComponent<Image>();
            if (btnImg != null)
            {
                Sprite circleSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
                if (circleSprite != null)
                {
                    btnImg.sprite = circleSprite;
                    btnImg.type = Image.Type.Simple;
                }
            }

            TMP_Text micText = voiceInputButton.GetComponentInChildren<TMP_Text>();
            if (micText != null)
            {
                micText.fontSize = 28;
            }

            PulsingHoldButton pulsingScript = voiceInputButton.gameObject.AddComponent<PulsingHoldButton>();

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

        // 6. Status Text overlay (Positioned directly ABOVE the panel at ~25% screen height)
        statusText = UIHelper.CreateStatusText(parentCanvasTransform, 20f);
        if (statusText != null)
        {
            statusText.raycastTarget = false;
            statusText.alignment = TextAlignmentOptions.Center;
            statusText.textWrappingMode = TextWrappingModes.NoWrap;

            RectTransform statusRect = statusText.GetComponent<RectTransform>();
            if (statusRect != null)
            {
                statusRect.anchorMin = new Vector2(0.5f, 0.2f);
                statusRect.anchorMax = new Vector2(0.5f, 0.2f);
                statusRect.pivot = new Vector2(0.5f, 0f);
                statusRect.sizeDelta = new Vector2(600f, 30f);
                statusRect.anchoredPosition = new Vector2(0f, 60f); // Positioned above the panel
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