using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using Yvonta;


    public class UISettingsDialog : MonoBehaviour
    {
        private GameObject settingsPanelObj;
        private Transform optionsContainer;

        // Configuration struct for dynamic settings options
        public struct SettingsOption
        {
            public string label;
            public Action onClickAction;

            public SettingsOption(string label, Action onClickAction)
            {
                this.label = label;
                this.onClickAction = onClickAction;
            }
        }

        public void BuildUI(Transform parentCanvasTransform, List<SettingsOption> options)
        {
            if (settingsPanelObj != null)
            {
                SetVisible(true);
                return;
            }

            // 1. Modal Central Settings Panel
            settingsPanelObj = UIHelper.CreatePanel(
                parentCanvasTransform,
                "SettingsDialogPanel",
                new Vector2(350, 450),
                new RectOffset(15, 15, 15, 15),
                12f
            );

            // Anchor to Center
            RectTransform panelRect = settingsPanelObj.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                panelRect.anchorMin = new Vector2(0.5f, 0.5f);
                panelRect.anchorMax = new Vector2(0.5f, 0.5f);
                panelRect.pivot = new Vector2(0.5f, 0.5f);
                panelRect.anchoredPosition = Vector2.zero;
            }

            // Clean existing layout components
            LayoutGroup existingLayout = settingsPanelObj.GetComponent<LayoutGroup>();
            if (existingLayout != null)
            {
                DestroyImmediate(existingLayout);
            }

            // Vertical Layout Setup
            VerticalLayoutGroup verticalGroup = settingsPanelObj.AddComponent<VerticalLayoutGroup>();
            verticalGroup.spacing = 12;
            verticalGroup.padding = new RectOffset(20, 20, 20, 20);
            verticalGroup.childControlWidth = true;
            verticalGroup.childControlHeight = false;
            verticalGroup.childForceExpandWidth = true;
            verticalGroup.childForceExpandHeight = false;
            verticalGroup.childAlignment = TextAnchor.UpperCenter;

            // 2. Header Title
            GameObject titleObj = new GameObject("SettingsTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(settingsPanelObj.transform, false);
            TextMeshProUGUI titleText = titleObj.GetComponent<TextMeshProUGUI>();
            titleText.text = "Settings";
            titleText.fontSize = 22f;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = UIHelper.TextLight;

            LayoutElement titleLayout = titleObj.AddComponent<LayoutElement>();
            titleLayout.preferredHeight = 35f;

            // 3. Dynamic Option Buttons
            if (options != null)
            {
                foreach (var option in options)
                {
                    SettingsOption currentOption = option; // Closure capture safety
                    Button optButton = UIHelper.CreateButton(
                        settingsPanelObj.transform,
                        currentOption.label,
                        UIHelper.GetSecondaryButtonColors(),
                        UIHelper.TextLight,
                        40f
                    );

                    if (optButton != null)
                    {
                        LayoutElement btnLayout = optButton.gameObject.GetComponent<LayoutElement>();
                        if (btnLayout == null) btnLayout = optButton.gameObject.AddComponent<LayoutElement>();
                        btnLayout.preferredHeight = 40f;

                        optButton.onClick.AddListener(() =>
                        {
                            currentOption.onClickAction?.Invoke();
                        });
                    }
                }
            }

            // Flexible Spacer
            GameObject spacer = new GameObject("Spacer", typeof(RectTransform), typeof(LayoutElement));
            spacer.transform.SetParent(settingsPanelObj.transform, false);
            spacer.GetComponent<LayoutElement>().flexibleHeight = 1f;

            // 4. Close Button
            Button closeButton = UIHelper.CreateButton(
                settingsPanelObj.transform,
                "Close",
                UIHelper.GetPrimaryButtonColors(),
                UIHelper.TextLight,
                40f
            );

            if (closeButton != null)
            {
                LayoutElement closeLayout = closeButton.gameObject.GetComponent<LayoutElement>();
                if (closeLayout == null) closeLayout = closeButton.gameObject.AddComponent<LayoutElement>();
                closeLayout.preferredHeight = 40f;

                closeButton.onClick.AddListener(() => SetVisible(false));
            }

            SetVisible(true);
        }

        public void SetVisible(bool isVisible)
        {
            if (settingsPanelObj != null)
            {
                settingsPanelObj.SetActive(isVisible);
            }
        }
    }
