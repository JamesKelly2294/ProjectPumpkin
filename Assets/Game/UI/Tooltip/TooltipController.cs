using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public CanvasGroup tooltip;

    public float delayTime = 0.25f;
    public float currentValue = 0;
    public float animationTime = 0.25f;
    public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private bool isHovered = false;

    public void Update()
    {
        float maxValue = delayTime + animationTime;
        if (isHovered) {
            currentValue += Time.deltaTime;
            currentValue = Mathf.Min(currentValue, maxValue);
        } else {
            currentValue -= Time.deltaTime;
            currentValue = Mathf.Max(0, currentValue);
        }

        if (currentValue > delayTime) {
            tooltip.gameObject.SetActive(true);
            tooltip.alpha = animationCurve.Evaluate((currentValue - delayTime) / animationTime);
        } else {
            tooltip.gameObject.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }
}
