using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

    public static class UIHelper
    {
        // Visual Palette Defaults
        public static readonly Color PanelBgColor = new Color(0.16f, 0.14f, 0.13f, 0.98f);
        public static readonly Color CardBgColor = new Color(0.20f, 0.18f, 0.17f, 1f);
        public static readonly Color InputBgColor = new Color(0.12f, 0.11f, 0.10f, 1f);
        public static readonly Color InputOutlineColor = new Color(0.40f, 0.36f, 0.33f, 1f);
        public static readonly Color InputFocusOutline = new Color(0.88f, 0.48f, 0.37f, 1f);
        public static readonly Color PrimaryAccent = new Color(0.88f, 0.48f, 0.37f, 1f);
        public static readonly Color PrimaryHover = new Color(0.92f, 0.55f, 0.45f, 1f);
        public static readonly Color SecondaryAccent = new Color(0.35f, 0.6f, 0.52f, 1f);
        public static readonly Color SecondaryHover = new Color(0.42f, 0.68f, 0.59f, 1f);
        public static readonly Color TextDark = new Color(0.95f, 0.93f, 0.90f, 1f);
        public static readonly Color TextLight = new Color(0.98f, 0.96f, 0.93f, 1f);
        public static readonly Color SubtitleColor = new Color(0.75f, 0.70f, 0.65f, 1f);
        public static readonly Color MeterColor = new Color(0.95f, 0.8f, 0.5f, 1f);

        public static GameObject CreatePanel(Transform parent, string name, Vector2 size, RectOffset padding, float spacing)
        {
            GameObject panelObj = new GameObject(name);
            panelObj.transform.SetParent(parent, false);

            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.sizeDelta = size;

            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = PanelBgColor;

            Shadow panelShadow = panelObj.AddComponent<Shadow>();
            panelShadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
            panelShadow.effectDistance = new Vector2(4, -6);

            VerticalLayoutGroup layout = panelObj.AddComponent<VerticalLayoutGroup>();
            layout.padding = padding;
            layout.spacing = spacing;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.UpperCenter;

            return panelObj;
        }

        public static GameObject CreateHeader(Transform parent, string title, string subtitle, float height = 48f, float titleSize = 22f, float subSize = 12f)
        {
            GameObject headerObj = new GameObject("HeaderContainer");
            headerObj.transform.SetParent(parent, false);

            LayoutElement le = headerObj.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;
            le.flexibleHeight = 0;

            VerticalLayoutGroup vlg = headerObj.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(0, 0, 0, 0);
            vlg.spacing = 2;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;
            vlg.childAlignment = TextAnchor.MiddleCenter;

            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(headerObj.transform, false);
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = title;
            titleText.fontSize = titleSize;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = TextLight;
            titleText.alignment = TextAlignmentOptions.Center;

            GameObject subObj = new GameObject("Subtitle");
            subObj.transform.SetParent(headerObj.transform, false);
            TextMeshProUGUI subText = subObj.AddComponent<TextMeshProUGUI>();
            subText.text = subtitle;
            subText.fontSize = subSize;
            subText.color = SubtitleColor;
            subText.alignment = TextAlignmentOptions.Center;

            return headerObj;
        }

        public static TextMeshProUGUI CreateCard(Transform parent, string defaultText, float height = 110f)
        {
            GameObject cardOutlineObj = new GameObject("CardOutline");
            cardOutlineObj.transform.SetParent(parent, false);
            
            LayoutElement le = cardOutlineObj.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;
            le.flexibleHeight = 0;

            Image cardOutlineImg = cardOutlineObj.AddComponent<Image>();
            cardOutlineImg.color = InputOutlineColor;

            GameObject cardObj = new GameObject("CardContent");
            cardObj.transform.SetParent(cardOutlineObj.transform, false);
            RectTransform cardRect = cardObj.AddComponent<RectTransform>();
            cardRect.anchorMin = Vector2.zero;
            cardRect.anchorMax = Vector2.one;
            cardRect.offsetMin = new Vector2(2, 2);
            cardRect.offsetMax = new Vector2(-2, -2);

            Image cardBg = cardObj.AddComponent<Image>();
            cardBg.color = CardBgColor;

            GameObject sentenceObj = new GameObject("CardText");
            sentenceObj.transform.SetParent(cardObj.transform, false);
            RectTransform sentenceRect = sentenceObj.AddComponent<RectTransform>();
            sentenceRect.anchorMin = Vector2.zero;
            sentenceRect.anchorMax = Vector2.one;
            sentenceRect.offsetMin = new Vector2(15, 10);
            sentenceRect.offsetMax = new Vector2(-15, -10);

            TextMeshProUGUI cardText = sentenceObj.AddComponent<TextMeshProUGUI>();
            cardText.fontSize = 14;
            cardText.lineSpacing = 1.2f;
            cardText.alignment = TextAlignmentOptions.Center;
            cardText.color = TextLight;
            cardText.text = defaultText;

            return cardText;
        }

        public static TMP_InputField CreateInputField(Transform parent, string placeholderText, TMP_InputField.ContentType contentType = TMP_InputField.ContentType.Standard, float height = 40f)
        {
            GameObject outlineObj = new GameObject("InputOutline_" + placeholderText);
            outlineObj.transform.SetParent(parent, false);

            LayoutElement le = outlineObj.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;
            le.flexibleHeight = 0;

            Image outlineImg = outlineObj.AddComponent<Image>();
            outlineImg.color = InputOutlineColor;

            GameObject inputObj = new GameObject("InputField");
            inputObj.transform.SetParent(outlineObj.transform, false);
            RectTransform inputRect = inputObj.AddComponent<RectTransform>();
            inputRect.anchorMin = Vector2.zero;
            inputRect.anchorMax = Vector2.one;
            inputRect.offsetMin = new Vector2(1, 1);
            inputRect.offsetMax = new Vector2(-1, -1);

            Image bg = inputObj.AddComponent<Image>();
            bg.color = InputBgColor;

            TMP_InputField inputField = inputObj.AddComponent<TMP_InputField>();
            inputField.contentType = contentType;

            inputField.onSelect.AddListener((_) => outlineImg.color = InputFocusOutline);
            inputField.onDeselect.AddListener((_) => outlineImg.color = InputOutlineColor);

            GameObject textArea = new GameObject("TextArea");
            textArea.transform.SetParent(inputObj.transform, false);
            RectTransform taRect = textArea.AddComponent<RectTransform>();
            taRect.anchorMin = Vector2.zero;
            taRect.anchorMax = Vector2.one;
            taRect.offsetMin = new Vector2(12, 2);
            taRect.offsetMax = new Vector2(-12, -2);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(textArea.transform, false);
            RectTransform tRect = textObj.AddComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.offsetMin = Vector2.zero;
            tRect.offsetMax = Vector2.zero;

            TextMeshProUGUI textComp = textObj.AddComponent<TextMeshProUGUI>();
            textComp.fontSize = 13;
            textComp.color = TextDark;
            textComp.alignment = TextAlignmentOptions.MidlineLeft;

            GameObject holderObj = new GameObject("Placeholder");
            holderObj.transform.SetParent(textArea.transform, false);
            RectTransform hRect = holderObj.AddComponent<RectTransform>();
            hRect.anchorMin = Vector2.zero;
            hRect.anchorMax = Vector2.one;
            hRect.offsetMin = Vector2.zero;
            hRect.offsetMax = Vector2.zero;

            TextMeshProUGUI holderComp = holderObj.AddComponent<TextMeshProUGUI>();
            holderComp.text = placeholderText;
            holderComp.fontSize = 13;
            holderComp.color = new Color(0.55f, 0.50f, 0.45f, 0.7f);
            holderComp.alignment = TextAlignmentOptions.MidlineLeft;

            inputField.textViewport = taRect;
            inputField.textComponent = textComp;
            inputField.placeholder = holderComp;

            return inputField;
        }

        public static Slider CreateSlider(Transform parent, float height = 8f)
        {
            GameObject sliderObj = new GameObject("VolumeMeter");
            sliderObj.transform.SetParent(parent, false);

            LayoutElement le = sliderObj.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;
            le.flexibleHeight = 0; // FIXED: Removed maxHeight property call

            Slider slider = sliderObj.AddComponent<Slider>();
            slider.interactable = false;

            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(sliderObj.transform, false);
            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;

            Image bgImg = bgObj.AddComponent<Image>();
            bgImg.color = InputBgColor;

            GameObject fillAreaObj = new GameObject("Fill Area");
            fillAreaObj.transform.SetParent(sliderObj.transform, false);
            RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.sizeDelta = Vector2.zero;

            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(fillAreaObj.transform, false);
            RectTransform fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;

            Image fillImg = fillObj.AddComponent<Image>();
            fillImg.color = MeterColor;

            slider.fillRect = fillRect;
            slider.targetGraphic = fillImg;

            return slider;
        }

        public static TMP_Dropdown CreateDropdown(Transform parent, string name, List<string> options, float height = 40f)
        {
            GameObject outlineObj = new GameObject("DropdownOutline_" + name);
            outlineObj.transform.SetParent(parent, false);

            LayoutElement le = outlineObj.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;
            le.flexibleHeight = 0;

            Image outlineImg = outlineObj.AddComponent<Image>();
            outlineImg.color = InputOutlineColor;

            GameObject dropdownObj = new GameObject("Dropdown_" + name);
            dropdownObj.transform.SetParent(outlineObj.transform, false);
            RectTransform dropRect = dropdownObj.AddComponent<RectTransform>();
            dropRect.anchorMin = Vector2.zero;
            dropRect.anchorMax = Vector2.one;
            dropRect.offsetMin = new Vector2(1, 1);
            dropRect.offsetMax = new Vector2(-1, -1);

            Image dropdownBg = dropdownObj.AddComponent<Image>();
            dropdownBg.color = InputBgColor;

            TMP_Dropdown dropdown = dropdownObj.AddComponent<TMP_Dropdown>();

            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(dropdownObj.transform, false);
            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(12, 0);
            labelRect.offsetMax = new Vector2(-25, 0);

            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.fontSize = 13;
            labelText.color = TextDark;
            labelText.alignment = TextAlignmentOptions.MidlineLeft;
            dropdown.captionText = labelText;

            GameObject templateObj = new GameObject("Template");
            templateObj.transform.SetParent(dropdownObj.transform, false);
            RectTransform templateRect = templateObj.AddComponent<RectTransform>();
            templateRect.anchorMin = new Vector2(0, 0);
            templateRect.anchorMax = new Vector2(1, 0);
            templateRect.pivot = new Vector2(0.5f, 1f);
            templateRect.anchoredPosition = new Vector2(0, -2);
            templateRect.sizeDelta = new Vector2(0, 130);

            Image templateImage = templateObj.AddComponent<Image>();
            templateImage.color = CardBgColor;

            ScrollRect scrollRect = templateObj.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            GameObject viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(templateObj.transform, false);
            RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;

            Mask viewportMask = viewportObj.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false;
            Image vpImg = viewportObj.AddComponent<Image>();
            vpImg.color = Color.white;

            scrollRect.viewport = viewportRect;

            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(viewportObj.transform, false);
            RectTransform contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.sizeDelta = new Vector2(0, 0);

            scrollRect.content = contentRect;

            VerticalLayoutGroup contentLayout = contentObj.AddComponent<VerticalLayoutGroup>();
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.spacing = 2f;

            ContentSizeFitter csf = contentObj.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            GameObject itemObj = new GameObject("Item");
            itemObj.transform.SetParent(contentObj.transform, false);

            LayoutElement itemLe = itemObj.AddComponent<LayoutElement>();
            itemLe.minHeight = 30f;
            itemLe.preferredHeight = 30f;
            itemLe.flexibleHeight = 0;

            Toggle itemToggle = itemObj.AddComponent<Toggle>();

            GameObject itemBgObj = new GameObject("Item Background");
            itemBgObj.transform.SetParent(itemObj.transform, false);
            RectTransform itemBgRect = itemBgObj.AddComponent<RectTransform>();
            itemBgRect.anchorMin = Vector2.zero;
            itemBgRect.anchorMax = Vector2.one;
            itemBgRect.sizeDelta = Vector2.zero;

            Image itemBgImage = itemBgObj.AddComponent<Image>();
            itemBgImage.color = CardBgColor;
            itemToggle.targetGraphic = itemBgImage;

            GameObject itemLabelObj = new GameObject("Item Label");
            itemLabelObj.transform.SetParent(itemObj.transform, false);
            RectTransform itemLabelRect = itemLabelObj.AddComponent<RectTransform>();
            itemLabelRect.anchorMin = Vector2.zero;
            itemLabelRect.anchorMax = Vector2.one;
            itemLabelRect.offsetMin = new Vector2(12, 0);
            itemLabelRect.offsetMax = new Vector2(-12, 0);

            TextMeshProUGUI itemLabelText = itemLabelObj.AddComponent<TextMeshProUGUI>();
            itemLabelText.fontSize = 13;
            itemLabelText.color = TextDark;
            itemLabelText.alignment = TextAlignmentOptions.MidlineLeft;

            dropdown.itemText = itemLabelText;
            dropdown.template = templateRect;

            dropdown.options.Clear();
            if (options != null)
            {
                foreach (string opt in options)
                {
                    dropdown.options.Add(new TMP_Dropdown.OptionData(opt));
                }
            }

            dropdown.RefreshShownValue();
            templateObj.SetActive(false);

            return dropdown;
        }

        public static Button CreateButton(Transform parent, string labelText, ColorBlock colors, Color textColor, float height = 40f)
        {
            return CreateButton(parent, labelText, colors, textColor, height, out _);
        }

        public static Button CreateButton(Transform parent, string labelText, ColorBlock colors, Color textColor, float height, out TextMeshProUGUI buttonText)
        {
            GameObject btnObj = new GameObject("Button_" + labelText);
            btnObj.transform.SetParent(parent, false);

            LayoutElement le = btnObj.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;
            le.flexibleHeight = 0;

            Image btnImg = btnObj.AddComponent<Image>();
            Button btn = btnObj.AddComponent<Button>();
            btn.colors = colors;

            Shadow btnShadow = btnObj.AddComponent<Shadow>();
            btnShadow.effectColor = new Color(0f, 0f, 0f, 0.25f);
            btnShadow.effectDistance = new Vector2(1, -2);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform tRect = textObj.AddComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;

            buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = labelText;
            buttonText.fontSize = 13;
            buttonText.fontStyle = FontStyles.Bold;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = textColor;

            return btn;
        }

        public static TextMeshProUGUI CreateStatusText(Transform parent, float height = 20f)
        {
            GameObject statusObj = new GameObject("StatusText");
            statusObj.transform.SetParent(parent, false);

            LayoutElement le = statusObj.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;
            le.flexibleHeight = 0;

            TextMeshProUGUI statusText = statusObj.AddComponent<TextMeshProUGUI>();
            statusText.fontSize = 12;
            statusText.alignment = TextAlignmentOptions.Center;
            statusText.color = new Color(0.95f, 0.75f, 0.35f);

            return statusText;
        }

        public static ColorBlock GetPrimaryButtonColors()
        {
            ColorBlock cb = ColorBlock.defaultColorBlock;
            cb.normalColor = PrimaryAccent;
            cb.highlightedColor = PrimaryHover;
            cb.pressedColor = PrimaryAccent * 0.8f;
            return cb;
        }

        public static ColorBlock GetSecondaryButtonColors()
        {
            ColorBlock cb = ColorBlock.defaultColorBlock;
            cb.normalColor = new Color(1f, 1f, 1f, 0.05f);
            cb.highlightedColor = new Color(1f, 1f, 1f, 0.12f);
            cb.pressedColor = new Color(1f, 1f, 1f, 0.03f);
            return cb;
        }

        public static GameObject CreateDivider(Transform parent)
        {
            GameObject divider = new GameObject("Divider");
            divider.transform.SetParent(parent, false);

            LayoutElement le = divider.AddComponent<LayoutElement>();
            le.minHeight = 1f;
            le.preferredHeight = 1f;
            le.flexibleHeight = 0;

            Image img = divider.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.12f);

            return divider;
        }
    }
