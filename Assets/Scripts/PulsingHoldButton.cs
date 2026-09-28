using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class PulsingHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public float pulseSpeed = 5f;
    public float minScale = 0.90f;
    public float maxScale = 1.15f;

    private bool isHolding = false;
    private Vector3 originalScale;
    private Coroutine pulseCoroutine;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isHolding = true;
        if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
        pulseCoroutine = StartCoroutine(PulseRoutine());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isHolding = false;
        if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
        transform.localScale = originalScale;
    }

    private IEnumerator PulseRoutine()
    {
        float timer = 0f;
        while (isHolding)
        {
            timer += Time.deltaTime * pulseSpeed;
            float scaleFactor = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(timer) + 1f) / 2f);
            transform.localScale = originalScale * scaleFactor;
            yield return null;
        }
        transform.localScale = originalScale;
    }
}