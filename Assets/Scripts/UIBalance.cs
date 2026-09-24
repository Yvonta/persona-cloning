using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIBalance : MonoBehaviour
{
    private GameObject buttonContainer;
    private Button balanceButton;
    private Text legacyText;
    private TextMeshProUGUI tmpText;

    public string buyCreditsUrl = "https://yvonta.com/en-buy-credits";

    public void BuildUI(Transform parent)
    {
        // Ensure this script's GameObject is attached to the Canvas root properly
        if (transform.parent != parent)
        {
            transform.SetParent(parent, false);
        }

        // 1. Create a dedicated Child GameObject for the Button so it never stretches the main canvas/panel
        if (buttonContainer == null)
        {
            Transform existing = transform.Find("BalanceButton");
            if (existing != null)
            {
                buttonContainer = existing.gameObject;
            }
            else
            {
                buttonContainer = new GameObject("BalanceButton");
                buttonContainer.transform.SetParent(transform, false);
            }
        }

        // 2. Position and Anchor the Button Container to Top-Right
        RectTransform buttonRect = buttonContainer.GetComponent<RectTransform>();
        if (buttonRect == null)
        {
            buttonRect = buttonContainer.AddComponent<RectTransform>();
        }

        buttonRect.anchorMin = new Vector2(1f, 1f); // Top-Right Anchor
        buttonRect.anchorMax = new Vector2(1f, 1f); // Top-Right Anchor
        buttonRect.pivot = new Vector2(1f, 1f);     // Top-Right Pivot
        buttonRect.sizeDelta = new Vector2(220f, 45f);       // Explicit width & height
        buttonRect.anchoredPosition = new Vector2(-20f, -20f); // 20px padding from top & right

        // 3. Setup Button and Visual Background
        balanceButton = buttonContainer.GetComponent<Button>();
        if (balanceButton == null)
        {
            balanceButton = buttonContainer.AddComponent<Button>();
        }

        Image buttonImage = buttonContainer.GetComponent<Image>();
        if (buttonImage == null)
        {
            buttonImage = buttonContainer.AddComponent<Image>();
            buttonImage.color = new Color(0.12f, 0.12f, 0.12f, 0.9f); // Dark background container
        }

        balanceButton.onClick.RemoveAllListeners();
        balanceButton.onClick.AddListener(OnBalanceButtonClicked);

        // 4. Setup Text Component Inside the Button Container
        tmpText = buttonContainer.GetComponentInChildren<TextMeshProUGUI>(true);
        legacyText = buttonContainer.GetComponentInChildren<Text>(true);

        if (tmpText == null && legacyText == null)
        {
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonContainer.transform, false);

            // Attempt Legacy Text creation using Unity's default UI font
            legacyText = textObj.AddComponent<Text>();
            legacyText.font = Font.CreateDynamicFontFromOSFont("Arial", 16);
            if (legacyText.font == null)
            {
                legacyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            legacyText.alignment = TextAnchor.MiddleCenter;
            legacyText.color = Color.white;
            legacyText.fontSize = 16;

            RectTransform textRect = legacyText.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }
    }

    public void SetBalance(string balance)
    {
        // If UI hasn't been built yet, build it before setting text
        if (buttonContainer == null)
        {
            BuildUI(transform.parent != null ? transform.parent : transform);
        }

        string formattedBalance = balance;
        if (long.TryParse(balance, out long rawAmount))
        {
            formattedBalance = rawAmount.ToString("N0"); // e.g. 999,990,088,480
        }

        string displayText = $"Balance: {formattedBalance}";

        if (tmpText != null)
        {
            tmpText.text = displayText;
        }
        else if (legacyText != null)
        {
            legacyText.text = displayText;
        }
    }

    private void OnBalanceButtonClicked()
    {
        if (!string.IsNullOrEmpty(buyCreditsUrl))
        {
            Debug.Log($"Opening purchase webpage: {buyCreditsUrl}");
            Application.OpenURL(buyCreditsUrl);
        }
        else
        {
            Debug.LogWarning("Buy credits URL is not configured.");
        }
    }
}