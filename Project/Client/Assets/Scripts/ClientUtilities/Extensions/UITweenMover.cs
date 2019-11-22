using System;
using UnityEngine;

public class UITweenMover : MonoBehaviour
{
    [SerializeField]
    public RectTransform RectTransformPanel;
    [SerializeField]
    public Vector2 AnchorMinValueOut;
    [SerializeField]
    public Vector2 AnchorMaxValueOut;
    [SerializeField]
    public Vector2 AnchorMinValueIn;
    [SerializeField]
    public Vector2 AnchorMaxValueIn;
    [Range(0.0F, 10.0F)]
    public float AnimateTime = 0.5F;

    public LeanTweenType InsideInTweenType = LeanTweenType.easeSpring;
    public LeanTweenType InsideOutTweanType = LeanTweenType.easeSpring;
    private bool isTweening = false;
    private Action onComplete = null;

    public void OnAnimateInsideIn(Action OnComplete = null)
    {
        if (RectTransformPanel == null || LeanTween.isTweening(RectTransformPanel.gameObject) || isTweening)
            return;

        isTweening = true;
        onComplete = null;
        onComplete = OnComplete;
        LeanTween.value(RectTransformPanel.gameObject, AnchorMinValueOut, AnchorMinValueIn, AnimateTime).setOnUpdateVector2(SetMinAnchor).setEase(InsideInTweenType);

        LeanTween.value(RectTransformPanel.gameObject, AnchorMaxValueOut, AnchorMaxValueIn, AnimateTime).setOnUpdateVector2(SetMaxAnchor).setEase(InsideInTweenType).setOnComplete(complete);
    }

    private void complete()
    {
        isTweening = false;
        onComplete?.Invoke();
        onComplete = null;
    }

    public void OnAnimateInsideOut(Action OnComplete = null)
    {
        if (RectTransformPanel == null || LeanTween.isTweening(RectTransformPanel.gameObject)|| isTweening)
            return;

        isTweening = true;
        onComplete = null;
        onComplete = OnComplete;

        LeanTween.value(RectTransformPanel.gameObject, AnchorMinValueIn, AnchorMinValueOut, AnimateTime).setOnUpdateVector2(SetMinAnchor).setEase(InsideOutTweanType);

        LeanTween.value(RectTransformPanel.gameObject, AnchorMaxValueIn, AnchorMaxValueOut, AnimateTime).setOnUpdateVector2(SetMaxAnchor).setEase(InsideInTweenType).setOnComplete(complete);
    }

    public void CancelTween()
    {
        LeanTween.cancel(RectTransformPanel.gameObject, true);
    }

    private void SetMaxAnchor(Vector2 AnchorMax)
    {
        RectTransformPanel.anchorMax = AnchorMax;
    }

    private void SetMinAnchor(Vector2 AnchorMin)
    {
        RectTransformPanel.anchorMin = AnchorMin;
    }

    public void SetToOutInstantly()
    {
        if (RectTransformPanel == null)
            return;

        RectTransformPanel.anchorMax = AnchorMaxValueOut;
        RectTransformPanel.anchorMin = AnchorMinValueOut;
    }

    public void SetToInInstantly()
    {
        if (RectTransformPanel == null)
            return;

        RectTransformPanel.anchorMax = AnchorMaxValueIn;
        RectTransformPanel.anchorMin = AnchorMinValueIn;
    }
}
