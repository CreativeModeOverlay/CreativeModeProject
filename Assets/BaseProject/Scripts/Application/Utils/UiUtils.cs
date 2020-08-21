using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace CreativeMode
{
    public static class UiUtils
    {
        public static Sequence ChangeText(this Text t, string text, float duration = 1f, Action onTextChanged = null)
        {
            if (t.text != text)
            {
                DOTween.Kill(t);
                var halfDuration = duration / 2f;
                return DOTween.Sequence().SetTarget(t)
                    .Append(t.DOFade(0f, halfDuration))
                    .AppendCallback(() =>
                    {
                        t.text = text;
                        onTextChanged?.Invoke();
                    })
                    .Append(t.DOFade(1f, halfDuration));
            }

            return null;
        }
    }
}